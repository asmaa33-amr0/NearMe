using Microsoft.AspNetCore.Mvc;

namespace NearMe.Services.Common
{
    public enum ServiceError { None, NotFound, Forbidden, Conflict, Invalid }

    public class ServiceResult<T>
    {
        public bool Success { get; init; }
        public T? Data { get; init; }
        public ServiceError Error { get; init; }
        public string? Message { get; init; }

        public static ServiceResult<T> Ok(T data) => new() { Success = true, Data = data };
        public static ServiceResult<T> Fail(ServiceError e, string msg) => new() { Error = e, Message = msg };
    }

    public static class ServiceResultExtensions
    {
        public static ActionResult ToActionResult<T>(this ControllerBase c, ServiceResult<T> r) =>
            r.Success ? c.Ok(r.Data) : r.Error switch
            {
                ServiceError.NotFound => c.NotFound(r.Message),
                ServiceError.Forbidden => c.Forbid(),
                ServiceError.Conflict => c.Conflict(r.Message),
                _ => c.BadRequest(r.Message)
            };
    }
}
