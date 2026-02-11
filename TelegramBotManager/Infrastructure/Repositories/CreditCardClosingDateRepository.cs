using Supabase;
using TelegramBotManager.Domain.Entities.FinancialControl;
using TelegramBotManager.Domain.Interfaces;
using TelegramBotManager.Infrastructure.Persistence.Models;
using TelegramBotManager.Infrastructure.Repositories.Mappers;
using static Supabase.Postgrest.Constants;

namespace TelegramBotManager.Infrastructure.Repositories;

public class CreditCardClosingDateRepository : BaseRepository<CreditCardClosingDateModel>, ICreditCardClosingDateRepository
{
    public CreditCardClosingDateRepository(Client supabaseClient) : base(supabaseClient)
    {
    }

    public async Task<CreditCardClosingDate?> GetByBankNameAsync(string bankName, CancellationToken cancellationToken)
    {
        var normalizedBankName = bankName.Trim().ToLower();

        var response = await _supabaseClient
            .From<CreditCardClosingDateModel>()
            .Select("*")
            .Filter("bank_name", Operator.Equals, normalizedBankName)
            .Get(cancellationToken: cancellationToken);

        var model = response.Models?.FirstOrDefault();
        return CreditCardClosingDateMapper.ToDomain(model);
    }

    public async Task<CreditCardClosingDate> SaveAsync(CreditCardClosingDate entity, CancellationToken cancellationToken)
    {
        var model = CreditCardClosingDateMapper.ToModel(entity);

        var result = model.Id > 0
            ? await base.UpdateAsync(model, cancellationToken)
            : await base.InsertAsync(model, cancellationToken);

        return CreditCardClosingDateMapper.ToDomain(result);
    }

    public async Task<List<CreditCardClosingDate>> GetAllAsync(CancellationToken cancellationToken)
    {
        var models = await base.GetAllAsync(cancellationToken);
        return models.Select(CreditCardClosingDateMapper.ToDomain).ToList();
    }
}
