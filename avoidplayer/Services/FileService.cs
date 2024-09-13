using System.IO;
using System.Threading.Tasks;

namespace avoidplayer.Services
{
    public class FileService
    {
        public static readonly string filePath = "C:/Users/nbruu/Desktop/dota2avoidlist.txt";

        public async void AppendToFile(string name, string Reason, string steamImage)
        {
            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                writer.WriteLine($"SteamId: {name}, Reason: {Reason}, SteamImage: {steamImage}");
            }
        }

        public  async Task UpdateFileAsync(string steamId)
        {
            string tempFile = Path.GetTempFileName();
            using (var reader = new StreamReader(filePath))
            using (var writer = new StreamWriter(tempFile))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (!line.Contains($"SteamId: {steamId}"))
                    {
                        writer.WriteLine(line);
                    }
                }
            }

            File.Delete(filePath);
            File.Move(tempFile, filePath);
        }
    }
}
