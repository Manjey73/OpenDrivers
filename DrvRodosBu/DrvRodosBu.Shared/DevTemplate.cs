using System.Xml.Serialization;

namespace Scada.Comm.Drivers.DrvRodosBu
{
    [Serializable]
    public class DevTemplate
    {
        public DevTemplate()
        {
            HttpRequest = new List<HttpRequests>();
            Value = new List<Values>();

        }

        [XmlElement] public List<HttpRequests> HttpRequest { get; set; }
        [XmlElement] public List<Values> Value { get; set; }

    }

    public class HttpRequests
    {
        public HttpRequests()
        {
        }

        public HttpRequests(string Uri, string Header, string Content, string ContentType)
        {
            this.Uri = Uri;
            this.Header = Header;
            this.Content = Content;
            this.ContentType = ContentType;
        }

        [XmlAttribute] public string Uri { get; set; }
        [XmlAttribute] public string Header { get; set; }
        [XmlAttribute] public string Content { get; set; }
        [XmlAttribute] public string ContentType { get; set; }
    }

    public class Values
    {
        public Values()
        {
        }

        public Values(bool active, string code, string name) // 
        {
            this.active = active;
            this.code = code;
            this.name = name;
        }

        [XmlAttribute] public bool active { get; set; }
        [XmlAttribute] public string code { get; set; }
        [XmlAttribute] public string name { get; set; }
    }


}
