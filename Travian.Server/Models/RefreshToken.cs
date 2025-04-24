namespace YourProjectName.Models;
// Models/RefreshToken.cs
public class RefreshToken
{
    public Guid Id { get; set; }
    public string Token { get; set; }
    public DateTime Expires { get; set; }
    public DateTime Created { get; set; }
    public string CreatedByIp { get; set; }
    public bool IsExpired => DateTime.UtcNow >= Expires;
    
    public Guid PlayerId { get; set; }
    public Player Player { get; set; }
}