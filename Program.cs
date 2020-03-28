using System;
using HtmlAgilityPack;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Net;

namespace GetSubtitle
{
    class Program
    {
        static string CleanStr(string str)
        {
            return Regex.Replace(str, @"\t|\n|\r", "");            
        }   
        static int Main(string[] args)
        {
            int                 index;
            string              query;
            string              url;
            HtmlWeb             web;
            HtmlDocument        doc;
            HtmlNodeCollection  nodes;
            List<Movie>         movies = new List<Movie>();
            List<Subtitle>      FullList = new List<Subtitle>();
            string[]            LanguageList = new string[100];
            Subtitle[]          FilteredSubtitles = new Subtitle[500];

            Console.WriteLine("Get subtitle from Subscene.com");
            Console.Write("Search title: ");            
            query = Console.ReadLine().Trim().Replace(' ', '-');            

            url = $"https://subscene.com/subtitles/searchbytitle?query={query}";
            web = new HtmlWeb();

            try
            {
                doc = web.Load(url);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return -1;
            }

            nodes = doc.DocumentNode.SelectNodes("//*[@id=\"left\"]/div/div"); 
            
            index = 0;
            foreach (var ul in nodes.Elements("ul"))
            {
                foreach (var li in ul.Elements("li"))
                {                       
                    movies.Add(new Movie(){
                        id      = index,
                        title   = CleanStr(li.Elements("div").ElementAt(0).InnerText),
                        link    = CleanStr(li.Elements("div").ElementAt(0).Element("a").Attributes["href"].Value)
                    });

                    index ++;
                }
            }

            Console.WriteLine("-------------");
            foreach (var item in movies)
            {
                System.Console.WriteLine($"{item.id}\t{item.title}");
            }
            
            Console.Write("Select Title Index: ");
            index = int.Parse(Console.ReadLine());

            url = $"https://subscene.com{movies.First(n => n.id == index).link}";            

            try
            {
                doc = web.Load(url);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return -1;
            }                       
                        
            nodes = doc.DocumentNode.SelectNodes(@"/html/body/div[1]/div[2]/div[4]/table/tbody/tr/td/a");
                        
            foreach (var item in nodes)
            {                
                if(item.Elements("span").Count() >= 2)
                {                    
                    FullList.Add(
                        new Subtitle() {                            
                            Language = CleanStr(item.Elements("span").ElementAt(0).InnerText),                            
                            BodyText = CleanStr(item.Elements("span").ElementAt(1).InnerText),
                            Link = CleanStr(item.Attributes["href"].Value)
                        }
                    );
                }                        
            }

            int langIndex = 0;
            Console.WriteLine("-------------");
            foreach (var item in FullList.Select(n => n.Language).Distinct())
            {
                LanguageList[langIndex] = item;
                Console.WriteLine($"{langIndex} \t {item}");
                langIndex ++;
            }

            Console.Write("Select language index: ");
            langIndex = int.Parse(Console.ReadLine());


            int subIndex = 0;
            Console.WriteLine("-------------");
            foreach (var item in FullList.Where(n => n.Language == LanguageList[langIndex]))
            {
                FilteredSubtitles[subIndex] = item;
                Console.WriteLine($"{subIndex} \t {item.BodyText}");
                subIndex ++;
            }
            
            Console.Write("Select Subtitle Index: ");
            subIndex = int.Parse(Console.ReadLine());

            url = "https://subscene.com" + FilteredSubtitles[subIndex].Link;
            doc = web.Load(url);
            var node = doc.DocumentNode.SelectSingleNode(@"/html/body/div[1]/div[2]/div[2]/div[2]/div[2]/ul/li[4]/div/a");
            string downloadLink = "https://subscene.com" + node.Attributes["href"].Value;

            using (var client = new WebClient())
            {
                try
                {
                    client.DownloadFile(downloadLink, FilteredSubtitles[subIndex].BodyText);
                    Console.WriteLine("Done!");                    
                }
                catch (System.Exception)
                {
                    Console.WriteLine("Couldn't do it!");
                    return -1;
                }
                
                return 1;
            }            

        }
    }
}
