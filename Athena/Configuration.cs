using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.IO;

namespace Athena
{
    public enum HMDTypes
    {
        NoHmd,
        Default,
        GoogleVR,
        Occulus,
        SteamVR
    };

    public class UserConfig
    {
        public bool with2017 = false;
        public bool skipShaderCompiler = false;
        public bool skipFrontend = false;
        public bool skipLightmass = false;
        public bool buildDebugGame = false;
        public bool startServerDebug = false;
        public bool startGameDebug = false;
        public bool startEditorDebug = false;
        public int gameInstances = 1;
        public HMDTypes gameHmdType = HMDTypes.Default;
        public HMDTypes editorHmdType = HMDTypes.Default;
        public RunParametersConfig gameParam = new RunParametersConfig();
        public RunParametersConfig serverParam = new RunParametersConfig();
    }

    public class SharedConfig
    {
        public RunParametersConfig gameParam = new RunParametersConfig();
        public RunParametersConfig serverParam = new RunParametersConfig();
    }

    public class AthenaConfig
    {
        static private string userConfigFileName = "athena_config.json";
        static private string sharedConfigFileName = "athena_shared_config.json";
        public UserConfig userConfig = new UserConfig();
        public SharedConfig sharedConfig = new SharedConfig();

        [ScriptIgnore]
        public Dictionary<HMDTypes, string> HmdTypeStrings = new Dictionary<HMDTypes, string>
        {
            { HMDTypes.NoHmd, "-nohmd" },
            { HMDTypes.Default, "" },
            { HMDTypes.GoogleVR, "-hmd=SteamVR" },
            { HMDTypes.Occulus, "-hmd=OculusRift" },
            { HMDTypes.SteamVR, "-hmd=googlevrhmd" }
        };

        public static void Save(AthenaConfig config)
        {
            InternalSave<UserConfig>(userConfigFileName, config.userConfig);
            InternalSave<SharedConfig>(sharedConfigFileName, config.sharedConfig);
        }

        public static void InternalSave<T>(string filename, T config)
        {
            var json = new JavaScriptSerializer().Serialize(config);
            Console.WriteLine(json);

            TextWriter writer = null;
            try
            {
                writer = new StreamWriter(filename);
                writer.Write(json);
            }
            finally
            {
                if (writer != null)
                    writer.Close();
            }
        }

        public static AthenaConfig Load()
        {
            AthenaConfig config = new AthenaConfig();
            config.userConfig = InternalLoad<UserConfig>(userConfigFileName);
            config.sharedConfig = InternalLoad<SharedConfig>(sharedConfigFileName);

            // make sure config is always valid, in case the config file has "null" in it
            if (config.userConfig == null)
            {
                config.userConfig = new UserConfig();
            }
            if (config.sharedConfig == null)
            {
                config.sharedConfig = new SharedConfig();
            }

            return config;
        }

        public static T InternalLoad<T>(string filename) where T : new()
        {
            T config = new T();
            if (File.Exists(filename))
            {
                TextReader reader = null;
                try
                {
                    reader = new StreamReader(filename);
                    var json = reader.ReadToEnd();
                    config = new JavaScriptSerializer().Deserialize<T>(json);
                }
                finally
                {
                    if (reader != null)
                    {
                        reader.Close();
                    }
                }
            }

            return config;
        }
    }

    public class RunParametersConfig
    {
        public struct RunParameter
        {
            public int Id;
            public string Param;
        }

        public List<RunParameter> Parameters = new List<RunParameter>();
    }
}
