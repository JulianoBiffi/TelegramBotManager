using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using TelegramBotManager.Domain.Interfaces;

namespace TelegramBotManager.Domain.Entities.FinancialControl;

[Table("category")]
public class category : BaseModel, IEntity
{
    [PrimaryKey("id", false)]
    public long Id { get; set; }

    [Column("description")]
    public string Description { get; set; }
}
