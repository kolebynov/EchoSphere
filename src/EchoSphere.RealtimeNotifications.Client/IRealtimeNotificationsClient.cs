using R3;

namespace EchoSphere.RealtimeNotifications.Client;

public interface IRealtimeNotificationsClient
{
	Observable<TEvent> GetEventObservable<TEvent>();
}