using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASP_NewsModule.Models.News
{
    public class News
    {
        public Guid Id { get; set; }
        public string NewsTitle { get; set; }
        public string NewsBody { get; set; }
        public DateTime NewsDate { get; set; }
        public string UserName { get; set; }
        public virtual ICollection<NewsImage> NewsImages { get; set; }
    }
}
