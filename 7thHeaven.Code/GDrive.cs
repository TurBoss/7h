/*
  This source is subject to the Microsoft Public License. See LICENSE.TXT for details.
  The original developer is Iros <irosff@outlook.com>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.ComponentModel;
using _7thHeaven.Code;

namespace Iros._7th.Workshop {
    public class GDrive {

        public event ProgressChangedEventHandler FileDownloadProgressChanged;
        public event DownloadProgressChangedEventHandler DownloadProgressChanged;
        public event AsyncCompletedEventHandler DownloadFileCompleted;

        private string _file, _url;
        private int _mode = 0;
        private object _state;
        private CookieContainer _cookies;
        private WebClientEx _webClient;
        //0 = Not sure, trying initial download
        //1 = First download completed, HTML detecting, retrying

        public void CancelAsync() {
            if (_webClient != null)
            {
                _webClient.CancelAsync();
            }
        }

        public long GetContentLength() {
            if (_webClient == null)
            {
                return -1;
            }

            string crange = _webClient.ResponseHeaders["Content-Range"];
            if (crange != null) {
                int spos = crange.LastIndexOf('/');
                if (spos >= 0) {
                    long range = -1;
                    long.TryParse(crange.Substring(spos + 1), out range);
                    return range;
                }
            }
            return -1;
        }

        private class WebClientEx : WebClient {
            public WebClientEx(CookieContainer container) {
                this.container = container;
            }

            private readonly CookieContainer container = new CookieContainer();

            protected override WebRequest GetWebRequest(Uri address) {
                WebRequest r = base.GetWebRequest(address);
                var request = r as HttpWebRequest;
                if (request != null) {
                    request.CookieContainer = container;
                    request.AddRange(0);
                }
                return r;
            }

            protected override WebResponse GetWebResponse(WebRequest request, IAsyncResult result) {
                WebResponse response = base.GetWebResponse(request, result);
                ReadCookies(response);
                return response;
            }

            protected override WebResponse GetWebResponse(WebRequest request) {
                WebResponse response = base.GetWebResponse(request);
                ReadCookies(response);
                return response;
            }

            private void ReadCookies(WebResponse r) {
                var response = r as HttpWebResponse;
                if (response != null) {
                    CookieCollection cookies = response.Cookies;
                    container.Add(cookies);
                }
            }
        }

        public void Download(string gUrl, string destination, object userState) {
            //https://docs.google.com/uc?id=0B-Q_AObuWRSXNW9rb3FxS0F1Qk0&export=download
            //https://docs.google.com/uc?export=download&confirm=_bQe&id=0B-Q_AObuWRSXNW9rb3FxS0F1Qk0
            gUrl = String.Format("https://docs.google.com/uc?id={0}&export=download", gUrl);
            _cookies = new CookieContainer();
            var wc = new WebClientEx(_cookies);

            wc.DownloadProgressChanged += wc_DownloadProgressChanged;
            wc.DownloadFileCompleted += wc_DownloadFileCompleted;

            _file = destination;
            _url = gUrl;
            _state = userState;
            _webClient = wc;

            System.Diagnostics.Debug.WriteLine("GDrive: requesting " + gUrl);
            wc.Headers.Add(HttpRequestHeader.AcceptEncoding, "");
            wc.DownloadFileAsync(new Uri(gUrl), destination, userState);
        }

        void wc_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e) {
            if (e.Error != null || e.Cancelled)
                DownloadFileCompleted(this, e);
            else {
                if (_mode == 0) {
                    if (new System.IO.FileInfo(_file).Length < 100 * 1024) {
                        string text = System.IO.File.ReadAllText(_file);
                        int html = text.IndexOf("<html", StringComparison.InvariantCultureIgnoreCase);
                        if (html >= 0 && html < 100) {
                            int ilink = text.IndexOf("\"uc-download-link\"", StringComparison.InvariantCultureIgnoreCase);
                            if (ilink > 0) {
                                int href = text.IndexOf("href=\"", ilink, StringComparison.InvariantCultureIgnoreCase);
                                if (href > 0) {
                                    int hrefend = text.IndexOf('"', href + 6);
                                    if (hrefend > 0) {
                                        string url = text.Substring(href + 6, hrefend - href - 6).Replace("&amp;", "&");
                                        if (url.IndexOf("://") < 0) url = new Uri(_url).GetLeftPart(UriPartial.Authority) + url;

                                        System.Diagnostics.Debug.WriteLine("GDrive: redirecting to " + url);
                                        System.Diagnostics.Debug.WriteLine("GDrive: {0} cookies set", _cookies.Count);

                                        DownloadItem downloadInfo = _state as DownloadItem;

                                        FileDownloadTask fileDownload = new FileDownloadTask(url, _file, downloadInfo, _cookies)
                                        {
                                            Headers = new WebHeaderCollection()
                                        };
                                        fileDownload.Headers.Add("Referer", _url);

                                        downloadInfo.PerformCancel = () =>
                                        {
                                            fileDownload.CancelAsync();
                                            downloadInfo.OnCancel?.Invoke();
                                        };

                                        fileDownload.DownloadProgressChanged += FileDownload_DownloadProgressChanged;
                                        fileDownload.DownloadFileCompleted += wc_DownloadFileCompleted;

                                        System.IO.File.Delete(_file); // delete temp html file just downloaded

                                        downloadInfo.FileDownloadTask = fileDownload;
                                        fileDownload.Start();
                                        _mode = 1;

                                        CleanUpWebClient();
                                        return;
                                    }
                                }
                            }

                            int tstart = text.IndexOf("<title>", StringComparison.InvariantCultureIgnoreCase);
                            int tend = text.IndexOf("</title>", StringComparison.InvariantCultureIgnoreCase);
                            string err = "Couldn't parse data";
                            if (tstart > 0 && tend > 0) {
                                err = text.Substring(tstart + 7, tend - tstart - 7);
                            }

                            //If we get here, it went wrong
                            System.IO.File.Delete(_file);
                            DownloadFileCompleted?.Invoke(this, new AsyncCompletedEventArgs(new Exception(err), false, _state));
                        } else {
                            DownloadFileCompleted?.Invoke(this, e);
                        }
                    } else
                        DownloadFileCompleted?.Invoke(this, e);
                } else
                {
                    // actual file being downloaded has finished successfully at this point
                    CleanUpFileDownloadTask();
                    DownloadFileCompleted?.Invoke(this, e);
                }
            }
        }

        private void CleanUpWebClient()
        {
            if (_webClient == null)
            {
                return;
            }

            _webClient.DownloadProgressChanged -= wc_DownloadProgressChanged;
            _webClient.DownloadFileCompleted -= wc_DownloadFileCompleted;
            _webClient.Dispose();
            _webClient = null;
        }

        internal void CleanUpFileDownloadTask()
        {
            DownloadItem downloadInfo = _state as DownloadItem;

            if (downloadInfo?.FileDownloadTask != null)
            {
                downloadInfo.FileDownloadTask.DownloadProgressChanged -= FileDownload_DownloadProgressChanged;
                downloadInfo.FileDownloadTask.DownloadFileCompleted -= wc_DownloadFileCompleted;
                downloadInfo.FileDownloadTask = null;
            }
        }

        private void FileDownload_DownloadProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            FileDownloadProgressChanged?.Invoke(this, e);
        }

        void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e) {
            if (_mode != 0 || e.BytesReceived > 100 * 1024) {
                DownloadProgressChanged?.Invoke(this, e);
            }
        }
    }
}
