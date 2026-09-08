using OpenCvSharp;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MotionTrackerFaceBlur
{
    // Custom JSON converter for OpenCvSharp.Rect
    public class RectJsonConverter : JsonConverter<Rect>
    {
        public override Rect Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException();

            int x = 0, y = 0, width = 0, height = 0;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    return new Rect(x, y, width, height);

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    string propertyName = reader.GetString()!;
                    reader.Read();

                    switch (propertyName.ToLowerInvariant())
                    {
                        case "x":
                        case "left":
                            x = reader.GetInt32();
                            break;
                        case "y":
                        case "top":
                            y = reader.GetInt32();
                            break;
                        case "width":
                        case "size":
                            // Handle both "Width" and "Size" property names
                            if (propertyName.Equals("width", StringComparison.OrdinalIgnoreCase))
                                width = reader.GetInt32();
                            else if (propertyName.Equals("size", StringComparison.OrdinalIgnoreCase))
                            {
                                // Size is an object, skip it
                                reader.Skip();
                            }
                            break;
                        case "height":
                            height = reader.GetInt32();
                            break;
                        case "location":
                        case "topleft":
                        case "bottomright":
                            // Skip these complex objects
                            reader.Skip();
                            break;
                    }
                }
            }

            return new Rect(x, y, width, height);
        }

        public override void Write(Utf8JsonWriter writer, Rect value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber("X", value.X);
            writer.WriteNumber("Y", value.Y);
            writer.WriteNumber("Width", value.Width);
            writer.WriteNumber("Height", value.Height);
            writer.WriteEndObject();
        }
    }
}
