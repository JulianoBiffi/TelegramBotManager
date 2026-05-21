namespace TelegramBotManager.Domain.Entities.FinancialControl;

public class Category : IEntity
{
    public long Id { get; private set; }
    public string Description { get; private set; }

    protected Category() { }

    public Category(string description)
    {
        if (string.IsNullOrWhiteSpace(description)) throw new ArgumentNullException(nameof(description));
        Description = description;
    }

    public void SetId(long id)
    {
        Id = id;
    }
}
