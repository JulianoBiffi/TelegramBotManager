using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBotManager.Application.DTOs;

public class PurchaseDto
{
    public string Package { get; set; }
    public string Title { get; set; }
    public string FullText { get; set; }
    public long Timestamp { get; set; }
}
