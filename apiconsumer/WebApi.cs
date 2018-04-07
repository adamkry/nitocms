using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace apiconsumer
{
    public class WebApi
    {
        public static void PostFile(HttpClient client, Guid blogPostId, byte[] fileData, string fileName)
        {
            using (var content = new MultipartFormDataContent())
            {
                var fileContent = new ByteArrayContent(fileData);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpg");
                //fileContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
                //{
                //    FileName = fileName
                //};
                content.Add(fileContent, "photo", fileName);
                var requestUri = $"http://localhost:5000/artykuly/addphoto/{blogPostId}";
                var result = client.PostAsync(requestUri, content).Result;
                if (result.StatusCode == System.Net.HttpStatusCode.Created)
                {
                    string m = result.Content.ReadAsStringAsync().Result;
                }
            }            
        }
    }
}
