using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace docxtools.HtmlHelpers
{
    public class HtmlPageParser
    {
        public static HtmlPage Parse(string html)
        {
            string htmlContent = HtmlTools.GetBodyInnerHtml(html);
            string htmlStyles = HtmlTools.GetStyles(html);
            string innerText = HtmlTools.GetText(htmlContent);
            return new HtmlPage
            {
                HtmlContent = htmlContent,
                CssStyles = htmlStyles,
                TextContent = innerText
            };
        }
    }
}
