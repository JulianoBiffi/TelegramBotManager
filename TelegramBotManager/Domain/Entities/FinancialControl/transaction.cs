using TelegramBotManager.Domain.ValueObjects;

namespace TelegramBotManager.Domain.Entities.FinancialControl;

public class Transaction : IEntity
{
    public long Id { get; internal set; }
    public DateTime Date { get; private set; }
    public CreditCard CreditCard { get; private set; }
    public Money Value { get; private set; }
    public string Description { get; private set; }
    public long? CategoryId { get; private set; }
    public int? ParcelNumber { get; private set; }
    public Category? Category { get; private set; }

    protected Transaction() { }

    public Transaction(string description, Money value, DateTime date, CreditCard creditCard, long? categoryId, int? parcelNumber)
    {
        if (string.IsNullOrWhiteSpace(description)) throw new ArgumentNullException(nameof(description));
        if (value == null) throw new ArgumentNullException(nameof(value));

        Description = description;
        Value = value;
        Date = date;
        CreditCard = creditCard;
        CategoryId = categoryId;
        ParcelNumber = parcelNumber;
    }

    public void SetCategory(Category category)
    {
        if (category == null)
            return;

        Category = category;
        CategoryId = category?.Id;
    }
}