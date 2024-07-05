using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.Text.RegularExpressions;
using System.Text;

namespace Gmail_Traduction.Services
{
    public class GmailApiService
    {
        private static string[] Scopes = { GmailService.Scope.GmailReadonly };
        private static string ApplicationName = "Blazor Gmail API App";
        private readonly GmailService gmailService;

        public class EmailDetail
        {
            public string Id { get; set; }
            public string ThreadId { get; set; }
            public string Sender { get; set; }
            public string Subject { get; set; }
            public string Body { get; set; }
            public string Service { get; set; }
            public string Deadline { get; set; }
            public decimal TotalWords { get; set; }
            public decimal StandardWords { get; set; }
        }

        public GmailApiService(IConfiguration configuration)
        {
            UserCredential credential;
            using (var stream = new FileStream("wwwroot/client_secret.json", FileMode.Open, FileAccess.Read))
            {
                var secrets = GoogleClientSecrets.FromStream(stream).Secrets;

                var codeReceiver = new LocalServerCodeReceiver();

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore("token.json", true),
                    codeReceiver
                ).Result;
            }

            gmailService = new GmailService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });
        }

        public async Task<List<EmailDetail>> GetEmailsFromDomain(string domain, string[] keywords, int maxResults)
        {
            string query = $"from:{domain} -from:noreply@{domain}";
            if (keywords != null && keywords.Length > 0)
            {
                query += " (";
                for (int i = 0; i < keywords.Length; i++)
                {
                    query += $"subject:\"{keywords[i]}\"";
                    if (i < keywords.Length - 1)
                    {
                        query += " OR ";
                    }
                }
                query += ")";
            }

            var request = gmailService.Users.Messages.List("me");
            request.Q = query;
            request.MaxResults = maxResults;

            var response = await request.ExecuteAsync();
            var emails = new List<EmailDetail>();
            var threadIds = new HashSet<string>();

            if (response.Messages != null)
            {
                foreach (var msg in response.Messages)
                {
                    if (!threadIds.Contains(msg.ThreadId))
                    {
                        var emailRequest = gmailService.Users.Messages.Get("me", msg.Id);
                        var email = await emailRequest.ExecuteAsync();

                        var headers = email.Payload.Headers;
                        var sender = headers.FirstOrDefault(h => h.Name == "From")?.Value;
                        var subject = headers.FirstOrDefault(h => h.Name == "Subject")?.Value;

                        emails.Add(new EmailDetail
                        {
                            ThreadId = email.ThreadId,
                            Id = email.Id,
                            Sender = ExtractSenderName(sender),
                            Subject = subject
                        });

                        threadIds.Add(msg.ThreadId);

                        if (emails.Count >= maxResults)
                        {
                            break;
                        }
                    }
                }
            }
            return emails;
        }

        private string ExtractSenderName(string sender)
        {
            if (string.IsNullOrEmpty(sender))
                return string.Empty;
            sender = sender.Replace("\\\"", "").Replace("\"", "").Trim();
            var match = Regex.Match(sender, @"^(.*?)(?=<)");
            if (match.Success)
            {
                return match.Groups[1].Value.Trim();
            }
            // If no name part, return the full sender string (email address in this case)
            return sender;
        }

        public async Task<EmailDetail> GetEmailById(string id)
        {
            var emailRequest = gmailService.Users.Messages.Get("me", id);
            var email = await emailRequest.ExecuteAsync();
            var headers = email.Payload.Headers;
            var sender = headers.FirstOrDefault(h => h.Name == "From")?.Value;
            var subject = headers.FirstOrDefault(h => h.Name == "Subject")?.Value;

            var body = GetBodyFromMessagePart(email.Payload);

            var emailDetail = new EmailDetail
            {
                Id = email.Id,
                ThreadId = email.ThreadId,
                Sender = ExtractSenderName(sender),
                Subject = subject,
                Body = body
            };

            ExtractEmailDetails(emailDetail);

            return emailDetail;
        }

        private string GetBodyFromMessagePart(MessagePart part)
        {
            if (part == null) return null;

            if (part.MimeType == "text/plain" || part.MimeType == "text/html")
            {
                return Encoding.UTF8.GetString(Convert.FromBase64String(part.Body.Data.Replace("-", "+").Replace("_", "/")));
            }

            if (part.Parts != null && part.Parts.Count > 0)
            {
                foreach (var subPart in part.Parts)
                {
                    var result = GetBodyFromMessagePart(subPart);
                    if (!string.IsNullOrEmpty(result))
                    {
                        return result;
                    }
                }
            }

            return null;
        }

        private void ExtractEmailDetails(EmailDetail email)
        {
            if (string.IsNullOrEmpty(email.Subject) && string.IsNullOrEmpty(email.Body))
                return;

            var serviceMatch = Regex.Match(email.Subject, @"^(Translation|Revision|MT Post Edit)");
            if (serviceMatch.Success)
            {
                email.Service = serviceMatch.Value;
            }

            var deadlineMatch = Regex.Match(email.Subject, @"(Monday|Tuesday|Wednesday|Thursday|Friday|Saturday|Sunday)\s\d{2}/\d{2}\s\d{4}\s@\d{2}:\d{2}(AM|PM)");
            if (deadlineMatch.Success)
            {
                email.Deadline = deadlineMatch.Value;
            }

            var totalWordsMatch = Regex.Match(email.Body ?? "", @"Volume:\s[^\d]*([\d,]+)\swords?", RegexOptions.IgnoreCase);
            if (totalWordsMatch.Success)
            {
                var totalWordsStr = totalWordsMatch.Groups[1].Value.Replace(",", "");
                email.TotalWords = decimal.Parse(totalWordsStr);
            }

            var standardWordsMatch = Regex.Match(email.Body, @"\(([\d,]+)\sWWC\)", RegexOptions.IgnoreCase);
            if (standardWordsMatch.Success)
            {
                var standardWordsStr = standardWordsMatch.Groups[1].Value.Replace(",", "");
                email.StandardWords = decimal.Parse(standardWordsStr);
            }
        }
    }
}