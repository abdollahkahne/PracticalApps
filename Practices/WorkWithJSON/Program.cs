using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace WorkWithJSON
{
    class Program
    {
        static void Main(string[] args)
        {
            // fromJsonString();
            // createJsonNodeUsingConstructor();
            // DeserializeUsingJsonNode();
            // WorkWithJsonDocument();
            // WriteJsonDocument();
            var jsonString = serializeWithUtf8JsonWriter(); // This return a generated json
            Console.WriteLine(jsonString);
        }

        private static string serializeWithUtf8JsonWriter()
        {
            using (var ms = new MemoryStream())
            {
                var jsonWriterOptions = new JsonWriterOptions { Indented = true };
                using (var writer = new Utf8JsonWriter(ms, jsonWriterOptions))
                {
                    writer.WriteStartObject();
                    writer.WriteString("BirthDate", new DateTime(1984, 9, 21));
                    writer.WriteNumber("Age", 37);
                    writer.WriteBoolean("Single", true);
                    writer.WriteStartArray("Educations");
                    writer.WriteStartObject();
                    writer.WritePropertyName("Level");
                    writer.WriteStringValue("Master of Science");
                    writer.WriteString("Title", "Operations Research");
                    writer.WriteEndObject();
                    writer.WriteStartObject();
                    writer.WritePropertyName("Level");
                    writer.WriteStringValue("Bachelor of Science");
                    writer.WriteString("Title", "Mathematics");
                    writer.WriteEndObject();
                    writer.WriteEndArray();
                    writer.WriteEndObject();
                    writer.Flush();
                }
                // If we have a memory stream we can do things to it. for example we can get it as string using encoding Get String Method or copy to another stream using copyto or convert to array of byte(byte[])
                // Since memory stream is a sequence of bytes, the current position after writing is end of it. to read it again we should back to begining for example!
                ms.Seek(0, SeekOrigin.Begin);
                // ms.CopyTo(fs); // Copy to other stream
                // Since stream is utf8 encoded we should use it too
                var json = Encoding.UTF8.GetString(ms.ToArray());
                return json;
            }
        }

        private static void WriteJsonDocument() // we have a
        {
            var output = Path.Combine(".", "students.json");
            string jsonString =
            @"{
            ""Class Name"": ""Science"",
            ""Teacher\u0027s Name"": ""Jane"",
            ""Semester"": ""2019-01-01"",
            ""Students"": [
                {
                ""Name"": ""John"",
                ""Grade"": 94.3
                },
                {
                ""Name"": ""James"",
                ""Grade"": 81.0
                },
                {
                ""Name"": ""Julia"",
                ""Grade"": 91.9
                },
                {
                ""Name"": ""Jessica"",
                ""Grade"": 72.4
                },
                {
                ""Name"": ""Johnathan""
                }
            ],
            ""Final"": true
            }
            ";
            var parseOptions = new JsonDocumentOptions
            {
                CommentHandling = JsonCommentHandling.Skip,
            };
            using (var document = JsonDocument.Parse(jsonString, parseOptions))
            {
                using (var fs = File.Create(output))
                {
                    var writerOptions = new JsonWriterOptions
                    {
                        Indented = true
                    };
                    using (var writer = new Utf8JsonWriter(fs, writerOptions))
                    {
                        // document.WriteTo(writer);
                        // document.RootElement.WriteTo(writer);
                        writer.WriteStartObject();
                        writer.WritePropertyName("Students");
                        document.RootElement.GetProperty("Students").WriteTo(writer);
                        writer.WriteEndObject(); // we need this for every write start object (It apparantly works automaticly)
                        writer.Flush(); // An alternative is to let the writer auto-flush when it's disposed. But the buffer assigned to stream may overflow
                    }

                }
            }
        }

        private static void WorkWithJsonDocument()
        {

            string jsonString =
            @"{
            ""Class Name"": ""Science"",
            ""Teacher\u0027s Name"": ""Jane"",
            ""Semester"": ""2019-01-01"",
            ""Students"": [
                {
                ""Name"": ""John"",
                ""Grade"": 94.3
                },
                {
                ""Name"": ""James"",
                ""Grade"": 81.0
                },
                {
                ""Name"": ""Julia"",
                ""Grade"": 91.9
                },
                {
                ""Name"": ""Jessica"",
                ""Grade"": 72.4
                },
                {
                ""Name"": ""Johnathan""
                }
            ],
            ""Final"": true
            }
            ";
            using var document = JsonDocument.Parse(jsonString);// This is immutable. We only use the JsonDocument For Parsing and after that we use JsonElement

            if (document.RootElement.TryGetProperty("Students", out var students))
            {
                foreach (var item in students.EnumerateArray())
                {
                    bool graded = item.TryGetProperty("Grade", out var grade);// since grade may be null we should use Try 

                    var name = item.GetProperty("Name");
                    if (graded)
                    {
                        graded = grade.TryGetSingle(out var gradeFloat); // since the value may not be convertable to float we should use try
                        Console.WriteLine($"{name.GetString()}:{gradeFloat}");
                    }

                }
            }
        }

        private static void DeserializeUsingJsonNode()
        {
            string jsonString = @"{
            ""Date"": ""2019-08-01T00:00:00"",
            ""Temperature"": 25,
            ""Summary"": ""Hot"",
            ""DatesAvailable"": [
                ""2019-08-01T00:00:00"",
                ""2019-08-02T00:00:00""
            ],
            ""TemperatureRanges"": {
                ""Cold"": {
                    ""High"": 20,
                    ""Low"": -10
                },
                ""Hot"": {
                    ""High"": 60,
                    ""Low"": 20
                }
            }
            }
            ";
            var jsonNode = JsonNode.Parse(jsonString);
            var cold = jsonNode["TemperatureRanges"]["Cold"];
            // var coldProfile = cold.GetValue<TemperatureProfile>();// This does not work since T is not primitive (value type/struct)!
            // we can deserialize the string to our type T as below using Utf8JsonWriter
            using (var ms = new MemoryStream())
            {
                using (var writer = new Utf8JsonWriter(ms))
                {
                    cold.WriteTo(writer);
                    writer.Flush();// If the stream has a buffer, then the bytes in the buffer
                                   // are written to the stream and the buffer is cleared.
                }
                ms.Seek(0, SeekOrigin.Begin);
                var coldProfile = JsonSerializer.Deserialize<TemperatureProfile>(ms);
                Console.WriteLine($"Cold Profile is from {coldProfile.Low} to {coldProfile.High}");
            }
            var profiles = jsonNode["TemperatureRanges"];
            using (var ms = new MemoryStream())
            {
                using (var jsonWriter = new Utf8JsonWriter(ms))
                {
                    profiles.WriteTo(jsonWriter);
                    jsonWriter.Flush();
                }
                ms.Seek(0, SeekOrigin.Begin);// Go to Start of Stream (This needs if we already worked with stream )
                var profilesDictionary = JsonSerializer.Deserialize<Dictionary<string, TemperatureProfile>>(ms);
                var hotProfile = profilesDictionary["Hot"];
                Console.WriteLine($"Hot Profile is from {hotProfile.Low} to {hotProfile.High}");
            }

        }
        private static void createJsonNodeUsingConstructor()
        {
            var forcastObject = new JsonObject()
            {
                ["Date"] = DateTime.Now,
                ["Temperature"] = 25,
                ["Summary"] = "Hot",
                ["DatesAvailable"] = new JsonArray(new DateTime(1984, 1, 5), new DateTime(1984, 2, 5)),
                ["Forecasts"] = new JsonArray(
                    new JsonObject
                    {
                        ["Date"] = new DateTime(2021, 11, 29),
                        ["Min"] = 10,
                        ["Max"] = 34,
                        ["Summary"] = "Cold"
                    },
                    new JsonObject
                    {
                        ["Date"] = new DateTime(2021, 11, 28),
                        ["Min"] = 15,
                        ["Max"] = 61,
                        ["Summary"] = "Hot"
                    }
                ),
            };
            var newforecast = new JsonObject()
            {
                ["Date"] = new DateTime(2021, 11, 27),
                ["Min"] = 15,
                ["Max"] = 61,
                ["Summary"] = "Hot"
            };
            (forcastObject["Forecasts"] as JsonArray).Add(newforecast);
            // To add a property to object simply add the key
            forcastObject["Author"] = "Mehdi";
            forcastObject["Summary"] = "Mild";
            forcastObject["Temperature"] = null;// this makes null and NOT remove the property
            (forcastObject as JsonObject).Remove("Date");

            var JsonSerializerOptions = new JsonSerializerOptions() { WriteIndented = true };
            Console.WriteLine(forcastObject.ToJsonString(JsonSerializerOptions));
        }
        private static void fromJsonString() // This create the json node from a string using Parse static method
        {
            string jsonString = @"{
            ""Date"": ""2019-08-01T00:00:00"",
            ""Temperature"": 25,
            ""Summary"": ""Hot"",
            ""DatesAvailable"": [
                ""2019-08-01T00:00:00"",
                ""2019-08-02T00:00:00""
            ],
            ""TemperatureRanges"": {
                ""Cold"": {
                    ""High"": 20,
                    ""Low"": -10
                },
                ""Hot"": {
                    ""High"": 60,
                    ""Low"": 20
                }
            }
            }
            ";
            JsonNode forecastNode = JsonNode.Parse(jsonString); // This create the json node from json string
            var serializationOption = new JsonSerializerOptions { WriteIndented = true };
            Console.WriteLine(forecastNode.ToJsonString(serializationOption));
            var temperature = forecastNode["Temperature"];
            Console.WriteLine($"{temperature.GetType()}:{temperature.GetValue<int>()}:{temperature.ToJsonString()}");// GetType is inherited from Object. Get value if we have its type and tojsonstring if we have not its type!
            Console.WriteLine(temperature.ToString());// This does not any special thing. It is inherited from Object!
            // var cold = forecastNode["TemperatureRanges:Cold"]; // this does not work!!
            var cold = forecastNode["TemperatureRanges"]["Cold"];// this works
            Console.WriteLine(cold.ToJsonString());
            int temperatureInt = (int)forecastNode["Temperature"]; // can cast when indexing to its type
            Console.WriteLine(temperatureInt);
        }
        internal class TemperatureProfile
        {
            public int High { get; set; }
            public int Low { get; set; }
        }
    }
}



