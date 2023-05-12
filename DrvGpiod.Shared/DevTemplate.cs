using System.Xml.Serialization;

namespace Scada.Comm.Drivers.DrvGpiod
{
    [Serializable]
    public class DevTemplate
    {
        public DevTemplate()
        {
            Gpios = new List<Gpiod>();
        }

        [XmlAttribute] public string Name { get; set; } // Имя устройства

        public List<Gpiod> Gpios { get; set; }

        public class Gpiod
        {
            public Gpiod()
            {
            }

            public Gpiod(string Name, bool Active, string Code, string Pin, string PinMode, string startValue) // string Format
            {
                this.Name = Name;
                this.Active = Active;
                this.Code = Code;
                this.Pin = Pin;
                this.PinMode = PinMode;
                this.startValue = startValue;
            }

            [XmlAttribute] public string Name { get; set; }         // Имя Устройства - является так же и именем Меню в Коммуникаторе
            [XmlAttribute] public bool Active { get; set; }         // Активность устройства
            [XmlAttribute] public string Code { get; set; }         // Код тега
            [XmlAttribute] public string Pin { get; set; }          // Номер пина
            [XmlAttribute] public string PinMode { get; set; }      // Режим Pin
            [XmlAttribute] public string startValue { get; set; }   // Сьартовое значение Low, Hihg
        }
    }
}
