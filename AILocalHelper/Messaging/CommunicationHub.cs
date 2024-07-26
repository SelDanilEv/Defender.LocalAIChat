using AILocalHelper.AI;
using AILocalHelper.DB;
using AILocalHelper.Domain;
using Microsoft.AspNetCore.SignalR;

namespace AILocalHelper.Messaging;

public class CommunicationHub : Hub
{
    private readonly CommunicationService _communicationService;
    private readonly ChatService _chatService;
    private readonly LiteDBService _dBService;

    public CommunicationHub(
        CommunicationService communicationService, 
        ChatService chatService,
        LiteDBService dBService)
    {
        _communicationService = communicationService;
        _chatService = chatService;
        _dBService = dBService;
    }

    public async Task<List<string>> GetHistoryRecords()
    {
        var historyRecords = _dBService.GetHistoryRecords();
        return historyRecords.Select(x => x.ToString()).ToList();
    }

    public async Task AskAI(string userPrompt)
    {
        await SetLock(true);

        try
        {
            await _chatService.Ask(userPrompt);
        }
        finally
        {
            await SetLock(false);
        }
    }

    public async Task SetLock(bool isLocked)
    {
        await _communicationService.SetLock(isLocked);
    }

    public async Task AddHistoryRecord(HistoryRecord record)
    {
        await _communicationService.AddHistoryRecord(record);
    }

    public async Task AddPartialAIResponse(HistoryRecord record)
    {
        await _communicationService.SetPartialAIResponse(record);
    }
}
