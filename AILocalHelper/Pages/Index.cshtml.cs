using AILocalHelper.AI;
using AILocalHelper.DB;
using AILocalHelper.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;

namespace AILocalHelper.Pages
{
    public class IndexModel : PageModel
    {
        private readonly LiteDBService _dBService;
        private readonly ChatService _chatService;
        private Settings _config;

        [BindProperty(SupportsGet = true)]
        public string PathToModel { get; set; }
        [BindProperty(SupportsGet = true)]
        public string UserPrompt { get; set; }
        [BindProperty(SupportsGet = true)]
        public string CommunicationHistory { get; set; }
        [BindProperty(SupportsGet = true)]
        public bool IsLocked { get; set; }


        public IndexModel(
            LiteDBService dBService,
            ChatService chatService
        )
        {
            _dBService = dBService;
            _chatService = chatService;
            _config = _dBService.GetConfig();
        }

        public void OnGet()
        {
            CommunicationHistory = FormatHistory(_config.HistoryRecords);
            PathToModel = _config.PathToModel;
            IsLocked = _config.IsLocked;
        }

        public IActionResult OnPostSavePath()
        {
            _dBService.SetPathToModel(PathToModel);

            return RedirectToPage();
        }

        public IActionResult OnPostResetConfig()
        {
            _dBService.ResetConfig();

            return RedirectToPage();
        }

        public IActionResult OnPostCleanContext()
        {
            _dBService.ClearContext();

            return RedirectToPage();
        }

        public IActionResult OnPostAskAI()
        {
            if (string.IsNullOrWhiteSpace(UserPrompt))
            {
                return RedirectToPage();
            }

            _chatService.Ask(UserPrompt);

            return RedirectToPage();
        }

        public JsonResult OnGetCheckLock()
        {
            _config = _dBService.GetConfig();
            IsLocked = _config.IsLocked;
            return new JsonResult(IsLocked);
        }

        public JsonResult OnGetUpdateHistory()
        {
            _config = _dBService.GetConfig();
            CommunicationHistory = FormatHistory(_config.HistoryRecords);
            return new JsonResult(CommunicationHistory);
        }

        private string FormatHistory(List<HistoryRecord> historyRecords)
        {
            if (historyRecords == null || !historyRecords.Any())
                return "No history available.";

            var formattedHistory = new StringBuilder();

            historyRecords.Reverse();
            foreach (var record in historyRecords)
            {
                formattedHistory.AppendLine(
                    $"{record.CreatedDateTime.ToShortDateString()} {record.CreatedDateTime.ToShortTimeString()}: {record.Actor} - {record.Message}");
            }

            return formattedHistory.ToString();
        }
    }
}
