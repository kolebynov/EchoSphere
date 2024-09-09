using EchoSphere.ApiGateway.Contracts;
using Refit;

namespace EchoSphere.ApiGateway.Client;

public interface IChatClient
{
	[Get("/chats")]
	Task<IReadOnlyList<ChatInfoDtoV1>> GetUserChats(CancellationToken cancellationToken);

	[Get("/chats/{chatId}/messages")]
	Task<IReadOnlyList<ChatMessageDtoV1>> GetChatMessages(Guid chatId, CancellationToken cancellationToken);

	[Post("/chats")]
	Task<Guid> CreateChat([Body] CreateChatRequestV1 request, CancellationToken cancellationToken);

	[Post("/chats/{chatId}/messages")]
	Task SendMessage(Guid chatId, [Body] SendMessageRequestV1 request, CancellationToken cancellationToken);
}