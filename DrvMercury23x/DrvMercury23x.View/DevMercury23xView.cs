using Scada.Comm.Config;
using Scada.Comm.Devices;
using Scada.Lang;
using ScadaCommFunc;
using Scada.Data.Const;

namespace Scada.Comm.Drivers.DrvMercury23x.View
{
    internal class DevMercury23xView : DeviceView
    {
        private DevTemplate devTemplate = new DevTemplate();
        Dictionary<string, CnlPrototypeFactory.ActiveChannel> channels = new Dictionary<string, CnlPrototypeFactory.ActiveChannel>();
        private bool readstatus = false;

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public DevMercury23xView(DriverView parentView, LineConfig lineConfig, DeviceConfig deviceConfig)
            : base(parentView, lineConfig, deviceConfig)
        {
            CanShowProperties = true;
        }

        /// <summary>
        /// Gets the default polling options for the device.
        /// </summary>
        public override PollingOptions GetPollingOptions()
        {
            PollingOptions pollingOptions = new PollingOptions(1500, 300);
            pollingOptions.Period = TimeSpan.FromSeconds(30);
            new Mercury23xOptions().AddToOptionList(pollingOptions.CustomOptions);
            return pollingOptions;
        }

        /// <summary>
        /// Shows a modal dialog box for editing device properties.
        /// </summary>
        public override bool ShowProperties()
        {
            if (new FrmDeviceProps(AppDirs, LineConfig, DeviceConfig).ShowDialog() == DialogResult.OK) // , customUi
            {
                LineConfigModified = true;
                DeviceConfigModified = true;
                return true;
            }
            else
            {
                return false;
            }
        }

        public override ICollection<CnlPrototype> GetCnlPrototypes()
        {
            GetDictChannel();
            return CnlPrototypeFactory.GetCnlPrototypes(channels);
        }

