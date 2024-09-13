using HtmlAgilityPack;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace avoidplayer.Services
{
    public class SteamService
    {
        private static readonly string urlProfile = "https://steamcommunity.com/profiles/";
        private static readonly string urlSteam = "https://steamcommunity.com/id/";

        public async Task<string> GetUserImageAsync(string user)
        {
            HttpClient httpClient = new HttpClient();
            string profileUrl = user.Length == 17 ? urlProfile : urlSteam;
            var html = await httpClient.GetStringAsync(profileUrl + user);
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);

            var userImageNode = htmlDocument.DocumentNode.SelectSingleNode("//div[contains(@class,'playerAvatarAutoSizeInner')]");
            if (userImageNode != null)
            {
                foreach (var childNode in userImageNode.ChildNodes)
                {
                    if (childNode.Name == "img")
                    {
                        var urlImage = childNode.OuterHtml;
                        int startIndex = urlImage.IndexOf("https");
                        int endIndex = urlImage.Contains(".jpg") ? urlImage.IndexOf(".jpg") + 4 : urlImage.IndexOf(".gif") + 4;
                        var fixUrlImage = urlImage.Substring(startIndex, endIndex - startIndex);
                        if (fixUrlImage.StartsWith("https://avatars.akamai.steamstatic.com/") || fixUrlImage.StartsWith("https://cdn.akamai.steamstatic.com/"))
                        {
                            return fixUrlImage;
                        }
                    }
                }
            }
            return "https://avatars.akamai.steamstatic.com/fef49e7fa7e1997310d705b2a6158ff8dc1cdfeb_full.jpg";
        }

        public async Task<string> GetUserNameAsync(string user, bool isProfile)
        {
            HttpClient httpClient = new HttpClient();
            string url = isProfile ? urlProfile : urlSteam;
            var html = await httpClient.GetStringAsync(url + user);
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);

            var usernameNode = htmlDocument.DocumentNode.SelectSingleNode("//span[contains(@class,'actual_persona_name')]");
            return usernameNode?.InnerText ?? user;
        }
    }
}
