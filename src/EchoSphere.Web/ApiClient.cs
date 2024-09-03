namespace EchoSphere.Web;

public class ApiClient(HttpClient httpClient)
{
	public Task SendMessage(string fromUserId, string toUserId, string text, CancellationToken cancellationToken = default)
	{
		return httpClient.PostAsJsonAsync(
			"/messages",
			new Dictionary<string, string>
			{
				["fromUserId"] = fromUserId,
				["toUserId"] = toUserId,
				["text"] = text,
			},
			cancellationToken);
	}

	public Task<IReadOnlyList<string>> GetMessages(string fromUserId, string toUserId,
		CancellationToken cancellationToken = default)
	{
		return httpClient.GetFromJsonAsync<IReadOnlyList<string>>($"/messages?fromUserId={fromUserId}&toUserId={toUserId}", cancellationToken)!;
	}
}