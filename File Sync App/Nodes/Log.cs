using Library;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;

namespace File_Sync_App.Nodes
{
    /// <summary>
    /// Represents a log entry for a file sync operation.
    /// </summary>
    public abstract class Log
    {
        /// <summary>
        /// Represents the synchronization status of a folder or file.
        /// </summary>
        public enum SyncStatus
        {
            Error,
            NotSynced,

            NoChanges,

            Added,
            New,

            Deleted,
            NotExisting,
            Removed,

            Changed,

            Added_Synced,
            Synced
        }

        /* Color hierarchy
         * 
         * Error			Red
         * 
         * Deleted			Orange
         * Removed			Orange
         * 
         * NotExisting		Red
         * 
         * Added			Blue
         * New				Blue
         * 
         * Added_Synced     Blue
         * 
         * Changed			Green
         * 
         * Synced			Green
         * 
         * NoChanges		Black
         * 
         * NotSynced		Gray
         * 
         * Exception: NotExisting with NotExisting, NoChanges or NotSynced
         * 
         */

        /// <summary>
        /// A mapping of sync statuses and their local/infranode statuses to brushes.
        /// </summary>
        private static Dictionary<(SyncStatus local, SyncStatus infrakit), Brush> colors = new()
        {
            #region Error

            { (SyncStatus.Error,        SyncStatus.Error),        Brushes.Red },
            { (SyncStatus.Error,        SyncStatus.NotSynced),    Brushes.Red },

            { (SyncStatus.Error,        SyncStatus.NoChanges),    Brushes.Red },

            { (SyncStatus.Error,        SyncStatus.Added),        Brushes.Red },
            { (SyncStatus.Error,        SyncStatus.New),          Brushes.Red },

            { (SyncStatus.Error,        SyncStatus.Deleted),      Brushes.Red },
            { (SyncStatus.Error,        SyncStatus.NotExisting),  Brushes.Red },
            { (SyncStatus.Error,        SyncStatus.Removed),      Brushes.Red },

            { (SyncStatus.Error,        SyncStatus.Changed),      Brushes.Red },

            { (SyncStatus.Error,        SyncStatus.Added_Synced), Brushes.Red },
            { (SyncStatus.Error,        SyncStatus.Synced),       Brushes.Red },

            #endregion Error
            #region NotSynced
            
            { (SyncStatus.NotSynced,    SyncStatus.Error),        Brushes.Red },
            { (SyncStatus.NotSynced,    SyncStatus.NotSynced),    Brushes.Gray },

            { (SyncStatus.NotSynced,    SyncStatus.NoChanges),    Brushes.Black },

            { (SyncStatus.NotSynced,    SyncStatus.Added),        Brushes.Blue },
            { (SyncStatus.NotSynced,    SyncStatus.New),          Brushes.Blue },

            { (SyncStatus.NotSynced,    SyncStatus.Deleted),      Brushes.Orange },
            { (SyncStatus.NotSynced,    SyncStatus.NotExisting),  Brushes.Gray },
            { (SyncStatus.NotSynced,    SyncStatus.Removed),      Brushes.Orange },

            { (SyncStatus.NotSynced,    SyncStatus.Changed),      Brushes.Green },

            { (SyncStatus.NotSynced,    SyncStatus.Added_Synced), Brushes.Blue },
            { (SyncStatus.NotSynced,    SyncStatus.Synced),       Brushes.Green },

            #endregion NotSynced
            
            #region NoChanges
            
            { (SyncStatus.NoChanges,    SyncStatus.Error),        Brushes.Red },
            { (SyncStatus.NoChanges,    SyncStatus.NotSynced),    Brushes.Black },

            { (SyncStatus.NoChanges,    SyncStatus.NoChanges),    Brushes.Black },

            { (SyncStatus.NoChanges,    SyncStatus.Added),        Brushes.Blue },
            { (SyncStatus.NoChanges,    SyncStatus.New),          Brushes.Blue },

            { (SyncStatus.NoChanges,    SyncStatus.Deleted),      Brushes.Orange },
            { (SyncStatus.NoChanges,    SyncStatus.NotExisting),  Brushes.Black },
            { (SyncStatus.NoChanges,    SyncStatus.Removed),      Brushes.Orange },

            { (SyncStatus.NoChanges,    SyncStatus.Changed),      Brushes.Green },

            { (SyncStatus.NoChanges,    SyncStatus.Added_Synced), Brushes.Blue },
            { (SyncStatus.NoChanges,    SyncStatus.Synced),       Brushes.Green },

            #endregion NoChanges
            
            #region Added
            
            { (SyncStatus.Added,        SyncStatus.Error),        Brushes.Red },
            { (SyncStatus.Added,        SyncStatus.NotSynced),    Brushes.Blue },

            { (SyncStatus.Added,        SyncStatus.NoChanges),    Brushes.Blue },

            { (SyncStatus.Added,        SyncStatus.Added),        Brushes.Blue },
            { (SyncStatus.Added,        SyncStatus.New),          Brushes.Blue },

            { (SyncStatus.Added,        SyncStatus.Deleted),      Brushes.Orange },
            { (SyncStatus.Added,        SyncStatus.NotExisting),  Brushes.Red },
            { (SyncStatus.Added,        SyncStatus.Removed),      Brushes.Orange },

            { (SyncStatus.Added,        SyncStatus.Changed),      Brushes.Blue },

            { (SyncStatus.Added,        SyncStatus.Added_Synced), Brushes.Blue },
            { (SyncStatus.Added,        SyncStatus.Synced),       Brushes.Blue },

            #endregion Added
            #region New
            
            { (SyncStatus.New,          SyncStatus.Error),        Brushes.Red },
            { (SyncStatus.New,          SyncStatus.NotSynced),    Brushes.Blue },

            { (SyncStatus.New,          SyncStatus.NoChanges),    Brushes.Blue },

            { (SyncStatus.New,          SyncStatus.Added),        Brushes.Blue },
            { (SyncStatus.New,          SyncStatus.New),          Brushes.Blue },

            { (SyncStatus.New,          SyncStatus.Deleted),      Brushes.Orange },
            { (SyncStatus.New,          SyncStatus.NotExisting),  Brushes.Red },
            { (SyncStatus.New,          SyncStatus.Removed),      Brushes.Orange },

            { (SyncStatus.New,          SyncStatus.Changed),      Brushes.Blue },

            { (SyncStatus.New,          SyncStatus.Added_Synced), Brushes.Blue },
            { (SyncStatus.New,          SyncStatus.Synced),       Brushes.Blue },

            #endregion New
            
            #region Deleted
            
            { (SyncStatus.Deleted,      SyncStatus.Error),        Brushes.Red },
            { (SyncStatus.Deleted,      SyncStatus.NotSynced),    Brushes.Orange },

            { (SyncStatus.Deleted,      SyncStatus.NoChanges),    Brushes.Orange },

            { (SyncStatus.Deleted,      SyncStatus.Added),        Brushes.Orange },
            { (SyncStatus.Deleted,      SyncStatus.New),          Brushes.Orange },

            { (SyncStatus.Deleted,      SyncStatus.Deleted),      Brushes.Orange },
            { (SyncStatus.Deleted,      SyncStatus.NotExisting),  Brushes.Orange },
            { (SyncStatus.Deleted,      SyncStatus.Removed),      Brushes.Orange },

            { (SyncStatus.Deleted,      SyncStatus.Changed),      Brushes.Orange },

            { (SyncStatus.Deleted,      SyncStatus.Added_Synced), Brushes.Orange },
            { (SyncStatus.Deleted,      SyncStatus.Synced),       Brushes.Orange },

            #endregion Deleted
            #region Removed
            
            { (SyncStatus.Removed,      SyncStatus.Error),        Brushes.Red },
            { (SyncStatus.Removed,      SyncStatus.NotSynced),    Brushes.Orange },

            { (SyncStatus.Removed,      SyncStatus.NoChanges),    Brushes.Orange },

            { (SyncStatus.Removed,      SyncStatus.Added),        Brushes.Orange },
            { (SyncStatus.Removed,      SyncStatus.New),          Brushes.Orange },

            { (SyncStatus.Removed,      SyncStatus.Deleted),      Brushes.Orange },
            { (SyncStatus.Removed,      SyncStatus.NotExisting),  Brushes.Orange },
            { (SyncStatus.Removed,      SyncStatus.Removed),      Brushes.Orange },

            { (SyncStatus.Removed,      SyncStatus.Changed),      Brushes.Orange },

            { (SyncStatus.Removed,      SyncStatus.Added_Synced), Brushes.Orange },
            { (SyncStatus.Removed,      SyncStatus.Synced),       Brushes.Orange },

            #endregion Removed
            
            #region NotExisting
            
            { (SyncStatus.NotExisting,  SyncStatus.Error),        Brushes.Red },
            { (SyncStatus.NotExisting,  SyncStatus.NotSynced),    Brushes.Gray },

            { (SyncStatus.NotExisting,  SyncStatus.NoChanges),    Brushes.Black },

            { (SyncStatus.NotExisting,  SyncStatus.Added),        Brushes.Red },
            { (SyncStatus.NotExisting,  SyncStatus.New),          Brushes.Red },

            { (SyncStatus.NotExisting,  SyncStatus.Deleted),      Brushes.Orange },
            { (SyncStatus.NotExisting,  SyncStatus.NotExisting),  Brushes.Gray },
            { (SyncStatus.NotExisting,  SyncStatus.Removed),      Brushes.Orange },

            { (SyncStatus.NotExisting,  SyncStatus.Changed),      Brushes.Red },

            { (SyncStatus.NotExisting,  SyncStatus.Added_Synced), Brushes.Red },
            { (SyncStatus.NotExisting,  SyncStatus.Synced),       Brushes.Red },

            #endregion NotExisting

            #region Changed
            
            { (SyncStatus.Changed,      SyncStatus.Error),        Brushes.Red },
            { (SyncStatus.Changed,      SyncStatus.NotSynced),    Brushes.Green },

            { (SyncStatus.Changed,      SyncStatus.NoChanges),    Brushes.Green },

            { (SyncStatus.Changed,      SyncStatus.Added),        Brushes.Blue },
            { (SyncStatus.Changed,      SyncStatus.New),          Brushes.Blue },

            { (SyncStatus.Changed,      SyncStatus.Deleted),      Brushes.Orange },
            { (SyncStatus.Changed,      SyncStatus.NotExisting),  Brushes.Red },
            { (SyncStatus.Changed,      SyncStatus.Removed),      Brushes.Orange },

            { (SyncStatus.Changed,      SyncStatus.Changed),      Brushes.Green },

            { (SyncStatus.Changed,      SyncStatus.Added_Synced), Brushes.Blue },
            { (SyncStatus.Changed,      SyncStatus.Synced),       Brushes.Green },

            #endregion Changed

            #region Added_Synced

            { (SyncStatus.Added_Synced, SyncStatus.Error),        Brushes.Red },
            { (SyncStatus.Added_Synced, SyncStatus.NotSynced),    Brushes.Blue },

            { (SyncStatus.Added_Synced, SyncStatus.NoChanges),    Brushes.Blue },

            { (SyncStatus.Added_Synced, SyncStatus.Added),        Brushes.Blue },
            { (SyncStatus.Added_Synced, SyncStatus.New),          Brushes.Blue },

            { (SyncStatus.Added_Synced, SyncStatus.Deleted),      Brushes.Orange },
            { (SyncStatus.Added_Synced, SyncStatus.NotExisting),  Brushes.Red },
            { (SyncStatus.Added_Synced, SyncStatus.Removed),      Brushes.Orange },

            { (SyncStatus.Added_Synced, SyncStatus.Changed),      Brushes.Green },

            { (SyncStatus.Added_Synced, SyncStatus.Added_Synced), Brushes.Blue },
            { (SyncStatus.Added_Synced, SyncStatus.Synced),       Brushes.Blue },
                                        
            #endregion Added_Synced     
            #region Synced              
                                        
            { (SyncStatus.Synced,       SyncStatus.Error),        Brushes.Red },
            { (SyncStatus.Synced,       SyncStatus.NotSynced),    Brushes.Green },

            { (SyncStatus.Synced,       SyncStatus.NoChanges),    Brushes.Green },

            { (SyncStatus.Synced,       SyncStatus.Added),        Brushes.Blue },
            { (SyncStatus.Synced,       SyncStatus.New),          Brushes.Blue },

            { (SyncStatus.Synced,       SyncStatus.Deleted),      Brushes.Orange },
            { (SyncStatus.Synced,       SyncStatus.NotExisting),  Brushes.Red },
            { (SyncStatus.Synced,       SyncStatus.Removed),      Brushes.Orange },

            { (SyncStatus.Synced,       SyncStatus.Changed),      Brushes.Green },

            { (SyncStatus.Synced,       SyncStatus.Added_Synced), Brushes.Blue },
            { (SyncStatus.Synced,       SyncStatus.Synced),       Brushes.Green },

            #endregion Synced
        };

