namespace EchoSphere.BlazorShared;

public sealed class LoaderOverlayService
{
	public event Action? OnLoaderShow;

	public event Action? OnLoaderHide;

	public void ShowLoader() => OnLoaderShow?.Invoke();

	public void HideLoader() => OnLoaderHide?.Invoke();
}