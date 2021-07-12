using DevExpress.XtraEditors;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileWatcher
{
    public partial class Config : DevExpress.XtraEditors.XtraForm
    {
        public Config()
        {
            InitializeComponent();

            propertyGrid1.SelectedObject = FileWatcherConfig.Instance();
        }

        private static Config instance;
        public static Config Instance()
        {
            if (instance == null)
            {
                instance = new Config();
            }
            return instance;
        }

        private void simpleButton1_Click(object sender, EventArgs e)    // Save
        {
            string json = JsonConvert.SerializeObject(FileWatcherConfig.Instance().FilesToWatch, Formatting.Indented);
            File.WriteAllText(FileWatcherConfig.Instance().FilePathForFilesData, json);

            json = JsonConvert.SerializeObject(FileWatcherConfig.Instance(), Formatting.Indented);
            File.WriteAllText(FileWatcherConfig.Instance().FilePathForConfigValues, json);
        }
    }
}