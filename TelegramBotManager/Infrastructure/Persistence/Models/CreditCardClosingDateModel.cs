using Newtonsoft.Json;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace TelegramBotManager.Infrastructure.Persistence.Models;

[Table("credit_card_closing_date")]
public class CreditCardClosingDateModel : BaseModel
{
    [PrimaryKey("id", false)]
    public long Id { get; set; }

    [Column("closing_date")]
    [JsonProperty("closing_date")]
    public int ClosingDate { get; set; }

    [Column("best_day_to_buy")]
    [JsonProperty("best_day_to_buy")]
    public int BestDayToBuy { get; set; }

    [Column("bank_name")]
    [JsonProperty("bank_name")]
    public string BankName { get; set; } = null!;
}
