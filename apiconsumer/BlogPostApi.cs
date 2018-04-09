using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace apiconsumer
{
    public class BlogPostApi
    {
        private HttpClient _client;
        private string _rootUrl;
        public BlogPostApi(HttpClient client, string rootUrl)
        {
            _client = client;
            _rootUrl = rootUrl + "/artykuly";
        }
        public async Task<bool> SendBlogPostAsync(CreateBlogPostViewModel blogPost)
        {
            HttpResponseMessage response = await _client.PostAsJsonAsync(_rootUrl + "/nowy", blogPost);
            response.EnsureSuccessStatusCode();
            return true;
        }

        public bool SendBlogPostImage(HttpClient client, Guid blogPostId, byte[] fileData, string fileName)
        {
            WebApi.PostFile(client, $"{_rootUrl}/addphoto/{blogPostId}", fileData, fileName);
            return true;
        }

        public async Task<bool> SendDeleteBlogPostAsync(Guid blogPostId)
        {
            var response = await _client.DeleteAsync($"{_rootUrl}/{blogPostId}");
            response.EnsureSuccessStatusCode();
            return true;
        }

        public async Task<bool> SendRenewBlogPostAsync(Guid blogPostId)
        {
            var response = await _client.GetAsync($"{_rootUrl}/renew/{blogPostId}");
            response.EnsureSuccessStatusCode();
            return true;
        }
    }

    public class CreateBlogPostViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string TextContent { get; set; }
        public string Styles { get; set; }
    }
}
