namespace AnalysisTagger.Application.Exceptions;

public abstract class AnalysisTaggerException : Exception
{
    protected AnalysisTaggerException(string message) : base(message) { }
    protected AnalysisTaggerException(string message, Exception inner) : base(message, inner) { }
}
