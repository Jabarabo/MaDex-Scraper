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
                uint MangaChapterNum;
                while (!uint.TryParse(Console.ReadLine(), out MangaChapterNum))
                {
                    Console.WriteLine("Please enter a valid number.");
                }

                Console.Clear();
                List<JToken> EnglishChapters;
                GetTranslations(MangaIDNum, MangaChapterNum, "en", out EnglishChapters);

                if (EnglishChapters.Count == 0)
                {
                    Console.Clear();
                    Console.WriteLine("There are no english translations available. We don't support any language other than english. I'm too lazy to add support.");
                    Console.WriteLine("Cry More\n\n");
                    Console.WriteLine("Press any key to continue.");
                    Console.ReadKey();
                }
                else if (EnglishChapters.Count == 1)
                {
                    if (ChapterToPDF(EnglishChapters[0]))
                    {
                        Console.WriteLine("Successfully converted chapter to PDF.\n\nPress any key to continue.");
                        Console.ReadKey();
                    }
                    else
                    {
                        Console.WriteLine("Failed to convert chapter to PDF.\n\nPress any key to continue.");
                        Console.ReadKey();
                    }
                }
                else
                {
                    Console.WriteLine("We don't support multiple translations. It's their fault for being so damn prolific.");
                    Console.WriteLine("Press any key to continue");
                    Console.ReadKey();
                }
            }
        }

        static void GetTranslations(string MangaID, uint ChapterNumber, string Language, out List<JToken> Translations)
        {

            Console.Clear();
            Console.WriteLine($"Finding all {Language} translations for chapter {ChapterNumber} of Manga {MangaID}...");

            string MangaDexAPICall = $"https://api.mangadex.org/chapter?chapter={ChapterNumber}&manga={MangaID}";

            HttpResponseMessage response = Client.SendAsync(new HttpRequestMessage(HttpMethod.Get, MangaDexAPICall)).Result;
            JObject JSON = JObject.Parse(response.Content.ReadAsStringAsync().Result);

            Translations = new List<JToken>();

            foreach (JToken jtoken in JSON["results"])
            {
                if (jtoken["data"]["attributes"]["translatedLanguage"].Value<string>() == Language)
                    Translations.Add(jtoken);
            };

            Console.WriteLine($"\nFound {Translations.Count} translation(s).");
        }
        static bool ChapterToPDF(JToken chapter)
        {
            string hash = chapter["data"]["attributes"]["hash"].Value<string>();
            List<string> pageURLs = new List<string>();
            foreach (JToken jtoken in chapter["data"]["attributes"]["data"])
            {
                pageURLs.Add($"https://uploads.mangadex.org/data/{hash}/{jtoken.Value<string>()}");
            };

            if (pageURLs.Count < 1)
            {
                Console.Clear();
                Console.WriteLine("No pages could be found for chapter.");
                return false;
            }

            Console.Clear();
            Console.WriteLine("What would you like to name the PDF?");
            string PDFName = Path.GetFileNameWithoutExtension(Console.ReadLine()); //Names the PDF file

            Console.Clear();
            Console.WriteLine("Where would you like to save the PDF?");
            SaveFileDialog NewOutputDir = new SaveFileDialog(); //Creates the object for finding the folder.
            NewOutputDir.FileName = PDFName;
            NewOutputDir.Filter = "PDF Files | *.pdf";
            NewOutputDir.DefaultExt = "pdf";

            do { } while (NewOutputDir.ShowDialog() != System.Windows.Forms.DialogResult.OK); //Opens dialog and user selects new output folder.
            string path = NewOutputDir.FileName;

            Console.Clear();
            Console.WriteLine("Converting chapter to PDF...");

            Document document = new Document();

            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    PdfWriter.GetInstance(document, fs);
                    document.Open();
                    document.SetMargins(0, 0, 0, 0);

                    foreach (string url in pageURLs)
                    {
                        iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(url);
                        image.ScaleToFit(document.PageSize);
                        document.Add(image);
                    };
                    document.Close();
                }
            }
            catch (System.IO.IOException exception)
            {
                Console.Clear();
                Console.WriteLine($"Error writing to selected file:\n{exception.Message}");
                Console.ReadKey();
                return false;
            }
            Console.Clear();
            return true;
        }
    }
}
