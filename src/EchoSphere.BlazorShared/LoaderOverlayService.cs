namespace EchoSphere.BlazorShared;

public sealed class LoaderOverlayService
{
#pragma warning disable CA1003
	public event Action? OnLoaderShow;

	public event Action? OnLoaderHide;
#pragma warning restore CA1003

	public void ShowLoader() => OnLoaderShow?.Invoke();

	public void HideLoader() => OnLoaderHide?.Invoke();
}