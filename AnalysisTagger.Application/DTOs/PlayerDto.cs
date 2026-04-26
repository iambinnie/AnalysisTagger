namespace AnalysisTagger.Application.DTOs;

public class PlayerDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int ShirtNumber { get; set; }
    public string Position { get; set; } = string.Empty;
    public string? PhotoPath { get; set; }
}

public class CreatePlayerDto
{
    public string Name { get; set; } = string.Empty;
    public int ShirtNumber { get; set; }
    public string Position { get; set; } = string.Empty;
    public string? PhotoPath { get; set; }
}

public class UpdatePlayerDto
{
    public string Name { get; set; } = string.Empty;
    public int ShirtNumber { get; set; }
    public string Position { get; set; } = string.Empty;
    public string? PhotoPath { get; set; }
}
