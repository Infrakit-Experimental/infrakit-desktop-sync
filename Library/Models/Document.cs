using MimeTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace Library.Models
{
    /// <summary>
    /// Represents a document.
    /// </summary>
    public class Document
    {
        /// <summary>
        /// The unique identifier of the document.
        /// </summary>
        public Guid uuid { get; set; }

        /// <summary>
        /// The name of the document.
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// Version sequence head UUID (aka. “version group”) of a document.
        /// </summary>
        public Guid headUuid { get; set; }

        /// <summary>
        /// Gets or sets the path of the folder.
        /// </summary>
        public string folderPath { get; set; }

        /// <summary>
        /// A dictionary of properties and values associated with the document.
        /// </summary>
        public Dictionary<string, List<string>> properties { get; set; }

        /// <summary>
        /// The timestamp of the last time the document was modified.
        /// </summary>
        public DateTime timestamp { get; set; }

        /// <summary>
        /// The version of the document.
        /// </summary>
        public int version { get; set; }

        /// <summary>
        /// The MD5 checksum of the document, as a hexadecimal string.
        /// </summary>
        public string md5 { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Document"/> class.
        /// </summary>
        /// <param name="uuid">The unique identifier of the document.</param>
        /// <param name="name">The name of the document.</param>
        /// <param name="headUuid">The unique identifier of the head document of the document tree.</param>
        /// <param name="folderPath">The path of the folder where the document is stored.</param>
        /// <param name="properties">A dictionary of properties and values associated with the document.</param>
        /// <param name="timestamp">The timestamp of the last time the document was modified.</param>
        /// <param name="version">The version of the document.</param>
        /// <param name="md5">The MD5 checksum of the document, as a hexadecimal string.</param>
        public Document(Guid uuid, string name, Guid headUuid, string folderPath, Dictionary<string, List<string>> properties, DateTime timestamp, int version, string md5)
        {
            this.uuid = uuid;
            this.name = name;

            this.headUuid = headUuid;

            this.folderPath = folderPath;

            this.properties = properties;

            this.timestamp = timestamp;

            this.version = version;

            this.md5 = md5;
        }

        /// <summary>
        /// Represents information about a document upload.
        /// </summary>
        public class Upload
        {
            /// <summary>
            /// The filename of the uploaded file.
            /// </summary>
            public string filename;

            /// <summary>
            /// The unique identifier of the folder in which the file was uploaded.
            /// </summary>
            public Guid folderUuid;

            /// <summary>
            /// The content type of the uploaded file.
            /// </summary>
            public string contentType;

            /// <summary>
            /// The MD5 checksum of the uploaded file, as a hexadecimal string.
            /// </summary>
            public string checksum;

            /// <summary>
            /// The size of the uploaded file in bytes.
            /// </summary>
            public long size;

            /// <summary>
            /// Initializes a new instance of the <see cref="Upload"/> class.
            /// </summary>
            /// <param name="filePath">The path to the file to upload.</param>
            /// <param name="folderUuid">The unique identifier of the folder in which to upload the file.</param>
            public Upload(string filePath, Guid folderUuid)
            {
                this.filename = Path.GetFileName(filePath);
                this.folderUuid = folderUuid;

                var extension = Path.GetExtension(filePath);
                extension = extension.Substring(1);
                
                if(extension.Equals("geojson", StringComparison.InvariantCultureIgnoreCase))
                {
                    this.contentType = "text/plain";
                }
                else
                {
                    this.contentType = MimeTypeMap.GetMimeType(extension);
                }

                this.size = new FileInfo(filePath).Length;

                using (var md5Stream = MD5.Create())
                {
                    using (var stream = File.OpenRead(filePath))
                    {
                        var md5Result = md5Stream.ComputeHash(stream);
                        this.checksum = Convert.ToHexString(md5Result);
                    }
                }
            }
        }
    }
}