using System;
using Shared.Generics;

namespace Shared.Models
{
  public class Score{
    public int id {get; set; }
    [Required] public string UserId { get; set; }
    public ApplicationUser User { get; set; }
    public int SubmissionId {get; set; }
    public Submission Submission {get; set; }
    public int Score {get; set; }
  }
}
