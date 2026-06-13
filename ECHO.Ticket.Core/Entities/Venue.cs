namespace ECHO.Ticket.Core.Entities;

public class Venue
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Rows { get; set; }
    public int Columns { get; set; }
    
    public ICollection<Event> Events { get; set; } = new List<Event>();
}