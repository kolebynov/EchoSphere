using Microsoft.Extensions.Configuration;

namespace EchoSphere.BlazorShared.Extensions;

public static class ConfigurationExtensions
{
	public static IEnumerable<(string Key, string Value)> EnumerateDescendantValues(this IConfiguration configuration)
	{
		foreach (var section in configuration.GetChildren())
		{
			if (section.Value != null)
			{
				yield return (section.Path, section.Value);
			}
			else
			{
				foreach (var tuple in EnumerateDescendantValues(section))
				{
					yield return tuple;
				}
			}
		}
	}
}