using File_Sync_App.Nodes;
using File_Sync_App.Nodes.Folders;
using Library;
using Library.Models;
using System;
using System.Xml;
using static Library.Utils;

namespace File_Sync_App
{
    /// <summary>
    /// Represents a link between a local folder and an Infrakit folder.
    /// </summary>
    public class Link
    {
        #region variables

        /// <summary>
        /// The name of the link.
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// The local folder associated with the link.
        /// </summary>
        internal LFolder localData;

        /// <summary>
        /// The Infrakit folder associated with the link.
        /// </summary>
        internal IKFolder infrakitData;

        /// <summary>
        /// The Infrakit project associated with the link.
        /// </summary>
        internal Project infrakitProject;

        /// <summary>
        /// Indicates whether to sync uploads to the Infrakit folder.
        /// </summary>
        internal bool syncUpload;

        /// <summary>
        /// Indicates whether to sync downloads from the Infrakit folder.
        /// </summary>
        internal bool syncDownload;

        /// <summary>
        /// Indicates whether the link is active.
        /// </summary>
        public bool active { get; set; }

        #endregion variables

        /// <summary>
        /// Initializes a new instance of the Link class.
        /// </summary>
        /// <param name="name">The name of the link.</param>
        /// <param name="localData">The local folder associated with the link.</param>
        /// <param name="infrakitData">The Infrakit folder associated with the link.</param>
        /// <param name="active">Indicates whether the link is active.</param>
        /// <param name="syncUpload">Indicates whether to sync uploads to the Infrakit folder.</param>
        /// <param name="syncDowload">Indicates whether to sync downloads from the Infrakit folder.</param>
        /// <param name="infrakitProject">The Infrakit project associated with the link.</param>
        internal Link(string name, LFolder localData, IKFolder infrakitData, bool active, bool syncUpload, bool syncDowload, Project infrakitProject)
        {
            this.name = name;

            this.localData = localData;
            this.infrakitData = infrakitData;
            this.infrakitProject = infrakitProject;

            this.active = active;

            this.syncUpload = syncUpload;
            this.syncDownload = syncDowload;
        }

        #region im-/export Nodes

        /// <summary>
        /// Initializes a new instance of the Link class from an XML node.
        /// </summary>
        /// <param name="node">The XML node containing the link data.</param>
        internal Link(XmlNode node)
        {
            var name = node.Attributes[0];
            this.name = name.InnerText;

            var syncUpload = node.SelectSingleNode("upload");
            this.syncUpload = Boolean.Parse(syncUpload.InnerText);

            var syncDownload = node.SelectSingleNode("download");
            this.syncDownload = Boolean.Parse(syncDownload.InnerText);

            var active = node.SelectSingleNode("active");
            this.active = Boolean.Parse(active.InnerText);

            var localNode = node.SelectSingleNode("localData");
            this.localData = new LFolder(localNode.FirstChild);

            var infrakitNode = node.SelectSingleNode("infrakitData");
            this.infrakitData = new IKFolder(infrakitNode.FirstChild);

            #region read Project

            var project = infrakitNode.SelectSingleNode("project");

            var projectNameNode = project.SelectSingleNode("projectName");
            var projectName = projectNameNode.InnerText;

            var projectIDNode = project.SelectSingleNode("projectID");
            var projectID = int.Parse(projectIDNode.InnerText);

            var projectUuidNode = project.SelectSingleNode("projectUuid");
            var projectUuid = new Guid(projectUuidNode.InnerText);

            this.infrakitProject = new Project(projectID, projectUuid, projectName);

            #endregion read Project
        }

        /// <summary>
        /// Gets an XML node representing the link.
        /// </summary>
        /// <returns>An XML node representing the link.</returns>
        internal (XmlDocument doc, XmlNode node) getXmlNode()
        {
            var resources = Resources.get();

            var node = resources.CreateElement("link");

            #region name attribute

            var name = resources.CreateAttribute("name");
            name.Value = this.name;
            node.Attributes.Append(name);

            #endregion name attribute

            #region local node

            var localData = resources.CreateElement("localData");
            localData.AppendChild(this.localData.getXmlNode(resources));
            node.AppendChild(localData);

            #endregion local node

            #region infrakit node

            var infrakitData = resources.CreateElement("infrakitData");
            infrakitData.AppendChild(this.infrakitData.getXmlNode(resources));

            #region add Project

            var project = resources.CreateElement("project");

            var projectName = resources.CreateElement("projectName");
            projectName.InnerText = this.infrakitProject.name;
            project.AppendChild(projectName);

            var projectID = resources.CreateElement("projectID");
            projectID.InnerText = "" + this.infrakitProject.id;
            project.AppendChild(projectID);

            var projectUuid = resources.CreateElement("projectUuid");
            projectUuid.InnerText = this.infrakitProject.uuid.ToString();
            project.AppendChild(projectUuid);

            infrakitData.AppendChild(project);

            #endregion add Project

            node.AppendChild(infrakitData);

            #endregion infrakit node

            #region upload node

            var syncUpload = resources.CreateElement("upload");
            syncUpload.InnerText = this.syncUpload.ToString();
            node.AppendChild(syncUpload);

            #endregion upload node

            #region download node

            var syncDownload = resources.CreateElement("download");
            syncDownload.InnerText = this.syncDownload.ToString();
            node.AppendChild(syncDownload);

            #endregion download node

            #region active node

            var active = resources.CreateElement("active");
            active.InnerText = this.active.ToString();
            node.AppendChild(active);

            #endregion active node

            return (resources, node);
        }

        #endregion im-/export Nodes

        /// <summary>
        /// Syncs the local folder and Infrakit folder associated with the link.
        /// </summary>
        /// <returns>A <see cref="Log.Link"/> object containing the results of the sync operation.</returns>
        internal Nodes.Log.Link Sync()
        {
            var lData = this.localData.getRoot();
            var ikData = this.infrakitData.getRoot();

            try
            {
                Utils.Log.write("log.sync.start: \"" + this.name + "\"");
                Settings.defaultFileSync = null;

                Nodes.Log.Folder logRoot;

                if (this.syncUpload && this.syncDownload)
                {
                    logRoot = Nodes.Folders.Folder.sync(lData, ikData);
                }
                else if (this.syncUpload)
                {
                    logRoot = lData.sync(ikData);
                }
                else
                {
                    logRoot = ikData.sync(lData);
                }

                var linkNode = this.getXmlNode();
                Resources.edit(linkNode.doc, linkNode.node, "link[@name='" + this.name + "']");

                Utils.Log.write("log.sync.end: \"" + this.name + "\"");

                return logRoot.toLink(this.name, lData.content, ikData.content);
            }
            catch (OutOfMemoryException ex)
            {
                Utils.Log.write("log.sync.outOfMemoryException: \"" + ex.Message + "\"");

                Utils.Log.write("log.sync.end: \"" + this.name + "\"");
                return new Nodes.Log.Link(this.name, Nodes.Log.SyncStatus.Error, Nodes.Log.SyncStatus.Error, lData.content, ikData.content, DateTime.Now);
            }
            catch (Exception ex)
            {
                Utils.Log.write("log.sync.exception: \"" + ex.Message + "\"");

                Utils.Log.write("log.sync.end: \"" + this.name + "\"");
                return new Nodes.Log.Link(this.name, Nodes.Log.SyncStatus.Error, Nodes.Log.SyncStatus.Error, lData.content, ikData.content, DateTime.Now);
            }
        }
    }
}