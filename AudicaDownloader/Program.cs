using System;
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
            var songs = await downloader.FetchSongPage(pageIndex).ConfigureAwait(false);
            pageIndex++;
            int totalPages = (int)songs.TotalPages;

            Console.WriteLine($"Found {songs.SongCount} songs");
            Console.Read();
        }
    }
}
