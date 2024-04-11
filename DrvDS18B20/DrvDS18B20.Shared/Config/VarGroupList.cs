using System.Collections;

namespace Scada.Comm.Drivers.DrvDS18B20.Config
{
    /// <summary>
    /// Represents a list of variable groups.
    /// <para>Представляет список групп переменных.</para>
    /// </summary>
    [Serializable]
    internal class VarGroupList : List<VarGroupConfig>, ITreeNode
    {
        /// <summary>
        /// Gets or sets the parent tree node.
        /// </summary>
        ITreeNode ITreeNode.Parent
        {
            get => null;
            set => throw new InvalidOperationException();
        }

        /// <summary>
        /// Gets the child tree nodes.
        /// </summary>
        IList ITreeNode.Children => this;
    }
}
