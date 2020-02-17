using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;

namespace AudicaDownloader
{
    public class Downloader
    {
        private const string PAGEKEY = "{PAGEKEY}";
        private readonly HttpClient HttpClient;
        private static readonly string FetchUrl = $"http://www.audica.wiki:5000/api/customsongs?page={PAGEKEY}";
        public string DownloadFolder { get; set; }
        public string GameDirectory { get; set; }
        public Downloader(Config config)
        {
            DownloadFolder = config.TempDirectory;
            GameDirectory = config.AudicaGameDirectory;
            HttpClient = AudicaHttpClient.GetClient();
        }
        public async Task<AudicaSongList> FetchSongPage(int page)
        {
            if (page < 1)
                throw new ArgumentException($"Page cannot be less than 1");
            AudicaSongList songList = null;
            try
            {
                var response = await HttpClient.GetAsync(FetchUrl.Replace(PAGEKEY, page.ToString())).ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    songList = AudicaSongList.FromJson(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                }
                else
                {
                    Console.WriteLine($"Http failed retrieving song list: {response.StatusCode}: {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving song list: {ex.Message}");
                Console.WriteLine(ex);
            }
            return songList;
        }

        public async Task<DownloadResult[]> DownloadSongs(IEnumerable<AudicaSong> songs)
        {
            string tempFolder = DownloadFolder;
            string gameFolder = GameDirectory;
            try
            {
                Directory.CreateDirectory(tempFolder);
                Directory.CreateDirectory(gameFolder);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating DownloadFolder or GameDirectory folder.");
                Console.WriteLine(ex);
                return Array.Empty<DownloadResult>();
            }
            List<DownloadResult> downloadResults = new List<DownloadResult>();
            foreach (var song in songs)
            {
                DownloadResult result = await DownloadSong(song, gameFolder);
                downloadResults.Add(result);
            }

            return downloadResults.ToArray();
        }

        public Task<DownloadResult> DownloadSong(AudicaSong song, string downloadFolder)
        {
            string downloadPath = Path.Combine(downloadFolder, song.SongId, ".audica");
            return DownloadSong(song.DownloadUrl, song.SongId, downloadPath);
        }

        public async Task<DownloadResult> DownloadSong(Uri url, string songId, string fileTarget)
        {
            bool successful = false;
            Exception exception = null;
            try
            {
                var response = await HttpClient.GetAsync(FetchUrl, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                using (var fs = new FileStream(fileTarget, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await (await response.Content.ReadAsStreamAsync().ConfigureAwait(false)).CopyToAsync(fs).ConfigureAwait(false);
                }
                successful = true;
            }
            catch (HttpRequestException ex)
            {
                exception = ex;
                Console.WriteLine($"Http failed downloading song at {url}: {ex.Message}");
            }
            catch (IOException ex)
            {
                exception = ex;
                Console.WriteLine($"IO Error downloading song to {fileTarget}");
                Console.WriteLine(ex);
            }
            catch (Exception ex)
            {
                exception = ex;
                Console.WriteLine($"Error downloading song from {url}: {ex.Message}");
                Console.WriteLine(ex);
            }
            return new DownloadResult(songId, successful, fileTarget, exception);
        }
    }
}
