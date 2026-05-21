namespace TelegramBotManager.Domain.ValueObjects;

public class Money
{
    public decimal Value { get; }

    public Money(decimal value)
    {
        if (value <= 0)
            throw new ArgumentException("O valor deve ser maior que zero.", nameof(value));

        Value = value;
    }

    public static implicit operator decimal(Money money) => money?.Value ?? 0m;
    public static explicit operator Money(decimal value) => new Money(value);

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj)) return true;
        if (obj is null) return false;
        return obj is Money money && money.Value == Value;
    }

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value.ToString("C");
}