// Work with JSON Document/ serialize/ Deserialize/ make custome Serializer/Deserializer
// JsonSerializer:
// This Type has two generic method to serialize POCOs to json document or vice-verse. It has convention rules to do the job.
// For example it convert all public properties. But what if we want change this behaviour? Altough there is some options to work with it but it is limited
// So There is two type for creatin custome serializer and parser/ deserializer including Utf8Jsonreader and Utf8Jsonwrite which we cover later

// JSON DOM:
// There is situation that we do not know the type of Object or even its schema to deserialize the json doc to it. Here we have an alternative to use JSON DOM.
// JSOM DOM have two element type including JSONDocument (JDocument) and JSONNode. Json document is immutable (readonly) and can not change after creation but it is faster. JsonNode can be changed after creation
// Json Document build the readonly dom using Utf8JsonReader and its element can be reached using JsonElement type which have APIs for working and deserializing to .NET Object. Json Document has a RootElement Property too.
// (We should add System.Text.Json package first to include it inside serialization) Json Node (And other derived class in System.Text.Json.Nodes namespace) create a mutable dom using JsonNode.Parse method and the element of payloads can be reached using multiple elements including JsonNode, JsonObject, JsonArray,JsonValue and JsonElement types APIs.

// JSON Nodes:
// We can do following with JsonNode:
// 1- Parse a json string to a json node using its static method JsonNode.Pars
// 2- Work with dom using its instance method. For example serialize it again with more options (beautify for example!)
// 3- Work with subnodes using index []. For example we can reach its first level node temperature using myNode["temperature"] or other with forecastNode["TemperatureRanges"]["Cold"] which
// themselves are JsonNode and can be worked similary. But we can cast them to their type simply using for example this: int temp=(int)myNode["Temperature"];
// 4- If subnodes are Object, Array, primitive Value, It is smart enough to get its real type which can be JsonObject, JsonArray or JsonValue respectively.
// For json array indexing is similar to array start from zero. Even we can parse an array in json form and work with index with it.
// 5- JsonObject and JsonArray are syntatic sugar too. We can use their constructor to build a complex json node
// we can add/change/remove nodes to it using indexing too. And to work with Array/Object properties we should cast to corrected type first.
// 6- Define Custome Deserializer: Here we can use Utf8JsonWriter to write a json node (which can have every type which we define using constructor or using Parse method) to an stream and then use that stream with Normal JsonSerializarion Deserializer method to get a variable with our intersted .Net type.
// 7- JsonNode used to derive Types like JsonArray or JsonObject which we can cast (normal casting or AsArray, AsObject,AsValue methods) our node to them (Or directly use their static Parse or Create methods) and then use special method which we have for Array or Object or Value like Add, Remove for Array and ContainKey for object

