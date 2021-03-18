using System;
using HtmlAgilityPack;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Net;
using System.IO;

namespace GetSubtitle
{
    static class Program
    {
        static void Main(string[] args)
        {
            string title;

            Console.WriteLine("#################################");
            Console.WriteLine("## Subscene.com Sub Downloader ##");
            Console.WriteLine("## By: ArmanNM ##################");
            Console.WriteLine("#################################");

            if (args.Length < 2)
            {
                Console.Write("Enter Movie Title: ");
                title = Console.ReadLine().CleanString();
            }
            else
            {
                title = args[1].CleanString();
            }

            try
            {
                var movies = SearchMovieByTitle(title);
                PopulateList<Movie>(movies);
                var index = GeneratePrompt("Movie");

                var subs = GetMovieSubtitles(movies, int.Parse(index));

                var languages = ExtractLanguages(subs);
                PopulateList<BaseClass>(languages);
                index = GeneratePrompt("Language");

                subs = GetFilteredSubtitles(subs, languages.FirstOrDefault(l => l.Id == int.Parse(index)).Title);
                PopulateList<Subtitle>(subs);
                index = GeneratePrompt("Subtitle");

                DownloadSubtitle(subs, int.Parse(index));

                Console.WriteLine("Done!");
            }
            catch (Exception e)
            {
                Console.WriteLine("Couldnt do that! \t" + e.Message);
                FriendlyClose();
            }
        }

        static string CleanString(this String str)
        {
            return Regex.Replace(str.Trim(), @"\t|\n|\r", "");
        }

        static List<Movie> SearchMovieByTitle(string title)
        {
            string url = $"https://subscene.com/subtitles/searchbytitle?query={title}";
            var doc = GetHtmlDocument(url);
            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//*[@id=\"left\"]/div/div");
            var movies = new List<Movie>();
            int counter = 0;
            foreach (var ul in nodes.Elements("ul"))
            {
                foreach (var li in ul.Elements("li"))
                {
                    movies.Add(new Movie()
                    {
                        Id = counter,
                        Title = li.Elements("div").ElementAt(0).InnerText.CleanString(),
                        Url = li.Elements("div").ElementAt(0).Element("a").Attributes["href"].Value.CleanString()
                    });

                    counter++;
                }
            }

            return movies;
        }

        static List<Subtitle> GetMovieSubtitles(List<Movie> movies, int movieIndex)
        {
            string url = $"https://subscene.com{movies.FirstOrDefault(m => m.Id == movieIndex).Url}";
            var doc = GetHtmlDocument(url);
            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes(@"/html/body/div[1]/div[2]/div[4]/table/tbody/tr/td/a");
            var subs = new List<Subtitle>();
            int counter = 0;
            foreach (var item in nodes)
            {
                if (item.Elements("span").Count() >= 2)
                {
                    subs.Add(
                        new Subtitle()
                        {
                            Id = counter,
                            Language = item.Elements("span").ElementAt(0).InnerText.CleanString(),
                            Title = item.Elements("span").ElementAt(1).InnerText.CleanString(),
                            Url = item.Attributes["href"].Value.CleanString()
                        }
                    );

                    counter++;
                }
            }

            return subs;
        }

        static List<BaseClass> ExtractLanguages(List<Subtitle> subs)
        {
            var distintSubLanguages = subs.Select(s => s.Language).Distinct();
            var languages = new List<BaseClass>();
            int counter = 0;
            foreach (var item in distintSubLanguages)
            {
                languages.Add(new BaseClass
                {
                    Id = counter,
                    Title = item
                });
                counter++;
            }

            return languages;
        }

        static List<Subtitle> GetFilteredSubtitles(List<Subtitle> subtitles, string language)
        {
            return subtitles.Where(n => n.Language == language).ToList();
        }

        static void DownloadSubtitle(List<Subtitle> subtitles, int subIndex)
        {
            string url = "https://subscene.com" + subtitles.FirstOrDefault(s => s.Id == subIndex).Url;
            var doc = GetHtmlDocument(url);
            HtmlNode node = doc.DocumentNode.SelectSingleNode(@"/html/body/div[1]/div[2]/div[2]/div[2]/div[2]/ul/li[4]/div/a");
            url = "https://subscene.com" + node.Attributes["href"].Value;
            using (var client = new WebClient())
            {
                client.DownloadFile(url, $"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}{subtitles.First(s => s.Id == subIndex).Title.Trim()}.zip");
            }
        }

        static string GeneratePrompt(string valueToPromptFor)
        {
            Console.Write($"Select {valueToPromptFor} Index: ");
            return Console.ReadLine();
        }

        static void GenerateListHeader(string message)
        {
            Console.WriteLine(message);
            Console.WriteLine("-------------");
        }

        static void PopulateList<T>(List<T> listOfItems) where T : BaseClass
        {
            int index = 0;
            foreach (var item in listOfItems)
            {
                Console.WriteLine($"{item.Id}\t{item.Title}");
                index++;
            }
        }

        static HtmlDocument GetHtmlDocument(string url)
        {
            var web = new HtmlWeb();
            return web.Load(url);
        }

        static void FriendlyClose()
        {
            Environment.ExitCode = -1;
            if (Environment.UserInteractive)
            {
                Console.WriteLine("Press <Enter> to close");
                _ = Console.ReadLine();
            }
        }

    }

    //
    // MODELS
    //

    public class BaseClass
    {
        public int Id { get; set; }
        public string Title { get; set; }
    }

    public class Movie : BaseClass
    {
        public string Url { get; set; }
    }

    public class Subtitle : BaseClass
    {
        public string Language { get; set; }
        public string Url { get; set; }
    }
}