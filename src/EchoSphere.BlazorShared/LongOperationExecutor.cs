namespace EchoSphere.BlazorShared;

public sealed class LongOperationExecutor
{
	private readonly ErrorPresenter _errorPresenter;
	private readonly LoaderOverlayService _loaderOverlayService;

	public LongOperationExecutor(ErrorPresenter errorPresenter, LoaderOverlayService loaderOverlayService)
	{
		_errorPresenter = errorPresenter;
		_loaderOverlayService = loaderOverlayService;
	}

	public Task<bool> ExecuteLongOperation(Func<Task> longOperation, string? titleLocalizationKey = null, string? afterMessageLocalizationKey = null)
	{
		_ = longOperation ?? throw new ArgumentNullException(nameof(longOperation));

		return ExecuteLongOperationAndReturnDefaultIfError(
			async () =>
			{
				await longOperation();
				return true;
			}, false, titleLocalizationKey, afterMessageLocalizationKey);
	}

	public async Task<T> ExecuteLongOperationAndReturnDefaultIfError<T>(Func<Task<T>> longOperation, T defaultValue,
		string? titleLocalizationKey = null, string? afterMessageLocalizationKey = null, bool showLoader = true)
	{
		_ = longOperation ?? throw new ArgumentNullException(nameof(longOperation));

		try
		{
			ShowLoader(showLoader);
			return await longOperation();
		}
		catch (Exception e)
		{
			_errorPresenter.DisplayError(e, titleLocalizationKey, afterMessageLocalizationKey);
			return defaultValue;
		}
		finally
		{
			HideLoader(showLoader);
		}
	}

	private void ShowLoader(bool show)
	{
		if (show)
		{
			_loaderOverlayService.ShowLoader();
		}
	}

	private void HideLoader(bool show)
	{
		if (show)
		{
			_loaderOverlayService.HideLoader();
		}
	}
}