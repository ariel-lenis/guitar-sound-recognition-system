using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.IO;

namespace AldExtractMidiFromUrl
{
    public partial class Form1 : Form
    {
        List<AldUrlTools.UrlInfo> result;
        int downloadpos = 0;
        FolderBrowserDialog fbd;

        public Form1()
        {
            InitializeComponent();
            fbd = new FolderBrowserDialog();
        }

        private void btnGetMidis_Click(object sender, EventArgs e)
        {
    

            string url = txtGetMidis.Text;

            if (!url.Contains("http://") && !url.Contains("https://"))
                url = "http://" + url;

            string urlbase = url.AldJump("//");

            var urlalready = new List<string>();
            result = RecursiveGetUrls(urlbase,url,urlalready).Distinct().ToList();

            foreach (var i in urlalready)
                Console.WriteLine(i);

            gridUrls.DataSource = null;
            gridUrls.DataSource = result;
            this.Text = "Founded:" + result.Count;

            btnDownload.Enabled = true;
            btnGetMidis.Enabled = false;
            txtGetMidis.Enabled = false;
        }

        private List<AldUrlTools.UrlInfo> RecursiveGetUrls(string urlbase,string url,List<string> urlalready=null)
        {
            List<AldUrlTools.UrlInfo> res = new List<AldUrlTools.UrlInfo>();            
            //if (depth == 0) return res;
            if (urlalready == null) urlalready = new List<string>();

            urlalready.Add(url);
            var subres = AldUrlTools.GetUrls(url).Distinct().ToList();

            AldUrlTools.CompleteUrls(subres,url);

            subres = subres.Where(x => x.Url.Contains(urlbase)).ToList();

            foreach (var i in subres)
            {
                
                if (i.Url.Contains(".mid") || i.Url.Contains(".midi"))
                {
                    if (i.Information.Length > 1 && !res.Exists(x=>x.Url==i.Url))
                        res.Add(i);
                }
                else
                {
                    if (!urlalready.Contains(i.Url) && IsValidUrl(i.Url))
                    {
                        urlalready.Add(i.Url);
                        res.AddRange(RecursiveGetUrls(urlbase, i.Url, urlalready));
                    }
                }
                
            }
            return res.Distinct().ToList();
        }

        private bool IsValidUrl(string u)
        {
            if (u.EndsWith(".mid")) throw new Exception("o.O");
            //string[] invalid = {"zip", "mp3", "ogg", "avi", "mp4", "flv", "doc", "pdf","wmv" };
            string[] valid = { "htm", "html", "php", "jsp", "asp", "aspx", "xhtml" };
            foreach (var i in valid)
            {
                if (u.EndsWith("." + i))
                {
                    //Console.WriteLine(u);
                    return true;
                }
                //
            }
            return false;
        }

        private void gridUrls_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void btnDownload_Click(object sender, EventArgs e)
        {
            downloadpos = 0;            
            if (fbd.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

            if (Directory.GetFiles(fbd.SelectedPath).Count() > 0) { MessageBox.Show("Select a empty folder."); return; }

            btnDownload.Enabled = false;

            string res = "";
            foreach (var i in result)
                res += i.Information+"\t"+i.Url+"\n";

            File.WriteAllText(fbd.SelectedPath+"\\resume.txt", res);

            progressBar1.Maximum = result.Count;
            progressBar1.Value = 0;

            DownloadNext();
        }

        private void DownloadNext()
        {
            WebRequest request = WebRequest.Create(result[downloadpos].Url);
            request.BeginGetResponse(new AsyncCallback(ResultNext), request);            
        }
        private void ResultNext(IAsyncResult res)
        {
            try
            {
                var request = res.AsyncState as WebRequest;
                var response = request.EndGetResponse(res);
                var stream = response.GetResponseStream();

                byte[] buffer = new byte[1024];
                string path = fbd.SelectedPath + "\\" + AldUrlTools.GetFileName(result[downloadpos].Url);

                FileStream fs = new FileStream(path, FileMode.OpenOrCreate);
                int bytesreaded;
                do
                {
                    bytesreaded = stream.Read(buffer, 0, buffer.Length);
                    fs.Write(buffer, 0, bytesreaded);
                } while (bytesreaded > 0);

                stream.Dispose();
                response.Dispose();
                fs.Dispose();
            }
            catch { }

            downloadpos++;

            this.Invoke(new Action(delegate {
                progressBar1.Value = downloadpos;
                if (downloadpos == result.Count)
                {
                    MessageBox.Show("End xD");
                    btnGetMidis.Enabled = true;
                    txtGetMidis.Enabled = true;
                    btnDownload.Enabled = false;
                }
            }));
            if (downloadpos == result.Count)
                return;
            DownloadNext();            
        }
             
    }
}
