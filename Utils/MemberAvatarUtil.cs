using System.Text.Json;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Teknikern.Utils;

public static class MemberAvatarUtil
{
  public static string? GetProfilePictureUrl(IMember? member, IMediaService mediaService)
  {
    var raw = member?.GetValue<string>("profilePicture");
    if (string.IsNullOrEmpty(raw)) return null;

    try
    {
      using var json = JsonDocument.Parse(raw);
      if (json.RootElement.GetArrayLength() == 0) return null;

      var mediaKey = json.RootElement[0].GetProperty("mediaKey").GetGuid();
      var media = mediaService.GetById(mediaKey);
      var rawFile = media?.GetValue<string>("umbracoFile");
      if (string.IsNullOrEmpty(rawFile)) return null;

      using var fileJson = JsonDocument.Parse(rawFile);
      return fileJson.RootElement.GetProperty("src").GetString();
    }
    catch
    {
      return null;
    }
  }
}