using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AldExtractMidiFromUrl
{
    public class AldUrlTools
    {
        public class UrlInfo:IComparable
        {
            public string Information { get; set; }
            public string Url { get; set; }
            public UrlInfo(string information, string url)
            {
                this.Information = information;
                this.Url = url;
            }

            public override string ToString()
            {
                return Url;
            }
            public int CompareTo(object obj)
            {
                return Url.CompareTo((obj as UrlInfo).Url);
            }
        }

        public static List<UrlInfo> GetUrls(string url)
        {
            url = url.Replace("../", "");
            WebRequest request = WebRequest.CreateHttp(url);
            string data="";
            try
            {

                var stream = request.GetResponse().GetResponseStream();
                var reader = new StreamReader(stream);

                data = reader.ReadToEnd();

                reader.Dispose();
                stream.Dispose();
            }
            catch { 
                return new List<UrlInfo>();
            }
            return SearchUrls(data);
            /*
            List<byte> data = new List<byte>();
            while (!reader.EndOfStream)
                data.Add((byte)reader.Read());
            MessageBox.Show(data.Count.ToString());
             * */

        }

        public static string GetFileName(string url)
        {
            return url.Substring(url.LastIndexOf("/")+1);
        }
        public static void CompleteUrls(List<AldUrlTools.UrlInfo> res, string url)
        {
            url = QuitLastPart(url);
            if (!url.EndsWith("/")) url += "/";
            foreach (var i in res)
                if (!i.Url.StartsWith("http") && !i.Url.StartsWith("https") && !i.Url.StartsWith("www."))
                    i.Url = url + i.Url;
        }

        private static string QuitLastPart(string url)
        {
            string[] invalid = { "htm", "html", "php", "jsp", "asp", "aspx", "xhtml" };
            foreach (var i in invalid)
            {
                if (url.EndsWith("." + i))
                {
                    int pos = url.LastIndexOf("/");
                    return url.Substring(0,pos);
                }
            }
            return url;
        }
        public static List<UrlInfo> SearchUrls(string html)
        {
            List<UrlInfo> urllist = new List<UrlInfo>();
            while ((html = html.AldJump("<a ")) != null)
            {
                string info = html.AldJump(">");
                info = info.AldTake("</a>");
                string url;
                if (info != null)
                {
                    html = html.AldJump("href=\"");
                    url = html.AldTake("\"");
                    if(url.Length>0 && url[0]!='\'')
                        urllist.Add(new UrlInfo(info, url));
                }
            }
            return urllist;
        }
    }
}
