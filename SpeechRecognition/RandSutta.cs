using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using HtmlAgilityPack;
using System.Net.Http;
using System.Text;
using System.Net;
using System.Speech.Synthesis;

namespace SpeechRecognition
{
    class RandSutta
    {
        private string ret_str = "";

        public string read_sutta()
        {
            string website = "http://www.accesstoinsight.org/index-sutta.html";
            Parsing(website);
            while (ret_str == "")
                System.Threading.Thread.Sleep(100);
            return ret_str;
        }
        private async void Parsing(string website)
        {
            List<HtmlNode> toftitle = null;
            Random rnd = new Random();
            
            try
            {
                while (toftitle == null || toftitle.Count == 0)
                {
                    HttpClient http = new HttpClient();
                    var suttalist = await http.GetByteArrayAsync(website);
                    String suttasource = Encoding.GetEncoding("utf-8").GetString(suttalist, 0, suttalist.Length - 1);
                    suttasource = WebUtility.HtmlDecode(suttasource);
                    HtmlAgilityPack.HtmlDocument suttanodes = new HtmlAgilityPack.HtmlDocument();
                    suttanodes.LoadHtml(suttasource);
                    List<HtmlNode> randsutta = suttanodes.DocumentNode.SelectNodes
                        ("//li/a[@href]").ToList();
                    string suttaurl = randsutta[rnd.Next(1, 1302)].GetAttributeValue("href", "");
                    suttaurl = "http://www.accesstoinsight.org/" + suttaurl;
                    HttpClient http2 = new HttpClient();
                    var response = await http2.GetByteArrayAsync(suttaurl);
                    if (response == null)
                        continue;
                    String source = Encoding.GetEncoding("utf-8").GetString(response, 0, response.Length - 1);
                    source = WebUtility.HtmlDecode(source);
                    HtmlAgilityPack.HtmlDocument resultant = new HtmlAgilityPack.HtmlDocument();
                    resultant.LoadHtml(source);
                    toftitle = resultant.DocumentNode.SelectNodes
                        ("//div[contains(@class,'chapter')]/p | //div[contains(@class,'freeverse')]").ToList();
                }
                foreach (HtmlNode elm in toftitle)
                {
                   ret_str += elm.InnerText;
                }
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
                return;
            }
        }
    }
}
