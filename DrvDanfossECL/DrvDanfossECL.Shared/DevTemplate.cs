using System.Xml;
using System.Xml.Serialization;

namespace Scada.Comm.Drivers.DrvDanfossECL
{

    [Serializable]
    public class DevTemplate // : INotifyPropertyChanged
    {
        public DevTemplate()
        {
            Parameter = new List<Parameters>();
        }

        [XmlAttribute] public string Name { get; set; } // Имя устройства
        [XmlAttribute] public string WriteEvenRAM { get; set; } // Записывать младший байт по нечетному адресу RAM
        [XmlAttribute] public string WriteOnlyRAM { get; set; } // Записы только в RAM

        [XmlIgnore]
        public bool WriteEvenRAMSpecified { get { return WriteEvenRAM != ""; } }
        public bool WriteOnlyRAMSpecified { get { return WriteOnlyRAM != ""; } }


        [XmlElement] public List<Parameters> Parameter { get; set; }
                public class Parameters
        {
            public Parameters()
            {
            }

            public Parameters(string Code, string Name, string Address, bool Active, bool Write, string min_val, string max_val, string Format, string Multiplier) // string Format
            {
                this.Code = Code;
                this.Name = Name;
                this.Address = Address;
                this.Active = Active;
                this.Write = Write;
                this.min_val = min_val;
                this.max_val = max_val;
                this.Format = Format;
                this.Multiplier = Multiplier;
            }

            [XmlAttribute] public string Code { get; set; }         // Код параметра
            [XmlAttribute] public string Name { get; set; }         // Имя параметра
            [XmlAttribute] public string Address { get; set; }         // Имя параметра
            [XmlAttribute] public bool Active { get; set; }         // Активность параметра
            [XmlAttribute] public bool Write { get; set; }       // Разрешение записи
            [XmlAttribute] public string min_val { get; set; }      // Минимальное значение
            [XmlAttribute] public string max_val { get; set; }      // Максимальное значение
            [XmlAttribute] public string Format { get; set; }       // Формат переменной (float, int, string и т.д.)
            [XmlAttribute] public string Multiplier { get; set; }   // Множитель параметра

            [XmlIgnore]
            public bool min_valSpecified { get { return min_val != ""; } }
            public bool max_valSpecified { get { return max_val != ""; } }
            public bool MultiplierSpecified { get { return Multiplier != ""; } }

        }

    }
}
