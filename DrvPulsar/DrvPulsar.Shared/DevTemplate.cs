using System.Xml;
using System.Xml.Serialization;

namespace Scada.Comm.Drivers.DrvPulsar
{
    [Serializable]
    public class DevTemplate // Сформированный класс шаюлона под задачу для сериализации XML
    {
        public DevTemplate()
        {
            SndGroups = new List<SndGroup>();   // Список каталога запросов
            CmdGroups = new List<CmdGroup>();   // Список каталога команд
        }

        [XmlAttribute] public string Name { get; set; }

        public List<SndGroup> SndGroups { get; set; } // это позволяет сделать сериализацию без создания соответствующих групп если они пустые
        [XmlIgnore]
        public bool SndGroupsSpecified { get { return SndGroups.Count != 0; } }
        public List<CmdGroup> CmdGroups { get; set; }
        [XmlIgnore]
        public bool CmdGroupsSpecified { get { return CmdGroups.Count != 0; } }

        public class SndGroup
        {
            public SndGroup()
            {
            }

            public SndGroup(int Counter, bool Active, string Name, string GroupName, string Command, string userData)
            {
                this.Counter = Counter;
                this.Active = Active;
                this.Name = Name;
                this.GroupName = GroupName;
                this.Command = Command;
                this.userData = userData;

                Vals = new List<Val>();
            }

            [XmlAttribute] public int Counter { get; set; }
            [XmlAttribute] public bool Active { get; set; }
            [XmlAttribute] public string Name { get; set; }
            [XmlAttribute] public string GroupName { get; set; }
            [XmlAttribute] public string Command { get; set; }
            [XmlAttribute] public string userData { get; set; }
            [XmlElement] public List<Val> Vals { get; set; }


            public class Val
            {
                public Val()
                {
                }

                public Val(int Channel, string Code, bool Active, string Name, string Format, double Multiplier, bool Writable, string Command, string userData)
                {
                    this.Channel = Channel;
                    this.Code = Code;
                    this.Active = Active;
                    this.Name = Name;
                    this.Format = Format;
                    this.Multiplier = Multiplier;
                    this.Writable = Writable;
                    this.Command = Command;
                    this.userData = userData;

                }

                [XmlAttribute] public int Channel { get; set; }
                [XmlAttribute] public string Code { get; set; }
                [XmlAttribute] public bool Active { get; set; }
                [XmlAttribute] public string Name { get; set; }
                [XmlAttribute] public string Format { get; set; }
                [XmlAttribute] public double Multiplier { get; set; }
                [XmlAttribute] public bool Writable { get; set; }
                [XmlAttribute] public string Command { get; set; }
                [XmlAttribute] public string userData { get; set; }


                [XmlIgnore]
                public bool WritableSpecified { get { return Writable == true; } }
                public bool CommandSpecified { get { return Command != ""; } }
                public bool userDataSpecified { get { return userData != ""; } }

            }

        }

        public class CmdGroup
        {
            public CmdGroup()
            {
            }

            public CmdGroup(int Channel, string Code, bool Active, string Name, string Format, string Command, string userData)
            {
                this.Channel = Channel;
                this.Code = Code;
                this.Active = Active;
                this.Name = Name;
                this.Format = Format;
                this.Command = Command;
                this.userData = userData;
            }

            [XmlAttribute] public int Channel { get; set; }
            [XmlAttribute] public string Code { get; set; }
            [XmlAttribute] public bool Active { get; set; }
            [XmlAttribute] public string Name { get; set; }
            [XmlAttribute] public string Format { get; set; }
            [XmlAttribute] public string Command { get; set; }
            [XmlAttribute] public string userData { get; set; }
        }
    }
}
