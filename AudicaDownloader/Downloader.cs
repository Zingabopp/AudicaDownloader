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
        private static readonly HttpClient HttpClient = AudicaHttpClient.GetClient();
        private static readonly string FetchUrl = $"http://www.audica.wiki:5000/api/customsongs?page={PAGEKEY}";
        public async Task<AudicaSongList> FetchSongPage(int page)
        {
            if (page < 1)
                throw new ArgumentException($"Page cannot be less than 1");
            AudicaSongList songList = null;
            try
            {
                var response = await HttpClient.GetAsync(FetchUrl.Replace(PAGEKEY, page.ToString())).ConfigureAwait(false);
                if(response.IsSuccessStatusCode)
                {
                    songList = AudicaSongList.FromJson(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                }
                else
                {
                    Console.WriteLine($"Http failed retrieving song list: {response.StatusCode}: {response.ReasonPhrase}");
                }
            } catch(Exception ex)
            {
                Console.WriteLine($"Error retrieving song list: {ex.Message}");
                Console.WriteLine(ex);
            }
            return songList;
        }

        public async Task<bool> DownloadSong(string url, string fileTarget)
        {
            bool successful = false;
            try
            {
                var response = await HttpClient.GetAsync(FetchUrl).ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    using (var fs = new FileStream(fileTarget, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await (await response.Content.ReadAsStreamAsync().ConfigureAwait(false)).CopyToAsync(fs).ConfigureAwait(false);
                    }
                }
                else
                {
                    Console.WriteLine($"Http failed downloading song at {url}: {response.StatusCode}: {response.ReasonPhrase}");
                }
            }
            catch(IOException ex)
            {
                Console.WriteLine($"IO Error downloading song to {fileTarget}");
                Console.WriteLine(ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading song from {url}: {ex.Message}");
                Console.WriteLine(ex);
            }
            return successful;
        }
    }
}
