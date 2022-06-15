using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Scada.Comm.Drivers.DrvDanfossECL
{

    [Serializable]
    public class DevTemplate : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public DevTemplate()
        {
            CmdGroups = new List<CmdGroup>();
        }

        [XmlAttribute] public string Name { get; set; } // Имя устройства

        public List<CmdGroup> CmdGroups { get; set; }

        public class CmdGroup
        {
            public CmdGroup()
            {
            }

            public CmdGroup(string Name, bool Active, string Code, string Read, string Write, string Parameter, string Format, string Mul, string Div) // string Format
            {
                this.Name = Name;
                this.Active = Active;
                this.Code = Code;
                this.Read = Read;
                this.Write = Write;
                this.Parameter = Parameter;
                this.Format = Format;
                this.Mul = Mul;
                this.Div = Div;
            }

            [XmlAttribute] public string Name { get; set; }         // Имя параметра
            [XmlAttribute] public bool Active { get; set; }         // Активность параметра
            [XmlAttribute] public string Code { get; set; }         // Код параметра
            [XmlAttribute] public string Read { get; set; }         // Байт для чтения параметра
            [XmlAttribute] public string Write { get; set; }        // Байт для записи параметра
            [XmlAttribute] public string Parameter { get; set; }    // Байт параметра
            [XmlAttribute] public string Format { get ; set; }       // Формат переменной (float, int, string и т.д.) // public string Format { get; set; }
            [XmlAttribute] public string Mul { get; set; }          // Множитель параметра
            [XmlAttribute] public string Div { get; set; }          // Делитель параметра

            public bool MulSpecified { get { return Mul != ""; } }
            public bool DivSpecified { get { return Div != ""; } }
        }

        //public class Choice
        //{
        //    public string Value { get; private set; }
        //    public Choice(string value) // string name, int value
        //    {
        //        Value = value;
        //    }

        //    private static readonly List<Choice> possibleChoices = new List<Choice>
        //    {
        //        { new Choice(null) },
        //        { new Choice("float") },
        //        { new Choice("int16") }
        //    };

        //    public static List<Choice> GetChoices()
        //    {
        //        return possibleChoices;
        //    }
        //}
    }
}
