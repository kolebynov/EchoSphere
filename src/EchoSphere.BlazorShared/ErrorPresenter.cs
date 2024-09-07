using Microsoft.Extensions.Localization;
using MudBlazor;

namespace EchoSphere.BlazorShared;

public sealed class ErrorPresenter(ISnackbar snackbar, IStringLocalizer<ErrorPresenter> localizer)
{
	public void DisplayError(Exception exception, string? titleLocalizationKey, string? afterMessageLocalizationKey)
	{
		_ = exception ?? throw new ArgumentNullException(nameof(exception));
		var prefix = localizer[titleLocalizationKey ?? "ErrorOccured"];
		var message = exception switch
		{
			_ => exception.Message,
		};

		message = !string.IsNullOrEmpty(afterMessageLocalizationKey)
			? $"{message} {localizer[afterMessageLocalizationKey]}"
			: message;

		snackbar.Add(
			builder =>
			{
				builder.OpenElement(1, "div");
				builder.OpenElement(2, "h3");
				builder.AddContent(3, prefix);
				builder.CloseElement();
				builder.OpenElement(4, "span");
				builder.AddContent(5, message);
				builder.CloseElement();
				builder.CloseElement();
			},
			Severity.Error);
	}
}