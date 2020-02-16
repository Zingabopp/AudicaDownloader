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
            var songs = await downloader.FetchSongPage().ConfigureAwait(false);
            Console.WriteLine($"Found {songs.SongCount} songs");
            Console.Read();
        }
    }
}
