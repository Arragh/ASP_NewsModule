using ASP_NewsModule.Models.Home;
using System.Collections.Generic;

namespace ASP_NewsModule.ViewModels.Home
{
    public class IndexViewModel
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public List<News> News { get; set; }
        public List<NewsImage> NewsImages { get; set; }
    }
}
