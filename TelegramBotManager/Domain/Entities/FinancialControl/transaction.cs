using Newtonsoft.Json;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using TelegramBotManager.Domain.Interfaces;

namespace TelegramBotManager.Domain.Entities.FinancialControl;

[Table("transaction")]
public class transaction : BaseModel, IEntity
{
    [PrimaryKey("id", false)]
    public long Id { get; set; }

    [Column("date")]
    public DateTime Date { get; set; }

    [Column("credit_card")]
    public string CreditCard { get; set; }

    [Column("value")]
    public decimal Value { get; set; }

    [Column("description")]
    public string Description { get; set; }

    [Column("category_id")]
    public long? CategoryId { get; set; }

    [Column("parcel_number")]
    public int? ParcelNumber { get; set; }

    [Reference(typeof(category), useInnerJoin: false)]
    public category? Category { get; set; } = new();
}