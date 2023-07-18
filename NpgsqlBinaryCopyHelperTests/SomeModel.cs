namespace NpgsqlBinaryCopyHelperTests;

public class SomeModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public int Age { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public decimal Balance { get; set; }
    public string? Comment { get; set; }
}