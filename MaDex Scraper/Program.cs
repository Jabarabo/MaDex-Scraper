using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Windows.Forms;
using iTextSharp.text.pdf;
using System.IO;
using iTextSharp.text;

namespace MaDex_Scraper
{
    class Program

    {
        static HttpClient Client = new HttpClient();
        [STAThread]
        static void Main(string[] args)
        {
            while (true)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Welcome to Jabarabo's MangaDex Scraper.");
                Console.WriteLine("--------------------------------------");
                Console.WriteLine("Please paste manga ID. This is the large line of alphanumerical characters in the URL for the manga.");
                Console.Write("Ex. https://mangadex.org/title/");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("304ceac3-8cdb-4fe7-acf7-2b6ff7a60613\n\n");
                Console.ForegroundColor = ConsoleColor.White;
                string MangaIDNum = Console.ReadLine();
                
                
                Console.Clear();
                Console.WriteLine("Enter the number of the chapter you would like to download?");
                Console.WriteLine("Ex. 46");
                string MangaChapterNum = Console.ReadLine();
                
                
                Console.Clear();
                string MangaDexAPICall = $"https://api.mangadex.org/chapter?chapter={MangaChapterNum}&manga={MangaIDNum}";
                
                
                HttpResponseMessage response = Client.SendAsync(new HttpRequestMessage(HttpMethod.Get, MangaDexAPICall)).Result;
                JObject JSON = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                
                
                List<JToken> EnglishChapters = new List<JToken>();
                foreach (JToken jtoken in JSON["results"])
                    {
                        if (jtoken["data"]["attributes"]["translatedLanguage"].Value<string>() == "en")
                           EnglishChapters.Add(jtoken);
                    };
                if (EnglishChapters.Count == 0)
                    {
                        Console.Clear();
                        Console.WriteLine("There are no english chapters available. We don't support any language other than english. I'm too lazy to add support.");
                        Console.WriteLine("Cry More\n\n");
                        Console.WriteLine("Press any key to continue.");
                        Console.ReadKey();
                    }
                else if (EnglishChapters.Count == 1)
                    {
                        JToken chapter = EnglishChapters[0];
                        string hash = chapter["data"]["attributes"]["hash"].Value<string>();
                        List<string> pageURLs = new List<string>();
                        foreach (JToken jtoken in chapter["data"]["attributes"]["data"])
                            {
                                pageURLs.Add($"https://uploads.mangadex.org/data/{hash}/{jtoken.Value<string>()}");
                            };


                        Console.Clear();
                        Console.WriteLine("What would you like to name the file?");
                        string PDFName = Console.ReadLine(); //Names the PDF file


                        Console.Clear();
                        Console.WriteLine("Where would you like to save the PDF to?");
                        FolderBrowserDialog NewOutputDir = new FolderBrowserDialog(); //Creates the object for finding the folder.
                        string Outpath = "big monkey nuts";
                        if (NewOutputDir.ShowDialog() == System.Windows.Forms.DialogResult.OK) //Opens dialog and user selects new output folder.
                            {
                                Outpath = NewOutputDir.SelectedPath;
                            };
                        string path = $"{Outpath}\\{PDFName}.pdf";


                        Document document = new Document();
                        PdfWriter.GetInstance(document, new FileStream(path, FileMode.Create));
                        document.Open();
                        foreach (string url in pageURLs)
                            {
                                iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(url);
                                document.SetMargins(0, 0, 0, 0);
                                image.ScaleToFit(document.PageSize);
                                document.Add(image);
                            };
                        document.Close();
                        Console.Clear();
                    }
                else
                    {
                        Console.WriteLine("We don't support multiple translators. It's their fault for being so damn prolific.");
                        Console.WriteLine("Press any key to continue");
                        Console.ReadKey();
                    }
            }
        }
    }
}
