namespace MenuDB.Data;

public class MenuUserEntity
{
    public int Id { get; set; }

    public required string AuthSubject { get; set; }

    public required string DisplayName { get; set; }

    public string? Email { get; set; }

    public string? AvatarUrl { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime LastSeenAtUtc { get; set; }
}
