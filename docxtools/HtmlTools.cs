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
            string bodySection = htmlDoc.DocumentNode.SelectSingleNode("//body").InnerHtml;
            return bodySection;
        }

        public static string GetStyles(string htmlText)
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlText);
            string stylesSection = htmlDoc.DocumentNode.SelectSingleNode("//style").InnerText;
            int bodyStylesPosition = stylesSection.IndexOf("body");
            if (bodyStylesPosition >= 0)
            {
                stylesSection = stylesSection.Substring(0, bodyStylesPosition + 1);
            }
            return stylesSection;
        }
    }
}
