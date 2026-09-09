using OpenCvSharp;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShutterFace
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

                    switch (propertyName.ToUpperInvariant())
                    {
                        case "X":
                        case "LEFT":
                            x = reader.GetInt32();
                            break;
                        case "TOPLEFT":
                        case "BOTTOMRIGHT":
                            reader.Skip();
                            break;
                        case "Y":
                        case "TOP":
                            y = reader.GetInt32();
                            break;
                        case "WIDTH":
                        case "SIZE":
                            // Handle both "Width" and "Size" property names
                            if (propertyName.Equals("WIDTH", StringComparison.Ordinal))
                                width = reader.GetInt32();
                            else if (propertyName.Equals("SIZE", StringComparison.Ordinal))
                            {
                                // Size is an object, skip it
                                reader.Skip();
                            }
                            break;
                        case "HEIGHT":
                            height = reader.GetInt32();
                            break;
                    }
                }
            }

            return new Rect(x, y, width, height);
        }

        public override void Write(Utf8JsonWriter writer, Rect value, JsonSerializerOptions options)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));

            writer.WriteStartObject();
            writer.WriteNumber("X", value.X);
            writer.WriteNumber("Y", value.Y);
            writer.WriteNumber("Width", value.Width);
            writer.WriteNumber("Height", value.Height);
            writer.WriteEndObject();
        }
    }
}
