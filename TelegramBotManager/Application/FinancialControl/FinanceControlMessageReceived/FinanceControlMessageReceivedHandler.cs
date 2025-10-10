using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace TelegramBotManager.Application.FinancialControl.FinanceControlMessageReceived;

public class FinanceControlMessageReceivedHandler(IMapper _mapper, IConfiguration _configuration) : IRequestHandler<FinanceControlMessageReceivedCommand, FinanceControlMessageReceivedResult>
{
    public async Task<FinanceControlMessageReceivedResult> Handle(FinanceControlMessageReceivedCommand command, CancellationToken cancellationToken)
    {
        var validator = new FinanceControlMessageReceivedValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);


        return new FinanceControlMessageReceivedResult();
    }
}
