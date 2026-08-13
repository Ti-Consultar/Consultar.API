namespace _2___Application._1_Services.BreakEven.V2;

public sealed class BreakEvenValidationException : ArgumentException
{
    public BreakEvenValidationException(string message) : base(message)
    {
    }
}

public sealed class BreakEvenCalculationException : InvalidOperationException
{
    public BreakEvenCalculationException(string message) : base(message)
    {
    }
}

public sealed class BreakEvenNotFoundException : KeyNotFoundException
{
    public BreakEvenNotFoundException(string message) : base(message)
    {
    }
}

public sealed class BreakEvenAccessDeniedException : UnauthorizedAccessException
{
    public BreakEvenAccessDeniedException() : base("O usuário não possui acesso ao escopo financeiro informado.")
    {
    }
}
