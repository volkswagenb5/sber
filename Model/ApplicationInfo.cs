using System;

namespace sberbank.Model
{
    public class ApplicationInfo
    {
        public int ApplicationId { get; set; }
        public string ProductName { get; set; }
        public string ProductType { get; set; }
        public string StatusName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Comment { get; set; }
    }
}
