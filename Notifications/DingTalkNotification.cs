using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace Judge1.Notifications
{
    public class DingTalkText
    {
        [JsonProperty("content")] public string Content;
    }

    public class DingTalkAt
    {
        [JsonProperty("atMobiles")] public IList<string> AtMobiles;
        [JsonProperty("isAtAll")] public bool IsAtAll;
    }

    public sealed class DingTalkRequest
    {
        [JsonProperty("msgtype")] public string Type { get; set; }
        [JsonProperty("text")] public DingTalkText Text { get; set; }
        [JsonProperty("at")] public DingTalkAt At { get; set; }

        public DingTalkRequest(string content, IList<string> atMobiles, bool isAtAll)
        {
            Type = "text";
            Text = new DingTalkText {Content = content};
            At = new DingTalkAt {AtMobiles = atMobiles, IsAtAll = isAtAll};
        }
    }

    public sealed class DingTalkResponse
    {
        [JsonProperty("errcode")] public int Code { get; set; }
        [JsonProperty("errmsg")] public string Message { get; set; }
    }

    public interface IDingTalkNotification
    {
        public Task SendText(string message, params object[] args);
        public Task SendText(bool isAtAll, string message, params object[] args);
        public Task SendText(IList<string> atMobiles, bool isAtAll, string message, params object[] args);
    }

    public sealed class DingTalkNotification : NotificationBase<DingTalkNotification>, IDingTalkNotification
    {
        private const string BaseUrl = "https://oapi.dingtalk.com/robot/send?access_token=";
        private bool Enabled => Options.Value.DingTalk.Enabled;
        private string Endpoint => BaseUrl + Options.Value.DingTalk.Token;
        private string Secret => Options.Value.DingTalk.Secret;
        private List<string> Admins => Options.Value.DingTalk.Admins;

        public DingTalkNotification(IServiceProvider provider) : base(provider)
        {
        }

        public override bool IsEnabled()
        {
            return Enabled;
        }

        public override async Task SendNotification(bool atAdmins, string message, params object[] args)
        {
            await SendText(atAdmins ? Admins : null, false, message, args);
        }

        private string CalculateSignature(long timestamp)
        {
            var original = timestamp + "\n" + Options.Value.DingTalk.Secret;
            using var hash = new HMACSHA256(Encoding.UTF8.GetBytes(Secret));
            var signed = hash.ComputeHash(Encoding.UTF8.GetBytes(original));
            return HttpUtility.UrlEncode(Convert.ToBase64String(signed), Encoding.UTF8);
        }

        public async Task SendText(string message, params object[] args)
        {
            await SendText(null, false, message, args);
        }

        public async Task SendText(bool isAtAll, string message, params object[] args)
        {
            await SendText(null, isAtAll, message, args);
        }

        public async Task SendText(IList<string> atMobiles, bool isAtAll, string message, params object[] args)
        {
            if (!Enabled) return;

            using var client = Factory.CreateClient();
            var request = new DingTalkRequest(string.Format(message, args), atMobiles, isAtAll);
            var jsonReq = JsonConvert.SerializeObject(request);
            var content = new StringContent(jsonReq, Encoding.UTF8, MediaTypeNames.Application.Json);

            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var signature = CalculateSignature(timestamp);
            var result = await client.PostAsync(Endpoint + "&timestamp=" + timestamp + "&sign=" + signature, content);

            var jsonRes = await result.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<DingTalkResponse>(jsonRes);
            if (response.Code == 310000)
            {
                Logger.LogError($"SendText Error Message={response.Message}");
            }
        }
    }
}