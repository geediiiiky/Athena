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
        public bool With2017 = false;
        public bool SkipShaderCompiler = false;
        public bool SkipFrontend = false;
        public bool SkipLightmass = false;
        public bool BuildDebugGame = false;
        public bool StartServer = false;
        public bool StartServerDebug = false;
        public bool StartGame = false;
        public bool StartGameDebug = false;
        public int GameInstances = 1;
        public HMDTypes HmdType = HMDTypes.Default;

        [ScriptIgnore]
        public Dictionary<HMDTypes, string> HmdTypeStrings = new Dictionary<HMDTypes, string>
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
        public Process GitAutoFetchProcess;
        public Dictionary<AthenaConfig.HMDTypes, RadioButton> HmdTypes = new Dictionary<AthenaConfig.HMDTypes, RadioButton> { };


        public MainWindow()
        {
            InitializeComponent();

            HmdTypes.Add(AthenaConfig.HMDTypes.NoHmd, NoHmd);
            HmdTypes.Add(AthenaConfig.HMDTypes.Default, DefaultHmd);
            HmdTypes.Add(AthenaConfig.HMDTypes.GoogleVR, GoogleVR);
            HmdTypes.Add(AthenaConfig.HMDTypes.Occulus, Occulus);
            HmdTypes.Add(AthenaConfig.HMDTypes.SteamVR, SteamVR);

            Load();

            // generate project
            WithVS2017.IsChecked = config.With2017;

            // build
            BuildWithVS2017.IsChecked = config.With2017;
            SkipShaderCompiler.IsChecked = config.SkipShaderCompiler;
            SkipFrontend.IsChecked = config.SkipFrontend;
            SkipLightmass.IsChecked = config.SkipLightmass;
            DebugGame.IsChecked = config.BuildDebugGame;

            // run
            StartServer.IsChecked = config.StartServer;
            StartServerDebug.IsChecked = config.StartServerDebug;
            StartGame.IsChecked = config.StartGame;
            StartGameDebug.IsChecked = config.StartGameDebug;
            GameInstances.Text = config.GameInstances.ToString();
            HmdTypes[config.HmdType].IsChecked = true;
        }

        private Process ExecuteCommand(string command, bool createNoWindow = false, bool dontWaitForExit = false)
        {
            Console.WriteLine(command);
            var scriptName = command;
            var scriptLength = command.IndexOf(" ");
            if (scriptLength > 0)
            {
                scriptName = command.Substring(0, scriptLength);
            }
            var scriptFound = false;
            if (File.Exists(scriptName))
            {
                scriptFound = true;
            }
            else
            {
                string[] folders = { @"script\", @"scripts\" };
                foreach (var folder in folders)
                {
                    if (File.Exists(folder + scriptName))
                    {
                        command = folder + command;
                        scriptFound = true;
                        break;
                    }
                }
            }

            if (!scriptFound)
            {
                Console.WriteLine("Script {0} cannot be found!!", scriptName);
                return null;
            }

            var processInfo = new ProcessStartInfo("cmd.exe", "/c " + command);
            processInfo.CreateNoWindow = createNoWindow;
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

            if (!dontWaitForExit)
            {
                process.WaitForExit();
                Console.WriteLine("ExitCode: {0}", process.ExitCode);
                process.Close();
            }
            else
            {
                process.EnableRaisingEvents = true;
                process.Exited += (object sender, System.EventArgs e) =>
                {
                    process.Close();
                };
            }

            return process;
        }

        private void GenerateProjectButton_Click(object sender, RoutedEventArgs e)
        {
            config.With2017 = WithVS2017.IsChecked == true;
            Save();

            if (!config.With2017)
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
            config.With2017 = BuildWithVS2017.IsChecked == true;
            config.SkipShaderCompiler = SkipShaderCompiler.IsChecked == true;
            config.SkipFrontend = SkipFrontend.IsChecked == true;
            config.SkipLightmass = SkipLightmass.IsChecked == true;
            config.BuildDebugGame = DebugGame.IsChecked == true;
            Save();

            string command = "BuildEditor.bat";
            if (config.With2017)
            {
                command += " -2017";
            }
            if (config.SkipShaderCompiler)
            {
                command += " --no-shader-compiler";
            }
            if (config.SkipFrontend)
            {
                command += " --no-frontend";
            }
            if (config.SkipLightmass)
            {
                command += " --no-lightmass";
            }
            if (config.BuildDebugGame)
            {
                command += " --debug-game";
            }
            ExecuteCommand(command);
        }

        private void Run_Click(object sender, RoutedEventArgs e)
        {
            config.StartServer = StartServer.IsChecked == true;
            config.StartServerDebug = StartServerDebug.IsChecked == true;

            config.StartGame = StartGame.IsChecked == true;
            config.StartGameDebug = StartGameDebug.IsChecked == true;
            config.GameInstances = 1;
            Int32.TryParse(GameInstances.Text, out config.GameInstances);

            config.HmdType = AthenaConfig.HMDTypes.Default;
            foreach (KeyValuePair<AthenaConfig.HMDTypes, RadioButton> entry in HmdTypes)
            {
                if (entry.Value.IsChecked == true)
                {
                    config.HmdType = entry.Key;
                    break;
                }
            }

            Save();

            if (config.StartServer)
            {
                string command = "RunAssociatedEngine.cmd -log -server";
                if (config.StartGameDebug)
                {
                    command += " -debug";
                }
                ExecuteCommand(command, true);
            }

            if (config.StartGame)
            {
                for (int i = 0; i < config.GameInstances; ++i)
                {
                    string command = "RunAssociatedEngine.cmd -log -game -resx=1280 -resy=720 -windowed";
                    if (config.StartGameDebug)
                    {
                        command += " -debug";
                    }
                    command += " ";
                    command += config.HmdTypeStrings[config.HmdType];
                    ExecuteCommand(command, true);
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
                    {
                        reader.Close();
                    }
                }
            }
        }

        private void WithVS2017_Checked(object sender, RoutedEventArgs e)
        {
            BuildWithVS2017.IsChecked = WithVS2017.IsChecked;
        }

        private void WithVS2017_Unchecked(object sender, RoutedEventArgs e)
        {
            BuildWithVS2017.IsChecked = WithVS2017.IsChecked;
        }

        private void BuildWithVS2017_Checked(object sender, RoutedEventArgs e)
        {
            WithVS2017.IsChecked = BuildWithVS2017.IsChecked;
        }

        private void BuildWithVS2017_Unchecked(object sender, RoutedEventArgs e)
        {
            WithVS2017.IsChecked = BuildWithVS2017.IsChecked;
        }

        private void SyncGit_Click(object sender, RoutedEventArgs e)
        {
            ExecuteCommand("Sync.bat");
        }

        private void AutoFetch_Click(object sender, RoutedEventArgs e)
        {
            if (GitAutoFetchProcess == null)
            {
                GitAutoFetchProcess = ExecuteCommand("FrequentlyFetch.bat", false, true);
                GitAutoFetchProcess.Exited += (object _sender, System.EventArgs _e) =>
                {
                    GitAutoFetchProcess = null;
                    this.Dispatcher.Invoke(() =>
                    {
                        AutoFetch.IsEnabled = true;
                    });
                    
                };

                AutoFetch.IsEnabled = false;
            }
        }

        private void StartFrontend_Click(object sender, RoutedEventArgs e)
        {
            ExecuteCommand("StartFrontend.bat", true);
        }
    }
}
