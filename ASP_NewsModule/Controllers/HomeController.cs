using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ASP_NewsModule.Models.Home;
using ASP_NewsModule.Models.Service;
using ASP_NewsModule.ViewModels.Home;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ASP_NewsModule.Controllers
{
    public class HomeController : Controller
    {
        NewsContext newsDB;

        public HomeController(NewsContext newsContext)
        {
            newsDB = newsContext;
        }

        public async Task<IActionResult> Index(int pageNumber = 1)
        {
            // Количество записей на страницу
            int pageSize = 10;

            IQueryable<News> source = newsDB.News;
            List<News> news = await source.OrderByDescending(n => n.NewsDate).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            int newsCount = await source.CountAsync();

            Guid[] arr = news.Select(n => n.Id).ToArray();
            List<NewsImage> newsImages = new List<NewsImage>();
            foreach (var image in newsDB.NewsImages)
            {
                foreach (var item in arr)
                {
                    if (image.NewsId == item)
                    {
                        newsImages.Add(image);
                    }
                }
            }

            IndexViewModel model = new IndexViewModel()
            {
                News = news,
                NewsImages = newsImages,
                CurrentPage = pageNumber,
                TotalPages = (int)Math.Ceiling(newsCount / (double)pageSize)
            };

            return View(model);
        }
    }
}
