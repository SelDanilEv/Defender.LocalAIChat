using AILocalHelper.DB;
using AILocalHelper.Domain;
using Microsoft.AspNetCore.SignalR;

namespace AILocalHelper.Messaging;


public class CommunicationService
{
    private readonly IHubContext<CommunicationHub> _hubContext;
    private readonly LiteDBService _dbService;

    public CommunicationService(
        IHubContext<CommunicationHub> hubContext,
        LiteDBService liteDBService)
    {
        _hubContext = hubContext;
        _dbService = liteDBService;
    }

    public async Task AddHistoryRecord(HistoryRecord record)
    {
        _dbService.AddToHistory(record);
        await _hubContext.Clients.All.SendAsync("AddHistoryRecord", record.ToString());
    }

    public async Task SetPartialAIResponse(HistoryRecord record)
    {
        await _hubContext.Clients.All.SendAsync("SetPartialAIResponse", record.ToString());
    }

    public async Task SetLock(bool isLocked)
    {
        await _hubContext.Clients.All.SendAsync("SetLock", isLocked);
    }
}
