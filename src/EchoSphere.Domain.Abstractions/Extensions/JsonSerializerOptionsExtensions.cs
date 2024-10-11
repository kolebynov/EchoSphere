using System.Text.Json;

namespace EchoSphere.Domain.Abstractions.Extensions;

public static class JsonSerializerOptionsExtensions
{
	public static JsonSerializerOptions AddIdValueConverters(this JsonSerializerOptions options)
	{
		options.Converters.Add(new IdValueJsonConverter<long>());
		options.Converters.Add(new IdValueJsonConverter<Guid>());

		return options;
	}
}