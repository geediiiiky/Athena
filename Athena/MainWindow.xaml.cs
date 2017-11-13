using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Web.Script.Serialization;
using System.IO;

namespace Athena
{
    public class AthenaConfig
    {
        public enum HMDTypes
        {
            NoHmd,
            Default,
            GoogleVR,
            Occulus,
            SteamVR
        };

        static public string FileName = "athena_config.json";
        public bool _With2017 { get; set; } = false;
        public bool _StartServer = false;
        public bool _StartServerDebug = false;
        public bool _StartGame = false;
        public bool _StartGameDebug = false;
        public int _GameInstances = 1;
        public HMDTypes _HmdType = HMDTypes.Default;

        [ScriptIgnore]
        public Dictionary<HMDTypes, string> _HmdTypeStrings = new Dictionary<HMDTypes, string>
        {
            { HMDTypes.NoHmd, "-nohmd" },
            { HMDTypes.Default, "" },
            { HMDTypes.GoogleVR, "-hmd=SteamVR" },
            { HMDTypes.Occulus, "-hmd=OculusRift" },
            { HMDTypes.SteamVR, "-hmd=googlevrhmd" }
        };
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public AthenaConfig config = new AthenaConfig();
        public Dictionary<AthenaConfig.HMDTypes, RadioButton> _HmdTypes = new Dictionary<AthenaConfig.HMDTypes, RadioButton> { };


        public MainWindow()
        {
            InitializeComponent();

            _HmdTypes.Add(AthenaConfig.HMDTypes.NoHmd, NoHmd);
            _HmdTypes.Add(AthenaConfig.HMDTypes.Default, DefaultHmd);
            _HmdTypes.Add(AthenaConfig.HMDTypes.GoogleVR, GoogleVR);
            _HmdTypes.Add(AthenaConfig.HMDTypes.Occulus, Occulus);
            _HmdTypes.Add(AthenaConfig.HMDTypes.SteamVR, SteamVR);

            Load();

            WithVS2017.IsChecked = config._With2017;

            StartServer.IsChecked = config._StartServer;
            StartServerDebug.IsChecked = config._StartServerDebug;

            StartGame.IsChecked = config._StartGame;
            StartGameDebug.IsChecked = config._StartGameDebug;
            GameInstances.Text = config._GameInstances.ToString();

            _HmdTypes[config._HmdType].IsChecked = true;
        }

        private void ExecuteCommand(string command)
        {
            Console.WriteLine(command);

            var processInfo = new ProcessStartInfo("cmd.exe", "/c " + command);
            processInfo.CreateNoWindow = false;
            processInfo.UseShellExecute = false;
            //processInfo.RedirectStandardError = true;
            //processInfo.RedirectStandardOutput = true;

            var process = Process.Start(processInfo);

            //process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
            //    Console.WriteLine("output>>" + e.Data);
            //process.BeginOutputReadLine();

            //process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
            //    Console.WriteLine("error>>" + e.Data);
            //process.BeginErrorReadLine();

            process.WaitForExit();

            Console.WriteLine("ExitCode: {0}", process.ExitCode);
            process.Close();
        }

        private void GenerateProjectButton_Click(object sender, RoutedEventArgs e)
        {
            config._With2017 = WithVS2017.IsChecked == true;

            if (!config._With2017)
            {
                ExecuteCommand("GenerateProjectFiles.bat");
            }
            else
            {
                ExecuteCommand("GenerateProjectFiles_2017.bat");
            }
        }

        private void BuildEditor_Click(object sender, RoutedEventArgs e)
        {
            ExecuteCommand("BuildEditor.bat");
        }

        private void Run_Click(object sender, RoutedEventArgs e)
        {
            config._StartServer = StartServer.IsChecked == true;
            config._StartServerDebug = StartServerDebug.IsChecked == true;

            config._StartGame = StartGame.IsChecked == true;
            config._StartGameDebug = StartGameDebug.IsChecked == true;
            config._GameInstances = 1;
            Int32.TryParse(GameInstances.Text, out config._GameInstances);

            config._HmdType = AthenaConfig.HMDTypes.Default;
            foreach (KeyValuePair<AthenaConfig.HMDTypes, RadioButton> entry in _HmdTypes)
            {
                if (entry.Value.IsChecked == true)
                {
                    config._HmdType = entry.Key;
                    break;
                }
            }

            Save();

            if (config._StartServer)
            {
                string command = "RunAssociatedEngine.cmd -log -server";
                if (config._StartGameDebug)
                {
                    command += " -debug";
                }
                ExecuteCommand(command);
            }

            if (config._StartGame)
            {
                for (int i = 0; i < config._GameInstances; ++i)
                {
                    string command = "RunAssociatedEngine.cmd -log -game -resx=1280 -resy=720 -windowed";
                    if (config._StartGameDebug)
                    {
                        command += " -debug";
                    }
                    command += " ";
                    command += config._HmdTypeStrings[config._HmdType];
                    ExecuteCommand(command);
                }
            }
        }

        private void Save()
        {
            var json = new JavaScriptSerializer().Serialize(config);
            Console.WriteLine(json);

            TextWriter writer = null;
            try
            {
                writer = new StreamWriter(AthenaConfig.FileName);
                writer.Write(json);
            }
            finally
            {
                if (writer != null)
                    writer.Close();
            }
        }

        private void Load()
        {
            if (File.Exists(AthenaConfig.FileName))
            {
                TextReader reader = null;
                try
                {
                    reader = new StreamReader(AthenaConfig.FileName);
                    var json = reader.ReadToEnd();
                    config = new JavaScriptSerializer().Deserialize<AthenaConfig>(json);
                }
                finally
                {
                    if (reader != null)
                        reader.Close();
                }
            }
        }
    }
}
