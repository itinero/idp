// The MIT License (MIT)

// Copyright (c) 2017 Ben Abelshausen

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using Itinero.Logging;
using Serilog;
using System;
using System.IO;
using System.Net.Http;

namespace IDP
{
    /// <summary>
    /// Contains functionality to download files, to allow url as input file arguments.
    /// </summary>
    public static class Downloader
    {
        /// <summary>
        /// Opens or downloads and opens the given file.
        /// </summary>
        public static string DownloadOrOpen(string file)
        {
            if (Downloader.IsUrl(file))
            { // download file and get local filename.
                file = Downloader.Download(file);
            }
            return file;
        }

        /// <summary>
        /// Downloads data from the source URL to the working folder.
        /// </summary>
        public static string Download(string url)
        {
            Logger.Log("Dowloader.Download", TraceEventType.Information, 
                "Downloading file: {0}", url);
            try
            {
                // download file.
                var uri = new Uri(url);
                var filename = System.IO.Path.GetFileName(uri.LocalPath);
                var client = new HttpClient();
                using (var stream = client.GetStreamAsync(url).GetAwaiter().GetResult())
                using (var localStream = File.Open(filename, FileMode.Create))
                {
                    stream.CopyTo(localStream);
                    localStream.Flush();
                }
                
                Logger.Log("Dowloader.Download", TraceEventType.Information,
                    "Downloaded file succesfully: {0}", filename);
                return filename;
            }
            catch (Exception ex)
            {
                Logger.Log("Dowloader.Download", TraceEventType.Critical,
                    "A fatal error occured, failed to download: {0} - {1}.", url, ex.ToString());
            }
            return string.Empty;
        }

        /// <summary>
        /// Returns true if the given string represents a url.
        /// </summary>
        public static bool IsUrl(string url)
        {
            return (Uri.IsWellFormedUriString(url, UriKind.Absolute));
        }
    }
}