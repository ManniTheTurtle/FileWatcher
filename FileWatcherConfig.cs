using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;

namespace FileWatcher
{
    [Serializable]
    public class FileWatcherConfig
    {
        public FileWatcherConfig()
        {

        }

        private int timerIntervallTime = 60;
        [DisplayName("Timer Intervall")]
        [Description("Zeitspanne für die Updateprüfung in Sekunden. Gilt für alle Dateien.")]
        [Category("General Settings")]
        public int TimerIntervallTime
        {
            get 
            {
                if (timerIntervallTime < 1)
                {
                    timerIntervallTime = 10;
                }
                return timerIntervallTime; }
            set 
            { 
                timerIntervallTime = value; 
            }
        }

        private double bugFixingCheckIntervallTime = 12;
        [DisplayName("FehlerbehebungsTimer Intervall")]
        [Description("Zeitspanne für die einmalige Prüfung auf Fehlerbehebung in Stunden. Gilt für alle Dateien.")]
        [Category("General Settings")]
        public double BugFixingCheckIntervallTime
        {
            get
            {
                if (bugFixingCheckIntervallTime < 0.1)
                {
                    bugFixingCheckIntervallTime = 0.1;
                }
                return bugFixingCheckIntervallTime;
            }
            set
            {
                bugFixingCheckIntervallTime = value;
            }
        }

        private List<SingleFile> filestowatch;
        [Browsable(false)]
        public List<SingleFile> FilesToWatch
        {
            get 
            {
                if (filestowatch == null)
                {
                    filestowatch = new List<SingleFile>();
                }
                return filestowatch; 
            }
            set 
            { filestowatch = value; }
        }

        private static FileWatcherConfig instance;
        public static FileWatcherConfig Instance()
        {
            if (instance == null)
            {
                instance = new FileWatcherConfig();
            }
            return instance;
        }

        [DisplayName("Absender Email Adresse")]
        [Category("Email Properties")]
        public string AbsenderEmail { get; set; }

        [DisplayName("Absender Name")]
        [Category("Email Properties")]
        public string AbsenderName { get; set; }

        [DisplayName("Empfänger Email Adresse")]
        [Description("Primärempfänger (Pflichtfeld)")]
        [Category("Email Properties")]
        public string EmpfaengerEmail { get; set; }

        private BindingList<string> weitereempfaengeremail;

        [Browsable(true)]
        [DisplayName("Weitere Empfänger Email Adresse")]
        [Description("Zusätziche Empfänger (Optional)")]
        [Category("Email Properties")]
        public BindingList<string> WeitereEmpfaengerEmail
        { 
            get
            {
                if (weitereempfaengeremail == null)
                {
                    weitereempfaengeremail = new BindingList<string>();
                }
                return weitereempfaengeremail;
            }
            set
            {
                weitereempfaengeremail = value;
            }
        }


        [DisplayName("SMTP Host")]
        [Description("Postausgangsserver des Mail Providers")]
        [Category("Email Properties")]
        public string SMTP_Host { get; set; }

        [DisplayName("SMTP Port")]
        [Category("Email Properties")]
        public int SMTP_Port { get; set; }

        [DisplayName("Username")]
        [Category("Email Properties")]
        public string Username { get; set; }

        [DisplayName("Passwort")]
        [Category("Email Properties"), PasswordPropertyText(true)]
        public string Password { get; set; }

        private string subject;

        [DisplayName("Email Betreff")]
        [Category("Email Properties")]
        public string Subject 
        { 
            get
            {
                return subject; 
            }
            set 
            { 
                subject = value;
            }
        }

        private string body;

        [DisplayName("Email Text")]
        [Category("Email Properties")]
        public string Body
        {
            get
            {
                return body;
            }
            set
            {
                body = value;
            }
        }

        private bool ssl;
        [DisplayName("Verbindungstyp SSL")]
        [Category("Email Properties")]
        public bool SSL 
        { 
            get
            {
                return ssl;
            }
            set
            {
                ssl = value;
            }
        }

        [ReadOnly(true)]
        [DisplayName("Speicherpfad für Config Datei")]
        [Category("General Settings")]
        public string FilePathForConfigValues { get { return AppDomain.CurrentDomain.BaseDirectory + "config.json"; } }

        private string filepathforfilesdata;
        [DisplayName("Speicherpfad für FilesListe mit Errorlog")]
        [Category("General Settings")]
        [Description("Liste zu überwachender Dateien inklusive ÜberwachungsKriterien und ErrorLogs werden hier gespeichert")]
        public string FilePathForFilesData
        {
            get
            {
                if (string.IsNullOrEmpty(filepathforfilesdata))
                    filepathforfilesdata = AppDomain.CurrentDomain.BaseDirectory + "filestowatch.json";
                return filepathforfilesdata;
            }
            set
            {
                filepathforfilesdata = value;
            }
        }

        public int GetIntervallinMilliseconds()
        {
            int intervall = TimerIntervallTime * 1000;
            return intervall;
        }
    }
}
