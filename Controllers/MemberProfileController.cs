
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Web.Website.Controllers;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Routing;
using System.Text.Json;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Core.Web;

namespace Teknikern.Controllers;

public class MemberProfileController : SurfaceController
{
  private readonly IMemberManager _memberManager;
  private readonly IMemberService _memberService;
  private readonly IMediaService _mediaService;
  private readonly MediaFileManager _mediaFileManager;
  private readonly MediaUrlGeneratorCollection _mediaUrlGeneratorCollection;
  private readonly IShortStringHelper _shortStringHelper;
  private readonly IContentTypeBaseServiceProvider _contentTypeBaseServiceProvider;

  public MemberProfileController(
      IUmbracoContextAccessor umbracoContextAccessor,
      IUmbracoDatabaseFactory databaseFactory,
      ServiceContext services,
      AppCaches appCaches,
      IProfilingLogger profilingLogger,
      IPublishedUrlProvider publishedUrlProvider,
      IMemberManager memberManager,
      IMemberService memberService,
      IMediaService mediaService,
      MediaFileManager mediaFileManager,
      MediaUrlGeneratorCollection mediaUrlGeneratorCollection,
      IShortStringHelper shortStringHelper,
      IContentTypeBaseServiceProvider contentTypeBaseServiceProvider)
      : base(umbracoContextAccessor, databaseFactory, services,
          appCaches, profilingLogger, publishedUrlProvider)
  {
    _memberManager = memberManager;
    _memberService = memberService;
    _mediaService = mediaService;
    _mediaFileManager = mediaFileManager;
    _mediaUrlGeneratorCollection = mediaUrlGeneratorCollection;
    _shortStringHelper = shortStringHelper;
    _contentTypeBaseServiceProvider = contentTypeBaseServiceProvider;
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> UploadProfilePicture(IFormFile profilePicture)
  {
    var currentUser = await _memberManager.GetCurrentMemberAsync();

    if (currentUser == null)
      return Unauthorized();

    if (profilePicture == null || profilePicture.Length == 0)
      return RedirectToCurrentUmbracoPage();

    // Limit upload size to 5 MB
    if (profilePicture.Length > 5 * 1024 * 1024)
      return RedirectToCurrentUmbracoPage();

    var allowedTypes = new[] { ".jpg", ".jpeg", ".png", ".webp" };
    var extension = Path.GetExtension(profilePicture.FileName).ToLowerInvariant();

    if (!allowedTypes.Contains(extension))
      return RedirectToCurrentUmbracoPage();

    // Get the actual Umbraco member
    var member = _memberService.GetByEmail(currentUser.Email!);

    if (member == null)
      return Unauthorized();

    // Create media item
    var media = _mediaService.CreateMedia(
        Path.GetFileNameWithoutExtension(profilePicture.FileName),
        Constants.System.Root,
        Constants.Conventions.MediaTypes.Image);

    // Save uploaded image to Umbraco Media Library
    using (var stream = profilePicture.OpenReadStream())
    {
      media.SetValue(
          _mediaFileManager,
          _mediaUrlGeneratorCollection,
          _shortStringHelper,
          _contentTypeBaseServiceProvider,
          Constants.Conventions.Media.File,
          Path.GetFileName(profilePicture.FileName),
          stream);
    }

    var mediaResult = _mediaService.Save(media);

    if (!mediaResult.Success)
      return RedirectToCurrentUmbracoPage();

    // Set Media Picker 3 value
    var pickerValue = JsonSerializer.Serialize(new[]
    {
            new
            {
                key = Guid.NewGuid(),
                mediaKey = media.Key,
                mediaTypeAlias = "Image",
                crops = Array.Empty<object>(),
                focalPoint = (object?)null
            }
        });

    member.SetValue("profilePicture", pickerValue);

    _memberService.Save(member);

    return RedirectToCurrentUmbracoPage();
  }
}
