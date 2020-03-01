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
        public static string CleanStr(string str)
        {
            str = Regex.Replace(str, @"\t|\n|\r", "");
            return str;
        }   
        static void Main(string[] args)
        {
            Console.WriteLine("Get subtitle from Subscene.com");
            Console.Write("Enter Movie Title: ");
            string movieName;
            movieName = Console.ReadLine().Trim().Replace(' ', '-');

            string html = @"https://subscene.com/subtitles/" + movieName;
            HtmlWeb web = new HtmlWeb();                        
            var htmlDoc = web.Load(html);
            
            var nodes = htmlDoc.DocumentNode.SelectNodes(@"/html/body/div[1]/div[2]/div[4]/table/tbody/tr/td/a");
            
            var FullList = new List<Subtitle>();
            string[] LanguageList = new string[100];
            Subtitle[] FilteredSubtitles = new Subtitle[500];
            
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
                Console.WriteLine($"{langIndex + 1} \t {item}");
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

            html = "https://subscene.com" + FilteredSubtitles[subIndex].Link;
            htmlDoc = web.Load(html);
            var node = htmlDoc.DocumentNode.SelectSingleNode(@"/html/body/div[1]/div[2]/div[2]/div[2]/div[2]/ul/li[4]/div/a");
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
                }
                
            }            

        }
    }
}
