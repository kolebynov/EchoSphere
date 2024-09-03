namespace EchoSphere.UserMessagesService.Models;

internal sealed class UserMessage
{
	public required Guid FromUserId { get; init; }

	public required Guid ToUserId { get; init; }

	public required string Text { get; init; }
}