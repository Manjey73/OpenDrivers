using System.Xml.Serialization;

namespace Scada.Comm.Drivers.DrvRodosBu.Logic
{
    [Serializable]
    public class response
    {

            [XmlElement]
            public int rl0string { get; set; }

            [XmlElement]
            public int rl1string { get; set; }

            [XmlElement]
            public int rl2string { get; set; }

            [XmlElement]
            public int rl3string { get; set; }

            [XmlElement]
            public int rl4string { get; set; }

            [XmlElement]
            public int rl5string { get; set; }

            [XmlElement]
            public int rl6string { get; set; }

            [XmlElement]
            public int rl7string { get; set; }

        [XmlElement]
        public string rl8string { get; set; }

        [XmlElement]
        public string rl9string { get; set; }

        [XmlElement]
        public string rl10string { get; set; }

        [XmlElement]
        public string rl11string { get; set; }

        [XmlElement]
        public string rl12string { get; set; }

        [XmlElement]
        public string rl13string { get; set; }

        [XmlElement]
        public string rl14string { get; set; }

        [XmlElement]
        public string rl15string { get; set; }

    }
}
