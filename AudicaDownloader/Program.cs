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
            Downloader downloader = new Downloader();
            int pageIndex = 1;
            AudicaSongList songList = await downloader.FetchSongPage(pageIndex).ConfigureAwait(false);
            List<AudicaSong> songs = new List<AudicaSong>(songList.SongCount);
            songs.AddRange(songList.Songs);
            int totalPages = songList.TotalPages;
            File.WriteAllText($"Songs_{pageIndex}.txt", songList.ToJson());
            int songCount = songList.Songs.Count;
            while (pageIndex < totalPages)
            {
                pageIndex++;
                songList = await downloader.FetchSongPage(pageIndex).ConfigureAwait(false);
                songs.AddRange(songList.Songs);
                songCount += songList.Songs.Count;
                File.WriteAllText($"Songs_{pageIndex}.txt", songList.ToJson());
            }
            var wrongSongId = songs.Where(s => s.SongId != AudicaSong.CreateSongId(s)).Select(s => (s, AudicaSong.CreateSongId(s))).ToArray();
            var uniqueSongIds = songs.GroupBy(s => s.SongId).ToArray();
            var duplicateSongs = uniqueSongIds.Where(g => g.Count() != 1).ToArray();
            var songsToDownload = songs.GroupBy(s => s.SongId).Select(g => g.First()).ToList();
            var skippeSongs = songs.Except(songsToDownload).ToArray();
            Console.WriteLine($"Found {songs.Count} songs");
            Console.Read();
        }
    }
}
