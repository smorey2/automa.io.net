namespace System.Text.Json.Serialization
{
    public class ValueTupleFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert) => typeToConvert.GetInterface("System.Runtime.CompilerServices.ITuple") != null;

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var genericArguments = typeToConvert.GetGenericArguments();
            Type converterType;
            switch (genericArguments.Length)
            {
                case 1: converterType = typeof(ValueTupleConverter<>).MakeGenericType(genericArguments); break;
                case 2: converterType = typeof(ValueTupleConverter<,>).MakeGenericType(genericArguments); break;
                case 3: converterType = typeof(ValueTupleConverter<,,>).MakeGenericType(genericArguments); break;
                default: throw new NotSupportedException();
            };
            return (JsonConverter)Activator.CreateInstance(converterType);
        }
    }

    public class ValueTupleConverter<T1> : JsonConverter<ValueTuple<T1>>
    {
        public override ValueTuple<T1> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            ValueTuple<T1> result = default;
            if (!reader.Read())
                throw new JsonException();
            while (reader.TokenType != JsonTokenType.EndObject)
            {
                if (reader.ValueTextEquals("Item1") && reader.Read()) result.Item1 = JsonSerializer.Deserialize<T1>(ref reader, options);
                else throw new JsonException();
                reader.Read();
            }
            return result;
        }

        public override void Write(Utf8JsonWriter writer, ValueTuple<T1> value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("Item1"); JsonSerializer.Serialize(writer, value.Item1, options);
            writer.WriteEndObject();
        }
    }

    public class ValueTupleConverter<T1, T2> : JsonConverter<ValueTuple<T1, T2>>
    {
        public override (T1, T2) Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            (T1, T2) result = default;
            if (!reader.Read())
                throw new JsonException();
            while (reader.TokenType != JsonTokenType.EndObject)
            {
                if (reader.ValueTextEquals("Item1") && reader.Read()) result.Item1 = JsonSerializer.Deserialize<T1>(ref reader, options);
                else if (reader.ValueTextEquals("Item2") && reader.Read()) result.Item2 = JsonSerializer.Deserialize<T2>(ref reader, options);
                else throw new JsonException();
                reader.Read();
            }
            return result;
        }

        public override void Write(Utf8JsonWriter writer, (T1, T2) value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("Item1"); JsonSerializer.Serialize(writer, value.Item1, options);
            writer.WritePropertyName("Item2"); JsonSerializer.Serialize(writer, value.Item2, options);
            writer.WriteEndObject();
        }
    }

    public class ValueTupleConverter<T1, T2, T3> : JsonConverter<ValueTuple<T1, T2, T3>>
    {
        public override (T1, T2, T3) Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            (T1, T2, T3) result = default;
            if (!reader.Read())
                throw new JsonException();
            while (reader.TokenType != JsonTokenType.EndObject)
            {
                if (reader.ValueTextEquals("Item1") && reader.Read()) result.Item1 = JsonSerializer.Deserialize<T1>(ref reader, options);
                else if (reader.ValueTextEquals("Item2") && reader.Read()) result.Item2 = JsonSerializer.Deserialize<T2>(ref reader, options);
                else if (reader.ValueTextEquals("Item3") && reader.Read()) result.Item3 = JsonSerializer.Deserialize<T3>(ref reader, options);
                else throw new JsonException();
                reader.Read();
            }
            return result;
        }

        public override void Write(Utf8JsonWriter writer, (T1, T2, T3) value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("Item1"); JsonSerializer.Serialize(writer, value.Item1, options);
            writer.WritePropertyName("Item2"); JsonSerializer.Serialize(writer, value.Item2, options);
            writer.WritePropertyName("Item3"); JsonSerializer.Serialize(writer, value.Item3, options);
            writer.WriteEndObject();
        }
    }
}
