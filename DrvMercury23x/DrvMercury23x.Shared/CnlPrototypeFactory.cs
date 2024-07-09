// Copyright (c) Rapid Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Scada.Comm.Devices;
using Scada.Data.Const;
//using ScadaCommFunc;
//using System.Collections.Generic;
//using System.Linq;

namespace Scada.Comm.Drivers.DrvMercury23x
{
    /// <summary>
    /// Creates channel prototypes for a simulator device.
    /// <para>Создает прототипы каналов для симулятора устройства.</para>
    /// </summary>
    internal class CnlPrototypeFactory
    {
        public class ActiveChannel
        {
            public string GroupName; // = ""
            public string Name;
            public string Code;
            public int CnlType;
            public int Mode;
            public string range;
            public byte[] setCommand;
            public int scCnt;           // Количество принимаемы байт
            public int datalen;         // длина блока данных в байтах
            public string format;       // формат данных для вывода
            public int quantity;        // идентификатор запроса группы для поиска
            public int idxPar;          // Индекс параметра в группе тегов
            public string CnlQuantity;  // Величина переменной
            public string unitCode;     // Размерность переменной
        }

        /// <summary>
        /// Gets the grouped channel prototypes.
        /// </summary>
        public static List<CnlPrototypeGroup> GetCnlPrototypeGroups(Dictionary<string, ActiveChannel> activeChannels) // Dictionary<string, ActiveChannel> activeChannel
        {
            List<CnlPrototypeGroup> groups = new List<CnlPrototypeGroup>();

            var menuCount =
                from p in activeChannels.Values
                group p by p.GroupName into g
                select new { MenuName = g.Key, counts = g.Count() };

            foreach (var menu in menuCount)
            {
                CnlPrototypeGroup group = new CnlPrototypeGroup();

                group = new CnlPrototypeGroup(menu.MenuName);
                var listMenu = activeChannels.Where(x => x.Value.GroupName == menu.MenuName);


                foreach (var cnlprot in listMenu)
                {
                    group.AddCnlPrototype(cnlprot.Value.Code, cnlprot.Value.Name).Configure(cnl =>
                    {
                        cnl.UnitCode = cnlprot.Value.unitCode;
                        cnl.QuantityCode = cnlprot.Value.CnlQuantity; // TEST
                        cnl.CnlTypeID = cnlprot.Value.CnlType;
                        cnl.DataTypeID = DataTypeID.Double;
                        cnl.FormatCode = FormatCode.N2;
                        if (cnlprot.Value.format == "hex") cnl.FormatCode = FormatCode.X8;
                        else if (cnlprot.Value.format == "string") cnl.FormatCode = FormatCode.String; // Для команд, передающих строку HEX
                    });
                }
                groups.Add(group);
            }
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
