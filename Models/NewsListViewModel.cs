using Umbraco.Cms.Core.Models.PublishedContent;

namespace Teknikern.Models
{
    public class NewsListViewModel
    {
        public IEnumerable<IPublishedContent> Articles { get; set; } = Enumerable.Empty<IPublishedContent>();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string ListingUrl { get; set; } = "/";
    }
}
