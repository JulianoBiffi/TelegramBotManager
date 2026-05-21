namespace TelegramBotManager.Domain.Entities.FinancialControl;

public class CreditCardClosingDate : IEntity
{
    public long Id { get; private set; }
    public int ClosingDate { get; private set; }
    public int BestDayToBuy { get; private set; }
    public string BankName { get; private set; }

    protected CreditCardClosingDate() { }

    public CreditCardClosingDate(string bankName, int closingDate, int bestDayToBuy)
    {
        if (string.IsNullOrWhiteSpace(bankName))
            throw new ArgumentNullException(nameof(bankName));

        if (closingDate < 1 || closingDate > 31)
            throw new ArgumentException("Closing date must be between 1 and 31", nameof(closingDate));

        if (bestDayToBuy < 1 || bestDayToBuy > 31)
            throw new ArgumentException("Best day to buy must be between 1 and 31", nameof(bestDayToBuy));

        BankName = bankName.Trim().ToUpper();
        ClosingDate = closingDate;
        BestDayToBuy = bestDayToBuy;
    }

    public void SetId(long id)
    {
        Id = id;
    }

    public void UpdateClosingDate(int closingDate)
    {
        if (closingDate < 1 || closingDate > 31)
            throw new ArgumentException("Closing date must be between 1 and 31", nameof(closingDate));

        ClosingDate = closingDate;
    }

    public void UpdateBestDayToBuy(int bestDayToBuy)
    {
        if (bestDayToBuy < 1 || bestDayToBuy > 31)
            throw new ArgumentException("Best day to buy must be between 1 and 31", nameof(bestDayToBuy));

        BestDayToBuy = bestDayToBuy;
    }
}
