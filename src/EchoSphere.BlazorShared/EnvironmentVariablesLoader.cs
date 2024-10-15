using System.Globalization;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;

namespace EchoSphere.BlazorShared;

[SupportedOSPlatform("browser")]
internal static partial class EnvironmentVariablesLoader
{
	public static void LoadEnvironmentVariables()
	{
		var environmentVariablesObject = GetEnvironmentVariables();
		var keys = environmentVariablesObject.GetPropertyAsJSObject("keys")!;
		var values = environmentVariablesObject.GetPropertyAsJSObject("values")!;
		var length = keys.GetPropertyAsInt32("length");

		for (var i = 0; i < length; i++)
		{
			var indexStr = i.ToString(CultureInfo.InvariantCulture);
			Environment.SetEnvironmentVariable(keys.GetPropertyAsString(indexStr)!, values.GetPropertyAsString(indexStr));
		}
	}

	[JSImport("globalThis.getEnvironmentVariables")]
	private static partial JSObject GetEnvironmentVariables();
}