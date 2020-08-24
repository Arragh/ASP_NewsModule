using ASP_NewsModule.Models.News;
using System.Collections.Generic;

namespace ASP_NewsModule.ViewModels.News
{
    public class IndexViewModel
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public List<NewsObject> News { get; set; }
        public List<NewsImage> NewsImages { get; set; }
    }
}
