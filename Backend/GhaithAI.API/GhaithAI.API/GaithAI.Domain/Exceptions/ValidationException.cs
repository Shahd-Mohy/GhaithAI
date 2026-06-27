namespace GhaithAI.GaithAI.Domain.Exceptions
{
    public class ValidationException : BadRequestException
    {
        public ValidationException(string message) : base(message) { }
    }
}