        /// <summary>
        /// A mapping of sync statuses to their localized strings
        /// </summary>
        private static readonly Dictionary<SyncStatus, string> translation = new()
        {
            { SyncStatus.Error,        "syncStatus.error" },

            { SyncStatus.NotSynced,    "syncStatus.notSynced" },
            { SyncStatus.NoChanges,    "syncStatus.noChanges" },

            { SyncStatus.Added,        "syncStatus.added" },
            { SyncStatus.New,          "syncStatus.new" },

            { SyncStatus.Deleted,      "syncStatus.deleted" },
            { SyncStatus.NotExisting,  "syncStatus.notExisting" },
            { SyncStatus.Removed,      "syncStatus.removed" },

            { SyncStatus.Changed,      "syncStatus.changed" },

            { SyncStatus.Added_Synced, "syncStatus.added_Synced" },
            { SyncStatus.Synced,       "syncStatus.synced" }
        };

        #region variables

        /// <summary>
        /// The timestamp of the log entry.
        /// </summary>
        public DateTime timestamp { get; set; }

        /// <summary>
        /// The synchronization status of the file on the local machine.
        /// </summary>
        public SyncStatus statusLocal { get; set; }

        /// <summary>
        /// The synchronization status of the file on Infrakit.
        /// </summary>
        public SyncStatus statusInfrakit { get; set; }

