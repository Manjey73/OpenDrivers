using Scada.Comm.Config;
using Scada.Comm.Devices;
using Scada.Comm.Drivers.DrvDS18B20.Config;
using Scada.Comm.Lang;
using Scada.Data.Models;
using Scada.Lang;
using System.Diagnostics;

namespace Scada.Comm.Drivers.DrvDS18B20.Logic
{
    internal class DevDS18B20Logic : DeviceLogic
    {
        string path = "/sys/devices/w1_bus_master1/";
        string path1 = "/w1_slave";
        string pathDS = "";

        private readonly Ds18b20DeviceConfig config;  // the device configuration
        private readonly List<VarGroup> varGroups; // the active variable groups

        private class VarGroup
        {
            public VarGroupConfig Config { get; init; }
            public List<Variable> Variables { get; } = new(); // Сюда свой класс с переменными - только активные заносить.
            public int StartTagIndex { get; init; }
        }

        private class Variable
        {
            public bool active { get; init; }
            public string code { get; set; }
            public string name { get; set; }
            public string dsId { get; set; }
        }

        static DevDS18B20Logic()
        {
        }

        private bool fatalError;                   // normal operation is impossible

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public DevDS18B20Logic(ICommContext commContext, ILineContext lineContext, DeviceConfig deviceConfig)
            : base(commContext, lineContext, deviceConfig)
        {
            ConnectionRequired = false;

            config = new Ds18b20DeviceConfig();
            varGroups = new List<VarGroup>();

            fatalError = false;
        }

        public override void InitDeviceTags()
        {
            if (fatalError)
                return;

            foreach (VarGroupConfig varGroupConfig in config.VarGroups)
            {
                if (varGroupConfig.Variables.Count == 0)
                    continue;

                // create tag group
                TagGroup tagGroup = new(varGroupConfig.Name);
                int startTagIdx = DeviceTags.Count;

                if (varGroupConfig.Active)
                {
                    int cnt = 0;
                    VarGroup varGroup = new()
                    {
                        Config = varGroupConfig,
                        StartTagIndex = startTagIdx
                    };

                    foreach (VariableConfig variableConfig in varGroupConfig.Variables)
                    {
                        if (variableConfig.Active)
                        {
                            DeviceTag deviceTag = tagGroup.AddTag(variableConfig.TagCode, variableConfig.Name);

                            varGroup.Variables.Add(CreateVariable(variableConfig));
                            cnt++;
                        }
                    }

                    if(cnt == 0)
                        continue;

                    varGroups.Add(varGroup);
                    DeviceTags.AddGroup(tagGroup);
                }
            }
        }

        /// <summary>
        /// Creates a variable according to the configuration.
        /// </summary>
        private static Variable CreateVariable(VariableConfig variableConfig)
        {
            try
            {
                return new Variable { code = variableConfig.TagCode, name = variableConfig.Name, dsId = variableConfig.DsId };

            }
            catch (Exception ex)
            {
                throw new ScadaException(Locale.IsRussian ?
                    "Ошибка при создании переменной \"{0}\" с идентификатором {1}: {2}" :
                    "Error creating variable \"{0}\" with identifier {1}: {2}",
                    variableConfig.Name, variableConfig.DsId, ex.Message);
            }
        }

        /// <summary>
        /// Выполнить действия при запуске линии связи
        /// </summary>
        public override void OnCommLineStart()
        {
            if (config.Load(Storage, Ds18b20DeviceConfig.GetFileName(DeviceNum), out string errMsg))
            {
                // Чтение параметров основных
            }
            else
            {
                Log.WriteLine(CommPhrases.DeviceMessage, Title, errMsg);
                fatalError = true;
            }
        }

        /// <summary>
        /// Performs actions after setting the connection.
        /// </summary>
        public override void OnConnectionSet()
        {
        }

        /// <summary>
        /// Performs a communication session.
        /// </summary>
        public override void Session()
        {
            base.Session();
            Stopwatch stopwatch = Stopwatch.StartNew();

            foreach (var varGroup in varGroups)
            {
                LastRequestOK = true;

                if (varGroup.Variables.Count > 0)
                {
                    for (int i = 0; i < varGroup.Variables.Count; i++)
                    {
                        var ds = varGroup.Variables[i];
                        pathDS = path + ds.dsId + path1;
                        int iOfChar;

                        string loadline = "";
                        StreamReader sr = null;
                        try
                        {
                            using (FileStream file = new FileStream(pathDS, FileMode.Open, FileAccess.Read))
                            {
                                Log.WriteLine($"Read {ds.dsId}");
                                sr = new StreamReader(file);
                                loadline = sr.ReadToEnd();
                                Log.WriteLine(loadline);

                                iOfChar = loadline.IndexOf("YES");
                                if (iOfChar != -1)
                                {
                                    iOfChar = loadline.IndexOf("t=");
                                    int res = int.Parse(loadline.Substring(iOfChar + 2));
                                    DeviceData.Set(ds.code, Convert.ToDouble(res) / 1000);
                                }
                                else
                                {
                                    DeviceData.Invalidate(ds.code);
                                    Log.WriteLine(CommPhrases.ResponseCrcError);
                                    LastRequestOK = false;
                                }
                            }
                        }
                        catch (Exception)
                        {
                            DeviceData.Invalidate(ds.code);

                            Log.WriteLine(Locale.IsRussian ?
                                $"Датчик {ds.dsId} не обнаружен" :
                                $"Sensor {ds.dsId} not detected");
                            LastRequestOK = false;
                        }
                        finally
                        {
                            if (sr != null)
                                ((IDisposable)sr).Dispose();
                        }
                        FinishRequest();
                    }
                }
            }

            stopwatch.Stop();
            Log.WriteLine(Locale.IsRussian ?
                "Получено за {0} мс" :
                "Received in {0} ms", stopwatch.ElapsedMilliseconds);

            FinishSession();
        }

        /// <summary>
        /// Sends the telecontrol command.
        /// </summary>
        public override void SendCommand(TeleCommand cmd)
        {
        }

        /// <summary>
        /// Performs actions when terminating a communication line.
        /// </summary>
        public override void OnCommLineTerminate()
        {
        }
    }

}
