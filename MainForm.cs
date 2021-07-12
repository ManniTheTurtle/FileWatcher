using DevExpress.XtraEditors;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace FileWatcher
{
    public partial class MainForm : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        #region Variables
        public System.Windows.Forms.Timer timer;
        bool ToolActive;
        int Counter;
        bool emailsuccessfullysent;
        #endregion

        public MainForm()
        {
            InitializeComponent();
        }

        #region Methoden & Events
        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                if (File.Exists(FileWatcherConfig.Instance().FilePathForConfigValues))
                {
                    string jsonstring = File.ReadAllText(FileWatcherConfig.Instance().FilePathForConfigValues);
                    var temp = JsonConvert.DeserializeObject<FileWatcherConfig>(jsonstring);
                    FileWatcherConfig.Instance().AbsenderEmail = temp.AbsenderEmail;
                    FileWatcherConfig.Instance().AbsenderName = temp.AbsenderName;
                    FileWatcherConfig.Instance().Body = temp.Body;
                    FileWatcherConfig.Instance().EmpfaengerEmail = temp.EmpfaengerEmail;
                    FileWatcherConfig.Instance().FilePathForFilesData = temp.FilePathForFilesData;
                    FileWatcherConfig.Instance().Password = temp.Password;
                    FileWatcherConfig.Instance().SMTP_Host = temp.SMTP_Host;
                    FileWatcherConfig.Instance().SMTP_Port = temp.SMTP_Port;
                    FileWatcherConfig.Instance().Subject = temp.Subject;
                    FileWatcherConfig.Instance().Username = temp.Username;
                    FileWatcherConfig.Instance().SSL = temp.SSL;
                    FileWatcherConfig.Instance().WeitereEmpfaengerEmail = temp.WeitereEmpfaengerEmail;
                    FileWatcherConfig.Instance().BugFixingCheckIntervallTime = temp.BugFixingCheckIntervallTime;
                    // FileWatcherConfig.Instance().FilesToWatch = temp.FilesToWatch; // nicht nötig, weil separater Speicherort in FilePathForFilesData

                    jsonstring = File.ReadAllText(FileWatcherConfig.Instance().FilePathForFilesData);
                    FileWatcherConfig.Instance().FilesToWatch = JsonConvert.DeserializeObject<List<SingleFile>>(jsonstring);

                    foreach (var item in FileWatcherConfig.Instance().FilesToWatch)
                    {
                        item.LatestUpdate = DateTime.Now;
                        item.UnderSurveillance = true;

                        FileSystemWatcher watcher = new FileSystemWatcher();

                        watcher.Path = Path.GetDirectoryName(item.FilePath);
                        watcher.Filter = item.FileName;

                        watcher.NotifyFilter = NotifyFilters.LastWrite;

                        watcher.Changed += new FileSystemEventHandler(OnFileChanged);
                        watcher.EnableRaisingEvents = true;
                    }
                }
            }
            catch { }

            gridControl1.DataSource = FileWatcherConfig.Instance().FilesToWatch;

            gridControl1.RefreshDataSource();
            gridView1.Columns.Clear();
            gridView1.PopulateColumns();

            timer = new System.Windows.Forms.Timer();
            timer.Interval = FileWatcherConfig.Instance().GetIntervallinMilliseconds();
            timer.Tick += new System.EventHandler(OnTimerTick);
            timer.Enabled = true;

            ToolActive = true;
            barButtonItem5.Caption = "Stop Watching";
            foreach (var item in FileWatcherConfig.Instance().FilesToWatch.Where(x => x.NotifyWhen == NotifyMode.NotConstantlyUpdated))
            {
                item.LatestUpdate = DateTime.Now;
            }

            labelControl1.Text = "FileWatcher is watching";
            labelControl1.Visible = true;
            labelControl1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            string json = JsonConvert.SerializeObject(FileWatcherConfig.Instance().FilesToWatch, Formatting.Indented);
            File.WriteAllText(FileWatcherConfig.Instance().FilePathForFilesData, json);

            json = JsonConvert.SerializeObject(FileWatcherConfig.Instance(), Formatting.Indented);
            File.WriteAllText(FileWatcherConfig.Instance().FilePathForConfigValues, json);
        }

        private void barButtonItem1_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)  // Choose File
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Wähle Dateien aus";
            ofd.CheckFileExists = true;
            ofd.CheckPathExists = true;
            ofd.Multiselect = true;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                foreach (var item in ofd.FileNames)
                {
                    if (FileWatcherConfig.Instance().FilesToWatch.Where(x => x.FilePath.Equals(item)).Count() == 0)
                    {
                        SingleFile newfiletowatch = new SingleFile();
                        newfiletowatch.FilePath = item;
                        newfiletowatch.FileName = Path.GetFileName(item);
                        newfiletowatch.LatestUpdate = DateTime.Now;
                        newfiletowatch.UnderSurveillance = true;
                        newfiletowatch.UpdateCycle = 0;
                        newfiletowatch.NotifyWhen = NotifyMode.NotConstantlyUpdated;
                        FileWatcherConfig.Instance().FilesToWatch.Add(newfiletowatch);

                        FileSystemWatcher watcher = new FileSystemWatcher();

                        watcher.Path = Path.GetDirectoryName(newfiletowatch.FilePath);
                        watcher.Filter = newfiletowatch.FileName;

                        watcher.NotifyFilter = NotifyFilters.LastWrite;
                        //| NotifyFilters.FileName       // wird benötigt um deleted event zu triggern
                        //| NotifyFilters.LastAccess     // zuletzt gelesen
                        //| NotifyFilters.Size

                        watcher.Changed += new FileSystemEventHandler(OnFileChanged);   // es gibt auch weitere events um zu checken ob die Datei gelöscht oder umbenannt... wurde
                        watcher.EnableRaisingEvents = true;
                    }
                }
            }
            gridControl1.RefreshDataSource();
            gridView1.Columns.Clear();
            gridView1.PopulateColumns();
        }

        private void barButtonItem2_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)  // Edit Config
        {
            Config config = new Config();
            config.StartPosition = FormStartPosition.CenterScreen;
            config.Show();
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)    // File has changed
        {
            if (e.ChangeType != WatcherChangeTypes.Changed)
            { return; }

            if (ToolActive == false)
            { return; }

            var fileToUpdate = FileWatcherConfig.Instance().FilesToWatch.Where(x => x.FilePath.Equals(e.FullPath)).FirstOrDefault();

            if (fileToUpdate == null)
            { return; }

            fileToUpdate.LatestUpdate = DateTime.Now;

            if (fileToUpdate.NotifyWhen == NotifyMode.Changed && fileToUpdate.UnderSurveillance == true)
            {
                SingleIssue newIssue = new SingleIssue();
                newIssue.ErrorDate = DateTime.Now;
                newIssue.ErrorType = Errors.FileHasChanged;
                newIssue.ChosenUpdateCycle = fileToUpdate.UpdateCycle;
                fileToUpdate.Log.Add(newIssue);
                fileToUpdate.LatestUpdate = DateTime.Now;
                fileToUpdate.UnderSurveillance = false;
                SendEmail(fileToUpdate);

                gridControl1.RefreshDataSource();
            }
        }

        private void OnTimerTick(object sender, EventArgs e)    // Timer has ticked
        {
            if (ToolActive == false)
            { return; }

            timer.Enabled = false;
            foreach (var item in FileWatcherConfig.Instance().FilesToWatch.Where(x => x.NotifyWhen == NotifyMode.NotConstantlyUpdated))
            {
                if (item != null)
                {
                    if (item.UnderSurveillance == true && item.UpdateCycle > 0 && item.TimePastSinceLastUpdate.TotalMinutes > item.UpdateCycle)
                    {
                        SingleIssue newIssue = new SingleIssue();
                        newIssue.ErrorDate = DateTime.Now;
                        newIssue.ErrorType = Errors.FileNotUpdating;
                        newIssue.ChosenUpdateCycle = item.UpdateCycle;
                        item.Log.Add(newIssue);
                        SendEmail(item);
                        item.UnderSurveillance = false; // wird in SendEmail Methode abgefragt, kann erst danach false gesetzt werden
                        item.ErrorToSolve = true;
                        if (emailsuccessfullysent == true)
                        {
                            item.Log[item.Log.Count - 1].ErrorTransmitted = true;
                        }

                        gridControl1.RefreshDataSource();
                    }
                    else if (item.ErrorToSolve == true && item.UnderSurveillance == false && item.UpdateCycle > 0 && item.TimePastSinceLastUpdate.TotalMinutes < item.UpdateCycle)
                    {
                        item.ErrorToSolve = false;
                        item.UnderSurveillance = true;
                        gridControl1.RefreshDataSource();
                    }
                    else if (item.ErrorToSolve == true && item.UnderSurveillance == false && item.TimePastSinceLastUpdate.TotalHours > FileWatcherConfig.Instance().BugFixingCheckIntervallTime)
                    {
                        SendEmail(item);
                        if (emailsuccessfullysent == true)
                        {
                            item.ErrorToSolve = false;
                        }
                        gridControl1.RefreshDataSource();
                    }
                }
            }
            timer.Enabled = true;
        }

        private void barButtonItem3_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)  // Clear all Logs
        {
            foreach (var item in FileWatcherConfig.Instance().FilesToWatch)
            {
                item.Log.Clear();
            }
        }

        private void barButtonItem4_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)  // Email Test
        {
            SingleFile testfile = new SingleFile();
            testfile.LatestUpdate = DateTime.Now;
            testfile.NotifyWhen = NotifyMode.Changed;
            testfile.UnderSurveillance = true; 
            testfile.FileName = "TestDatei";
            testfile.FilePath = "TestPfad";
            SingleIssue testissue = new SingleIssue();
            testissue.ErrorDate = DateTime.Now;
            testissue.ErrorType = Errors.FileHasChanged;
            testissue.ChosenUpdateCycle = 123456789;
            testfile.Log.Add(testissue);
            SendEmail(testfile);
            if (emailsuccessfullysent == false)
            {
                MessageBox.Show("Fehler beim Versenden.");
            }
        }

        private void barButtonItem5_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e) // Start/Stop Watching
        {
            StartOrStopWatchingSwitch();
        }

        private void StartOrStopWatchingSwitch()
        {
            if (ToolActive == false)
            {
                ToolActive = true;

                barButtonItem5.Caption = "Stop Watching";

                foreach (var item in FileWatcherConfig.Instance().FilesToWatch.Where(x => x.NotifyWhen == NotifyMode.NotConstantlyUpdated))
                {
                    item.LatestUpdate = DateTime.Now;
                }

                labelControl1.Text = "FileWatcher is watching";
                labelControl1.Visible = true;
                labelControl1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            }
            else if (ToolActive == true)
            {
                ToolActive = false;

                barButtonItem5.Caption = "Start Watching";

                labelControl1.Text = "FileWatcher is deactivated";
                labelControl1.Visible = true;
                labelControl1.ForeColor = Color.Red;
            }
        }
        
        private void SendEmail(SingleFile item)    // Email versenden
        {
            if (String.IsNullOrWhiteSpace(FileWatcherConfig.Instance().Username) || String.IsNullOrWhiteSpace(FileWatcherConfig.Instance().Password) || String.IsNullOrWhiteSpace(FileWatcherConfig.Instance().EmpfaengerEmail))
            {
                StartOrStopWatchingSwitch();
                MessageBox.Show("Email Daten fehlen");
                return;
            }

            emailsuccessfullysent = false;
            Counter = 0;
            Retry:
            Counter++;
            SmtpClient smtp;
            try
            {
                MailMessage mail = new MailMessage();

                mail.From = new MailAddress(FileWatcherConfig.Instance().AbsenderEmail, FileWatcherConfig.Instance().AbsenderName);
                mail.To.Clear();

                mail.Priority = MailPriority.Normal;

                mail.To.Add(new MailAddress(FileWatcherConfig.Instance().EmpfaengerEmail));

                if (FileWatcherConfig.Instance().WeitereEmpfaengerEmail != null && FileWatcherConfig.Instance().WeitereEmpfaengerEmail.Count() > 0)
                {
                    foreach (var email in FileWatcherConfig.Instance().WeitereEmpfaengerEmail)
                    {
                        mail.CC.Add(email);
                    }
                }

                mail.IsBodyHtml = true;
                mail.SubjectEncoding = System.Text.Encoding.UTF8;
                mail.BodyEncoding = System.Text.Encoding.UTF8;

                mail.Subject = FileWatcherConfig.Instance().Subject + $" Datei: {item.FileName}";
                mail.Body = FileWatcherConfig.Instance().Body + $"<br>Regel: {item.NotifyWhen} <br>Dateipfad: {item.FilePath}";

                if (item.NotifyWhen == NotifyMode.Changed)
                {
                    mail.Body += $"<br>Die Datei wurde verändert.";
                }
                else if (item.NotifyWhen == NotifyMode.NotConstantlyUpdated)
                {
                    mail.Body += $"<br>Die Datei wurde nicht regelmäßig geupdated.";
                }
                if (item.UnderSurveillance == false && item.NotifyWhen == NotifyMode.NotConstantlyUpdated)
                {
                    mail.Subject += " (Erinnerung)";
                    mail.Body += $"<br>Das Problem wurde bereits vor {FileWatcherConfig.Instance().BugFixingCheckIntervallTime} Stunden gemeldet und besteht immer noch.";
                }

                if (FileWatcherConfig.Instance().SMTP_Port == 25 | FileWatcherConfig.Instance().SMTP_Port == 0 | FileWatcherConfig.Instance().SMTP_Port == 465)
                {
                    smtp = new SmtpClient();
                    smtp.Host = FileWatcherConfig.Instance().SMTP_Host;
                    smtp.Port = FileWatcherConfig.Instance().SMTP_Port;
                }
                else
                {
                    smtp = new SmtpClient(FileWatcherConfig.Instance().SMTP_Host, 587);
                }

                smtp.EnableSsl = FileWatcherConfig.Instance().SSL;

                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential(FileWatcherConfig.Instance().Username, FileWatcherConfig.Instance().Password);
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.Timeout = 300000;
                smtp.Send(mail);
                mail.Dispose();
                emailsuccessfullysent = true;
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message);
                if (Counter <= 3)
                {
                    Thread.Sleep(6000);
                    goto Retry;
                }
                else
                {
                    MessageBox.Show("Emails Können nicht versand werden.", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
        }
        #endregion
    }
}
