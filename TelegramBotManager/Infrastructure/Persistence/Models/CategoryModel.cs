using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace TelegramBotManager.Infrastructure.Persistence.Models;

[Table("category")]
public class CategoryModel : BaseModel
{
    [PrimaryKey("id", false)]
    public long Id { get; set; }

    [Column("description")]
    public string Description { get; set; } = null!;
}
