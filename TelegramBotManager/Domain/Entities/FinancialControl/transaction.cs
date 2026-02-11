using TelegramBotManager.Domain.Interfaces;

namespace TelegramBotManager.Domain.Entities.FinancialControl;

public class Transaction : IEntity
{
    public long Id { get; private set; }
    public DateTime Date { get; private set; }
    public string CreditCard { get; private set; }
    public decimal Value { get; private set; }
    public string Description { get; private set; }
    public long? CategoryId { get; private set; }
    public int? ParcelNumber { get; private set; }
    public Category? Category { get; private set; }

    protected Transaction() { }

    public Transaction(string description, decimal value, DateTime date, string creditCard, long? categoryId, int? parcelNumber)
    {
        // Simple validation
        if (string.IsNullOrWhiteSpace(description)) throw new ArgumentNullException(nameof(description));
        if (value <= 0) throw new ArgumentException("Value must be greater than zero", nameof(value));

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

    public void SetId(long id)
    {
        Id = id;
    }

    public static List<Transaction> CreateInstallments(
        string baseDescription,
        decimal value,
        DateTime firstDate,
        string creditCard,
        Category? category,
        int? numberOfInstallments)
    {
        var transactions = new List<Transaction>();
        var categoryId = category?.Id;

        if (!numberOfInstallments.HasValue || numberOfInstallments.Value <= 1)
        {
            // Single transaction
            var t = new Transaction(baseDescription, value, firstDate, creditCard, categoryId, null);
            if (category != null) t.SetCategory(category);
            transactions.Add(t);
            return transactions;
        }

        // Installments
        var total = numberOfInstallments.Value;

        // First
        var firstDesc = $"{baseDescription} (Parcelado em {total})";
        var first = new Transaction(firstDesc, value, firstDate, creditCard, categoryId, 1);
        if (category != null) first.SetCategory(category);
        transactions.Add(first);

        // Others
        for (int i = 1; i < total; i++)
        {
            var date = firstDate.AddMonths(i);
            var desc = $"{baseDescription} (Parcela {i + 1}/{total})";
            var t = new Transaction(desc, value, date, creditCard, categoryId, i + 1);
            if (category != null) t.SetCategory(category);
            transactions.Add(t);
        }

        return transactions;
    }
}