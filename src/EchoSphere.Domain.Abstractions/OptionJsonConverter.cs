using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EchoSphere.Domain.Abstractions;

internal sealed class OptionJsonConverter : JsonConverter<IOptional>
{
	private readonly ConcurrentDictionary<Type, Converter> _converters = new();

	public override bool CanConvert(Type typeToConvert) =>
		typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(Option<>);

	public override IOptional Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
		GetConverter(typeToConvert).Reader(ref reader, options);

	public override void Write(Utf8JsonWriter writer, IOptional value, JsonSerializerOptions options) =>
		GetConverter(value.GetType()).Writer(writer, value, options);

	private Converter GetConverter(Type typeToConvert) =>
		_converters.GetOrAdd(
			typeToConvert,
			static t =>
			{
				var underlyingType = t.GetGenericArguments()[0];
				var someMethod = t.GetMethod(nameof(Option<string>.Some), BindingFlags.Public | BindingFlags.Static)!;
				var noneField = t.GetField(nameof(Option<string>.None), BindingFlags.Public | BindingFlags.Static)!;

				var reader = new Reader((ref Utf8JsonReader reader, JsonSerializerOptions options) =>
				{
					var value = JsonSerializer.Deserialize(ref reader, underlyingType, options);
					return value != null
						? (IOptional)someMethod.Invoke(null, [value])!
						: (IOptional)noneField.GetValue(null)!;
				});

				var writer = (Utf8JsonWriter writer, IOptional value, JsonSerializerOptions options) =>
				{
					value.MatchUntyped(
						some =>
						{
							JsonSerializer.Serialize(writer, some, options);
							return Unit.Default;
						},
						() =>
						{
							writer.WriteNullValue();
							return Unit.Default;
						});
				};

				return new Converter(reader, writer);
			});

	private delegate IOptional Reader(ref Utf8JsonReader reader, JsonSerializerOptions options);

	private record struct Converter(Reader Reader, Action<Utf8JsonWriter, IOptional, JsonSerializerOptions> Writer);
}