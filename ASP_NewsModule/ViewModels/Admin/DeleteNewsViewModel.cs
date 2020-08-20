using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASP_NewsModule.ViewModels.Admin
{
    public class DeleteNewsViewModel
    {
        public Guid NewsId { get; set; }
        public bool IsChecked { get; set; }
    }
}
