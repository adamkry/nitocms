using docxtools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public ActionResult Index(HttpPostedFileBase postedFile)
        {            
            if (postedFile != null)
            {
                var blogPostId = Guid.NewGuid();
                string path = Server.MapPath($"~/Uploads/BlogPosts/{blogPostId}");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                var resultFileName = Path.Combine(path, Path.GetFileName(postedFile.FileName));
                postedFile.SaveAs(resultFileName);

                DocxTools.ConvertToHtml(resultFileName, Path.Combine(path, "html"));

                ViewBag.Message = "File uploaded successfully.";
            }

            return View();
        }
    }
}