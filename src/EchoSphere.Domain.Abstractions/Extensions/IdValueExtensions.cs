using EchoSphere.Domain.Abstractions.Models;

namespace EchoSphere.Domain.Abstractions.Extensions;

public static class IdValueExtensions
{
	public static T Parse<T>(string value)
		where T : IIdValue<Guid>, new()
	{
		return new T
		{
			Value = Guid.Parse(value),
		};
	}

	public static Option<T> TryParse<T>(string value)
		where T : IIdValue<Guid>, new() =>
		Guid.TryParse(value, out var result) ? new T { Value = result } : None;

	public static string ToInnerString<T>(this T idValue)
		where T : IIdValue<Guid>, new()
		=> idValue.Value.ToString("D");
}