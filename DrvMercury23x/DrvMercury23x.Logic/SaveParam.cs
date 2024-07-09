using System.Xml;
using System.Xml.Serialization;

namespace Scada.Comm.Drivers.DrvMercury23x
{
    [Serializable]
    public class SaveParam
    {
        public SaveParam()
        {
        }

        [XmlAttribute] public string arcDt { get; set; }        // Время последнего считанного архива средних мощностей
        [XmlAttribute] public string lastSync { get; set; }     // Дата последней синхронизации времени
        [XmlAttribute] public string serial { get; set; }       // Серийный номер
        [XmlAttribute] public string madeDt { get; set; }       // Дата производства
        [XmlAttribute] public string constA { get; set; }       // Постоянная счетчика

    }
}
