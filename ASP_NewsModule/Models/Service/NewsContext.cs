using ASP_NewsModule.Models.News;
using Microsoft.EntityFrameworkCore;

namespace ASP_NewsModule.Models.Service
{
    public class NewsContext : DbContext
    {
        public DbSet<News.News> News { get; set; }
        public DbSet<NewsImage> NewsImages { get; set; }

        public NewsContext(DbContextOptions<NewsContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
    }
}
