namespace EchoSphere.ApiGateway.Contracts;

public sealed class NotificationDtoV1
{
	public required long Id { get; init; }

	public required string Text { get; init; }

	public required bool IsRead { get; init; }
}