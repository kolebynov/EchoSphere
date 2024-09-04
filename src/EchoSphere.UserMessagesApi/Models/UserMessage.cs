namespace EchoSphere.UserMessagesApi.Models;

internal sealed class UserMessage
{
	public long Id { get; }

	public required Guid FromUserId { get; init; }

	public required Guid ToUserId { get; init; }

	public required string Text { get; init; }
}