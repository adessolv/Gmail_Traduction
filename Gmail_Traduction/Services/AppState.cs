using System.Collections.Generic;

namespace Gmail_Traduction.Services
{
    public class AppState
    {
        public List<GmailApiService.EmailDetail> Emails { get; set; } = new List<GmailApiService.EmailDetail>();
    }
}