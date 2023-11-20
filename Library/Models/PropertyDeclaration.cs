using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Library.Models
{
    /// <summary>
    /// Represents a property declaration.
    /// </summary>
    public class PropertyDeclaration
    {
        /// <summary>
        /// The property key.
        /// </summary>
        public string propertyKey { get; set; }

        /// <summary>
        /// The UUID of the organization to which the property belongs.
        /// </summary>
        public Guid organizationUuid { get; set; }

        /// <summary>
        /// The UUID of the project to which the property belongs.
        /// </summary>
        public Guid projectUuid { get; set; }

        /// <summary>
        /// The labels of the property.
        /// </summary>
        public Dictionary<string, string>? labels { get; set; }

        /// <summary>
        /// The schema of the property.
        /// </summary>
        public Schema schema { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyDeclaration"/> class.
        /// </summary>
        /// <param name="propertyKey">The key of the property.</param>
        /// <param name="organizationUuid">The UUID of the organization to which the property belongs.</param>
        /// <param name="projectUuid">The UUID of the project to which the property belongs.</param>
        /// <param name="labels">The labels of the property.</param>
        /// <param name="schemata">The schema of the property.</param>
        public PropertyDeclaration(string propertyKey, Guid organizationUuid, Guid projectUuid, Dictionary<string, string>? labels, JObject schemata)
        {
            this.propertyKey = propertyKey;
            this.organizationUuid = organizationUuid;
            this.projectUuid = projectUuid;
            this.labels = labels;

            switch (schemata.SelectToken("type").Value<string>())
            {
                case "string":
                    #region pattern

                    string? pattern = null;
                    if (schemata["pattern"] is not null)
                    {
                        pattern = schemata.SelectToken("pattern").Value<string>();
                    }

                    #endregion pattern

                    this.schema = new Schema.String(pattern);
                    break;

                case "number":
                    #region (exclusive) minimum

                    int? minimum = null;
                    bool exclusiveMin = false;
                    if (schemata["minimum"] is not null)
                    {
                        minimum = schemata.SelectToken("minimum").Value<int>();
                    }
                    else if (schemata["exclusiveMinimum"] is not null)
                    {
                        minimum = schemata.SelectToken("exclusiveMinimum").Value<int>();
                        exclusiveMin = true;
                    }

                    #endregion (exclusive) minimum

                    #region (exclusive) maximum

                    int? maximum = null;
                    bool exclusiveMax = false;
                    if (schemata["maximum"] is not null)
                    {
                        maximum = schemata.SelectToken("maximum").Value<int>();
                    }
                    else if (schemata["exclusiveMaximum"] is not null)
                    {
                        maximum = schemata.SelectToken("exclusiveMaximum").Value<int>();
                        exclusiveMax = true;
                    }

                    #endregion (exclusive) maximum

                    #region pattern

                    pattern = null;
                    if (schemata["pattern"] is not null)
                    {
                        pattern = schemata.SelectToken("pattern").Value<string>();
                    }

                    #endregion pattern

                    this.schema = new Schema.Number(minimum, maximum, exclusiveMin, exclusiveMax, pattern);
                    break;

                case "array":
                    #region items

                    List<string>? @enum = null;

                    if (schemata["items"] is not null)
                    {
                        var items = schemata.SelectToken("items").Value<JObject>();
                        @enum = new();

                        foreach (var e in items.SelectToken("enum").Value<JArray>())
                        {
                            @enum.Add(e.ToString());
                        }
                    }

                    #endregion items

                    #region minItems

                    int minItems = schemata.SelectToken("minItems").Value<int>();

                    #endregion minItems

                    #region maxItems

                    int? maxItems = null;

                    if (schemata["maxItems"] is not null)
                    {
                        maxItems = schemata.SelectToken("maxItems").Value<int>();
                    }

                    #endregion maxItems

                    #region uniqueItems

                    bool uniqueItems = schemata.SelectToken("uniqueItems").Value<bool>();

                    #endregion uniqueItems

                    this.schema = new Schema.Array(@enum, minItems, maxItems, uniqueItems);
                    break;
            }
        }

        /// <summary>
        /// Gets the JSON representation of the property declaration.
        /// </summary>
        /// <returns>The JSON representation of the property declaration.</returns>
        public string getJSON()
        {
            var json = new StringBuilder("{");

            json.Append("\"propertyKey\":\"");
            json.Append(this.propertyKey);
            json.Append("\"");

            json.Append(",\"organizationUuid\":\"");
            json.Append(this.organizationUuid);
            json.Append("\"");

            json.Append(",\"projectUuid\":\"");
            json.Append(this.projectUuid);
            json.Append("\"");

            if (this.labels is not null)
            {
                json.Append(",\"labels\":{");

                foreach (var label in labels)
                {
                    json.Append("\"");
                    json.Append(label.Key);
                    json.Append("\":\"");
                    json.Append(label.Value);
                    json.Append("\",");
                }

                json.Length--;
                json.Append("}");
            }

            json.Append(",");
            json.Append(this.schema.getJSON());

            json.Append("}");

            return json.ToString();
        }

        /// <summary>
        /// Gets the label of the property declaration in the specified language.
        /// </summary>
        /// <param name="language">The language of the label.</param>
        /// <returns>The label of the property declaration in the specified language.</returns>
        public string getLabel(string language)
        {
            if (this.labels is not null && this.labels.ContainsKey(language))
            {
                return this.labels[language];
            }

            return this.propertyKey;
        }

        /// <summary>
        /// Represents the schema of the property.
        /// </summary>
        public abstract class Schema
        {
            /// <summary>
            /// The type of the schema.
            /// </summary>
            public enum Type
            {
                /// <summary>
                /// A string schema.
                /// </summary>
                @string,

                /// <summary>
                /// A number schema.
                /// </summary>
                number,

                /// <summary>
                /// An array schema.
                /// </summary>
                array
            }

            /// <summary>
            /// The type of the schema.
            /// </summary>
            public Type type { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="Schema"/> class.
            /// </summary>
            /// <param name="type">The type of the schema.</param>
            public Schema(Type type)
            {
                this.type = type;
            }

            /// <summary>
            /// Gets the base JSON representation of the schema.
            /// </summary>
            /// <param name="name">The name of the schema.</param>
            /// <returns>The base JSON representation of the schema.</returns>
            protected StringBuilder getBaseJSON(string name = "schema")
            {
                var json = new StringBuilder("\"" + name + "\":{");

                json.Append("\"type\":\"");
                json.Append(this.type);
                json.Append("\"");

                return json;
            }

            /// <summary>
            /// Gets the JSON representation of the schema.
            /// </summary>
            /// <returns>The JSON representation of the schema.</returns>
            public abstract string getJSON();

            /// <summary>
            /// Schema for a string property.
            /// </summary>
            public class String : Schema
            {
                /// <summary>
                /// The regular expression pattern that the string must match.
                /// </summary>
                public string? pattern { get; set; }

                /// <summary>
                /// Initializes a new instance of the <see cref="String"/> class.
                /// </summary>
                /// <param name="pattern">The regular expression pattern that the string must match.</param>
                public String(string? pattern = null) : base(Type.@string)
                {
                    this.pattern = pattern;
                }

                /// <summary>
                /// Gets the JSON representation of the schema.
                /// </summary>
                /// <returns>The JSON representation of the schema.</returns>
                public override string getJSON()
                {
                    var json = base.getBaseJSON();

                    if (this.pattern is not null)
                    {
                        json.Append(",\"pattern:\":\"");
                        json.Append(this.pattern);
                        json.Append("\"");
                    }

                    json.Append("}");

                    return json.ToString();
                }
            }

            /// <summary>
            /// Schema for a number property.
            /// </summary>
            public class Number : Schema
            {
                /// <summary>
                /// The minimum value of the number.
                /// </summary>
                public int? minimum { get; set; }

                /// <summary>
                /// The maximum value of the number.
                /// </summary>
                public int? maximum { get; set; }

                /// <summary>
                /// Whether the minimum value is exclusive.
                /// </summary>
                public bool exclusiveMin { get; set; }

                /// <summary>
                /// Whether the maximum value is exclusive.
                /// </summary>
                public bool exclusiveMax { get; set; }

                /// <summary>
                /// The regular expression pattern that the number must match.
                /// </summary>
                public string? pattern { get; set; }

                /// <summary>
                /// Initializes a new instance of the <see cref="Number"/> class.
                /// </summary>
                /// <param name="minimum">The minimum value of the number.</param>
                /// <param name="maximum">The maximum value of the number.</param>
                /// <param name="exclusiveMin">Whether the minimum value is exclusive or inclusice.</param>
                /// <param name="exclusiveMax">Whether the maximum value is exclusive or inclusice.</param>
                /// <param name="pattern">The regular expression pattern that the number must match.</param>
                public Number(int? minimum = null, int? maximum = null, bool exclusiveMin = true, bool exclusiveMax = true, string? pattern = null) : base(Type.number)
                {
                    this.minimum = minimum;
                    this.maximum = maximum;
                    this.exclusiveMin = exclusiveMin;
                    this.exclusiveMax = exclusiveMax;
                    this.pattern = pattern;
                }

                /// <summary>
                /// Gets the JSON representation of the schema.
                /// </summary>
                /// <returns>The JSON representation of the schema.</returns>
                public override string getJSON()
                {
                    var json = base.getBaseJSON();

                    if (this.minimum is not null)
                    {
                        if (this.exclusiveMin)
                        {
                            json.Append(",\"exclusiveMinimum:\":\"");
                        }
                        else
                        {
                            json.Append(",\"minimum:\":\"");
                        }
                        json.Append(this.minimum);
                        json.Append("\"");
                    }

                    if (this.maximum is not null)
                    {
                        if (this.exclusiveMax)
                        {
                            json.Append(",\"exclusiveMaximum:\":\"");
                        }
                        else
                        {
                            json.Append(",\"maximum:\":\"");
                        }
                        json.Append(this.maximum);
                        json.Append("\"");
                    }

                    if (this.pattern is not null)
                    {
                        json.Append(",\"pattern:\":\"");
                        json.Append(this.pattern);
                        json.Append("\"");
                    }

                    json.Append("}");

                    return json.ToString();
                }
            }

            /// <summary>
            /// Schema for an array property.
            /// </summary>
            public class Array : Schema
            {
                /// <summary>
                /// The schema of the items in the array.
                /// </summary>
                public Items? items { get; set; }

                /// <summary>
                /// The minimum number of items in the array.
                /// </summary>
                public int minItems { get; set; }

                /// <summary>
                /// The maximum number of items in the array, if any.
                /// </summary>
                public int? maxItems { get; set; }

                /// <summary>
                /// Whether all items in the array must be unique.
                /// </summary>
                public bool uniqueItems { get; set; }

                /// <summary>
                /// Initializes a new instance of the <see cref="Array"/> class.
                /// </summary>
                /// <param name="@enum">The enumeration of allowed values for the items in the array.</param>
                /// <param name="minItems">The minimum number of items in the array.</param>
                /// <param name="maxItems">The maximum number of items in the array, if any.</param>
                /// <param name="uniqueItems">Whether all items in the array must be unique.</param>
                public Array(List<string>? @enum, int minItems, int? maxItems = null, bool uniqueItems = true) : base(Type.array)
                {
                    if (@enum is not null)
                    {
                        this.items = new Items(@enum);
                    }
                    this.minItems = minItems;
                    this.maxItems = maxItems;
                    this.uniqueItems = uniqueItems;
                }

                /// <summary>
                /// Gets the JSON representation of the schema.
                /// </summary>
                /// <returns>The JSON representation of the schema.</returns>
                public override string getJSON()
                {
                    var json = base.getBaseJSON();

                    json.Append(",");
                    json.Append(this.items.getJSON());

                    json.Append(",\"minItems\":\"");
                    json.Append(this.minItems);
                    json.Append("\"");

                    if (this.maxItems is not null)
                    {
                        json.Append(",\"maxItems\":\"");
                        json.Append(this.maxItems);
                        json.Append("\"");
                    }

                    json.Append(",\"uniqueItems\":\"");
                    json.Append(this.uniqueItems);
                    json.Append("\"");

                    json.Append("}");

                    return json.ToString();
                }

                /// <summary>
                /// Represents the items of the array.
                /// </summary>
                public class Items : Schema
                {
                    /// <summary>
                    /// The enumeration of allowed values for the items in the array.
                    /// </summary>
                    public List<string> @enum { get; set; }

                    /// <summary>
                    /// Initializes a new instance of the <see cref="Items"/> class.
                    /// </summary>
                    /// <param name="@enum">The enumeration of allowed values for the items in the array.</param>
                    public Items(List<string> @enum) : base(Type.@string)
                    {
                        this.@enum = @enum;
                    }

                    /// <summary>
                    /// Gets the JSON representation of the schema.
                    /// </summary>
                    /// <returns>The JSON representation of the schema.</returns>
                    public override string getJSON()
                    {
                        var json = base.getBaseJSON("items");

                        #region enum

                        json.Append(",\"enum\":[");

                        foreach (var e in this.@enum)
                        {
                            json.Append("\"");
                            json.Append(e);
                            json.Append("\",");
                        }

                        if (this.@enum.Count > 0)
                        {
                            json.Length--;
                        }

                        json.Append("]");

                        #endregion enum

                        json.Append("}");

                        return json.ToString();
                    }
                }
            }
        }
    }
}
