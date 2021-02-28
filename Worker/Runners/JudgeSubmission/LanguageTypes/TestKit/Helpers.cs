using System.Collections.Generic;
using System.Linq;

namespace Worker.Runners.JudgeSubmission.LanguageTypes.TestKit
{
    public sealed partial class TestKitRunner
    {
        private bool IsStudentInGroups(IEnumerable<string> groups)
        {
            return groups?.Any(IsStudentInGroup) ?? true;
        }

        private bool IsStudentInGroup(string group)
        {
            return group.Equals("*") ||
                   (_manifest.Groups.TryGetValue(group, out var list) && list.Contains(_submission.User.ContestantId));
        }
    }
}