// Json Documents
// As it said we have two Types here. JsonDocument which only used for parsing and JsonElement which used for DOM Traversing
// After parsing we have a root element in the Json Document instance. Also it is disposable so we should use using block for Parsing. we can use elemnt.clone to clone from any element including root element:
//"If you return the RootElement or a sub-element directly without making a Clone, the caller (the method who called this method) won't be able to access the returned JsonElement after the JsonDocument that owns it is disposed."
// To Get A property in json element we can use GetProperty or TryGetProperty and for getting Value we can use GetInt for example and TryGetInt.
// Json Document and Json Element designed to minimize initial parse time rather than lookup time. So try to do searching using built in API like EnumerateArray Or EnumerateObject. And also try not to search for property yourself and use the schema data for getting properties instead of searching using loops.
// We can easily write every property or JsonElement existed in Json Document to an stream and then used that stream in every way we need (for example create files!). Here the main role is assigned to Utf8JsonWrite which writes in json to every target. It can add Start of Object, end of Object and Property name if required.

// Utf8JsonWriter
// Utf8JsonWriter is a high-performance way to write UTF-8 encoded JSON text from common .NET types like String, Int32, and DateTime. The writer is a low-level type that can be used to build custom serializers.
// The JsonSerializer.Serialize method uses Utf8JsonWriter under the covers.

// Utf8JsonReader
// The Utf8JsonReader is a low-level type that can be used to build custom parsers and deserializers. The JsonSerializer.Deserialize method uses Utf8JsonReader under the covers. 

