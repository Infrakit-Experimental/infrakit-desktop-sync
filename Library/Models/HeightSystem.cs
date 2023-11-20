/// <summary>
/// A class that represents a height system.
/// </summary>
public class HeightSystem
{
    /// <summary>
    /// The name of the height system.
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// The identifier of the height system.
    /// </summary>
    public string identifier { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="HeightSystem"/> class.
    /// </summary>
    /// <param name="name">The name of the height system.</param>
    /// <param name="identifier">The identifier of the height system.</param>
    public HeightSystem(string name, string identifier)
    {
        this.name = name;
        this.identifier = identifier;
    }
}
