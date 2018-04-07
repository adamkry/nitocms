using apiconsumer;
using docxtools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace nitocms.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Index(HttpPostedFileBase postedFile)
        {            
            if (postedFile != null)
            {
                var blogPostId = Guid.NewGuid();
                string path = Server.MapPath($"~/Uploads/BlogPosts/{blogPostId}");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                var uploadedFileName = Path.GetFileName(postedFile.FileName);
                var uploadedFilePath = Path.Combine(path, Path.GetFileName(postedFile.FileName));
                postedFile.SaveAs(uploadedFilePath);

                var htmlDirectoryPath = Path.Combine(path, "html");
                DocxTools.ConvertToHtml(uploadedFilePath,
                    htmlDirectoryPath,
                    $"/images/blogposts/{blogPostId}/",
                    imgNo => $"image" + imgNo.ToString());

                string htmlFileName = Path.ChangeExtension(uploadedFileName, ".html");
                string htmlFilePath = Path.Combine(htmlDirectoryPath, htmlFileName);

                

                using (var client = new HttpClient())
                {
                    var api = new BlogPostApi(client, "http://nitwinko-dev.azurewebsites.net");
                    string fileName = Path.GetFileNameWithoutExtension(htmlFileName);
                    string htmlContent = GetContentHtml(htmlFilePath);
                    string innerText = HtmlTools.GetText(htmlContent);
                    await api.SendBlogPostAsync(new CreateBlogPostViewModel
                    {
                        Id = blogPostId,
                        Title = fileName,
                        Content = htmlContent,
                        TextContent = innerText
                    });
                    var images = Directory.GetFiles(Path.Combine(htmlDirectoryPath, fileName + "_files"));
                    foreach (var imagePath in images)
                    {
                        var imageContent = System.IO.File.ReadAllBytes(imagePath);
                        WebApi.PostFile(client, blogPostId, imageContent, Path.GetFileName(imagePath));
                    }
                }

                ViewBag.Message = $"File uploaded successfully ({blogPostId})";
            }

            return View();
        }

        private string GetContentHtml(string htmlFilePath)
        {
            var wholeHtml = System.IO.File.ReadAllText(htmlFilePath);
            return HtmlTools.GetBodyInnerHtml(wholeHtml);
        }
    }
}