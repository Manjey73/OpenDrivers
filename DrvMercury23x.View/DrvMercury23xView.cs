using Scada.Lang;
using Scada.Comm.Config;
using Scada.Comm.Devices;

namespace Scada.Comm.Drivers.DrvMercury23x.View
{
    public class DrvMercury23xView : DriverView
    {
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public DrvMercury23xView()
        {
            CanCreateDevice = true;
        }

        /// <summary>
        /// Gets the driver name.
        /// </summary>
        public override string Name
        {
            get
            {
                return Locale.IsRussian ? "Драйвер счётчика Меркурий (230, 231, 232, 233, 236)" : "Mercury (230, 231, 232, 233, 236) Driver";
            }
        }

        /// <summary>
        /// Gets the driver description.
        /// </summary>
        public override string Descr
        {
            get
            {
                return Locale.IsRussian ?
                "Автор Бурахин Андрей, email: aburakhin@bk.ru\n\n" +
                "Адрес счетчика  = Устройства -> Числовой адрес\n\n" +

                "Параметры DevTamplate:\nreadparam(ReadParam) - 14h или 16h\n" +
                "multicast(MultiCast) = true - широковещательная команда фиксации данных\n" +
                "info(Information) = true - прочитать данные счетчика, SN, дату производства, постоянную счтечика\n" +
                "SyncTime(Synchronize Time) = true - синхронизировать время в пределах 4 минут\n" +
                "readStatus(Read Status) = true - Читать статус Аварии, коэфф. трансформации тока и напряжения, Ошибки Exx\n" +
                "halfArchStat(Status incorrect Profile) = номер Типа события для неполного среза - необходимо создать и задать цвет\n" +
                "ArchMask(Archive Mask) - Маска архива, в который писать профили средних мощностей\n\n"+

                "Параметры SndGroups - SndRequest: (можно удалить целиком)\n" +
                "Name - общее имя группы параметров\n" +
                "Active - активность чтения группы параметров\n" +
                "Bit - Номер бита для группы параметров (см. ниже, удалять и менять значения нельзя)\n\n" +
                "Список параметров value:\n" +
                "name - имя обособленного параметра \n" +
                "code - номер сигнала для параметра \n" +
                "active - активнось параметра \n" +
                "range  - множитель параметра \n\n" +

                "    Мгновенные значения:\n" +
                "bit 0 - Мощность P ∑, L1, L2, L3\n" +
                "bit 1 - Мощность Q ∑, L1, L2, L3\n" +
                "bit 2 - Мощность S ∑, L1, L2, L3\n" +
                "bit 3 - Cos f ∑, L1, L2, L3\n" +
                "bit 4 - Напряжение L1, L2, L3\n" +
                "bit 5 - Ток L1, L2, L3\n" +
                "bit 6 - Угол м-ду ф. L1-L2, L1-L3, L2-L3\n" +
                "bit 7 - Частота сети\n\n" +
                "    Энергия от сброса:\n" +
                "bit 8  - Энергия ∑ А+, А-, R+, R-\n" +
                "bit 9  - Тариф 1   А+, А-, R+, R-\n" +
                "bit 10 - Тариф 2   А+, А-, R+, R-\n" +
                "bit 11 - Тариф 3   А+, А-, R+, R-\n" +
                "bit 12 - Тариф 4   А+, А-, R+, R-\n" +
                "bit 13 - Энергия ∑ А+ L1, L2, L3\n" +
                "bit 14 - Тариф 1   А+ L1, L2, L3\n" +
                "bit 15 - Тариф 2   А+ L1, L2, L3\n" +
                "bit 16 - Тариф 3   А+ L1, L2, L3\n" +
                "bit 17 - Тариф 4   А+ L1, L2, L3" :

                    "Designed for testing communication channels.\n\n" +
                    "Commands:\n" +
                    "1, SendStr - send string;\n" +
                    "2, SendBin - send binary data.";
            }
        }

        /// <summary>
        /// Creates a new device user interface.
        /// </summary>
        public override DeviceView CreateDeviceView(LineConfig lineConfig, DeviceConfig deviceConfig)
        {
            return new DevMercury23xView(this, lineConfig, deviceConfig);
        }
    }
}
