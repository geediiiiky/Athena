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
using System.IO;
using System.Management;

namespace Athena
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public AthenaConfig config = new AthenaConfig();
        public Process GitAutoFetchProcess;
        public Dictionary<HMDTypes, RadioButton> gameHmdType2RadioButton;
        public Dictionary<HMDTypes, RadioButton> editorHmdType2RadioButton;


        public MainWindow()
        {
            InitializeComponent();

            gameHmdType2RadioButton = new Dictionary<HMDTypes, RadioButton> {
                { HMDTypes.NoHmd, GameNoHmd },
                { HMDTypes.Default, GameDefaultHmd },
                { HMDTypes.GoogleVR, GameGoogleVR },
                { HMDTypes.Occulus, GameOcculus },
                { HMDTypes.SteamVR, GameSteamVR }
            };

            editorHmdType2RadioButton = new Dictionary<HMDTypes, RadioButton> {
                { HMDTypes.NoHmd, EditorNoHmd },
                { HMDTypes.Default, EditorDefaultHmd },
                { HMDTypes.GoogleVR, EditorGoogleVR },
                { HMDTypes.Occulus, EditorOcculus },
                { HMDTypes.SteamVR, EditorSteamVR }
            };

            Load();

            // generate project
            WithVS2017.IsChecked = config.userConfig.with2017;

            // build
            BuildWithVS2017.IsChecked = config.userConfig.with2017;
            SkipShaderCompiler.IsChecked = config.userConfig.skipShaderCompiler;
            SkipFrontend.IsChecked = config.userConfig.skipFrontend;
            SkipLightmass.IsChecked = config.userConfig.skipLightmass;
            DebugGame.IsChecked = config.userConfig.buildDebugGame;

            // editor
            StartEditorDebug.IsChecked = config.userConfig.startEditorDebug;
            editorHmdType2RadioButton[config.userConfig.editorHmdType].IsChecked = true;

            // run server
            StartServerDebug.IsChecked = config.userConfig.startServerDebug;

            // run game
            StartGameDebug.IsChecked = config.userConfig.startGameDebug;
            GameInstances.Text = config.userConfig.gameInstances.ToString();
            gameHmdType2RadioButton[config.userConfig.gameHmdType].IsChecked = true;
        }

        private Process ExecuteCommand(string command, bool createNoWindow = false, bool disableMainWindow = true)
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

            if (disableMainWindow)
            {
                this.IsEnabled = false;
            }

            var process = Process.Start(processInfo);

            //process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
            //    Console.WriteLine("output>>" + e.Data);
            //process.BeginOutputReadLine();

            //process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
            //    Console.WriteLine("error>>" + e.Data);
            //process.BeginErrorReadLine();

            process.EnableRaisingEvents = true;
            process.Exited += (object sender, System.EventArgs e) =>
            {
                if (disableMainWindow)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        this.IsEnabled = true;
                    });
                }
                process.Close();
            };

            return process;
        }

        private void GenerateProjectButton_Click(object sender, RoutedEventArgs e)
        {
            config.userConfig.with2017 = WithVS2017.IsChecked == true;
            Save();

            if (!config.userConfig.with2017)
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
            config.userConfig.with2017 = BuildWithVS2017.IsChecked == true;
            config.userConfig.skipShaderCompiler = SkipShaderCompiler.IsChecked == true;
            config.userConfig.skipFrontend = SkipFrontend.IsChecked == true;
            config.userConfig.skipLightmass = SkipLightmass.IsChecked == true;
            config.userConfig.buildDebugGame = DebugGame.IsChecked == true;
            Save();

            string command = "BuildEditor.bat";
            if (config.userConfig.with2017)
            {
                command += " -2017";
            }
            if (config.userConfig.skipShaderCompiler)
            {
                command += " --no-shader-compiler";
            }
            if (config.userConfig.skipFrontend)
            {
                command += " --no-frontend";
            }
            if (config.userConfig.skipLightmass)
            {
                command += " --no-lightmass";
            }
            if (config.userConfig.buildDebugGame)
            {
                command += " --debug-game";
            }
            ExecuteCommand(command);
        }

        private void RunGame_Click(object sender, RoutedEventArgs e)
        {
            config.userConfig.startGameDebug = StartGameDebug.IsChecked == true;
            config.userConfig.gameInstances = 1;
            Int32.TryParse(GameInstances.Text, out config.userConfig.gameInstances);
            config.userConfig.gameHmdType = HMDTypes.Default;
            foreach (KeyValuePair<HMDTypes, RadioButton> entry in gameHmdType2RadioButton)
            {
                if (entry.Value.IsChecked == true)
                {
                    config.userConfig.gameHmdType = entry.Key;
                    break;
                }
            }
            Save();
            
            for (int i = 0; i < config.userConfig.gameInstances; ++i)
            {
                string command = "RunAssociatedEngine.cmd -log -game -resx=1280 -resy=720 -windowed";
                if (config.sharedConfig.runGameCommand != null && config.sharedConfig.runGameCommand.Length > 0)
                {
                    command = config.sharedConfig.runGameCommand;
                }
                if (config.userConfig.startGameDebug)
                {
                    command += " -debug";
                }
                command += config.HmdTypeStrings[config.userConfig.gameHmdType];
                command += " ";
                command += GetExtraParameter(config.sharedConfig.gameParam, config.userConfig.gameParam, config.userConfig.gameExtraParamHashes, i);
                ExecuteCommand(command, true);
            }
        }

        private void RunEditor_Click(object sender, RoutedEventArgs e)
        {
            config.userConfig.startEditorDebug = StartEditorDebug.IsChecked == true;
            config.userConfig.editorHmdType = HMDTypes.Default;
            foreach (KeyValuePair<HMDTypes, RadioButton> entry in editorHmdType2RadioButton)
            {
                if (entry.Value.IsChecked == true)
                {
                    config.userConfig.editorHmdType = entry.Key;
                    break;
                }
            }
            Save();

            string command = "RunAssociatedEngine.cmd";
            if (config.userConfig.startEditorDebug)
            {
                command += " -debug";
            }
            command += config.HmdTypeStrings[config.userConfig.editorHmdType];
            command += " ";
            command += GetExtraParameter(config.sharedConfig.editorParam, config.userConfig.editorParam, config.userConfig.editorExtraParamHashes, -1);
            ExecuteCommand(command, true);
        }

        private void RunServer_Click(object sender, RoutedEventArgs e)
        {
            config.userConfig.startServerDebug = StartServerDebug.IsChecked == true;
            Save();

            string command = "RunAssociatedEngine.cmd -log -server";
            if (config.sharedConfig.runServerCommand != null && config.sharedConfig.runServerCommand.Length > 0)
            {
                command = config.sharedConfig.runServerCommand;
            }
            if (config.userConfig.startGameDebug)
            {
                command += " -debug";
            }
            command += " ";
            command += GetExtraParameter(config.sharedConfig.serverParam, config.userConfig.serverParam, config.userConfig.serverExtraParamHashes, -1);
            ExecuteCommand(command, true);
        }

        private void Save()
        {
            AthenaConfig.Save(config);
        }

        private void Load()
        {
            config = AthenaConfig.Load();
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
                GitAutoFetchProcess = ExecuteCommand("FrequentlyFetch.bat", false, false);
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

        private void Window_Closed(object sender, EventArgs e)
        {
            if (GitAutoFetchProcess != null)
            {
                KillProcessAndChildrens(GitAutoFetchProcess.Id);
            }
        }

        private static void KillProcessAndChildrens(int pid)
        {
            ManagementObjectSearcher processSearcher = new ManagementObjectSearcher("Select * From Win32_Process Where ParentProcessID=" + pid);
            ManagementObjectCollection processCollection = processSearcher.Get();

            try
            {
                Process proc = Process.GetProcessById(pid);
                if (!proc.HasExited) proc.Kill();
            }
            catch (ArgumentException)
            {
                // Process already exited.
            }

            if (processCollection != null)
            {
                foreach (ManagementObject mo in processCollection)
                {
                    KillProcessAndChildrens(Convert.ToInt32(mo["ProcessID"])); //kill child processes(also kills childrens of childrens etc.)
                }
            }
        }

        private void ServerParameters_Click(object sender, RoutedEventArgs e)
        {
            ParameterWindow parameterWindow = new ParameterWindow(config.sharedConfig.serverParam, config.userConfig.serverParam, config.userConfig.serverExtraParamHashes);
            parameterWindow.ParameterModified += (List<int> selectedHashes) => 
            {
                config.userConfig.serverExtraParamHashes = selectedHashes;
                Save();
            };
            parameterWindow.ShowDialog();
        }

        private void GameParameters_Click(object sender, RoutedEventArgs e)
        {
            ParameterWindow parameterWindow = new ParameterWindow(config.sharedConfig.gameParam, config.userConfig.gameParam, config.userConfig.gameExtraParamHashes);
            parameterWindow.ParameterModified += (List<int> selectedHashes) =>
            {
                config.userConfig.gameExtraParamHashes = selectedHashes;
                Save();
            };
            parameterWindow.ShowDialog();
        }

        private void EditorParameters_Click(object sender, RoutedEventArgs e)
        {
            ParameterWindow parameterWindow = new ParameterWindow(config.sharedConfig.editorParam, config.userConfig.editorParam, config.userConfig.editorExtraParamHashes);
            parameterWindow.ParameterModified += (List<int> selectedHashes) =>
            {
                config.userConfig.editorExtraParamHashes = selectedHashes;
                Save();
            };
            parameterWindow.ShowDialog();
        }

        private string GetExtraParameter(RunParametersConfig shared, RunParametersConfig user, List<int> selectedHashes, int instance)
        {
            foreach (var param in shared.Parameters)
            {
                if (selectedHashes.Contains(param.Param.GetHashCode()) && (instance == -1 || param.instance == instance))
                {
                    return param.Param;
                }
            }

            foreach (var param in user.Parameters)
            {
                if (selectedHashes.Contains(param.Param.GetHashCode()) && (instance == -1 || param.instance == instance))
                {
                    return param.Param;
                }
            }

            return "";
        }
    }
}
