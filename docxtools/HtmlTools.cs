using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace docxtools
{
    public class HtmlTools
    {
        public static string GetText(string htmlText)
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlText);
            return string.Join(" ", htmlDoc.DocumentNode.Descendants()
              .Where(n => !n.HasChildNodes && !string.IsNullOrWhiteSpace(n.InnerText))
              .Select(n => n.InnerText));
        }

        public static string GetBodyInnerHtml(string htmlText)
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlText);
            return htmlDoc.DocumentNode.SelectSingleNode("//body").InnerHtml;
        }
    }
}
