using System;
using System.Collections.Generic;
using System.Text;

namespace AudicaDownloader
{
    public class DownloadResult
    {
        public string SongId { get; protected set; }
        public string FileLocation { get; protected set; }
        public bool Successful { get; protected set; }
        public Exception Exception { get; protected set; }

        public DownloadResult(string songId, bool successful, string fileLocation, Exception exception)
        {
            SongId = songId;
            Successful = successful;
            FileLocation = fileLocation;
            Exception = exception;
        }
    }
}
