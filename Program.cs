using System;
using System.IO;
using System.Linq;
using System.Net;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace a3_down
{

    class Program
    {
        private const string usageText = "Usage: a3-down url";
        private const string urlNotFound = "URL not found";

        private const string urlNotSupported = "URL not supported by now :(";


        private const string OBJNAME = "__PRELOADED_STATE__";

        static void Main(string[] args)
        {

            var seoTitle = string.Empty;
            var episodeNumber = string.Empty;
            var apiJson = string.Empty;

            if (args == null || args.Length == 0)
            {

                Console.WriteLine(usageText);
                return;
            }

            string url = args[0];

            var doc = new HtmlAgilityPack.HtmlWeb();

            var htmlDoc = doc.Load(url);

            if (htmlDoc == null)
            {
                Console.WriteLine(urlNotFound);
                return;
            }

            Console.WriteLine("HTML document adquired, parsing...");

            var script = htmlDoc.DocumentNode.Descendants().Where(d => d.Name == "script" && d.InnerText.Contains(OBJNAME)).FirstOrDefault();

            if (script == null)
            {
                Console.WriteLine(urlNotSupported);
                return;

            }

            var obj = cleanScript(script);

            dynamic data = JsonConvert.DeserializeObject(obj);

            if (data != null)
            {
                Console.WriteLine("Document parsed!");

                seoTitle = data.SelectToken("$..episodes..seoTitle");
                episodeNumber = data.SelectToken("$..episodes..numberOfEpisode");
                apiJson = data.SelectToken("$..episodes..urlVideo");

                Console.WriteLine($@"Found: {episodeNumber} - {seoTitle}");
                Console.WriteLine($@"Parsing: {apiJson}");

                htmlDoc = doc.Load(apiJson);

                if (htmlDoc == null)
                {
                    Console.WriteLine(urlNotFound);
                    return;
                }


                var jsonData = string.Empty;

                using (var webClient = new WebClient())
                {

                    jsonData = webClient.DownloadString(apiJson);
                }

                data = JsonConvert.DeserializeObject(jsonData);
                string m3u8pl = data["sources"][0]["src"];

                Console.WriteLine($@"Master Playlist Found: {m3u8pl}");


            }

        }

        private static string cleanScript(HtmlNode script)
        {

            var cleanScript = script.InnerText.Replace("window.", "");
            var objString = string.Empty;

            foreach (string line in cleanScript.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {

                if (line.Trim().StartsWith(OBJNAME))
                {
                    objString = line.Trim().Remove(0, OBJNAME.Length).Replace("=", "").Trim();
                    break;
                }
            }

            return objString.Replace(";", "");

        }

    }


}

