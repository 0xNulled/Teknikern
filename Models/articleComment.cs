using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Teknikern.Models
{
  public class CreateCommentModel
  {
    [Required(ErrorMessage = "Please enter an author.")]
    public string Author { get; set; }

    [Required(ErrorMessage = "Please enter your comment.")]
    public string Content { get; set; }

    [Required(ErrorMessage = "Could not get article Id.")]
    public int Id { get; set; }
    [Required(ErrorMessage = "Could not get time posted.")]
    public DateTime Date { get; set; }
  }
}