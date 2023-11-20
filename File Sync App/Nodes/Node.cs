using File_Sync_App.Nodes.Folders;
using System;
using System.Windows;
using System.Xml;

namespace File_Sync_App.Nodes
{
    public abstract class Node : DependencyObject
    {
        #region variables

        /// <summary>
        /// The name of the folder or file.
        /// </summary>
        public string content { get; set; }

        /// <summary>
        /// Whether or not the node is checked.
        /// </summary>
        public bool? isChecked { get; set; }

        /// <summary>
        /// The parent Folder of this node.
        /// </summary>
        public Folder? parent { get; set; }

        /// <summary>
        /// The depth of the node in the folder structure.
        /// </summary>
        internal int depth;

        /// <summary>
        /// The position of the node.
        /// </summary>
        /// <value>path or uuid</value>
        internal dynamic pos { get; set; }

        #endregion variables

        #region constructors

        /// <summary>
        /// Initializes a new instance of the Node class.
        /// </summary>
        /// <param name="content">The content of the node.</param>
        /// <param name="isChecked">Whether or not the node is checked.</param>
        /// <param name="parent">The parent node of this node.</param>
        protected Node(string content, bool? isChecked, Folder? parent)
        {
            this.content = content;

            if (parent is null)
            {
                depth = 0;
            }
            else
            {
                depth = parent.depth + 1;
                this.parent = parent;
            }

            this.isChecked = isChecked;
        }

        #endregion constructors

        #region im-/export

        /// <summary>
        /// Initializes a new instance of the Node class from an XML node.
        /// </summary>
        /// <param name="xmlNode">The XML node.</param>
        /// <param name="parent">The parent node of this node.</param>
        protected Node(XmlNode xmlNode, Folder? parent)
        {
            if (xmlNode.Attributes is null) return;

            #region name

            var nameNode = xmlNode.Attributes["name"];
            if (nameNode is not null)
            {
                content = nameNode.Value;
            }

            #endregion name

            #region is checked

            var isCheckedNode = xmlNode.Attributes["isChecked"];
            if (isCheckedNode is not null)
            {
                var isCheckedString = isCheckedNode.Value;

                isChecked = null;
                if (!isCheckedString.Equals("Null"))
                {
                    isChecked = Convert.ToBoolean(isCheckedString);
                }
            }

            #endregion is checked

            #region parent

            if (parent is null)
            {
                depth = 0;
            }
            else
            {
                depth = parent.depth + 1;
                this.parent = parent;
            }

            #endregion parent
        }

        /// <summary>
        /// Gets an XML node representation of the node.
        /// </summary>
        /// <param name="doc">The XML document.</param>
        /// <param name="type">The type of the node.</param>
        /// <returns>An XML node representation of the node.</returns>
        internal XmlNode getXmlNode(XmlDocument doc, string type)
        {
            var node = doc.CreateElement(type);

            #region name

            var name = doc.CreateAttribute("name");
            name.Value = content;
            node.Attributes.Append(name);

            #endregion name

            #region is checked

            var isCheckedNode = doc.CreateAttribute("isChecked");
            if (isChecked.HasValue)
            {
                isCheckedNode.Value = isChecked.Value.ToString();
            }
            else
            {
                isCheckedNode.Value = "Null";
            }

            node.Attributes.Append(isCheckedNode);

            #endregion is checked

            return node;
        }

        #endregion im-/export

        /// <summary>
        /// Determines whether or not a node needs to be synced.
        /// </summary>
        /// <returns>
        /// True if the node needs to be synced, False otherwise.
        /// </returns>
        internal bool isToSync()
        {
            if (this.isChecked.HasValue && !this.isChecked.Value) return false;

            return true;
        }
    }
}