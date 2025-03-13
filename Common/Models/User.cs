namespace lb_aspnetcore_filters.Common.Models;

public class User
{
    public int Id { get; set; }
    
    public required string FirstName { get; set; }
    
    public required string LastName { get; set; }
    
    public required string AccessToken { get; set; }
    
    public required string Role { get; set; }
    
    public DateTime LastRequest { get; set; }
}