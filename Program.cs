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
        static int Main(string[] args)
        {
            int                 index;
            string              query;
            string              url;
            HtmlWeb             web;
            HtmlDocument        doc;
            HtmlNode            node;
            HtmlNodeCollection  nodes;
            List<Movie>         movies = new List<Movie>();
            List<Subtitle>      FullList = new List<Subtitle>();
            string[]            LanguageList = new string[100]; // NOT GOOD
            Subtitle[]          FilteredSubtitles = new Subtitle[500]; // NOT GOOD

            // SEARCH BASED ON
            // MOVIE TITLE

            Console.WriteLine("Get subtitle from Subscene.com");
            Console.Write("Search title: ");            
            query = Console.ReadLine().Trim().Replace(' ', '-');  // NEEDS EVALUATION

            url = $"https://subscene.com/subtitles/searchbytitle?query={query}"; // SEARCH URL
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

            nodes = doc.DocumentNode.SelectNodes("//*[@id=\"left\"]/div/div"); // SEARCH RESULT SECTION
            
            // FILL
            // SEARCH RESULT LIST

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
                Console.WriteLine($"{item.id}\t{item.title}");
            }
            
            Console.Write("Select Title Index: ");
            index = int.Parse(Console.ReadLine()); // NEEDS EVALUATION

            url = $"https://subscene.com{movies.First(n => n.id == index).link}"; // LINK OF MOVIE PAGE

            try
            {
                doc = web.Load(url);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return -1;
            }                       
                        
            nodes = doc.DocumentNode.SelectNodes(@"/html/body/div[1]/div[2]/div[4]/table/tbody/tr/td/a"); // SUBTITLES SECTION

            // FILL LIST OF
            // ALL SUBTITLES

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

            index = 0;
            Console.WriteLine("-------------");
            foreach (var item in FullList.Select(n => n.Language).Distinct())
            {
                LanguageList[index] = item;
                Console.WriteLine($"{index} \t {item}");
                index ++;
            }

            Console.Write("Select language index: ");
            index = int.Parse(Console.ReadLine()); // NEEDS EVALUATION
            
            // SHOW SUTITLES
            // FILTERED BASED ON LANGUAGE

            index = 0;
            Console.WriteLine("-------------");
            foreach (var item in FullList.Where(n => n.Language == LanguageList[index]))
            {
                FilteredSubtitles[index] = item;
                Console.WriteLine($"{index} \t {item.BodyText}");
                index ++;
            }
            
            Console.Write("Select Subtitle Index: ");
            index = int.Parse(Console.ReadLine()); // NEEDS EVALUATION

            url = "https://subscene.com" + FilteredSubtitles[index].Link; // SUBTITLE PAGE

            try
            {
                doc = web.Load(url);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return -1;
            }      

            // GETTING THE DOWNLOAD LINK
            // AND DOWNLOAD

            node = doc.DocumentNode.SelectSingleNode(@"/html/body/div[1]/div[2]/div[2]/div[2]/div[2]/ul/li[4]/div/a"); // DOWNLOAD LINK SECTION
            url = "https://subscene.com" + node.Attributes["href"].Value; // DOWNLOAD URL

            using (var client = new WebClient())
            {
                try
                {
                    client.DownloadFile(url, FilteredSubtitles[index].BodyText);
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

        static string CleanStr(string str)
        {
            return Regex.Replace(str, @"\t|\n|\r", "");
        }   
    }
}
