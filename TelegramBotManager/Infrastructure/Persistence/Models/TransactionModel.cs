using Newtonsoft.Json;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using TelegramBotManager.Domain.Entities.FinancialControl;

namespace TelegramBotManager.Infrastructure.Persistence.Models;

[Table("transaction")]
public class TransactionModel : BaseModel
{
    [PrimaryKey("id", false)]
    public long Id { get; set; }

    [Column("date")]
    public DateTime Date { get; set; }

    [Column("credit_card")]
    public string CreditCard { get; set; } = null!;

    [Column("value")]
    public decimal Value { get; set; }

    [Column("description")]
    public string Description { get; set; } = null!;

    [Column("category_id")]
    public long? CategoryId { get; set; }

    [Column("parcel_number")]
    public int? ParcelNumber { get; set; }

    [Reference(typeof(CategoryModel), useInnerJoin: false)]
    public CategoryModel? Category { get; set; } = new();
}
