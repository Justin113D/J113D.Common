using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace J113D.Common.JSON
{
    /// <summary>
    /// Template converter for json objects.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class JsonObjectConverter<T> : JsonConverter<T> where T : notnull
    {
        /// <summary>
        /// Whether an exception should be thrown upon finding a property that does not appear in the value definitions.
        /// </summary>
        protected virtual bool ErrorOnUnknownProperty => true;

        /// <summary>
        /// Property definitions; Expected json token type (if expected) and the default value, if the propery is not in the object.
        /// </summary>
        public abstract ReadOnlyDictionary<string, PropertyDefinition> PropertyDefinitions { get; }

        /// <inheritdoc/>
        public sealed override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if(reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException($"Expected an object for type \"{typeof(T).Name}\"!");
            }

            Dictionary<string, object?> values = [];
            JsonObjectReaderInstance readerInstance = new(PropertyDefinitions, values);

            while(reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            {
                if(reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException("Expected a property!");
                }

                string propertyName = reader.GetString()!;
                if(readerInstance.CurrentPropertyDefinitions.TryGetValue(propertyName, out PropertyDefinition propdef))
                {
                    reader.Read();
                    if(!(propdef.Nullable && reader.TokenType == JsonTokenType.Null || VerifyTokenType(reader.TokenType, propdef.Type)))
                    {
                        throw new JsonException($"Expected {propdef.Type} token for property \"{propertyName}\" of type \"{typeof(T).Name}\"!");
                    }

                    try
                    {
                        values[propertyName] = ReadValueRaw(ref reader, ref readerInstance, propertyName, options);
                    }
                    catch(Exception e)
                    {
                        if(e is not JsonException)
                        {
                            throw new JsonException($"An error occured while reading property \"{propertyName}\" of type \"{typeof(T).Name}\"!", e);
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
                else if(ErrorOnUnknownProperty)
                {
                    throw new JsonException($"Property \"{propertyName}\" not part of type \"{typeof(T).Name}\"!");
                }
            }

            T result;
            try
            {
                result = CreateRaw(ref readerInstance);
            }
            catch(Exception e)
            {
                if(e is not JsonException)
                {
                    throw new JsonException($"An error occured while creating result for type \"{typeof(T).Name}\"!", e);
                }
                else
                {
                    throw;
                }
            }

            return result;
        }

        private static bool VerifyTokenType(JsonTokenType tokenType, PropertyTokenType propertyType)
        {
            return tokenType switch
            {
                JsonTokenType.StartObject => propertyType.HasFlag(PropertyTokenType.Object),
                JsonTokenType.StartArray => propertyType.HasFlag(PropertyTokenType.Array),
                JsonTokenType.String => propertyType.HasFlag(PropertyTokenType.String),
                JsonTokenType.Number => propertyType.HasFlag(PropertyTokenType.Number),
                JsonTokenType.True or JsonTokenType.False => propertyType.HasFlag(PropertyTokenType.Bool),
                JsonTokenType.None
                or JsonTokenType.EndObject
                or JsonTokenType.EndArray
                or JsonTokenType.PropertyName
                or JsonTokenType.Comment
                or JsonTokenType.Null
                or _ => false,
            };
        }

        /// <summary>
        /// Read a value
        /// </summary>
        /// <param name="reader">Reader responsible for reading</param>
        /// <param name="readerInstance">Current reader instance.</param>
        /// <param name="propertyName">Name of the property to read</param>
        /// <param name="options">An object that specifies serialization options to use</param>
        /// <returns></returns>
        protected abstract object? ReadValueRaw(ref Utf8JsonReader reader, ref JsonObjectReaderInstance readerInstance, string propertyName, JsonSerializerOptions options);

        /// <summary>
        /// Create the object from the read values
        /// </summary>
        /// <param name="readerInstance">Current reader instance.</param>
        /// <returns></returns>
        protected abstract T CreateRaw(ref JsonObjectReaderInstance readerInstance);

        /// <inheritdoc/>
        public sealed override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            try
            {
                WriteValues(writer, value, options);
            }
            catch(Exception e)
            {
                if(e is not JsonException)
                {
                    throw new JsonException($"An error occured while writing type \"{typeof(T).Name}\"!", e);
                }
                else
                {
                    throw;
                }
            }

            writer.WriteEndObject();
        }

        /// <summary>
        /// Writes the properties within the 
        /// </summary>
        /// <param name="writer">The writer to write to</param>
        /// <param name="value">The value to convert to JSON</param>
        /// <param name="options">An object that specifies serialization options to use</param>
        protected abstract void WriteValues(Utf8JsonWriter writer, T value, JsonSerializerOptions options);
    }
}
