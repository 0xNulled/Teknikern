using Microsoft.AspNetCore.Mvc;
using System.ServiceModel.Syndication;
using System.Xml;
using Umbraco.Cms.Core;
using Umbraco.Extensions;

namespace Teknikern.Controllers
{
    public class RssController : Controller
    {
        private readonly IPublishedContentQuery _publishedContentQuery;

        public RssController(IPublishedContentQuery publishedContentQuery)
        {
            _publishedContentQuery = publishedContentQuery;
        }

        [Route("/rss")]
        public IActionResult Index()
        {
            var articles = _publishedContentQuery.ContentAtRoot()
                .SelectMany(x => x.DescendantsOrSelf())
                .Where(x => x.ContentType.Alias == "newsArticle")
                .OrderByDescending(x => x.CreateDate)
                .Take(20)
                .ToList();

            var request = HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";

            var items = articles.Select(a => new SyndicationItem(
                a.Value<string>("title") ?? a.Name,
                a.Value<string>("breadtext") ?? string.Empty,
                new Uri(baseUrl + a.Url())
            )
            {
                PublishDate = a.CreateDate,
                Id = baseUrl + a.Url()
            }).ToList();

            var feed = new SyndicationFeed(
                "Teknikern",
                "Senaste tech-nyheterna från Teknikern",
                new Uri(baseUrl),
                items)
            {
                LastUpdatedTime = articles.Any() ? articles.Max(a => a.CreateDate) : DateTime.Now
            };

            using var stream = new MemoryStream();
            using (var xmlWriter = XmlWriter.Create(stream, new XmlWriterSettings { Encoding = System.Text.Encoding.UTF8, Indent = true }))
            {
                var rssFormatter = new Rss20FeedFormatter(feed);
                rssFormatter.WriteTo(xmlWriter);
            }

            return File(stream.ToArray(), "application/rss+xml; charset=utf-8");
        }
    }
}