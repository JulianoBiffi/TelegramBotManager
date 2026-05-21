using MediatR;
using TelegramBotManager.Application.DTOs;

namespace TelegramBotManager.Application.Interfaces;

public interface ITelegramMessageRouter
{
    IRequest<Unit> RouteMessage(TelegramUpdateDto update);
}