        /// <summary>
        /// The name of the folder or file.
        /// </summary>
        public string content { get; set; }


        #endregion variables

        #region constructors

        /// <summary>
        /// Initializes a new instance of the Log class.
        /// </summary>
        /// <param name="content">The content of the log entry.</param>
        /// <param name="statusLocal">The synchronization status of the file on the local machine.</param>
        /// <param name="statusInfrakit">The synchronization status of the file on Infrakit.</param>
        /// <param name="timestamp">The timestamp of the log entry. If null, the current time will be used.</param>
        protected Log(string content, SyncStatus statusLocal, SyncStatus statusInfrakit, DateTime? timestamp)
        {
            if (timestamp.HasValue)
            {
                this.timestamp = timestamp.Value;
            }
            else
            {
                this.timestamp = DateTime.Now;
            }

            this.statusLocal = statusLocal;
            this.statusInfrakit = statusInfrakit;
            this.content = content;
        }

        #endregion constructors

        #region im-/export

        /// <summary>
        /// Initializes a new instance of the Log class from an XML node.
        /// </summary>
        /// <param name="xmlNode">The XML node to deserialize the log entry from.</param>
        protected Log(XmlNode xmlNode)
        {
            if (xmlNode.Attributes is null) return;

            #region name

            var nameNode = xmlNode.Attributes["name"];
            if (nameNode is not null)
            {
                content = nameNode.Value;
            }

            #endregion name

            #region timestamp

            var timestampNode = xmlNode.Attributes["timestamp"];
            if (timestampNode is not null)
            {
                timestamp = DateTime.Parse(timestampNode.Value, CultureInfo.InvariantCulture);
            }

            #endregion timestamp

            #region status local

            var statusLocalNode = xmlNode.Attributes["statusLocal"];
            if (statusLocalNode is not null)
            {
                statusLocal = (SyncStatus)Enum.Parse(typeof(SyncStatus), statusLocalNode.Value);
            }

            #endregion status local

            #region status Infrakit

            var statusInfrakitNode = xmlNode.Attributes["statusInfrakit"];
            if (statusInfrakitNode is not null)
            {
                statusInfrakit = (SyncStatus)Enum.Parse(typeof(SyncStatus), statusInfrakitNode.Value);
            }

            #endregion status Infrakit
        }

