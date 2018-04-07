using DocumentFormat.OpenXml.Packaging;
using OpenXmlPowerTools;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace docxtools
{
    public static class DocxTools
    {
        public static bool ConvertToHtml(string docxPath, string outputDirectory, 
            string imageHtmlDirectory,
            Func<int, string> imagePathFormat = null)
        {
            if (!File.Exists(docxPath))
            {
                return false;
            }
            byte[] byteArray = File.ReadAllBytes(docxPath);

            using (MemoryStream memoryStream = new MemoryStream())
            {
                memoryStream.Write(byteArray, 0, byteArray.Length);

                using (WordprocessingDocument wDoc = WordprocessingDocument.Open(memoryStream, true))
                {
                    var htmlFileName = Path.ChangeExtension(Path.GetFileName(docxPath), ".html");
                    if (outputDirectory != null && outputDirectory != string.Empty)
                    {
                        if (!Directory.Exists(outputDirectory))
                        {
                            Directory.CreateDirectory(outputDirectory);
                        }
                        htmlFileName = Path.Combine(outputDirectory, htmlFileName);
                    }
                    var imageDirectoryName = Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(htmlFileName) + "_files");
                    var htmlElement = GetImages(docxPath, imageDirectoryName, wDoc, imageHtmlDirectory, imagePathFormat);
                    
                    // Produce HTML document with <!DOCTYPE html > declaration to tell the browser
                    // we are using HTML5.
                    var html = new XDocument(
                        new XDocumentType("html", null, null, null),
                        htmlElement);

                    // Note: the xhtml returned by ConvertToHtmlTransform contains objects of type
                    // XEntity.  PtOpenXmlUtil.cs define the XEntity class.  See
                    // http://blogs.msdn.com/ericwhite/archive/2010/01/21/writing-entity-references-using-linq-to-xml.aspx
                    // for detailed explanation.
                    //
                    // If you further transform the XML tree returned by ConvertToHtmlTransform, you
                    // must do it correctly, or entities will not be serialized properly.

                    var htmlString = html.ToString(SaveOptions.DisableFormatting);
                    File.WriteAllText(htmlFileName, htmlString, Encoding.UTF8);
                }
            }
            return true;
        }

        private static XElement GetImages(string docxPath, string imageDirectoryName, 
            WordprocessingDocument wDoc, string imageHtmlDirectory, Func<int, string> imagePathFormat)
        {
            int imageCounter = 0;
            var pageTitle = docxPath;
            var part = wDoc.CoreFilePropertiesPart;
            if (part != null)
            {
                pageTitle = (string)part.GetXDocument().Descendants(DC.title).FirstOrDefault() ?? docxPath;
            }
            // TODO: Determine max-width from size of content area.
            HtmlConverterSettings settings = new HtmlConverterSettings()
            {
                AdditionalCss = "body { margin: 1cm auto; max-width: 20cm; padding: 0; }",
                PageTitle = pageTitle,
                FabricateCssClasses = true,
                CssClassPrefix = "pt-",
                RestrictToSupportedLanguages = false,
                RestrictToSupportedNumberingFormats = false,
                ImageHandler = imageInfo =>
                {
                    if (!Directory.Exists(imageDirectoryName))
                    {
                        Directory.CreateDirectory(imageDirectoryName);
                    }
                    ++imageCounter;
                    string extension = imageInfo.ContentType.Split('/')[1].ToLower();
                    ImageFormat imageFormat = null;
                    if (extension == "png")
                    {
                        imageFormat = ImageFormat.Png;
                    }
                    else if (extension == "gif")
                    {
                        imageFormat = ImageFormat.Gif;
                    }
                    else if (extension == "bmp")
                    {
                        imageFormat = ImageFormat.Bmp;
                    }
                    else if (extension == "jpeg")
                    {
                        imageFormat = ImageFormat.Jpeg;
                    }
                    else if (extension == "tiff")
                    {
                        extension = "gif";
                        imageFormat = ImageFormat.Gif;
                    }
                    else if (extension == "x-wmf")
                    {
                        extension = "wmf";
                        imageFormat = ImageFormat.Wmf;
                    }

                    // If the image format isn't one that we expect, ignore it,
                    // and don't return markup for the link.
                    if (imageFormat == null)
                    {
                        return null;
                    }
                    string imageFileName = $"{imagePathFormat(imageCounter)}.{extension}";
                    string imageFilePath = Path.Combine(imageDirectoryName, imageFileName);

                    try
                    {
                        imageInfo.Bitmap.Save(imageFilePath, imageFormat);
                    }
                    catch (System.Runtime.InteropServices.ExternalException)
                    {
                        return null;
                    }

                    string imageSource = Path.Combine(imageHtmlDirectory, imageFileName);
                    
                    XElement img = new XElement(
                        Xhtml.img,
                        new XAttribute(NoNamespace.src, imageSource),
                        imageInfo.ImgStyleAttribute,
                        imageInfo.AltText != null 
                            ? new XAttribute(NoNamespace.alt, imageInfo.AltText) 
                            : null
                    );
                    return img;
                }
            };
            XElement htmlElement = HtmlConverter.ConvertToHtml(wDoc, settings);
            return htmlElement;
        }
    }
}
