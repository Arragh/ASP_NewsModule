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

        #region Главная страница
        public async Task<IActionResult> Index(int pageNumber = 1) // Страница по умолчанию = 1
        {
            // Количество записей на страницу
            int pageSize = 10;

            // Формируем список записей для обработки перед выводом на страницу
            IQueryable<News> source = newsDB.News;

            // Рассчитываем, какие именно записи будут выведены на странице
            List<News> news = await source.OrderByDescending(n => n.NewsDate).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            // Общее количество записей для дальнейшего рассчета количества страниц
            int newsCount = await source.CountAsync();

            // Создаем массив Id-шников записей для выборки изображений к ним
            Guid[] newsIdArray = news.Select(n => n.Id).ToArray();

            // Создаем список из изображений, которые будут поданы вместе со списом записей
            List<NewsImage> newsImages = new List<NewsImage>();

            // Перебираем изображения в БД
            foreach (var image in newsDB.NewsImages)
            {
                // Перебираем все элементы ранее созданного массива Guid[] newsIdArray
                foreach (var newsId in newsIdArray)
                {
                    // Если данные совпадают, то кладём изображение в список для вывода на странице
                    if (image.NewsId == newsId)
                    {
                        newsImages.Add(image);
                    }
                }
            }

            // Создаём модель для вывода на странице и кладём в неё все необходимые данные
            IndexViewModel model = new IndexViewModel()
            {
                News = news,
                NewsImages = newsImages,
                CurrentPage = pageNumber,
                TotalPages = (int)Math.Ceiling(newsCount / (double)pageSize)
            };

            // Выводим модель в представление
            return View(model);
        }
        #endregion
    }
}
