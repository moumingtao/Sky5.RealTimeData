using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sky5.RealTimeData
{
    public class NewtonsoftTextJsonConterter : JsonConverter<JToken>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeof(JToken).IsAssignableFrom(typeToConvert);
        }
        public override JToken Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, JToken token, JsonSerializerOptions options)
        {
            switch (token)
            {
                case JObject value:
                    writer.WriteStartObject();
                    foreach (var element in value)
                    {
                        writer.WritePropertyName(element.Key);
                        Write(writer, element.Value, options);
                    }
                    writer.WriteEndObject();
                    break;
                case JArray value:
                    writer.WriteStartArray();
                    foreach (var item in value)
                    {
                        Write(writer, item, options);
                    }
                    writer.WriteEndArray();
                    break;
                case JConstructor value:
                    writer.WriteStringValue(value.ToString());
                    break;
                case JProperty value:
                    writer.WritePropertyName(value.Name);
                    Write(writer, value.Value, options);
                    break;
                case JRaw value:
                    JsonSerializer.Serialize(writer, value.Value, options);
                    break;
                default:
                    switch (token.Type)
                    {
                        case JTokenType.None:
                            break;
                        case JTokenType.Comment://
                            break;
                        case JTokenType.Integer:
                            writer.WriteNumberValue(token.Value<long>());
                            break;
                        case JTokenType.Float:
                            writer.WriteNumberValue(token.Value<double>());
                            break;
                        case JTokenType.String:
                            writer.WriteStringValue(token.Value<string>());
                            break;
                        case JTokenType.Boolean:
                            writer.WriteBooleanValue(token.Value<bool>());
                            break;
                        case JTokenType.Null:
                            writer.WriteNullValue();
                            break;
                        case JTokenType.Undefined:
                            writer.WriteNullValue();
                            break;
                        case JTokenType.Date:
                            writer.WriteStringValue(token.Value<DateTime>().ToString());
                            break;
                        case JTokenType.Bytes:
                            var value = token.Value<byte[]>();
                            writer.WriteBase64StringValue(new ReadOnlySpan<byte>(value));
                            break;
                        case JTokenType.Guid:
                            writer.WriteStringValue(token.Value<Guid>());
                            break;
                        case JTokenType.Uri:
                            writer.WriteStringValue(token.Value<string>());
                            break;
                        case JTokenType.TimeSpan:
                            writer.WriteStringValue(token.Value<TimeSpan>().ToString());
                            break;
                        default:
                            break;
                    }
                    break;
            }
        }
    }
}
