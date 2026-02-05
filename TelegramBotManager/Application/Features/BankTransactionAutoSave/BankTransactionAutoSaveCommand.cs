using MediatR;

namespace TelegramBotManager.Application.Features.BankTransactionAutoSave;

public class BankTransactionAutoSaveCommand : IRequest<BankTransactionAutoSaveResult>
{
    public string MessageBody { get; set; }
}