        /// <summary>
        /// Serializes the log entry to an XML node of the specified type.
        /// </summary>
        /// <param name="doc">The XML document to create the node in.</param>
        /// <param name="type">The type of the XML node to create.</param>
        /// <returns>The serialized XML node.</returns>
        internal XmlNode getXmlNode(XmlDocument doc, string type)
        {
            var node = doc.CreateElement(type);

            var name = doc.CreateAttribute("name");
            name.Value = content;
            node.Attributes.Append(name);

            var timestampNode = doc.CreateAttribute("timestamp");
            timestampNode.Value = timestamp.ToString(CultureInfo.InvariantCulture);
            node.Attributes.Append(timestampNode);

            var statusLocalNode = doc.CreateAttribute("statusLocal");
            statusLocalNode.Value = statusLocal.ToString();
            node.Attributes.Append(statusLocalNode);

            var statusInfrakitNode = doc.CreateAttribute("statusInfrakit");
            statusInfrakitNode.Value = statusInfrakit.ToString();
            node.Attributes.Append(statusInfrakitNode);

            return node;
        }

        /// <summary>
        /// Serializes the log entry to an XML node.
        /// </summary>
        /// <param name="doc">The XML document to create the node in.</param>
        /// <returns>The serialized XML node.</returns>
        internal abstract XmlNode getXmlNode(XmlDocument doc);