        public void GetDictChannel()
        {
            devTemplate = null;

            // загрузка шаблона устройства
            string fileName = DeviceConfig.PollingOptions.CmdLine == null ? "" : DeviceConfig.PollingOptions.CmdLine.Trim();

            //if (fileName == "")
            //    return null;

            string filePath = Path.IsPathRooted(fileName) ? fileName : Path.Combine(AppDirs.ConfigDir, fileName);

            try
            {
                devTemplate = FileFunc.LoadXml(typeof(DevTemplate), filePath) as DevTemplate;
            }
            catch (Exception ex)
            {
                throw new ScadaException(string.Format(Locale.IsRussian ?
                "Ошибка при получении типа логики Устройства из библиотеки {0}" :
                "Error getting device logic type from the library {0}", ex.Message), ex);
            }

            // Проверка на наличие конфигурации XML
            if (devTemplate != null)
            {
                channels.Clear(); // Очищаем список, так как код срабатывает при выборе КП при Создании каналов каждый раз...

                readstatus = devTemplate.readStatus;

                if (devTemplate.SndGroups.Count != 0) // Определить активные запросы объектов и записать в список индексы запросов для создания тегов
                {
                    for (int sg = 0; sg < devTemplate.SndGroups.Count; sg++)
                    {
                        if (devTemplate.SndGroups[sg].Active)
                        {
                            if (devTemplate.SndGroups[sg].value.Count > 0)
                            {
                                for (int y = 0; y < devTemplate.SndGroups[sg].value.Count; y++)
                                {
                                    if (devTemplate.SndGroups[sg].value[y].active)
                                    {
                                        string uCode = "";
                                        string QuantCode = "";
                                        if (devTemplate.SndGroups[sg].Bit == 0)
                                        {
                                            QuantCode = QuantityCode.ActivePower;
                                            uCode = UnitCode.Watt;
                                        }
                                        else if (devTemplate.SndGroups[sg].Bit == 1)
                                        {
                                            QuantCode = QuantityCode.ReactivePower;
                                            uCode = UnitCode.VoltAmpereReactive;
                                        }
                                        else if (devTemplate.SndGroups[sg].Bit == 2)
                                        {
                                            QuantCode = QuantityCode.ApparentPower;
                                            uCode = UnitCode.VoltAmpere;
                                        }
                                        else if (devTemplate.SndGroups[sg].Bit == 3)
                                            QuantCode = QuantityCode.PowerFactor;
                                        else if (devTemplate.SndGroups[sg].Bit == 4)
                                        {
                                            QuantCode = QuantityCode.Voltage;
                                            uCode = UnitCode.Volt;
                                        }
                                        else if (devTemplate.SndGroups[sg].Bit == 5)
                                        {
                                            QuantCode = QuantityCode.Current;
                                            uCode = UnitCode.Ampere;
                                        }
                                        else if (devTemplate.SndGroups[sg].Bit == 7)
                                        {
                                            QuantCode = QuantityCode.Frequency;
                                            uCode = UnitCode.Hertz;
                                        }
                                        // Энергия ппотребленная в кВт/ч
                                        else if (devTemplate.SndGroups[sg].Bit == 8 || devTemplate.SndGroups[sg].Bit == 9 || devTemplate.SndGroups[sg].Bit == 10 || devTemplate.SndGroups[sg].Bit == 11 || devTemplate.SndGroups[sg].Bit == 12)
                                        {
                                            QuantCode = QuantityCode.EnergyConsumption;
                                        }
                                        // Активная энергия по фазам в кВт/ч
                                        else if (devTemplate.SndGroups[sg].Bit == 13 || devTemplate.SndGroups[sg].Bit == 14 || devTemplate.SndGroups[sg].Bit == 15 || devTemplate.SndGroups[sg].Bit == 16 || devTemplate.SndGroups[sg].Bit == 17)
                                        {
                                            QuantCode = QuantityCode.ActivePowerConsumption;
                                            uCode = UnitCode.KilowattHour;
                                        }

                                        string Allname = $"{devTemplate.SndGroups[sg].Name} {devTemplate.SndGroups[sg].value[y].name}";
                                        channels.Add(Allname,
                                        new CnlPrototypeFactory.ActiveChannel()
                                        {
                                            GroupName = "Статус:",
                                            Name = Allname,
                                            Code = devTemplate.SndGroups[sg].value[y].code,
                                            CnlType = CnlTypeID.Input,
                                            CnlQuantity = QuantCode,
                                            unitCode = uCode
                                        });
                                    }
                                }
                            }
                        }
                    }
                }

                if (devTemplate.ProfileGroups.Count > 0)
                {
                    for (int i = 0; i < devTemplate.ProfileGroups.Count; i++)
                    {
                        if (devTemplate.ProfileGroups[i].Active)
                        {
                            for (int k = 0; k < devTemplate.ProfileGroups[i].value.Count; k++)
                            {
                                if (devTemplate.ProfileGroups[i].value[k].active)
                                {
                                    string Allname = $"{devTemplate.ProfileGroups[i].Name} {devTemplate.ProfileGroups[i].value[k].name}";
                                    channels.Add(devTemplate.ProfileGroups[i].value[k].code,
                                    new CnlPrototypeFactory.ActiveChannel()
                                    {
                                        Name = Allname,
                                        Code = devTemplate.ProfileGroups[i].value[k].code,
                                        CnlType = CnlTypeID.Input,
                                    });
                                }
                            }
                        }
                    }
                }

                if (readstatus)
                {
                    channels.Add("error",
                        new CnlPrototypeFactory.ActiveChannel()
                        {
                            GroupName = "Статус:",
                            Name = "Код ошибки:",
                            Code = "error",
                            CnlType = CnlTypeID.Input,
                        });
                    channels.Add("kI",
                        new CnlPrototypeFactory.ActiveChannel()
                        {
                            GroupName = "Статус:",
                            Name = "коэфф. трансф. тока:",
                            Code = "kI",
                            CnlType = CnlTypeID.Input,
                        });
                    channels.Add("kU",
                        new CnlPrototypeFactory.ActiveChannel()
                        {
                            GroupName = "Статус:",
                            Name = "коэфф. трансф. напряжения:",
                            Code = "kU",
                            CnlType = CnlTypeID.Input,
                        });

                    channels.Add("wordSt",
                        new CnlPrototypeFactory.ActiveChannel()
                        {
                            GroupName = "Статус:",
                            Name = "Слово состояния:",
                            Code = "wordSt",
                            CnlType = CnlTypeID.Input,
                            format = "hex",
                        });
                }

                if (devTemplate.CmdGroups.Count > 0)
                {

                    for (int c = 0; c < devTemplate.CmdGroups.Count; c++)
                    {
                        if (devTemplate.CmdGroups[c].Active)
                        {
                            string format = string.IsNullOrEmpty(devTemplate.CmdGroups[c].Data) ? "" : "string";

                            channels.Add(devTemplate.CmdGroups[c].Code,
                                new CnlPrototypeFactory.ActiveChannel()
                                {
                                    GroupName = "Команды",
                                    Name = devTemplate.CmdGroups[c].Name,
                                    Code = devTemplate.CmdGroups[c].Code,
                                    CnlType = CnlTypeID.Output,
                                    format = format,
                                });
                        }
                    }
                }
            }
        }
    }
}
