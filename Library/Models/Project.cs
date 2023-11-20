using System;

namespace Library.Models
{
    /// <summary>
    /// Represents a project.
    /// </summary>
    public class Project
    {
        /// <summary>
        /// The unique identifier of the project.
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// The universally unique identifier (UUID) of the project.
        /// </summary>
        public Guid uuid { get; set; }

        /// <summary>
        /// The name of the project.
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// The timestamp of the project.
        /// </summary>
        public long? timestamp { get; set; }

        /// <summary>
        /// The organization to which the project belongs.
        /// </summary>
        public Organization? organization { get; set; }

        /// <summary>
        /// The coordinate system of the project.
        /// </summary>
        public CoordinateSystem? coordinateSystem { get; set; }

        /// <summary>
        /// The height system of the project.
        /// </summary>
        public HeightSystem? heightSystem { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Project"/> class.
        /// </summary>
        /// <param name="id">The unique identifier of the project.</param>
        /// <param name="uuid">The universally unique identifier (UUID) of the project.</param>
        /// <param name="name">The name of the project.</param>
        /// <param name="timestamp">The timestamp of the project.</param>
        /// <param name="organization">The organization to which the project belongs.</param>
        /// <param name="coordinateSystem">The coordinate system of the project.</param>
        /// <param name="heightSystem">The height system of the project.</param>
        public Project(int id, Guid uuid, string name, long? timestamp = null, Organization? organization = null, CoordinateSystem? coordinateSystem = null, HeightSystem? heightSystem = null)
        {
            this.id = id;
            this.uuid = uuid;
            this.name = name;
            this.timestamp = timestamp;

            this.organization = organization;
            this.coordinateSystem = coordinateSystem;
            this.heightSystem = heightSystem;
        }
    }
}