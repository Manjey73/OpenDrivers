using Scada.Comm.Devices;
using Scada.ComponentModel;
using System.Collections;
using System.Xml;
using NCM = System.ComponentModel;

namespace Scada.Comm.Drivers.DrvDS18B20.Config
{
    /// <summary>
    /// Represents a variable configuration.
    /// <para>Представляет конфигурацию переменной.</para>
    /// </summary>
    [Serializable]
    internal class VariableConfig : ITreeNode
    {
        /// <summary>
        /// Gets or sets the variable active.
        /// </summary>
        [DisplayName, Category, Description]
        public bool Active { get; set; } = true;

        /// <summary>
        /// Gets or sets the variable name.
        /// </summary>
        [DisplayName, Category, Description]
        public string Name { get; set; } = "";

        /// <summary>
        /// Gets or sets the tag code associated with the variable.
        /// </summary>
        [DisplayName, Category, Description]
        public string TagCode { get; set; } = "";

        /// <summary>
        /// Gets or sets the object identifier.
        /// </summary>
        [DisplayName, Category, Description]
        public string DsId { get; set; } = "";

        /// <summary>
        /// Gets or sets the parent tree node.
        /// </summary>
        [NCM.Browsable(false)]
        [field: NonSerialized]
        public ITreeNode Parent { get; set; }

        /// <summary>
        /// Gets the child tree nodes.
        /// </summary>
        [NCM.Browsable(false)]
        public IList Children => null;


        /// <summary>
        /// Loads the configuration from the XML node.
        /// </summary>
        public void LoadFromXml(XmlElement xmlElem)
        {
            ArgumentNullException.ThrowIfNull(xmlElem, nameof(xmlElem));
            Active = xmlElem.GetAttrAsBool("active");
            Name = xmlElem.GetAttrAsString("name");
            TagCode = xmlElem.GetAttrAsString("tagCode");
            DsId = xmlElem.GetAttrAsString("dsId");
        }

        /// <summary>
        /// Saves the configuration into the XML node.
        /// </summary>
        public void SaveToXml(XmlElement xmlElem)
        {
            ArgumentNullException.ThrowIfNull(xmlElem, nameof(xmlElem));
            xmlElem.SetAttribute("active", Active);
            xmlElem.SetAttribute("name", Name);
            xmlElem.SetAttribute("tagCode", TagCode);
            xmlElem.SetAttribute("dsId", DsId);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        public override string ToString()
        {
            return string.Format("{0} ({1})", Name, DsId);
        }
    }
}
