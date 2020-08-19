using ASP_NewsModule.Models.Home;
using ASP_NewsModule.Models.Service;
using ASP_NewsModule.ViewModels.Admin;
using LazZiya.ImageResize;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

namespace ASP_NewsModule.Controllers
{
    public class AdminController : Controller
    {
        NewsContext newsDB;
        IWebHostEnvironment _appEnvironment;

        public AdminController(NewsContext newsContext, IWebHostEnvironment appEnvironment)
        {
            newsDB = newsContext;
            _appEnvironment = appEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AddNews()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddNews(AddNewsViewModel model, IFormFileCollection uploads)
        {
            // Проверяем, чтобы размер файлов не превышал заданный объем
            foreach (var image in uploads)
            {
                if (image.Length > 2097152)
                {
                    ModelState.AddModelError("NewsImage", "Размер изображения не должен превышать 2-х мегабайт.");
                    break;
                }
            }

            // Если все в порядке, заходим в тело условия
            if (ModelState.IsValid)
            {
                // Создаем экземпляр класса News и присваиваем ему значения
                News news = new News()
                {
                    Id = Guid.NewGuid(),
                    NewsTitle = model.NewsTitle,
                    NewsBody = model.NewsBody,
                    NewsDate = DateTime.Now,
                    UserName = "Mnemonic"
                };

                // Далее начинаем обработку изображений
                List<NewsImage> newsImages = new List<NewsImage>();
                foreach (var uploadedImage in uploads)
                {
                    // Если размер входного файла больше 0, заходим в тело условия
                    if (uploadedImage.Length > 0)
                    {
                        // Присваиваем загружаемому файлу уникальное имя на основе Guid
                        string imageName = Guid.NewGuid() + "_" + uploadedImage.FileName;
                        // Пути сохранения файла
                        string pathNormal = "/files/images/normal/" + imageName; // изображение исходного размера
                        string pathScaled = "/files/images/scaled/" + imageName; // уменьшенное изображение
                        // В try делаем уменьшенную копию изображения
                        try
                        {
                            using (var stream = uploadedImage.OpenReadStream())
                            {
                                using (var img = Image.FromStream(stream))
                                {
                                    // Создаем уменьшенную копию изображения и сохраняем в папку /files/images/scaled/ в каталоге wwwroot
                                    // Данная копия уменьшится и обрежется до 200х150(4:3), даже если имеет изначально другие пропорции(например 16:9)
                                    img.ScaleAndCrop(200, 150).SaveAs(_appEnvironment.WebRootPath + pathScaled, 50); // 50 - качество картинки. Чем меньше качество - тем меньше объем изображения
                                }
                            }
                        }
                        // Если вдруг что-то пошло не так (напрмер, на вход подало не картинку), то выводим сообщение об ошибке
                        catch
                        {
                            ModelState.AddModelError("NewsImage", "Картинка не картинка вовсе...");
                            return View(model);
                        }

                        // Сохраняем исходный файл в папку /files/images/normal/ в каталоге wwwroot
                        using (FileStream file = new FileStream(_appEnvironment.WebRootPath + pathNormal, FileMode.Create))
                        {
                            await uploadedImage.CopyToAsync(file);
                        }
                        // Создаем объект класса NewsImage со всеми параметрами
                        NewsImage newsImage = new NewsImage()
                        {
                            Id = Guid.NewGuid(),
                            ImageName = imageName,
                            ImagePathNormal = pathNormal,
                            ImagePathScaled = pathScaled,
                            NewsId = news.Id
                        };
                        newsImages.Add(newsImage);
                    }
                }

                // Сохраняем всё в БД
                await newsDB.NewsImages.AddRangeAsync(newsImages);
                await newsDB.News.AddAsync(news);
                await newsDB.SaveChangesAsync();
                return RedirectToAction("Index", "Home");
            }
            return View(model);
        }
    }
}
