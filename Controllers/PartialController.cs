using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Extensions;

namespace Teknikern.Controllers
{
    public class PartialController : Controller
    {
        private readonly IPublishedContentQuery _publishedContentQuery;

        public PartialController(IPublishedContentQuery publishedContentQuery)
        {
            _publishedContentQuery = publishedContentQuery;
        }

        [Route("/news-list")]
        public IActionResult NewsList()
        {
            var newsPage = _publishedContentQuery.ContentAtRoot()
                .SelectMany(x => x.DescendantsOrSelf())
                .FirstOrDefault(x => x.Children().Any(c => c.ContentType.Alias == "newsArticle"));

            return newsPage == null
                ? NotFound()
                : PartialView("~/Views/Partials/newsListPartial.cshtml", newsPage);
        }
    }
}
