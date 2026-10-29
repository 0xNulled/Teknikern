using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Website.Controllers;
using Teknikern.Models;


namespace Teknikern.Controllers
{
  public class CommentController : SurfaceController
  {
    private readonly IContentService _contentService;

    public CommentController(
        IUmbracoContextAccessor umbracoContextAccessor,
        IUmbracoDatabaseFactory databaseFactory,
        ServiceContext services,
        AppCaches appCaches,
        IProfilingLogger profilingLogger,
        IPublishedUrlProvider publishedUrlProvider,
        IContentService contentService)
        : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
    {
      _contentService = contentService;
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SubmitNewComment(CreateCommentModel model)
    {
      if (!ModelState.IsValid)
      {
        return CurrentUmbracoPage();
      }

      var newComment = _contentService.Create(model.Author, model.Id, "articleComment");

      newComment.SetValue("commentAuthor", model.Author);
      newComment.SetValue("commentContent", model.Content);
      newComment.SetValue("commentId", model.Id);
      newComment.SetValue("commentDate", model.Date);

      var saveResult = _contentService.Save(newComment);

      if (saveResult.Success)
      {
        var publishResult = _contentService.Publish(newComment, new[] { "*" });

        if (publishResult.Success)
        {
          return RedirectToCurrentUmbracoPage();
        }
      }

      TempData["ErrorMessage"] = "Något gick fel.";
      return CurrentUmbracoPage();
    }

  }
}