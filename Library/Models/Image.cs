using System;

namespace Library.Models
{
    /// <summary>
    /// A class that represents an image.
    /// </summary>
    public class Image
    {
        /// <summary>
        /// The unique identifier of the image.
        /// </summary>
        public Guid uuid { get; set; }

        /// <summary>
        /// The name of the image.
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// The description of the image.
        /// </summary>
        public string description { get; set; }

        /// <summary>
        /// The timestamp of when the image was created.
        /// </summary>
        public long timestamp { get; set; }

        /// <summary>
        /// The original date of the image, if known.
        /// </summary>
        public long? originalDate { get; set; }

        /// <summary>
        /// The geographic point where the image was taken, if known.
        /// </summary>
        public GeographicPoint geographicPoint { get; set; }

        /// <summary>
        /// Whether the image is a panorama.
        /// </summary>
        public bool? panorama { get; set; }

        /// <summary>
        /// The creator of the image.
        /// </summary>
        public Creator creator { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Image"/> class.
        /// </summary>
        /// <param name="uuid">The unique identifier of the image.</param>
        /// <param name="name">The name of the image.</param>
        /// <param name="description">The description of the image.</param>
        /// <param name="timestamp">The timestamp of when the image was created.</param>
        /// <param name="originalDate">The original date of the image, if known.</param>
        /// <param name="geographicPoint">The geographic point where the image was taken, if known.</param>
        /// <param name="panorama">Whether the image is a panorama.</param>
        /// <param name="creator">The creator of the image.</param>
        public Image(Guid uuid, string name, string description, long timestamp, long? originalDate, (double lat, double lon, long elevation) geographicPoint, bool? panorama, (string username, Guid uuid) creator)
        {
            this.uuid = uuid;
            this.name = name;
            this.description = description;
            this.timestamp = timestamp;
            this.originalDate = originalDate;
            this.geographicPoint = new GeographicPoint(geographicPoint.lat, geographicPoint.lon, geographicPoint.elevation);
            this.panorama = panorama;
            this.creator = new Creator(creator.username, creator.uuid);
        }

        /// <summary>
        /// A class that represents the creator of an image.
        /// </summary>
        public class Creator
        {
            /// <summary>
            /// The username of the creator.
            /// </summary>
            public string username { get; set; }

            /// <summary>
            /// The unique identifier of the creator.
            /// </summary>
            public Guid uuid { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="Creator"/> class.
            /// </summary>
            /// <param name="username">The username of the creator.</param>
            /// <param name="uuid">The unique identifier of the creator.</param>
            public Creator(string username, Guid uuid)
            {
                this.username = username;
                this.uuid = uuid;
            }
        }
    }
}