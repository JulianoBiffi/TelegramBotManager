using Microsoft.Extensions.Azure;
using Supabase.Postgrest;
using Supabase.Postgrest.Models;
using static Supabase.Postgrest.Constants;
using static Supabase.Postgrest.QueryOptions;

namespace TelegramBotManager.Infrastructure.Repositories;

/// <summary>
/// Makes available the basic methods to persist data on the Supabase database
/// </summary>
/// <typeparam name="T">Your Entity</typeparam>
public abstract class BaseRepository<TModel> where TModel : BaseModel, new()
{
    /// <summary>
    /// Defines the Supabase client for querying the database
    /// </summary>
    public readonly Supabase.Client _supabaseClient;

    public BaseRepository(Supabase.Client supabaseClient) => (_supabaseClient) = (supabaseClient);

    public async Task<List<TModel>> GetAllAsync(CancellationToken cancellationToken)
    {
        var response = await _supabaseClient.From<TModel>().Select("*").Get(cancellationToken);

        return response.Models;
    }

    public async Task<TModel?> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        var response =
            await _supabaseClient.From<TModel>()
                                 .Select("*")
                                 .Filter("id", Operator.Equals, id.ToString())
                                 .Get(cancellationToken: cancellationToken);

        return response.Model;
    }

    public async Task<TModel> InsertAsync(TModel entity, CancellationToken cancellationToken)
    {
        var response =
            await _supabaseClient.From<TModel>()
                                 .Insert(entity, new QueryOptions { Returning = ReturnType.Representation });

        return response.Models.FirstOrDefault();
    }

    public async Task<TModel> UpdateAsync(TModel entity, CancellationToken cancellationToken)
    {
        var response =
            await entity.Update<TModel>();

        return response.Models.FirstOrDefault();
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken)
    {
        await _supabaseClient.From<TModel>().Filter("Id", Operator.Equals, id).Delete(cancellationToken: cancellationToken);
    }
}