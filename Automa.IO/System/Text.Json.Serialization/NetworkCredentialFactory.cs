using System.Net;

namespace System.Text.Json.Serialization
{
    public class NetworkCredentialFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(NetworkCredential);

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) => new NetworkCredentialConverter();

        public class NetworkCredentialConverter : JsonConverter<NetworkCredential>
        {
            public override NetworkCredential Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                var result = new NetworkCredential();
                if (!reader.Read())
                    throw new JsonException();
                while (reader.TokenType != JsonTokenType.EndObject)
                {
                    if (reader.ValueTextEquals("Domain") && reader.Read()) result.Domain = reader.GetString();
                    else if (reader.ValueTextEquals("Password") && reader.Read()) result.Password = reader.GetString();
                    else if (reader.ValueTextEquals("UserName") && reader.Read()) result.UserName = reader.GetString();
                    else throw new JsonException();
                    reader.Read();
                }
                return result;
            }

            public override void Write(Utf8JsonWriter writer, NetworkCredential value, JsonSerializerOptions options)
            {
                writer.WriteStartObject();
                writer.WriteString("Domain", value.Domain);
                writer.WriteString("Password", value.Password);
                writer.WriteString("UserName", value.UserName);
                writer.WriteEndObject();
            }
        }
    }
}
