// Copyright (c) Rapid Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Scada.Comm.Devices;
using Scada.Data.Const;

namespace Scada.Comm.Drivers.DrvDanfossECL
{
    /// <summary>
    /// Creates channel prototypes for a simulator device.
    /// <para>Создает прототипы каналов для симулятора устройства.</para>
    /// </summary>
    internal class CnlPrototypeFactory
    {
        public class ActiveChannel
        {
            public string Name;
            public string Code;
            public int CnlType;
            public int DataType;        // Тип данных Double, Integer, ASCII, Unicode
            public string format;       // формат данных для вывода
        }

        /// <summary>
        /// Gets the grouped channel prototypes.
        /// </summary>
        public static List<CnlPrototypeGroup> GetCnlPrototypeGroups(Dictionary<string, ActiveChannel> activeChannels) // Dictionary<string, ActiveChannel> activeChannel
        {
            List<CnlPrototypeGroup> groups = new List<CnlPrototypeGroup>();

            CnlPrototypeGroup group = new CnlPrototypeGroup();

            var listCnl = activeChannels;


            foreach (var cnlprot in listCnl)
            {
                group.AddCnlPrototype(cnlprot.Value.Code, cnlprot.Value.Name).Configure(cnl =>
                    {
                        cnl.CnlTypeID = cnlprot.Value.CnlType;
                        cnl.DataTypeID = cnlprot.Value.DataType;

                        cnl.FormatCode = FormatCode.N0;
                    });
            }
            groups.Add(group);
            return groups;
        }

        /// <summary>
        /// Gets a flatten list of the channel prototypes.
        /// </summary>
        public static List<CnlPrototype> GetCnlPrototypes(Dictionary<string, ActiveChannel> dict) // Dictionary<string, ActiveChannel> dict
        {
            return GetCnlPrototypeGroups(dict).SelectMany(group => group.CnlPrototypes).ToList();
        }
    }
}
