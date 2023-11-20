using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Library.Models;

namespace Library
{
    /// <summary>
    /// A class that contains methods for parsing JSON objects into Library.Models objects.
    /// </summary>
    internal static class Parse
    {
        /// <summary>
        /// Parses a JSON object into a Project object.
        /// </summary>
        /// <param name="value">The JSON object to parse.</param>
        /// <returns>A Project object.</returns>
        internal static Project project(JToken value)
        {
            var id = value.SelectToken("id").Value<int>();
            var uuid = new Guid(value.SelectToken("uuid").Value<string>());
            var name = value.SelectToken("name").Value<string>();
            var timestamp = value.SelectToken("timestamp").Value<long>();

            Organization? organization = null;
            if (value["organization"] is not null)
            {
                var jObject = value.SelectToken("organization").Value<JObject>();
                organization = Parse.organization(jObject);
            }

            CoordinateSystem? coordinateSystem = null;
            if (value["coordinateSystem"] is not null)
            {
                var jObject = value.SelectToken("coordinateSystem").Value<JObject>();
                coordinateSystem = Parse.coordinateSystem(jObject);
            }

            HeightSystem? heightSystem = null;
            if (value["heightSystem"] is not null)
            {
                var jObject = value.SelectToken("heightSystem").Value<JObject>();
                Parse.heightSystem(jObject);
            }

            return new Project(id, uuid, name, timestamp, organization, coordinateSystem, heightSystem);
        }

        /// <summary>
        /// Parses a JSON object into a Document object.
        /// </summary>
        /// <param name="value">The JSON object to parse.</param>
        /// <returns>A Document object.</returns>
        internal static Document document(JToken value)
        {
            var docUuid = new Guid(value.SelectToken("uuid").Value<string>());
            var name = value.SelectToken("name").Value<string>();

            var headUuid = new Guid(value.SelectToken("headUuid").Value<string>());

            var folderPath = value.SelectToken("folderPath").Value<string>();

            #region properties

            var propertiesValue = value.SelectToken("properties").Value<JArray>();
            Dictionary<string, List<string>> properties = null;

            if (propertiesValue is not null)
            {
                properties = new();
                foreach (var propertie in propertiesValue)
                {
                    var propKey = propertie.SelectToken("key").Value<string>();
                    var propValue = propertie.SelectToken("value").Value<String>();

                    if (propValue.StartsWith('['))
                    {
                        propValue = propValue.Substring(1, propValue.Length - 2);

                        var propValues = propValue.Split(',')
                            .Select(prop => prop.Trim('\"'))
                            .ToList();

                        properties[propKey] = propValues;
                    }
                    else
                    {
                        properties[propKey] = new List<string>()
                            {
                                propValue
                            };
                    }
                }
            }

            #endregion properties

            #region timestamp

            var timestamp = value.SelectToken("timestamp").Value<string>();

            DateTime docTimestamp;
            try
            {
                DateTime dateTime;

                var success = DateTime.TryParseExact(timestamp, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out dateTime);

                if (!success)
                {
                    success = DateTime.TryParseExact(timestamp, "yyyy-MM-ddTHH:mmZ", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out dateTime);
                }

                if (!success)
                {
                    DateTime.TryParse(timestamp, out dateTime);
                }

                docTimestamp = dateTime.ToUniversalTime();
            }
            catch
            {
                Utils.Log.write("api.parseDocument.timestamp: \"" + timestamp + "\"");

                docTimestamp = DateTime.MinValue;
            }

            #endregion timestamp

            var version = value.SelectToken("version").Value<int>();

            var md5 = value.SelectToken("md5").Value<string>();

            return new Document(docUuid, name, headUuid, folderPath, properties, docTimestamp, version, md5);
        }

        /// <summary>
        /// Parses a JSON object into a CoordinateSystem object.
        /// </summary>
        /// <param name="value">The JSON object to parse.</param>
        /// <returns>A CoordinateSystem object.</returns>
        internal static CoordinateSystem coordinateSystem(JToken value)
        {
            var csName = value.SelectToken("name").Value<string>();
            var csIdentifier = value.SelectToken("identifier").Value<string>();
            var csProjString = value.SelectToken("projString").Value<string>();
            var csWgs84Parameters = value.SelectToken("wgs84Parameters").Value<string>();
            var csOffsetN = value.SelectToken("offsetN").Value<double>();
            var csOffsetE = value.SelectToken("offsetE").Value<double>();

            return new CoordinateSystem(csName, csIdentifier, csProjString, csWgs84Parameters, csOffsetN, csOffsetE);
        }

        /// <summary>
        /// Parses a JSON object into a HeightSystem object.
        /// </summary>
        /// <param name="value">The JSON object to parse.</param>
        /// <returns>A HeightSystem object.</returns>
        internal static HeightSystem heightSystem(JToken value)
        {
            var hsName = value.SelectToken("name").Value<string>();
            var hsIdentifier = value.SelectToken("identifier").Value<string>();

            return new HeightSystem(hsName, hsIdentifier);
        }

        /// <summary>
        /// Parses a JSON object into an Organization object.
        /// </summary>
        /// <param name="value">The JSON object to parse.</param>
        /// <returns>An Organization object.</returns>
        internal static Organization organization(JToken value)
        {
            int? organizationID = null;
            if (value["id"] is not null)
            {
                organizationID = value.SelectToken("id").Value<int>();
            }

            var organizationName = value.SelectToken("name").Value<string>();

            Guid? organizationUuid = null;
            if (value["organizationUuid"] is not null)
            {
                organizationUuid = new Guid(value.SelectToken("organizationUuid").Value<string>());
            }
            else if (value["uuid"] is not null)
            {
                organizationUuid = new Guid(value.SelectToken("uuid").Value<string>());
            }

            return new Organization(organizationName, organizationUuid, organizationID);
        }

