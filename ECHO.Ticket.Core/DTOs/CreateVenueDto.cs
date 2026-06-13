namespace ECHO.Ticket.Core.DTOs;

public class CreateVenueDto
{
    public string Name { get; set; } = string.Empty;
    public int Rows { get; set; }
    public int Columns { get; set; }
}