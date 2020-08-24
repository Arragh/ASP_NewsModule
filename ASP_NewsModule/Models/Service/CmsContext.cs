using ASP_NewsModule.Models.News;
using Microsoft.EntityFrameworkCore;

namespace ASP_NewsModule.Models.Service
{
    public class CmsContext : DbContext
    {
        public DbSet<News.News> News { get; set; }
        public DbSet<NewsImage> NewsImages { get; set; }

        public CmsContext(DbContextOptions<CmsContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
    }
}
