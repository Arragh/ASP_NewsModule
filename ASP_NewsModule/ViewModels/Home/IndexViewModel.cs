using ASP_NewsModule.Models.Home;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
