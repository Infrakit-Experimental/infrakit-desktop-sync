/// <summary>
/// A class that represents a geographic point.
/// </summary>
public class GeographicPoint
{
    /// <summary>
    /// The latitude of the point.
    /// </summary>
    public double lat { get; set; }

    /// <summary>
    /// The longitude of the point.
    /// </summary>
    public double lon { get; set; }

    /// <summary>
    /// The elevation of the point.
    /// </summary>
    public long elevation { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GeographicPoint"/> class.
    /// </summary>
    /// <param name="lat">The latitude of the point.</param>
    /// <param name="lon">The longitude of the point.</param>
    /// <param name="elevation">The elevation of the point.</param>
    public GeographicPoint(double lat, double lon, long elevation)
    {
        this.lat = lat;
        this.lon = lon;
        this.elevation = elevation;
    }
}
