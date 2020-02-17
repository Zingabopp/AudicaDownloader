using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;

namespace AudicaDownloader
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting AudicaDownloader...");
            Console.WriteLine("Fetching song list...");
            Config config = new Config()
            {
                AudicaGameDirectory = Path.Combine(Environment.CurrentDirectory, "GameDirectory"),
                TempDirectory = Path.Combine(Environment.CurrentDirectory, "Temp")
            };
            Downloader downloader = new Downloader(config);
            int pageIndex = 1;
            AudicaSongList songList = await downloader.FetchSongPage(pageIndex).ConfigureAwait(false);
            List<AudicaSong> songs = new List<AudicaSong>(songList.SongCount);
            songs.AddRange(songList.Songs);
            int totalPages = songList.TotalPages;
            await File.WriteAllTextAsync($"Songs_{pageIndex}.txt", songList.ToJson());
            int songCount = songList.Songs.Count;
            while (pageIndex < totalPages)
            {
                pageIndex++;
                songList = await downloader.FetchSongPage(pageIndex).ConfigureAwait(false);
                songs.AddRange(songList.Songs);
                songCount += songList.Songs.Count;
                await File.WriteAllTextAsync($"Songs_{pageIndex}.txt", songList.ToJson());
            }
            var songsToDownload = songs.GroupBy(s => s.SongId).Select(g => g.First()).ToList();
            var mismatched = songsToDownload.Where(s => !(s.Filename.StartsWith(s.SongId, StringComparison.OrdinalIgnoreCase)));
            Console.WriteLine($"Found {songsToDownload.Count} songs");
            var downloadResults = await downloader.DownloadSongs(songsToDownload).ConfigureAwait(false);
            var successfulDownloads = downloadResults.Where(r => r.Successful).Count();
            var failedDownloads = downloadResults.Where(r => !r.Successful).ToList();
            Console.WriteLine($"Downloaded {successfulDownloads} songs.");
            if (failedDownloads.Count > 0)
                Console.WriteLine($"Failed to download {failedDownloads.Count} songs");
            Console.Read();
        }
    }
}
