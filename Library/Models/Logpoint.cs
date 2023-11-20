using System;

namespace Library.Models
{
    /// <summary>
    /// A class that represents an logpoint.
    /// </summary>
    public class Logpoint
    {
        /// <summary>
        /// The unique identifier of the logpoint.
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// The timestamp of the logpoint.
        /// </summary>
        public DateTime timestamp { get; set; }

        /// <summary>
        /// The name of the logpoint.
        /// </summary>
        public string? pointName { get; set; }

        /// <summary>
        /// The number of the logpoint.
        /// </summary>
        public string? pointNumber { get; set; }

        /// <summary>
        /// The station of the logpoint.
        /// </summary>
        public double? station { get; set; }

        /// <summary>
        /// The name of the alignment of the logpoint.
        /// </summary>
        public string? alignmentName { get; set; }

        /// <summary>
        /// The ID of the alignment of the logpoint.
        /// </summary>
        public int? alignmentID { get; set; }

        /// <summary>
        /// The name of the model of the logpoint.
        /// </summary>
        public string? modelName { get; set; }

        /// <summary>
        /// The ID of the model of the logpoint.
        /// </summary>
        public int? modelID { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Logpoint"/> class.
        /// </summary>
        /// <param name="id">The unique identifier of the logpoint.</param>
        /// <param name="timestamp">The timestamp of the logpoint.</param>
        /// <param name="pointName">The name of the logpoint.</param>
        /// <param name="pointNumber">The number of the logpoint.</param>
        /// <param name="station">The station of the logpoint.</param>
        /// <param name="alignmentName">The name of the alignment of the logpoint.</param>
        /// <param name="alignmentID">The ID of the alignment of the logpoint.</param>
        /// <param name="modelName">The name of the model of the logpoint.</param>
        /// <param name="modelID">The ID of the model of the logpoint.</param>
        public Logpoint(int id, long timestamp, string? pointName, string? pointNumber, double? station, string? alignmentName, int? alignmentID, string? modelName, int? modelID)
        {
            this.id = id;
            var time = TimeSpan.FromMilliseconds(timestamp);
            this.timestamp = new DateTime(1970, 1, 1) + time;

            this.pointName = pointName;
            this.pointNumber = pointNumber;

            this.station = station;
            this.alignmentName = alignmentName;
            this.alignmentID = alignmentID;

            this.modelName = modelName;
            this.modelID = modelID;
        }
    }
}
