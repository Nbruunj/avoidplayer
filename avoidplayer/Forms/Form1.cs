using avoidplayer.Models;
using avoidplayer.Services;
using avoidplayer.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using avoidplayer.Models.avoidplayer.Models;
using System.Runtime.InteropServices;

namespace avoidplayer.Forms
{
    public partial class Form1 : Form
    {
        private static List<User> dotaAvoidList = new List<User>();
        private static List<string> profileId = new List<string>();
        private List<string> steamId = new List<string>();
        private readonly SteamService _steamService = new SteamService();
        private readonly FileService _fileService = new FileService();

        public Form1()
        {
            InitializeComponent();
            Load += Form1_Load;
            textBox1.TextChanged += TextBox1_TextChanged;
            listView.SelectedIndexChanged += ListView_SelectedIndexChanged;
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                await InitializeForm();
                LoadImage();
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred during form load: " + ex.Message);
            }
        }

        private void LoadImage()
        {
            try
            {
                Image image = Image.FromFile("C:/Users/nbruu/Pictures/am.png");
                pictureBox.Image = image;
                pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            }
            catch (FileNotFoundException ex)
            {
                MessageBox.Show("Image file not found: " + ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show("Access to the image file is denied: " + ex.Message);
            }
            catch (ExternalException ex)
            {
                MessageBox.Show("An error occurred while loading the image: " + ex.Message);
            }
        }

        private async Task InitializeForm()
        {
            try
            {
                await LoadSteamData();
                UpdateListView();
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred during initialization: " + ex.Message);
            }
        }

        private async Task LoadSteamData()
        {
            await Task.Delay(100);
            if (File.Exists(FileService.filePath))
            {
                string[] lines = File.ReadAllLines(FileService.filePath);
                foreach (var line in lines)
                {
                    string[] parts = line.Split(new[] { ", Reason: " }, StringSplitOptions.None);
                    if (parts.Length != 2)
                        continue;

                    string[] subParts = parts[1].Split(new[] { ", SteamImage: " }, StringSplitOptions.None);
                    if (subParts.Length != 2)
                        continue;

                    string steamId = parts[0].Substring(parts[0].IndexOf(":") + 2);
                    string reason = subParts[0];
                    string image = subParts[1];

                    if (steamId.Length == 17)
                    {
                        profileId.Add(steamId);
                    }
                    else
                    {
                        this.steamId.Add(steamId);
                    }

                    dotaAvoidList.Add(new User
                    {
                        SteamId = steamId,
                        Name = steamId,
                        Reason = reason,
                        Image = image
                    });
                }

                foreach (var profile in profileId)
                {
                    if (string.IsNullOrEmpty(profile))
                        continue;

                    string username = await _steamService.GetUserNameAsync(profile, true);
                    var user = dotaAvoidList.FirstOrDefault(u => u.SteamId == profile);
                    if (user != null)
                    {
                        user.Name = username;
                    }
                }

                foreach (var steam in steamId)
                {
                    if (string.IsNullOrEmpty(steam))
                        continue;

                    string username = await _steamService.GetUserNameAsync(steam, false);
                    var user = dotaAvoidList.FirstOrDefault(u => u.SteamId == steam);
                    if (user != null)
                    {
                        user.Name = username;
                    }
                }
            }
        }

        private void UpdateListView()
        {
            listView.Items.Clear();
            foreach (var user in dotaAvoidList)
            {
                ListViewItem item = new ListViewItem(user.Name)
                {
                    SubItems = { user.Reason }
                };
                listView.Items.Add(item);
            }
            
        }

        private async void ListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView.SelectedItems.Count == 0)
                return;

            var selectedItem = listView.SelectedItems[0];
            var user = dotaAvoidList.FirstOrDefault(u => u.Name == selectedItem.Text);

            if (user != null && !string.IsNullOrEmpty(user.Image))
            {
                await LoadUserImageAsync(user.Image, user.SteamId);
            }
        }

        private async void AddPlayer_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox1.Text) || string.IsNullOrEmpty(textBox2.Text))
                return;

            string userName = PathUtils.CleanUrl(textBox1.Text);
            string userImage = await _steamService.GetUserImageAsync(userName);

            var user = new User
            {
                Name = userName,
                Reason = textBox2.Text,
                Image = userImage
            };
            dotaAvoidList.Add(user);
            _fileService.AppendToFile(userName, textBox2.Text, userImage);

            textBox1.Clear();
            textBox2.Clear();
            textBox1.Focus();
            dotaAvoidList.Clear();
            await InitializeForm();
        }

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {
            string searchText = textBox1.Text.Trim().ToLower();
            var filteredUsers = dotaAvoidList.Where(u => u.Name.ToLower().Contains(searchText)).ToList();

            listView.Items.Clear();
            foreach (var user in filteredUsers)
            {
                ListViewItem item = new ListViewItem(user.Name)
                {
                    SubItems = { user.Reason }
                };
                listView.Items.Add(item);
            }
        }

        private async void RemovePlayer_Click(object sender, EventArgs e)
        {
            if (listView.SelectedItems.Count > 0)
            {
                var selectedItem = listView.SelectedItems[0];
                var userToRemove = dotaAvoidList.FirstOrDefault(user => user.Name == selectedItem.Text);

                if (userToRemove != null)
                {
                    dotaAvoidList.Remove(userToRemove);
                    listView.Items.Remove(selectedItem);

                    string filePath = PathUtils.GetImageFilePath(userToRemove.SteamId, userToRemove.Image);
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }

                    await _fileService.UpdateFileAsync(userToRemove.SteamId);
                }
            }
        }

        private async Task LoadUserImageAsync(string imageUrl, string steamId)
        {
            try
            {
                string filePath = PathUtils.GetImageFilePath(steamId, imageUrl);
                await DownloadImageAsync(imageUrl, filePath);

                if (pictureBox.Image != null)
                {
                    pictureBox.Image.Dispose();
                    pictureBox.Image = null;
                }

                using (var stream = new MemoryStream(File.ReadAllBytes(filePath)))
                {
                    Image image = Image.FromStream(stream);
                    pictureBox.Image = image;
                    pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                }

                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load image: {ex.Message}");
                pictureBox.Image = null;
            }
        }

        private async Task DownloadImageAsync(string url, string filePath)
        {
            using (var httpClient = new HttpClient())
            {
                var imageBytes = await httpClient.GetByteArrayAsync(url);
                await File.WriteAllBytesAsync(filePath, imageBytes);
            }
        }
    }
}
