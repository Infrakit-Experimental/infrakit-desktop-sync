using System;

namespace Library.Models
{
    /// <summary>
    /// Represents an organization.
    /// </summary>
    public class Organization
    {
        /// <summary>
        /// The unique identifier of the organization.
        /// </summary>
        public int? id { get; set; }

        /// <summary>
        /// The universally unique identifier (UUID) of the organization.
        /// </summary>
        public Guid? uuid { get; set; }

        /// <summary>
        /// The name of the organization.
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Organization"/> class.
        /// </summary>
        /// <param name="name">The name of the organization.</param>
        /// <param name="uuid">The universally unique identifier (UUID) of the organization.</param>
        /// <param name="id">The unique identifier of the organization.</param>
        public Organization(string name, Guid? uuid = null, int? id = null)
        {
            this.uuid = uuid;
            this.name = name;
            this.id = id;
        }
    }
}