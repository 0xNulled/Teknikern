using Microsoft.AspNetCore.Mvc;
using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
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

        private static string StripHtml(string html)
        {
            var noTags = Regex.Replace(html ?? string.Empty, "<.*?>", " ");
            var collapsed = Regex.Replace(noTags, @"\s+", " ").Trim();
            return System.Net.WebUtility.HtmlDecode(collapsed);
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

            var items = articles.Select(a =>
            {
                var item = new SyndicationItem
                {
                    Title = new TextSyndicationContent(a.Value<string>("title") ?? a.Name),
                    Content = new TextSyndicationContent(StripHtml(a.Value<string>("breadtext") ?? string.Empty)),
                    PublishDate = a.CreateDate
                };
                item.Links.Add(SyndicationLink.CreateAlternateLink(new Uri(baseUrl + a.Url())));
                return item;
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