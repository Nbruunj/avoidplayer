using System;
using System.IO;

namespace avoidplayer.Utils
{
    public static class PathUtils
    {
        public static string GetImageFilePath(string steamId, string imageUrl)
        {
            string fileType = Path.GetExtension(imageUrl);
            return $"C:/Users/nbruu/Desktop/dotaavoidplayer/{steamId}{fileType}";
        }

        public static string CleanUrl(string url)
        {
            int idIndex = url.IndexOf("id/");
            int profilesIndex = url.IndexOf("profiles/");
            if (idIndex != -1)
            {
                return url.Substring(idIndex + 3).Trim('/');
            }
            else if (profilesIndex != -1)
            {
                return url.Substring(profilesIndex + 9).Trim('/');
            }
            return url;
        }
    }
}
