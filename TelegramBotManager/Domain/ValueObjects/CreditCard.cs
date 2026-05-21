namespace TelegramBotManager.Domain.ValueObjects;

public class CreditCard
{
    public string Name { get; }

    public CreditCard(string name)
    {
        Name = string.IsNullOrWhiteSpace(name) ? null : name.Trim();
    }

    public bool IsEmpty => string.IsNullOrEmpty(Name);

    public static implicit operator string(CreditCard creditCard) => creditCard?.Name;
    public static explicit operator CreditCard(string name) => new CreditCard(name);

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj)) return true;
        if (obj is null) return false;
        return obj is CreditCard card && card.Name == Name;
    }

    public override int GetHashCode() => Name?.GetHashCode() ?? 0;

    public override string ToString() => Name ?? string.Empty;
}
