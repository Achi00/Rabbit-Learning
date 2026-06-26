namespace RabbitMQ.Application.Results
{
    public class OperationResult
    {
        public bool Success { get; }
        public string? ErrorMessage { get; }

        private OperationResult(bool success, string? errorMessage)
        {
            Success = success;
            ErrorMessage = errorMessage;
        }

        public static OperationResult Ok()
        {
            return new OperationResult(true, null);
        }

        public static OperationResult Fail(string errorMessage)
        {
            return new OperationResult(false, errorMessage);
        }
    }
}
