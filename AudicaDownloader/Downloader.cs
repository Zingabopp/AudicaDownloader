using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

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
                HttpResponseMessage response = await HttpClient.GetAsync(FetchUrl.Replace(PAGEKEY, page.ToString())).ConfigureAwait(false);
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

        public async Task<DownloadResult[]> DownloadSongs(IList<AudicaSong> songs)
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
            for (int i = 0; i < songs.Count; i++)
            {
                AudicaSong song = songs[i];
                Console.Write($"({i + 1}/{songs.Count}) Downloading {song.SongId}...");
                if(File.Exists(Path.Combine(gameFolder, song.SongId + ".audica")))
                {
                    Console.WriteLine("Skipped (already exists)");
                    continue;
                }
                DownloadResult result = await DownloadSong(song, gameFolder);
                if (result.Successful)
                    Console.WriteLine("Done");
                else
                {
                    string message = "Failed";
                    if (result.Exception != null)
                    {
                        message += ": ";
                        if (result.Exception is HttpRequestException httpRequestException)
                        {
                            message += httpRequestException.Message;
                        }
                        else if (result.Exception is IOException ioException)
                        {
                            message += ioException.Message;
                        }
                        else
                            Console.WriteLine(result.Exception);
                    }

                    Console.WriteLine(message);
                }

                downloadResults.Add(result);
            }

            return downloadResults.ToArray();
        }

        public Task<DownloadResult> DownloadSong(AudicaSong song, string downloadFolder)
        {
            string downloadPath = Path.Combine(downloadFolder, song.SongId + ".audica");
            return DownloadSong(song.DownloadUrl, song.SongId, downloadPath);
        }

        public async Task<DownloadResult> DownloadSong(Uri url, string songId, string fileTarget)
        {
            bool successful = false;
            Exception exception = null;
            try
            {
                using (HttpResponseMessage response = await HttpClient.GetAsync(url).ConfigureAwait(false))
                {
                    response.EnsureSuccessStatusCode();
                    using (FileStream fs = new FileStream(fileTarget, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await (await response.Content.ReadAsStreamAsync().ConfigureAwait(false)).CopyToAsync(fs).ConfigureAwait(false);
                    }
                    successful = true;
                }
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            return new DownloadResult(songId, successful, fileTarget, exception);
        }
    }
}
