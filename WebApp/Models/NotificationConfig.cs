using System.Collections.Generic;

namespace Judge1.Models
{
    public class DingTalkConfig
    {
        public bool Enabled { get; set; }
        public string Token { get; set; }
        public string Secret { get; set; }
        public List<string> Admins { get; set; }
    }

    public class NotificationConfig
    {
        public DingTalkConfig DingTalk { get; set; }
    }
}