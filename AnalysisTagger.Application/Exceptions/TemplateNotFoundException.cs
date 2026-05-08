namespace AnalysisTagger.Application.Exceptions;

public class TemplateNotFoundException : Exception
{
    public TemplateNotFoundException(Guid id) : base($"Template '{id}' not found.") { }
}