        #endregion im-/export

        /// <summary>
        /// Gets a TreeViewItem representing the log entry.
        /// </summary>
        /// <returns>The TreeViewItem.</returns>
        protected TreeViewItem getTVItem()
        {
            var item = new TreeViewItem();

            item.Header = content + " (" + Utils.Language.getMessage("syncStatus.local") + ": " + Utils.Language.getMessage(translation[statusLocal]) + " | " + Utils.Language.getMessage("syncStatus.infrakit") + ": " + Utils.Language.getMessage(translation[statusInfrakit]) + ")";
            item.ToolTip = timestamp.ToString();

            item.Foreground = colors[(statusLocal, statusInfrakit)];

            return item;
        }

        #region (child) classes

        /// <summary>
        /// Represents a synchronization operation.
        /// </summary>
        public class Sync
        {
            #region variables

            /// <summary>
            /// A list of all the links that were synchronized.
            /// </summary>
            protected List<Link> _links;

            /// <summary>
            /// Getter for the list of all the links that were synchronized.
            /// </summary>
            internal List<Link> links
            {
                get { return _links; }
            }

            /// <summary>
            /// The timestamp of the synchronization operation.
            /// </summary>
            public DateTime timestamp;

            #endregion variables

            #region constructors

            /// <summary>
            /// Initializes a new instance of the Sync class.
            /// </summary>
            public Sync()
            {
                _links = new List<Link>();
                timestamp = DateTime.Now;
            }

