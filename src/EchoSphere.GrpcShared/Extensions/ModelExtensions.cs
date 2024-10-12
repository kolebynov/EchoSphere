using EchoSphere.Domain.Abstractions.Extensions;
using EchoSphere.Domain.Abstractions.Models;
using EchoSphere.GrpcShared.Contracts;

namespace EchoSphere.GrpcShared.Extensions;

public static class ModelExtensions
{
	public static UserId ToModel(this UserIdDto userIdDto) => IdValueExtensions.Parse<UserId>(userIdDto.Value);

	public static UserIdDto ToDto(this UserId userId) => new() { Value = userId.ToInnerString() };

	public static ChatId ToModel(this ChatIdDto chatIdDto) => IdValueExtensions.Parse<ChatId>(chatIdDto.Value);

	public static ChatIdDto ToDto(this ChatId chatId) => new() { Value = chatId.ToInnerString() };

	public static MessageId ToModel(this MessageIdDto messageIdDto) => new(messageIdDto.Value);

	public static MessageIdDto ToDto(this MessageId messageId) => new() { Value = messageId.Value };

	public static PostId ToModel(this PostIdDto chatIdDto) => IdValueExtensions.Parse<PostId>(chatIdDto.Value);

	public static PostIdDto ToDto(this PostId chatId) => new() { Value = chatId.ToInnerString() };
}