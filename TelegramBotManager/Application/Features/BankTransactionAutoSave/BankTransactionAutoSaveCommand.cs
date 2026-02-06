using MediatR;
using TelegramBotManager.Application.DTOs;

namespace TelegramBotManager.Application.Features.BankTransactionAutoSave;

public class BankTransactionAutoSaveCommand : IRequest<BankTransactionAutoSaveResult>
{
    public BankTransactionAutoSaveCommand(PurchaseDto purchaseData)
    {
        PurchaseData = purchaseData;
    }

    public PurchaseDto PurchaseData { get; set; }
}