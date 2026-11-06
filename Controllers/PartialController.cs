using Microsoft.AspNetCore.Mvc;
using Teknikern.Models;
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
        public IActionResult NewsList(int page = 1)
        {
            var newsPage = _publishedContentQuery.ContentAtRoot()
                .SelectMany(x => x.DescendantsOrSelf())
                .FirstOrDefault(x => x.Children().Any(c => c.ContentType.Alias == "newsArticle"));

            if (newsPage == null)
            {
                return NotFound();
            }

            var articles = newsPage.Children()
                .Where(c => c.ContentType.Alias == "newsArticle")
                .OrderByDescending(c => c.CreateDate)
                .ToList();

            var model = new NewsListViewModel
            {
                Articles = articles,
                CurrentPage = page > 0 ? page : 1,
                ListingUrl = newsPage.Url()
            };

            return PartialView("~/Views/Partials/newsListPartial.cshtml", model);
        }
    }
}
