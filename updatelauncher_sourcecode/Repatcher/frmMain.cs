using ICSharpCode.SharpZipLib.Zip;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace Repatcher
{
	public class frmMain : Form
	{
        private string RAN_SERVER = "http://tanvuong-update.vinaran.com";

		private string strVersion = "";

		private string strFullPatch = "";

		private string strPatchURL = "";

		private string strCurrentPatchFirst = "0";

		private string[] strPatch;

		private bool updateStatus = true;

		private bool needUpdate = true;

		private string strUnzipStatus = "";

		private IContainer components = null;

		private BackgroundWorker backgroundWorker1;

		private ProgressBar progressBar1;

		private Label label1;

		public frmMain()
		{
			this.InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			this.loadParam();
			this.backgroundWorker1.RunWorkerAsync();
		}

		private void loadParam()
		{
			this.strVersion = this.GetWebContent(this.RAN_SERVER + "/version/launcher_setting.html").Trim();
			try
			{
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.Load(Environment.CurrentDirectory + "\\sounds\\sfx\\02000222.xml");
				this.strCurrentPatchFirst = xmlDocument.GetElementsByTagName("CurrentPatch").Item(0).InnerText;
			}
			catch
			{
                MessageBox.Show("Không thể đọc file cấu hình , Vui lòng tải bản đầy đủ trên trang web.");
                Application.Exit();
			}
		}

		private void saveParam()
		{
			try
			{
				if (File.Exists(Environment.CurrentDirectory + "\\sounds\\sfx\\02000222.xml"))
				{
				}
				XmlDocument xmlDocument = new XmlDocument();
				XmlElement newChild = xmlDocument.CreateElement("Launcher");
				XmlNode xmlNode = xmlDocument.InsertAfter(newChild, null);
				XmlDocumentFragment xmlDocumentFragment = xmlDocument.CreateDocumentFragment();
				XmlNode xmlNode2 = xmlDocument.CreateElement("Config");
				xmlDocumentFragment.InsertAfter(xmlNode2, null);
				XmlNode xmlNode3 = xmlDocument.CreateElement("Version");
				xmlNode3.InnerText = this.strVersion;
				XmlNode xmlNode4 = xmlDocument.CreateElement("CurrentPatch");
				xmlNode4.InnerText = this.strCurrentPatchFirst;
				xmlNode2.AppendChild(xmlNode3);
				xmlNode2.AppendChild(xmlNode4);
				xmlDocument.DocumentElement.AppendChild(xmlNode2);
				xmlDocument.Save(Environment.CurrentDirectory + "\\sounds\\sfx\\02000222.xml");
			}
			catch
			{
				MessageBox.Show("Game client thiếu file hoặc thư mục chứa không đủ quyền, vui lòng download Game client mới hoặc liên hệ với BQT để được trợ giúp !");
				Application.Exit();
			}
		}

		private string GetWebContent(string strLink)
		{
			string result = "";
			try
			{
				WebRequest webRequest = WebRequest.Create(strLink);
				webRequest.Credentials = CredentialCache.DefaultCredentials;
				WebResponse response = webRequest.GetResponse();
				Stream responseStream = response.GetResponseStream();
				StreamReader streamReader = new StreamReader(responseStream, Encoding.UTF8);
				result = streamReader.ReadToEnd();
				response.Close();
				streamReader.Close();
			}
			catch (Exception var_5_50)
			{
				MessageBox.Show("Launcher version hiện tại không thể truy cập đến server, vui lòng vào trang chủ để tải phiên bản Launcher mới nhất !");
				Application.Exit();
			}
			return result;
		}

        private string Unzipfile(string ZipPath, string UnzipPath)
        {
            try
            {
                using (ZipInputStream s = new ZipInputStream(File.OpenRead(ZipPath)))
                {
                    ZipEntry theEntry;
                    while ((theEntry = s.GetNextEntry()) != null)
                    {
                        string directoryName = Path.GetDirectoryName(theEntry.Name);
                        string fileName = Path.GetFileName(theEntry.Name);
                        // create directory
                        if (directoryName.Length > 0)
                        {
                            Directory.CreateDirectory(directoryName);
                        }
                        if (fileName != String.Empty)
                        {
                            using (FileStream streamWriter = File.Create(theEntry.Name))
                            {
                                int size = 2048;
                                byte[] data = new byte[2048];
                                while (true)
                                {
                                    size = s.Read(data, 0, data.Length);
                                    if (size > 0)
                                    {
                                        streamWriter.Write(data, 0, size);
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    s.Close();
                }
            }
            catch (Exception ex)
            {
                
                return "false";
            }
            return "true";

        }

		private bool downloadPatch(string strURL)
		{
			bool result;
			try
			{
				int num = strURL.LastIndexOf('/');
				string str = strURL.Substring(num + 1, strURL.Length - num - 1);
				string path = Environment.CurrentDirectory + "\\" + str;
				Uri requestUri = new Uri(strURL);
				HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(requestUri);
				HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                if (httpWebResponse.StatusCode != HttpStatusCode.OK)
                    return false;
				httpWebResponse.Close();
				long contentLength = httpWebResponse.ContentLength;
				long num2 = 0L;
				using (WebClient webClient = new WebClient())
				{
					using (Stream stream = webClient.OpenRead(new Uri(strURL)))
					{
						using (Stream stream2 = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
						{
							byte[] array = new byte[contentLength];
							int num3;
							while ((num3 = stream.Read(array, 0, array.Length)) > 0)
							{
								stream2.Write(array, 0, num3);
								num2 += (long)num3;
								double num4 = (double)num2;
								double num5 = (double)array.Length;
								double num6 = num4 / num5;
								int percentProgress = (int)(num6 * 100.0);
								this.backgroundWorker1.ReportProgress(percentProgress);
							}
							stream2.Close();
						}
						stream.Close();
					}
				}
			}
			catch (Exception var_18_15A)
			{
				result = false;
				return result;
			}
			result = true;
			return result;
		}

		private void launchLauncher()
		{
			new Process
			{
				StartInfo = 
				{
					FileName = "Launcher.exe"
				}
			}.Start();
			Application.Exit();
		}

		private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
		{
            if (this.downloadPatch(this.RAN_SERVER + "/launcher/launcher" + this.strVersion + ".zip"))
			{
				this.strUnzipStatus = this.Unzipfile("launcher" + this.strVersion + ".zip", Environment.CurrentDirectory + "\\");
				if (this.strUnzipStatus == "false")
				{
					MessageBox.Show("Giải nén thất bại, vui lòng restart máy và thử lại sau, nếu vẫn không thực hiện được, vui lòng vào trang chủ download launcher version mới nhất!");
					Application.Exit();
				}
				File.Delete("launcher" + this.strVersion + ".zip");
			}
			else
			{
				MessageBox.Show("Không thể cập nhật launcher version mới, vui lòng vào trang chủ để download lại launcher!");
				Application.Exit();
			}
			this.saveParam();
			this.launchLauncher();
		}

		private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			this.progressBar1.Value = e.ProgressPercentage;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
            this.MaximizeBox = false;
			this.backgroundWorker1 = new BackgroundWorker();
			this.progressBar1 = new ProgressBar();
			this.label1 = new Label();
			base.SuspendLayout();
			this.backgroundWorker1.WorkerReportsProgress = true;
			this.backgroundWorker1.DoWork += new DoWorkEventHandler(this.backgroundWorker1_DoWork);
			this.backgroundWorker1.ProgressChanged += new ProgressChangedEventHandler(this.backgroundWorker1_ProgressChanged);
			this.progressBar1.Location = new Point(44, 38);
			this.progressBar1.Name = "progressBar1";
			this.progressBar1.Size = new Size(446, 14);
			this.progressBar1.TabIndex = 0;
			this.label1.AutoSize = true;
			this.label1.BackColor = Color.Transparent;
			this.label1.Location = new Point(181, 22);
			this.label1.Name = "label1";
			this.label1.Size = new Size(172, 13);
			this.label1.TabIndex = 1;
			this.label1.Text = "Launcher đang được cập nhật , vui lòng đợi trong giây lát!";
			base.AutoScaleDimensions = new SizeF(6f, 13f);
			base.AutoScaleMode = AutoScaleMode.Font;
			this.BackColor = SystemColors.ButtonHighlight;
			base.ClientSize = new Size(534, 75);
			base.ControlBox = false;
			base.Controls.Add(this.label1);
			base.Controls.Add(this.progressBar1);
			base.FormBorderStyle = FormBorderStyle.FixedSingle;
			base.Name = "Form1";
			base.StartPosition = FormStartPosition.CenterScreen;
			this.Text = "Auto Update";
			base.Load += new EventHandler(this.Form1_Load);
			base.ResumeLayout(false);
			base.PerformLayout();
		}
	}
}
