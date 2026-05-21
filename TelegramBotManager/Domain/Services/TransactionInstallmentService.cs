using TelegramBotManager.Domain.Entities.FinancialControl;

namespace TelegramBotManager.Domain.Services;

public static class TransactionInstallmentService
{
    public static List<Transaction> CreateInstallments(
        string baseDescription,
        decimal value,
        DateTime firstDate,
        string creditCardName,
        Category? category,
        int? numberOfInstallments)
    {
        var transactions = new List<Transaction>();
        var categoryId = category?.Id;
        
        var moneyValue = new ValueObjects.Money(value);
        var creditCard = new ValueObjects.CreditCard(creditCardName);

        if (!numberOfInstallments.HasValue || numberOfInstallments.Value <= 1)
        {
            // Single transaction
            var t = new Transaction(baseDescription, moneyValue, firstDate, creditCard, categoryId, null);
            if (category != null) t.SetCategory(category);
            transactions.Add(t);
            return transactions;
        }

        // Installments
        var total = numberOfInstallments.Value;

        // First
        var firstDesc = $"{baseDescription} (Parcelado em {total})";
        var first = new Transaction(firstDesc, moneyValue, firstDate, creditCard, categoryId, 1);
        if (category != null) first.SetCategory(category);
        transactions.Add(first);

        // Others
        for (int i = 1; i < total; i++)
        {
            var date = firstDate.AddMonths(i);
            var desc = $"{baseDescription} (Parcela {i + 1}/{total})";
            var t = new Transaction(desc, moneyValue, date, creditCard, categoryId, i + 1);
            if (category != null) t.SetCategory(category);
            transactions.Add(t);
        }

        return transactions;
    }
}
