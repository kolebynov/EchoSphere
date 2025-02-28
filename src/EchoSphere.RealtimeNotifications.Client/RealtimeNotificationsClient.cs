using Microsoft.AspNetCore.SignalR.Client;
using R3;

namespace EchoSphere.RealtimeNotifications.Client;

internal sealed class RealtimeNotificationsClient : IRealtimeNotificationsClient
{
	private readonly HubConnection _hubConnection;

	public RealtimeNotificationsClient(HubConnection hubConnection)
	{
		_hubConnection = hubConnection;
		_hubConnection.StartAsync();
	}

	public Observable<TEvent> GetEventObservable<TEvent>() =>
		Observable.Create<TEvent, HubConnection>(
			_hubConnection,
			static (observer, hubConnection) => hubConnection.On<TEvent>(typeof(TEvent).FullName!, observer.OnNext));
}