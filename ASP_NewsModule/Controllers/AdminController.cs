using ASP_NewsModule.Models.Home;
using ASP_NewsModule.Models.Service;
using ASP_NewsModule.ViewModels.Admin;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        #region Создать новость [GET]
        [HttpGet]
        public IActionResult AddNews()
        {
            return View();
        }
        #endregion

        #region Создать новость [POST]
        [HttpPost]
        public async Task<IActionResult> AddNews(AddNewsViewModel model, IFormFileCollection uploads)
        {
            // Проверяем, чтобы размер файлов не превышал заданный объем
            foreach (var file in uploads)
            {
                if (file.Length > 2097152)
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
                    UserName = "Mnemonic" // В рабочем варианте будет брать имя из User.Identity
                };

                // Далее начинаем обработку изображений
                List<NewsImage> newsImages = new List<NewsImage>();
                foreach (var uploadedImage in uploads)
                {
                    // Если размер входного файла больше 0, заходим в тело условия
                    if (uploadedImage.Length > 0)
                    {
                        // Создаем новый объект класса FileInfo из полученного изображения для дальнейшей обработки
                        FileInfo imgFile = new FileInfo(uploadedImage.FileName);
                        // Приводим расширение к нижнему регистру (если оно было в верхнем)
                        string imgExtension = imgFile.Extension.ToLower();
                        // Генерируем новое имя для файла
                        string newFileName = Guid.NewGuid() + imgExtension;
                        // Пути сохранения файла
                        string pathNormal = "/files/images/normal/" + newFileName; // изображение исходного размера
                        string pathScaled = "/files/images/scaled/" + newFileName; // уменьшенное изображение

                        // В операторе try/catch делаем уменьшенную копию изображения.
                        // Если входным файлом окажется не изображение, нас перекинет в блок CATCH и выведет сообщение об ошибке
                        try
                        {
                            // Создаем объект класса SixLabors.ImageSharp.Image и грузим в него полученное изображение
                            using (Image image = Image.Load(uploadedImage.OpenReadStream()))
                            {
                                // Создаем уменьшенную копию и обрезаем её
                                var clone = image.Clone(x => x.Resize(new ResizeOptions
                                {
                                    Mode = ResizeMode.Crop,
                                    Size = new Size(300, 200)
                                }));
                                // Сохраняем уменьшенную копию
                                await clone.SaveAsync(_appEnvironment.WebRootPath + pathScaled, new JpegEncoder { Quality = 50 });
                                // Сохраняем исходное изображение
                                await image.SaveAsync(_appEnvironment.WebRootPath + pathNormal);
                            }

                        }
                        // Если вдруг что-то пошло не так (например, на вход подало не картинку), то выводим сообщение об ошибке
                        catch
                        {
                            // Создаем сообщение об ошибке для вывода пользователю
                            ModelState.AddModelError("NewsImage", $"Файл {uploadedImage.FileName} имеет неверный формат.");

                            // Удаляем только что созданные файлы (если ошибка возникла не на первом файле)
                            foreach (var image in newsImages)
                            {
                                // Исходные (полноразмерные) изображения
                                FileInfo imageNormal = new FileInfo(_appEnvironment.WebRootPath + image.ImagePathNormal);
                                if (imageNormal.Exists)
                                {
                                    imageNormal.Delete();
                                }
                                // И их уменьшенные копии
                                FileInfo imageScaled = new FileInfo(_appEnvironment.WebRootPath + image.ImagePathScaled);
                                if (imageScaled.Exists)
                                {
                                    imageScaled.Delete();
                                }
                            }

                            // Возвращаем модель с сообщением об ошибке в представление
                            return View(model);
                        }

                        // Создаем объект класса NewsImage со всеми параметрами
                        NewsImage newsImage = new NewsImage()
                        {
                            Id = Guid.NewGuid(),
                            ImageName = newFileName,
                            ImagePathNormal = pathNormal,
                            ImagePathScaled = pathScaled,
                            NewsId = news.Id
                        };
                        // Добавляем объект newsImage в список newsImages
                        newsImages.Add(newsImage);
                    }
                }

                // Если в процессе выполнения не возникло ошибок, сохраняем всё в БД
                await newsDB.NewsImages.AddRangeAsync(newsImages);
                await newsDB.News.AddAsync(news);
                await newsDB.SaveChangesAsync();

                // Редирект на главную страницу
                return RedirectToAction("Index", "Home");
            }

            // Возврат модели в представление в случае, если запорится валидация
            return View(model);
        }
        #endregion

        #region Редактировать новость [GET]
        [HttpGet]
        public async Task<IActionResult> EditNews(Guid newsId, string imageToDeleteName = null)
        {
            //ViewBag.NewsId = newsId;

            if (imageToDeleteName != null)
            {
                NewsImage newsImage = await newsDB.NewsImages.FirstAsync(i => i.ImageName == imageToDeleteName);

                // Исходные (полноразмерные) изображения
                FileInfo imageNormal = new FileInfo(_appEnvironment.WebRootPath + newsImage.ImagePathNormal);
                if (imageNormal.Exists)
                {
                    imageNormal.Delete();
                }
                // И их уменьшенные копии
                FileInfo imageScaled = new FileInfo(_appEnvironment.WebRootPath + newsImage.ImagePathScaled);
                if (imageScaled.Exists)
                {
                    imageScaled.Delete();
                }

                newsDB.NewsImages.Remove(newsImage);
                await newsDB.SaveChangesAsync();
            }

            News news = await newsDB.News.FirstAsync(n => n.Id == newsId);
            List<NewsImage> images = new List<NewsImage>();
            foreach (var image in newsDB.NewsImages)
            {
                if (image.NewsId == newsId)
                {
                    images.Add(image);
                }
            }

            EditNewsViewModel model = new EditNewsViewModel()
            {
                NewsTitle = news.NewsTitle,
                NewsBody = news.NewsBody,
                NewsImages = images,
                // Скрытые поля
                NewsId = newsId,
                NewsDate = news.NewsDate,
                UserName = news.UserName,
                ImagesCount = images.Count
            };

            return View(model);
        }
        #endregion

        #region Редактировать новость [POST]
        [HttpPost]
        public async Task<IActionResult> EditNews(EditNewsViewModel model, IFormFileCollection uploads)
        {
            // Проверяем, чтобы размер файлов не превышал заданный объем
            foreach (var file in uploads)
            {
                if (file.Length > 2097152)
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
                    NewsTitle = model.NewsTitle,
                    NewsBody = model.NewsBody,
                    // Скрытые поля
                    Id = model.NewsId,
                    NewsDate = model.NewsDate,
                    UserName = model.UserName
                };

                // Далее начинаем обработку загружаемых изображений
                List<NewsImage> newsImages = new List<NewsImage>();
                foreach (var uploadedImage in uploads)
                {
                    // Если размер входного файла больше 0, заходим в тело условия
                    if (uploadedImage.Length > 0)
                    {
                        // Создаем новый объект класса FileInfo из полученного изображения для дальнейшей обработки
                        FileInfo imgFile = new FileInfo(uploadedImage.FileName);
                        // Приводим расширение к нижнему регистру (если оно было в верхнем)
                        string imgExtension = imgFile.Extension.ToLower();
                        // Генерируем новое имя для файла
                        string newFileName = Guid.NewGuid() + imgExtension;
                        // Пути сохранения файла
                        string pathNormal = "/files/images/normal/" + newFileName; // изображение исходного размера
                        string pathScaled = "/files/images/scaled/" + newFileName; // уменьшенное изображение

                        // В операторе try/catch делаем уменьшенную копию изображения.
                        // Если входным файлом окажется не изображение, нас перекинет в блок CATCH и выведет сообщение об ошибке
                        try
                        {
                            // Создаем объект класса SixLabors.ImageSharp.Image и грузим в него полученное изображение
                            using (Image image = Image.Load(uploadedImage.OpenReadStream()))
                            {
                                // Создаем уменьшенную копию и обрезаем её
                                var clone = image.Clone(x => x.Resize(new ResizeOptions
                                {
                                    Mode = ResizeMode.Crop,
                                    Size = new Size(300, 200)
                                }));
                                // Сохраняем уменьшенную копию
                                await clone.SaveAsync(_appEnvironment.WebRootPath + pathScaled, new JpegEncoder { Quality = 50 });
                                // Сохраняем исходное изображение
                                await image.SaveAsync(_appEnvironment.WebRootPath + pathNormal);
                            }

                        }
                        // Если вдруг что-то пошло не так (например, на вход подало не картинку), то выводим сообщение об ошибке
                        catch
                        {
                            // Создаем сообщение об ошибке для вывода пользователю
                            ModelState.AddModelError("NewsImage", $"Файл {uploadedImage.FileName} имеет неверный формат.");

                            // Удаляем только что созданные файлы (если ошибка возникла не на первом файле)
                            foreach (var image in newsImages)
                            {
                                // Исходные (полноразмерные) изображения
                                FileInfo imageNormal = new FileInfo(_appEnvironment.WebRootPath + image.ImagePathNormal);
                                if (imageNormal.Exists)
                                {
                                    imageNormal.Delete();
                                }
                                // И их уменьшенные копии
                                FileInfo imageScaled = new FileInfo(_appEnvironment.WebRootPath + image.ImagePathScaled);
                                if (imageScaled.Exists)
                                {
                                    imageScaled.Delete();
                                }
                            }

                            // Возвращаем модель с сообщением об ошибке в представление
                            return View(model);
                        }

                        // Создаем объект класса NewsImage со всеми параметрами
                        NewsImage newsImage = new NewsImage()
                        {
                            Id = Guid.NewGuid(),
                            ImageName = newFileName,
                            ImagePathNormal = pathNormal,
                            ImagePathScaled = pathScaled,
                            NewsId = news.Id
                        };
                        // Добавляем объект newsImage в список newsImages
                        newsImages.Add(newsImage);
                    }
                }

                // Если в процессе выполнения не возникло ошибок, сохраняем всё в БД
                await newsDB.NewsImages.AddRangeAsync(newsImages);
                newsDB.News.Update(news);
                await newsDB.SaveChangesAsync();

                // Редирект на главную страницу
                return RedirectToAction("Index", "Home");
            }

            List<NewsImage> images = new List<NewsImage>();
            foreach (var image in newsDB.NewsImages)
            {
                if (image.NewsId == model.NewsId)
                {
                    images.Add(image);
                }
            }
            model.NewsImages = images;
            model.ImagesCount = images.Count;

            // Возврат модели в представление в случае, если запорится валидация
            return View(model);
        }
        #endregion

        #region Удалить новость [POST]
        public async Task<IActionResult> DeleteNews(Guid newsId, bool isChecked, int imagesCount = 0)
        {
            // Если удаление подтверждено, заходим в условия
            if (isChecked)
            {
                // Создаем экземпляр новости для удаления. Достаточно просто присвоить Id удаляемой записи
                News news = new News { Id = newsId };

                // Проверяем, присутствуют ли в новости изображения
                if (imagesCount > 0)
                {
                    // Создаем список для привязанных к удаляемой записи изображений
                    List<NewsImage> newsImages = new List<NewsImage>();
                    // Просматриваем всю БД с изображениями
                    foreach (var image in newsDB.NewsImages)
                    {
                        // Находим нужное и кладём в список
                        if (image.NewsId == newsId)
                        {
                            newsImages.Add(image);
                        }
                        // Если счетчик изображений становится равным количеству изображений в списке, выходим из цикла
                        // Нужно для того, чтобы не делать лишние прогоны по БД
                        if (imagesCount == newsImages.Count)
                        {
                            break;
                        }
                    }

                    // Удаление изображений из папок
                    foreach (var image in newsImages)
                    {
                        // Исходные (полноразмерные) изображения
                        FileInfo imageNormal = new FileInfo(_appEnvironment.WebRootPath + image.ImagePathNormal);
                        if (imageNormal.Exists)
                        {
                            imageNormal.Delete();
                        }
                        // И их уменьшенные копии
                        FileInfo imageScaled = new FileInfo(_appEnvironment.WebRootPath + image.ImagePathScaled);
                        if (imageScaled.Exists)
                        {
                            imageScaled.Delete();
                        }
                    }

                    // Удаляем изображения из БД
                    // Эта строка не обязательна, т.к. при удалении новости, записи с изображениями теряют связь по Id и трутся сами
                    // Достаточно просто удалить изображения из папок, что уже сделано выше
                    newsDB.NewsImages.RemoveRange(newsImages);
                }

                // Удаляем новость из БД
                newsDB.News.Remove(news);
                await newsDB.SaveChangesAsync();
            }

            return RedirectToAction("Index", "Home");
        }
        #endregion
    }
}