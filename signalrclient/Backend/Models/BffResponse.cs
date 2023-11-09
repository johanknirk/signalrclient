#nullable enable
namespace TrumfApp.Framework.Backend.Models;

public class BffResponse<T>
{
    public BffResponse(T response)
    {
        Response = response;
    }

    public BffResponse(ErrorResponse error)
    {
        Error = error;
    }

    /// <summary>
    /// The response data, if the request was a success
    /// </summary>
    public T? Response { get; }

    /// <summary>
    /// Whether or not the request was successfully sent and handled by the backend
    /// </summary>
    public bool Success => Error == null;

    /// <summary>
    /// The error data, if the request was not a success
    /// </summary>
    public ErrorResponse? Error { get; }
}