            #endregion constructors

            #region im-/export

            /// <summary>
            /// Initializes a new instance of the Sync class from an XML node.
            /// </summary>
            /// <param name="xmlNode">The XML node to deserialize the synchronization operation from.</param>
            /// <param name="date">The date of the synchronization operation.</param>
            public Sync(XmlNode xmlNode, DateOnly date)
            {
                _links = new();

                if (xmlNode.Attributes is not null)
                {
                    #region timestamp

                    var timeNode = xmlNode.Attributes["time"];
                    if (timeNode is not null)
                    {
                        var time = TimeOnly.ParseExact(timeNode.Value, "HH:mm:ss", CultureInfo.InvariantCulture);

                        timestamp = date.ToDateTime(time);
                    }

                    #endregion timestamp
                }

                #region children

                if (!xmlNode.HasChildNodes) return;

                foreach (var child in xmlNode.ChildNodes)
                {
                    var link = new Link((XmlNode)child);

                    links.Add(link);
                }

                #endregion children
            }

            /// <summary>
            /// Serializes the synchronization operation to an XML node.
            /// </summary>
            /// <param name="doc">The XML document to create the node in.</param>
            /// <returns>The serialized XML node.</returns>
            internal XmlNode getXmlNode(XmlDocument doc)
            {
                var node = doc.CreateElement("Sync");

                var time = doc.CreateAttribute("time");
                time.Value = timestamp.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
                node.Attributes.Append(time);

                foreach (var log in _links)
                {
                    node.AppendChild(log.getXmlNode(doc));
                }

                return node;
            }

            #endregion im-/export

            /// <summary>
            /// Adds a link to the synchronization operation.
            /// </summary>
            /// <param name="link">The link to add.</param>
            internal void add(Link link)
            {
                _links.Add(link);
            }

            /// <summary>
            /// Exports the synchronization operation to an XML file.
            /// </summary>
            internal void export()
            {
                var path = Path.Combine(Utils.Log.directory, timestamp.ToString("yyyyMMdd", CultureInfo.InvariantCulture) + ".xml");
                var doc = new XmlDocument();

                XmlNode body;

                if (System.IO.File.Exists(path))
                {
                    doc.Load(path);

                    var selection = doc.DocumentElement.SelectSingleNode("/body");

                    if (selection is null)
                    {
                        body = doc.CreateElement("body");
                    }
                    else
                    {
                        body = selection;
                    }
                }
                else
                {
                    XmlDeclaration xmldecl;
                    xmldecl = doc.CreateXmlDeclaration("1.0", "UTF-8", null);

                    doc.InsertBefore(xmldecl, doc.DocumentElement);

                    body = doc.CreateElement("body");

                    doc.AppendChild(body);
                }

                var node = getXmlNode(doc);

                body.AppendChild(node);

                doc.Save(path);
            }
        }

        /// <summary>
        /// Represents a log for the link between a local folder and an Infrakit folder.
        /// </summary>
        public class Link : Folder
        {
            #region variables

            /// <summary>
            /// The root name of the local folder.
            /// </summary>
            protected string rootLocal;

            /// <summary>
            /// The root name of the Infrakit folder.
            /// </summary>
            protected string rootInfrakit;


            #endregion variables

            #region constructors

