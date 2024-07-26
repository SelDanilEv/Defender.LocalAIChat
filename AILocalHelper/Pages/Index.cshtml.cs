using AILocalHelper.AI;
using AILocalHelper.DB;
using AILocalHelper.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AILocalHelper.Pages
{
    public class IndexModel : PageModel
    {
        private readonly LiteDBService _dBService;
        private readonly ChatService _chatService;
        private Settings _config;

        [BindProperty(SupportsGet = true)]
        public string PathToModel { get; set; }


        public IndexModel(
            LiteDBService dBService,
            ChatService chatService)
        {
            _dBService = dBService;
            _chatService = chatService;
            _config = _dBService.GetConfig();
        }

        public void OnGet()
        {
            PathToModel = _config.PathToModel;
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
    }
}
