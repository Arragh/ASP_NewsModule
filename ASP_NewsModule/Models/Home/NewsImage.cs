using System;

namespace ASP_NewsModule.Models.Home
{
    public class NewsImage
    {
        public Guid Id { get; set; }
        public string ImageName { get; set; }
        public string ImagePathNormal { get; set; }
        public string ImagePathScaled { get; set; }
        public Guid NewsId { get; set; }
    }
}
