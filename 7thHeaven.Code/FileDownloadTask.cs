using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace _7thHeaven.Code
{
    /// <summary>
    /// Downloads a file using <see cref="WebRequest"/> which allows for pausing and resuming capability.
    /// </summary>
    /// <remarks>
    /// The class keeps track of how many bytes written so just call <see cref="Pause"/> to pause the download and <see cref="Start"/> to resume it.
    /// reference: https://stackoverflow.com/questions/15995705/adding-pause-and-continue-ability-in-my-downloader
    /// </remarks>
    public class FileDownloadTask
    {
        public event ProgressChangedEventHandler DownloadProgressChanged;
        public event AsyncCompletedEventHandler DownloadFileCompleted;

        private bool _isCanceled;
        private bool _isStarted;
        private bool _allowedToRun;
        private string _sourceUrl;
        private string _destination;
        private int _chunkSize;
        private Lazy<long> _contentLength;

        private object _userState;
        private object _lock = new object();


        public long BytesWritten { get; private set; }
        public long ContentLength { get { return _contentLength.Value; } }

        public bool Done { get { return ContentLength == BytesWritten; } }

        public bool IsPaused { get => !AllowedToRun; }

        public bool AllowedToRun
        {
            get
            {
                lock (_lock)
                {
                    return _allowedToRun;
                }
            }
            private set
            {
                lock (_lock)
                {
                    _allowedToRun = value;
                }
            }
        }

        public bool IsCanceled
        {
            get
            {
                lock (_lock)
                {
                    return _isCanceled;
                }
            }
            private set
            {
                lock (_lock)
                {
                    _isCanceled = value;
                }
            }
        }

        public bool IsStarted { get => _isStarted; }

        public FileDownloadTask(string source, string destination, object userState = null, int chunkSizeInBytes = 10000 /*Default to 0.01 mb*/)
        {
            System.Net.ServicePointManager.Expect100Continue = false; // ensure this is set to false
            AllowedToRun = true;

            _sourceUrl = source;
            _destination = destination;
            _chunkSize = chunkSizeInBytes;
            _contentLength = new Lazy<long>(() => GetContentLength());
            _userState = userState;
            _isStarted = false;

            BytesWritten = 0;
        }

        private long GetContentLength()
        {
            var request = (HttpWebRequest)WebRequest.Create(_sourceUrl);
            request.Method = "HEAD";

            using (var response = request.GetResponse())
                return response.ContentLength;
        }

        private async Task Start(long range)
        {
            if (IsPaused || IsCanceled)
                return;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_sourceUrl);
            request.Method = "GET";
            request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)";
            request.KeepAlive = false;
            request.Proxy = null;
            request.AddRange(range);

            using (WebResponse response = await request.GetResponseAsync())
            {
                using (Stream responseStream = response.GetResponseStream())
                {
                    FileMode fileMode = FileMode.Append;

                    if (!IsStarted)
                    {
                        fileMode = FileMode.Create;
                    }

                    using (FileStream fs = new FileStream(_destination, fileMode, FileAccess.Write, FileShare.ReadWrite))
                    {
                        while (AllowedToRun && !IsCanceled)
                        {
                            _isStarted = true;
                            byte[] buffer = new byte[_chunkSize];
                            int bytesRead = await responseStream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);

                            if (bytesRead == 0) break;

                            await fs.WriteAsync(buffer, 0, bytesRead);
                            BytesWritten += bytesRead;

                            float prog = (float)BytesWritten / (float)ContentLength;
                            var e = new ProgressChangedEventArgs((int) (prog * 100), _userState);
                            DownloadProgressChanged?.Invoke(this, e);
                        }

                        await fs.FlushAsync();
                    }
                }
            }

            if (AllowedToRun && !IsCanceled && (BytesWritten == ContentLength) || (ContentLength == -1)) // -1 is returned when response doesnt have the content-length
            {
                DownloadFileCompleted?.Invoke(this, new AsyncCompletedEventArgs(null, false, _userState));
            }
            else if (IsCanceled)
            {
                DownloadFileCompleted?.Invoke(this, new AsyncCompletedEventArgs(null, cancelled: true, _userState));
            }
        }

        public Task Start()
        {
            AllowedToRun = true;
            Task downloadTask = Start(BytesWritten);

            // wire up async task to handle exceptions that may occurr
            downloadTask.ContinueWith((result) =>
            {
                if (result.IsFaulted)
                {
                    DownloadFileCompleted?.Invoke(this, new AsyncCompletedEventArgs(result.Exception, false, _userState));
                }
            });

            return downloadTask;
        }

        public void Pause()
        {
            AllowedToRun = false;
        }

        public void CancelAsync()
        {
            IsCanceled = true;
            AllowedToRun = false;
        }
    }
}
