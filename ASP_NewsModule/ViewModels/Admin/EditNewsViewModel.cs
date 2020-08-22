using ASP_NewsModule.Models.Home;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ASP_NewsModule.ViewModels.Admin
{
    public class EditNewsViewModel
    {
        public Guid NewsId { get; set; }

        [Required(ErrorMessage = "Требуется ввести заголовок.")]
        [Display(Name = "Заголовок")]
        [StringLength(100, ErrorMessage = "Заголовок должен быть от {1} до {2} символов.", MinimumLength = 4)]
        [DataType(DataType.Text)]
        public string NewsTitle { get; set; }

        [Required(ErrorMessage = "Требуется ввести содержание")]
        [Display(Name = "Содержание")]
        [StringLength(5000, ErrorMessage = "Содержание должно быть от {1} до {2} символов.", MinimumLength = 10)]
        [DataType(DataType.Text)]
        public string NewsBody { get; set; }

        public DateTime NewsDate { get; set; }

        public string UserName { get; set; }

        [Display(Name = "Загрузить изображение")]
        public NewsImage NewsImage { get; set; }

        public List<NewsImage> NewsImages { get; set; }

        public int ImagesCount { get; set; }
    }
}
