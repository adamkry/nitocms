using apiconsumer;
using docxtools;
using docxtools.HtmlHelpers;
using nitocms.Models;
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
        string targetUrl = "http://nitwinko-dev.azurewebsites.net";

        protected string RootPath { get; set; } = "~/Uploads/BlogPosts";

        public HomeController()
        {
#if DEBUG
            targetUrl = "http://localhost:50943";
#endif
        }

        public ActionResult Index()
        {
            HomeViewModel model = GetHomeViewModel();
            return View(model);
        }

        public ActionResult Upload()
        {
            return View("Upload");
        }

        private HomeViewModel GetHomeViewModel()
        {
            Guid guid;
            var posts = Directory
                .GetDirectories(Server.MapPath(RootPath))
                .Where(d => Guid.TryParse(Path.GetFileName(d), out guid))
                .Select(d => new BlogPostViewModel
                {
                    Id = Guid.Parse(Path.GetFileName(d)),
                    Created = Directory.GetCreationTime(d),
                    Title = GetFileName(d),
                    IsDeleted = CheckIfDeleted(d)
                })
                .OrderByDescending(a => a.Created)
                .ToList();
            return new HomeViewModel
            {
                Posts = posts
            };
        }

        private string GetFileName(string d)
        {
            return Path.GetFileNameWithoutExtension(Directory
                .GetFiles(d)
                .Single(f => Path.GetExtension(f).ToLower() == ".docx"));
        }

        [HttpPost]
        public async Task<ActionResult> Index(HttpPostedFileBase postedFile)
        {
            try
            {
                if (postedFile != null)
                {
                    var blogPostId = Guid.NewGuid();
                    string path = GetBlogPostDirectory(blogPostId);

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

                    string fileName = "";
                    using (var client = new HttpClient())
                    {
                        var api = new BlogPostApi(client, targetUrl);
                        fileName = Path.GetFileNameWithoutExtension(htmlFileName);
                        var wholeHtml = System.IO.File.ReadAllText(htmlFilePath);
                        var page = HtmlPageParser.Parse(wholeHtml);

                        await api.SendBlogPostAsync(new CreateBlogPostViewModel
                        {
                            Id = blogPostId,
                            Title = fileName,
                            Content = page.HtmlContent,
                            TextContent = page.TextContent,
                            Styles = page.CssStyles
                        });
                        var images = Directory.GetFiles(Path.Combine(htmlDirectoryPath, fileName + "_files"));
                        foreach (var imagePath in images)
                        {
                            var imageContent = System.IO.File.ReadAllBytes(imagePath);
                            api.SendBlogPostImage(client, blogPostId, imageContent, Path.GetFileName(imagePath));
                        }
                    }

                    ViewBag.Message = $"Dodano artykuł: ({fileName})";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
            }

            return View(GetHomeViewModel());
        }

        private string GetBlogPostDirectory(Guid blogPostId)
        {
            string path = Server.MapPath($"{RootPath}/{blogPostId}");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        [HttpGet]
        [Route("delete/{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            using (var client = new HttpClient())
            {
                var api = new BlogPostApi(client, targetUrl);
                await api.SendDeleteBlogPostAsync(id);

                var directory = GetBlogPostDirectory(id);
                if (!CheckIfDeleted(directory))
                {
                    Delete(directory);
                }
            }
                
            return View("Index", GetHomeViewModel());
        }

        private void Delete(string path)
        {
            Directory.CreateDirectory(Path.Combine(path, "deleted"));
        }

        private void Restore(string path)
        {
            Directory.Delete(Path.Combine(path, "deleted"));
        }

        private bool CheckIfDeleted(string path)
        {
            var deletedDirectory = Path.Combine(path, "deleted");
            return Directory.Exists(deletedDirectory);
        }

        [HttpGet]
        [Route("renew/{id}")]
        public async Task<ActionResult> Renew(Guid id)
        {
            using (var client = new HttpClient())
            {
                var api = new BlogPostApi(client, targetUrl);
                await api.SendRenewBlogPostAsync(id);

                var directory = GetBlogPostDirectory(id);
                if (CheckIfDeleted(directory))
                {
                    Restore(directory);
                }
            }
            return View("Index", GetHomeViewModel());
        }
    }
}