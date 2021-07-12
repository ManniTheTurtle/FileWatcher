using System;

namespace FileWatcher
{
    public enum Errors
    {
        FileHasChanged, FileNotUpdating, Other
    }

    [Serializable]
    public class SingleIssue : IDisposable
    {
        public bool ErrorTransmitted { get; set; }

        public Errors ErrorType { get; set; }

        public DateTime ErrorDate { get; set; }

        public int ChosenUpdateCycle { get; set; }

        public void Dispose()
        { this.Dispose(); }
    }

}
