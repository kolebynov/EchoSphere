namespace EchoSphere.SharedModels.Extensions;

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

	public static string ToInnerString<T>(this T idValue)
		where T : IIdValue<Guid>, new()
		=> idValue.Value.ToString("D");
}