        /// <summary>
        /// Parses a JSON object into an Image object.
        /// </summary>
        /// <param name="value">The JSON object to parse.</param>
        /// <returns>An Image object.</returns>
        internal static Image image(JToken value)
        {
            var ImageUuid = new Guid(value.SelectToken("uuid").Value<string>());
            var name = value.SelectToken("name").Value<string>();
            var description = value.SelectToken("description").Value<string>();
            var timestamp = value.SelectToken("timestamp").Value<long>();
            var originalDate = value.SelectToken("originalDate").Value<long?>();
            var panorama = value.SelectToken("panorama").Value<bool?>();

            var CreatorUsername = value.SelectToken("creator.username").Value<string>();
            var CreatorUuid = new Guid(value.SelectToken("creator.uuid").Value<string>());

            var geographicPointLat = value.SelectToken("geographicPoint.lat").Value<double>();
            var geographicPointLon = value.SelectToken("geographicPoint.lon").Value<double>();
            var geographicPointElevation = value.SelectToken("geographicPoint.elevation").Value<long>();

            return new Image(ImageUuid, name, description, timestamp, originalDate,
                (geographicPointLat, geographicPointLon, geographicPointElevation),
                panorama, (CreatorUsername, CreatorUuid));
        }

        /// <summary>
        /// Parses a JSON object into a Folder object.
        /// </summary>
        /// <param name="value">The JSON object to parse.</param>
        /// <returns>A Folder object.</returns>
        internal static Folder folder(JToken value)
        {
            #region id

            int? id = null;
            if (value["id"] is not null)
            {
                id = value.SelectToken("id").Value<int>();
            }

            #endregion id

            #region project

            Project? project = null;
            if (value["project"] is not null)
            {
                var projectItem = value.SelectToken("project").Value<JObject>();

                project = Parse.project(projectItem);
            }

            #endregion project

            var folderUuid = new Guid(value.SelectToken("uuid").Value<string>());
            var name = value.SelectToken("name").Value<string>();
            var folderPath = value.SelectToken("folderPath").Value<string>();

            #region folder depth

            int? folderDepth = null;
            if (value["depth"] is not null)
            {
                folderDepth = value.SelectToken("depth").Value<int>();
            }

            #endregion folder depth

            #region parent folder uuid

            Guid? parentFolderUuid = null;
            var pfToken = value.SelectToken("parentFolderUuid");
            if (pfToken is not null)
            {
                var pfString = pfToken.Value<string?>();
                if (pfString is not null)
                {
                    parentFolderUuid = new Guid(pfString);
                }
            }

            #endregion parent folder uuid

            return new Folder(folderUuid, name, folderPath, parentFolderUuid, folderDepth, id, project);
        }

        /// <summary>
        /// Parses a JSON object into an Equipment object.
        /// </summary>
        /// <param name="value">The JSON object to parse.</param>
        /// <returns>An Equipment object.</returns>
        internal static Equipment equipment(JToken value)
        {
            var uuid = new Guid(value.SelectToken("uuid").Value<string>());
            var name = value.SelectToken("name").Value<string>();
            var identifier = value.SelectToken("identifier").Value<string>();
            var type = value.SelectToken("type").Value<string>();

            return new Equipment(uuid, name, identifier, type);
        }

        /// <summary>
        /// Parses a JSON object into a Logpoint object.
        /// </summary>
        /// <param name="value">The JSON object to parse.</param>
        /// <returns>A Logpoint object.</returns>
        internal static Logpoint logpoint(JToken value)
        {
            var id = value.SelectToken("id").Value<int>();
            var timestamp = value.SelectToken("timestamp").Value<long>();

            var pointName = value.SelectToken("pointName").Value<string?>();
            var pointNumber = value.SelectToken("pointNumber").Value<string?>();

            var station = value.SelectToken("station").Value<double?>();
            var alignmentName = value.SelectToken("alignmentName").Value<string?>();
            var alignmentID = value.SelectToken("alignmentId").Value<int?>();

            var modelName = value.SelectToken("modelName").Value<string?>();
            var modelID = value.SelectToken("modelId").Value<int?>();

            return new Models.Logpoint(id, timestamp, pointName, pointNumber, station, alignmentName, alignmentID, modelName, modelID);
        }

        /// <summary>
        /// Parses a JSON object into a PropertyDeclaration object.
        /// </summary>
        /// <param name="value">The JSON object to parse.</param>
        /// <returns>A PropertyDeclaration object.</returns>
        internal static PropertyDeclaration propertyDeclaration(JToken value)
        {
            var propertyKey = value.SelectToken("propertyKey").Value<string>();

            var organizationUuid = new Guid(value.SelectToken("organizationUuid").Value<string>());

            var projectUuid = new Guid(value.SelectToken("projectUuid").Value<string>());

            #region labels

            Dictionary<string, string>? labels = null;
            if (value["labels"] is not null)
            {
                labels = new Dictionary<string, string>();
                foreach (var label in value.SelectToken("labels").Value<JObject>())
                {
                    labels[label.Key] = label.Value.ToString();
                }
            }

            #endregion labels

            var schema = value.SelectToken("schema").Value<JObject>();

            return new PropertyDeclaration(propertyKey, organizationUuid, projectUuid, labels, schema);
        }
    }
}