            /// <summary>
            /// Initializes a new instance of the Link class.
            /// </summary>
            /// <param name="content">The content of the link.</param>
            /// <param name="statusLocal">The synchronization status of the link on the local machine.</param>
            /// <param name="statusInfrakit">The synchronization status of the link on Infrakit.</param>
            /// <param name="rootLocal">The root path of the local folder.</param>
            /// <param name="rootInfrakit">The root path of the Infrakit folder.</param>
            /// <param name="timestamp">The timestamp of the link. If null, the current time will be used.</param>
            public Link(string content, SyncStatus statusLocal, SyncStatus statusInfrakit, string rootLocal, string rootInfrakit, DateTime? timestamp = null) : base(content, statusLocal, statusInfrakit, timestamp)
            {
                children = new List<Log>();

                this.rootLocal = rootLocal;
                this.rootInfrakit = rootInfrakit;
            }

            #endregion constructors

            #region im-/export

            /// <summary>
            /// Initializes a new instance of the Link class from an XML node.
            /// </summary>
            /// <param name="xmlNode">The XML node to deserialize the link from.</param>
            public Link(XmlNode xmlNode) : base(xmlNode)
            {
                if (xmlNode.Attributes is not null)
                {
                    #region rootLocal

                    var rootLocalNode = xmlNode.Attributes["rootLocal"];
                    if (rootLocalNode is not null)
                    {
                        rootLocal = rootLocalNode.Value;
                    }

                    #endregion rootLocal

                    #region rootInfrakit

                    var rootInfrakitNode = xmlNode.Attributes["rootInfrakit"];
                    if (rootInfrakitNode is not null)
                    {
                        rootInfrakit = rootInfrakitNode.Value;
                    }

                    #endregion rootInfrakit
                }
            }

            /// <summary>
            /// Serializes the link to an XML node.
            /// </summary>
            /// <param name="doc">The XML document to create the node in.</param>
            /// <returns>The serialized XML node.</returns>
            internal override XmlNode getXmlNode(XmlDocument doc)
            {
                var node = getXmlNode(doc, "Link");

                var rootLocalNode = doc.CreateAttribute("rootLocal");
                rootLocalNode.Value = rootLocal;
                node.Attributes.Append(rootLocalNode);

                var rootInfrakitNode = doc.CreateAttribute("rootInfrakit");
                rootInfrakitNode.Value = rootInfrakit;
                node.Attributes.Append(rootInfrakitNode);

                return node;
            }

            #endregion im-/export

            /// <summary>
            /// Gets a TreeViewItem representing the link.
            /// </summary>
            /// <returns>The TreeViewItem.</returns>
            internal new TreeViewItem getTVItem()
            {
                var item = base.getTVItem();

                item.ToolTip += " | " + rootLocal + ", " + rootInfrakit;

                return item;
            }
        }

        /// <summary>
        /// Represents a folder in the file sync log.
        /// </summary>
        public class Folder : Log
        {
            #region variables

            /// <summary>
            /// A list of all the logs of the children of the folder.
            /// </summary>
            internal List<Log> children;

            #endregion variables

            #region constructors

            /// <summary>
            /// Initializes a new instance of the Folder class.
            /// </summary>
            /// <param name="content">The content of the folder.</param>
            /// <param name="statusLocal">The synchronization status of the folder on the local machine.</param>
            /// <param name="statusInfrakit">The synchronization status of the folder on Infrakit.</param>
            /// <param name="timestamp">The timestamp of the folder. If null, the current time will be used.</param>
            internal Folder(string content, SyncStatus statusLocal, SyncStatus statusInfrakit, DateTime? timestamp = null) : base(content, statusLocal, statusInfrakit, timestamp)
            {
                children = new();
            }

            #endregion constructors

            #region im-/export

            /// <summary>
            /// Initializes a new instance of the Folder class from an XML node.
            /// </summary>
            /// <param name="xmlNode">The XML node to deserialize the folder from.</param>
            public Folder(XmlNode xmlNode) : base(xmlNode)
            {
                children = new();

                if (xmlNode.Attributes is null) return;

                #region children

                if (!xmlNode.HasChildNodes) return;

                foreach (var child in xmlNode.ChildNodes)
                {
                    switch (((XmlNode)child).Name)
                    {
                        case "Folder":
                            children.Add(new Folder((XmlNode)child));
                            break;

                        case "File":
                            children.Add(new File((XmlNode)child));
                            break;
                    }
                }

                #endregion children
            }

