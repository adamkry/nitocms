using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace nitocms.Models
{
    public class BlogPostViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public DateTime Created { get; set; }        
    }
}