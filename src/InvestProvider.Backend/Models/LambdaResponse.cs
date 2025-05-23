using FluentValidation;

namespace InvestProvider.Backend.Models
{
    public class LambdaResponse
    {
        public LambdaResponse(object? handlerResponse)
        {
            HandlerResponse = handlerResponse;
        }

        public LambdaResponse(Exception exception)
        {
            ErrorType = exception.GetType().Name;
            ErrorMessage = exception.Message;
        }

        public LambdaResponse(ValidationException exception)
        {
            var error = exception.Errors.First();
            ErrorType = error.ErrorCode;
            ErrorMessage = error.ErrorMessage;
            ErrorData = error.CustomState;
        }

        public object? HandlerResponse { get; }
        public string? ErrorMessage { get; }
        public string? ErrorType { get; }
        public object? ErrorData { get; }
    }
}
