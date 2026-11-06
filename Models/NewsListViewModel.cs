using Umbraco.Cms.Core.Models.PublishedContent;

namespace Teknikern.Models
{
    public class NewsListViewModel
    {
        public IEnumerable<IPublishedContent> Articles { get; set; } = Enumerable.Empty<IPublishedContent>();
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 9;
        public string ListingUrl { get; set; } = "/";

        public int TotalPages => Math.Max(1, (int)Math.Ceiling(Articles.Count() / (double)PageSize));
    }
}


