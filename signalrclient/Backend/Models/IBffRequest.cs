
using TrumfApp.Framework.Backend.Models;

namespace signalrclient.Backend.Models;

/// <summary>
/// Marker interface to represent a request with a response
/// </summary>
/// <typeparam name="TResponse">Response type</typeparam>
public interface IBffRequest<out TResponse> : IBaseRequest { }

/// <summary>
/// Marker interface to represent a request with a void response
/// </summary>
public interface IBffRequest : IBffRequest<Default>
{
}
