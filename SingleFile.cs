using System;
using System.Collections.Generic;

namespace FileWatcher
{
    public enum NotifyMode
    {
        Changed, NotConstantlyUpdated
    }

    [Serializable]
    public class SingleFile : IDisposable
    {
        public bool ErrorToSolve { get; set; }

        public string GUID { get; set; }

        public string FileName { get; set; }

        public string FilePath { get; set; }

        private NotifyMode notifywhen;

        public NotifyMode NotifyWhen
        {
            get { return notifywhen; }
            set 
            {
                if (value == NotifyMode.Changed)
                {
                    UpdateCycle = 0;
                }
                notifywhen = value; 
            }
        }

        private DateTime latestupdate;

        public DateTime LatestUpdate
        {
            get 
            {
                if (latestupdate == null)
                {
                    latestupdate = DateTime.Now;
                }
                return latestupdate; 
            }
            set { latestupdate = value; }
        }

        public bool UnderSurveillance { get; set; }

        public int UpdateCycle { get; set; }

        private TimeSpan timePastSinceLastUpdate;
        public TimeSpan TimePastSinceLastUpdate 
        {   
            get
            {
                timePastSinceLastUpdate = DateTime.Now - LatestUpdate;
                return timePastSinceLastUpdate;
            }
            set
            {
                timePastSinceLastUpdate = value;
            }
        }

        public List<SingleIssue> Log { get; set; }

        public SingleFile()
        {
            if (string.IsNullOrWhiteSpace(GUID))
            {
                GUID = Guid.NewGuid().ToString("N").Substring(0, 10);
            }

            if (Log == null)
            {
                Log = new List<SingleIssue>();
            }
        }

        public void Dispose()
        { 
            this.Dispose(); 
        }
    }

}
