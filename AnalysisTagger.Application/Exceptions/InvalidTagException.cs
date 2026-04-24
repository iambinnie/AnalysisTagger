namespace AnalysisTagger.Application.Exceptions;

public class InvalidTagException : AnalysisTaggerException
{
    public InvalidTagException(string message) : base(message) { }
}
