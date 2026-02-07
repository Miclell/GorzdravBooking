namespace Core.Entities.Common;

public interface IOwnedEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
}