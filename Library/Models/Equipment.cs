using System;

namespace Library.Models
{
    /// <summary>
    /// Represents an equipment.
    /// </summary>
    public class Equipment
    {
        /// <summary>
        /// A unique identifier for the equipment.
        /// </summary>
        public Guid uuid { get; set; }

        /// <summary>
        /// The name of the equipment.
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// A unique identifier for the equipment.
        /// </summary>
        public string identifier { get; set; }

        /// <summary>
        /// The type of equipment.
        /// </summary>
        public string type { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Equipment"/> class.
        /// </summary>
        /// <param name="uuid">The unique identifier for the equipment.</param>
        /// <param name="name">The name of the equipment.</param>
        /// <param name="identifier">A unique identifier for the equipment.</param>
        /// <param name="type">The type of equipment.</param>
        public Equipment(Guid uuid, string name, string identifier, string type)
        {
            this.uuid = uuid;
            this.name = name;
            this.identifier = identifier;
            this.type = type;
        }
    }
}