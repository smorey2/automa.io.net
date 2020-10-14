using System.Net;

namespace System.Text.Json.Serialization
{
    public class CookieCollectionFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(CookieCollection);

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) => new CookieCollectionConverter();

        public class CookieCollectionConverter : JsonConverter<CookieCollection>
        {
            public override CookieCollection Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                var result = new CookieCollection();
                if (!reader.Read())
                    throw new JsonException();
                while (reader.TokenType != JsonTokenType.EndArray)
                {
                    reader.Read();
                    var cookie = new Cookie();
                    while (reader.TokenType != JsonTokenType.EndObject)
                    {
                        if (reader.ValueTextEquals("Name") && reader.Read()) cookie.Name = reader.GetString();
                        else if (reader.ValueTextEquals("Value") && reader.Read()) cookie.Value = reader.GetString();
                        else if (reader.ValueTextEquals("Path") && reader.Read()) cookie.Path = reader.GetString();
                        else if (reader.ValueTextEquals("Domain") && reader.Read()) cookie.Domain = reader.GetString();
                        else if (reader.ValueTextEquals("Expires") && reader.Read()) cookie.Expires = new DateTime(reader.GetInt64());
                        else if (reader.ValueTextEquals("HttpOnly") && reader.Read()) cookie.HttpOnly = reader.GetBoolean();
                        else if (reader.ValueTextEquals("Secure") && reader.Read()) cookie.Secure = reader.GetBoolean();
                        else throw new JsonException();
                        reader.Read();
                    }
                    result.Add(cookie);
                    reader.Read();
                }
                return result;
            }

            public override void Write(Utf8JsonWriter writer, CookieCollection value, JsonSerializerOptions options)
            {
                writer.WriteStartArray();
                foreach (Cookie cookie in value)
                {
                    writer.WriteStartObject();
                    writer.WriteString("Name", cookie.Name);
                    writer.WriteString("Value", cookie.Value);
                    writer.WriteString("Path", cookie.Path);
                    writer.WriteString("Domain", cookie.Domain);
                    writer.WriteNumber("Expires", cookie.Expires.Ticks);
                    writer.WriteBoolean("HttpOnly", cookie.HttpOnly);
                    writer.WriteBoolean("Secure", cookie.Secure);
                    writer.WriteEndObject();
                }
                writer.WriteEndArray();
            }
        }
    }
}