            /// <summary>
            /// Serializes the folder to an XML node.
            /// </summary>
            /// <param name="doc">The XML document to create the node in.</param>
            /// <returns>The serialized XML node.</returns>
            internal override XmlNode getXmlNode(XmlDocument doc)
            {
                return getXmlNode(doc, "Folder");
            }

            /// <summary>
            /// Serializes the folder to an XML node of the specified type.
            /// </summary>
            /// <param name="doc">The XML document to create the node in.</param>
            /// <param name="type">The type of the XML node to create.</param>
            /// <returns>The serialized XML node.</returns>
            protected XmlNode getXmlNode(XmlDocument doc, string type)
            {
                var node = base.getXmlNode(doc, type);

                if (children is not null)
                {
                    foreach (var child in children)
                    {
                        XmlNode childNode;

                        switch (child)
                        {
                            case Folder f:
                                node.AppendChild(f.getXmlNode(doc));
                                break;

                            case File f:
                                node.AppendChild(f.getXmlNode(doc));
                                break;
                        }
                    }
                }

                return node;
            }

            #endregion im-/export

            /// <summary>
            /// Gets a TreeViewItem representing the folder.
            /// </summary>
            /// <returns>The TreeViewItem.</returns>
            internal new TreeViewItem getTVItem()
            {
                var item = base.getTVItem();

                foreach (var child in children)
                {
                    switch (child)
                    {
                        case Folder f:
                            item.Items.Add(f.getTVItem());
                            break;

                        case File f:
                            item.Items.Add(f.getTVItem());
                            break;
                    }
                }

                return item;
            }

            /// <summary>
            /// Converts the folder to a link.
            /// </summary>
            /// <param name="name">The name of the link.</param>
            /// <param name="rootLocal">The root path of the local folder.</param>
            /// <param name="rootInfrakit">The root path of the Infrakit folder.</param>
            /// <returns>The link.</returns>
            internal Link toLink(string name, string rootLocal, string rootInfrakit)
            {
                var link = new Link(name, statusLocal, statusInfrakit, rootLocal, rootInfrakit, timestamp);

                link.children.AddRange(children);

                return link;
            }
        }

        /// <summary>
        /// Represents a file in the file sync log.
        /// </summary>
        public class File : Log
        {
            #region constructors

            /// <summary>
            /// Initializes a new instance of the File class.
            /// </summary>
            /// <param name="content">The content of the file.</param>
            /// <param name="statusLocal">The synchronization status of the file on the local machine.</param>
            /// <param name="statusInfrakit">The synchronization status of the file on Infrakit.</param>
            /// <param name="timestamp">The timestamp of the file. If null, the current time will be used.</param>
            internal File(string content, SyncStatus statusLocal, SyncStatus statusInfrakit, DateTime? timestamp = null) : base(content, statusLocal, statusInfrakit, timestamp) { }

            #endregion constructors

            #region im-/export

            /// <summary>
            /// Initializes a new instance of the File class from an XML node.
            /// </summary>
            /// <param name="xmlNode">The XML node to deserialize the file from.</param>
            public File(XmlNode xmlNode) : base(xmlNode) { }

            /// <summary>
            /// Serializes the file to an XML node.
            /// </summary>
            /// <param name="doc">The XML document to create the node in.</param>
            /// <returns>The serialized XML node.</returns>
            internal override XmlNode getXmlNode(XmlDocument doc)
            {
                var node = getXmlNode(doc, "File");

                return node;
            }

            #endregion im-/export
        }

        #endregion (child) classes
    }
}