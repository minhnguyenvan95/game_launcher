using ICSharpCode.SharpZipLib.Zip;
using Launcher_RanVietnam_New.Properties;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace Launcher_RanVietnam_New
{
	public class frmMain : Form
	{
		private delegate void SetTextCallback(Label lblLabel, string strText);

		private delegate void SetEnableCallback(TextBox txtTextbox, bool bStatus);

		private delegate void SetButtonEnableCallback(Button btnButton, bool bStatus);

		private delegate void SetProgressbarPercentCallback(ProgressBar progressBar, int iValue);

        private string RAN_SERVER = "http://daithanh-update.vinaran.com/";

        private string HOMEPAGE_SERVER = "http://daithanh.vinaran.com/";

		private string strVersion = "";

		private string strFullPatch = "";

		private string strPatchURL = "";

		private string strCurrentPatchFirst = "";

		private string[] strPatch;

		private bool updateStatus = true;

		private bool needUpdate = true;

		private string strUnzipStatus = "";

		private IContainer components = null;

		private Button btnLaunch;

		private BackgroundWorker backgroundWorker1;

		private ProgressBar progressBarDownload;

		private Label lblProgress;

		private WebBrowser webBrowser;

		private Button btnExit;

		private Button btnRegister;

		private Label label1;

		public frmMain()
		{
			this.InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			this.loadParam();
			this.checkLauncherVersion();
		}

		private void loadParam()
		{
			try
			{
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.Load(Environment.CurrentDirectory + "\\sounds\\sfx\\02000222.xml");
				this.strVersion = xmlDocument.GetElementsByTagName("Version").Item(0).InnerText;
				this.strCurrentPatchFirst = xmlDocument.GetElementsByTagName("CurrentPatch").Item(0).InnerText;
			}
			catch
			{
				MessageBox.Show("Cấu hình client không hợp lệ, vui lòng vào trang chủ download bản client mới nhất và thử lại !!");
				this.updateStatus = false;
				Application.Exit();
			}
			try
			{
                this.strPatchURL = this.RAN_SERVER + "version/" + "version" + this.strVersion + ".html";
				this.strFullPatch = this.GetWebContent(this.strPatchURL);
				this.strPatch = this.strFullPatch.Split(new char[]
				{
					','
				});
				this.webBrowser.Navigate(this.RAN_SERVER + "homepage.php");
                //this.webBrowser.Navigate("http://112.213.84.26/");
                this.webBrowser.Navigating += new WebBrowserNavigatingEventHandler(this.webBrowser_Navigating);               
			}
			catch (Exception var_1_E8)
			{
				MessageBox.Show("Đường dẫn trang chủ không đúng, vui lòng download bản update mới nhất và thử lại !!");
				this.updateStatus = false;
			}
            
		}

		private void checkLauncherVersion()
		{
			string webContent = this.GetWebContent(this.RAN_SERVER + "version/launcher_setting.html");
			if (webContent.Trim() == this.strVersion)
			{
				this.backgroundWorker1.RunWorkerAsync();
			}
			else
			{
				try
				{
					new Process
					{
						StartInfo = 
						{
							FileName = "Repatcher.exe"
						}
					}.Start();
				}
				catch
				{
					MessageBox.Show("Game client thiếu file hoặc thư mục chứa không đủ quyền, vui lòng download Game client mới hoặc liên hệ với BQT để được trợ giúp !");
				}
			}
		}

		private void saveParam(string strCurrentPatch)
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
			xmlNode4.InnerText = strCurrentPatch;
			xmlNode2.AppendChild(xmlNode3);
			xmlNode2.AppendChild(xmlNode4);
			xmlDocument.DocumentElement.AppendChild(xmlNode2);
			xmlDocument.Save(Environment.CurrentDirectory + "\\sounds\\sfx\\02000222.xml");
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
			catch (Exception ex)
			{
				MessageBox.Show("Không thể kết nối đến server - Vui lòng kiểm tra lại đường truyền. ");
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
            catch (Exception ex) {
                return "false";
            }
            return "true";
            
        }

        public void gotoSite(string url)
        {
            System.Diagnostics.Process.Start(url);
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
				httpWebResponse.Close();
                
                if (httpWebResponse.StatusCode != System.Net.HttpStatusCode.OK)
                    return false;

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
				result = true;
			}
			catch (Exception var_18_15C)
			{
				result = false;
			}
			return result;
		}

		private void launchGame()
		{
			if (this.btnLaunch.Enabled)
			{
                try
                {
                    new Process
                    {
                        StartInfo =
                        {
                            FileName = "game.exe",
                            Arguments = "/app_run"
                        }
                    }.Start();
                }
                catch (Exception ex) {
                    MessageBox.Show("Không tìm thấy game.exe");
                }
				base.Close();
			}
		}

		private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
		{
			if (int.Parse(this.strCurrentPatchFirst) < int.Parse(this.strPatch[this.strPatch.Length - 1].Trim()))
			{
				this.btnLaunch.Enabled = false;
				for (int i = int.Parse(this.strCurrentPatchFirst) + 1; i <= int.Parse(this.strPatch[this.strPatch.Length - 1].Trim()); i++)
				{
					this.SetText(this.lblProgress, string.Concat(new object[]
					{
						"Updating Patch ",
						i,
						"/",
						this.strPatch[this.strPatch.Length - 1].Trim()
					}));
					try
					{
                        if (this.downloadPatch(this.RAN_SERVER + "Patch/Patch" + this.strPatch[i].Trim() + ".zip"))
                        {
                            this.strUnzipStatus = this.Unzipfile("Patch" + this.strPatch[i].Trim() + ".zip", Environment.CurrentDirectory + "\\");
                            if (this.strUnzipStatus == "false")
                            {
                                MessageBox.Show("Giải nén Patch " + i + " thất bại, vui lòng tắt hết tất cả các chương trình hoặc file đang mở trong Game Client hoặc restart lại máy và thử lại, nếu vẫn không update được, vui lòng vào trang chủ download trực tiếp phiên bản update mới nhất!");
                                this.updateStatus = false;
                                break;
                            }
                            this.saveParam(this.strPatch[i].Trim());
                            File.Delete("Patch" + this.strPatch[i].Trim() + ".zip");
                        }
                        else
                        {
                            this.updateStatus = false;
                            MessageBox.Show("Không thể tải file Patch"+i+".zip\nVui lòng liên hệ Admin để được hỗ trợ.");
                            return;
                        }
					}
					catch (Exception var_1_185)
					{
						this.updateStatus = false;
						MessageBox.Show("Update Patch " + i + " thất bại, vui lòng kiểm tra lại đường truyền internet hoặc restart lại máy và thử lại, nếu vẫn không update được, vui lòng vào trang chủ download trực tiếp phiên bản update mới nhất!");
						break;
					}
				}
				this.SetText(this.lblProgress, "Cập nhật hoàn tất!");
			}
			else
			{
				this.SetButtonEnable(this.btnLaunch, true);
				this.needUpdate = false;
			}
		}

		private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			this.progressBarDownload.Value = e.ProgressPercentage;
		}



		private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (this.updateStatus && this.needUpdate)
			{
				MessageBox.Show("Update hoàn tất, chúc bạn có một buổi chơi game thật vui vẻ :)");
				this.btnLaunch.Enabled = true;
				this.btnLaunch.Image = Resources.button_startgame;
                this.progressBarDownload.Value = 100;
			}
			else if (this.updateStatus && !this.needUpdate)
			{
				this.SetText(this.lblProgress, "Tình Trạng : Rất Tốt");
				this.btnLaunch.Image = Resources.button_startgame;
				this.progressBarDownload.Value = 100;
			}
			else
			{
				Application.Exit();
			}
		}

        private void webBrowser_Navigating(object sender, WebBrowserNavigatingEventArgs e){
            e.Cancel = true;
            gotoSite(e.Url.ToString());
        }

		private void webBrowser_ProgressChanged(object sender, WebBrowserProgressChangedEventArgs e)
		{

		}

		private void btnRegister_Click(object sender, EventArgs e)
		{
            gotoSite(HOMEPAGE_SERVER + "register.php");
		}

		private void btnLaunch_Click(object sender, EventArgs e)
		{
			this.launchGame();
		}

		private void btnExit_Click(object sender, EventArgs e)
		{
			base.Close();
		}

		private void frmMain_KeyPress(object sender, KeyPressEventArgs e)
		{
		}

		private void frmMain_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Return)
			{
				this.launchGame();
			}
		}

		private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
		{
			Application.Exit();
		}

		private void progressBarDownload_Click(object sender, EventArgs e)
		{
		}

		public void SetText(Label lblLabel, string strText)
		{
			if (lblLabel.InvokeRequired)
			{
				frmMain.SetTextCallback method = new frmMain.SetTextCallback(this.SetText);
				base.Invoke(method, new object[]
				{
					lblLabel,
					strText
				});
			}
			else
			{
				lblLabel.Text = strText;
			}
		}

		public void SetEnable(TextBox txtTextbox, bool bStatus)
		{
			if (txtTextbox.InvokeRequired)
			{
				frmMain.SetEnableCallback method = new frmMain.SetEnableCallback(this.SetEnable);
				base.Invoke(method, new object[]
				{
					txtTextbox,
					bStatus
				});
			}
			else
			{
				txtTextbox.Enabled = bStatus;
			}
		}

		public void SetButtonEnable(Button btnButton, bool bStatus)
		{
			if (btnButton.InvokeRequired)
			{
				frmMain.SetButtonEnableCallback method = new frmMain.SetButtonEnableCallback(this.SetButtonEnable);
				base.Invoke(method, new object[]
				{
					btnButton,
					bStatus
				});
			}
			else
			{
				btnButton.Enabled = bStatus;
			}
		}

		public void SetProgressbarPercent(ProgressBar progressBar, int iValue)
		{
			if (progressBar.InvokeRequired)
			{
				frmMain.SetProgressbarPercentCallback method = new frmMain.SetProgressbarPercentCallback(this.SetProgressbarPercent);
				base.Invoke(method, new object[]
				{
					progressBar,
					iValue
				});
			}
			else
			{
				progressBar.Value = iValue;
			}
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
			ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(frmMain));
			this.backgroundWorker1 = new BackgroundWorker();
			this.progressBarDownload = new ProgressBar();
			this.lblProgress = new Label();
			this.webBrowser = new WebBrowser();
			this.btnExit = new Button();
			this.btnRegister = new Button();
			this.btnLaunch = new Button();
			this.label1 = new Label();
			base.SuspendLayout();
			this.backgroundWorker1.WorkerReportsProgress = true;
			this.backgroundWorker1.DoWork += new DoWorkEventHandler(this.backgroundWorker1_DoWork);
			this.backgroundWorker1.ProgressChanged += new ProgressChangedEventHandler(this.backgroundWorker1_ProgressChanged);
			this.backgroundWorker1.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.backgroundWorker1_RunWorkerCompleted);
			this.progressBarDownload.Location = new Point(668, 554);
			this.progressBarDownload.Name = "progressBarDownload";
			this.progressBarDownload.Size = new Size(162, 4);
			this.progressBarDownload.TabIndex = 1;
			this.progressBarDownload.Click += new EventHandler(this.progressBarDownload_Click);
			this.lblProgress.BackColor = Color.Transparent;
			this.lblProgress.FlatStyle = FlatStyle.Flat;
			this.lblProgress.Font = new Font("Microsoft Sans Serif", 9f, FontStyle.Bold, GraphicsUnit.Point, 0);
			this.lblProgress.ForeColor = Color.White;
			this.lblProgress.Location = new Point(473, 559);
			this.lblProgress.Name = "lblProgress";
			this.lblProgress.Size = new Size(428, 20);
			this.lblProgress.TabIndex = 2;
			this.lblProgress.Text = "";
			this.lblProgress.TextAlign = ContentAlignment.MiddleCenter;
			this.webBrowser.Location = new Point(400, 110);
			this.webBrowser.MinimumSize = new Size(20, 20);
			this.webBrowser.Name = "webBrowser";
			this.webBrowser.Size = new Size(415, 248);
			this.webBrowser.TabIndex = 3;
			this.webBrowser.TabStop = false;
            this.webBrowser.IsWebBrowserContextMenuEnabled = false;
            this.webBrowser.AllowWebBrowserDrop = false;
			this.webBrowser.ProgressChanged += new WebBrowserProgressChangedEventHandler(this.webBrowser_ProgressChanged);
            
			this.btnExit.BackColor = Color.Transparent;
			this.btnExit.FlatStyle = FlatStyle.Flat;
			this.btnExit.Font = new Font("Microsoft Sans Serif", 9f, FontStyle.Bold, GraphicsUnit.Point, 0);
			this.btnExit.ForeColor = Color.White;
			this.btnExit.Location = new Point(1028, 568);
			this.btnExit.Name = "btnExit";
			this.btnExit.Size = new Size(56, 26);
			this.btnExit.TabIndex = 0;
			this.btnExit.TabStop = false;
			this.btnExit.Text = "Close";
			this.btnExit.UseVisualStyleBackColor = false;
			this.btnExit.Click += new EventHandler(this.btnExit_Click);
            
			this.btnRegister.BackColor = Color.Transparent;
			this.btnRegister.BackgroundImage = (Image)componentResourceManager.GetObject("btnRegister.BackgroundImage");
			this.btnRegister.BackgroundImageLayout = ImageLayout.None;
			this.btnRegister.FlatAppearance.BorderSize = 0;
			this.btnRegister.FlatStyle = FlatStyle.Flat;
			this.btnRegister.Font = new Font("Microsoft Sans Serif", 11f, FontStyle.Bold, GraphicsUnit.Point, 0);
			this.btnRegister.ForeColor = Color.FromArgb(64, 64, 64);
			this.btnRegister.Location = new Point(528, 516);
			this.btnRegister.Name = "btnRegister";
			this.btnRegister.Size = new Size(135, 45);
			this.btnRegister.TabIndex = 0;
			this.btnRegister.TabStop = false;
			this.btnRegister.Text = "Đăng &Ký";
			this.btnRegister.UseVisualStyleBackColor = false;
            this.btnRegister.Cursor = Cursors.Hand;
            this.btnRegister.FlatAppearance.MouseOverBackColor = this.btnRegister.BackColor;
            this.btnRegister.Click += new EventHandler(this.btnRegister_Click);

			this.btnLaunch.BackgroundImage = Resources.button_startgame_gray;
			this.btnLaunch.Cursor = Cursors.Hand;
			this.btnLaunch.Font = new Font("Times New Roman", 15f, FontStyle.Regular, GraphicsUnit.Point, 0);
			this.btnLaunch.Location = new Point(668, 516);
			this.btnLaunch.Name = "btnLaunch";
			this.btnLaunch.Size = new Size(162, 40);
			this.btnLaunch.TabIndex = 0;
			this.btnLaunch.UseVisualStyleBackColor = true;
			this.btnLaunch.Click += new EventHandler(this.btnLaunch_Click);
			this.label1.AutoSize = true;
			this.label1.BackColor = Color.Transparent;
			this.label1.Font = new Font("Microsoft Sans Serif", 6f, FontStyle.Bold, GraphicsUnit.Point, 0);
			this.label1.ForeColor = Color.DimGray;
			this.label1.Location = new Point(802, 66);
			this.label1.Name = "label1";
			this.label1.Size = new Size(12, 27);
			this.label1.TabIndex = 4;
			this.label1.Text = "R\r\nV\r\nN";
			base.AutoScaleDimensions = new SizeF(6f, 13f);
			base.AutoScaleMode = AutoScaleMode.Font;
			this.BackColor = Color.FromArgb(150, 207, 255);
            this.BackgroundImage = Resource1.DAITHANH;
			this.BackgroundImageLayout = ImageLayout.None;
			base.ClientSize = new Size(1146, 739);
			base.ControlBox = false;
			base.Controls.Add(this.label1);
			base.Controls.Add(this.webBrowser);
			base.Controls.Add(this.lblProgress);
			base.Controls.Add(this.progressBarDownload);
			base.Controls.Add(this.btnRegister);
			base.Controls.Add(this.btnExit);
			base.Controls.Add(this.btnLaunch);
			this.DoubleBuffered = true;
			base.FormBorderStyle = FormBorderStyle.None;
			base.Icon = (Icon)componentResourceManager.GetObject("$this.Icon");
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.Name = "frmMain";
			base.StartPosition = FormStartPosition.CenterScreen;
			this.Text = "VinaRan Launcher";
			base.TransparencyKey = Color.FromArgb(150, 207, 255);
			base.FormClosing += new FormClosingEventHandler(this.frmMain_FormClosing);
			base.Load += new EventHandler(this.Form1_Load);
			base.KeyDown += new KeyEventHandler(this.frmMain_KeyDown);
			base.KeyPress += new KeyPressEventHandler(this.frmMain_KeyPress);
			base.ResumeLayout(false);
			base.PerformLayout();
		}
	}
}
