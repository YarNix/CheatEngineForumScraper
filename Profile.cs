using System;
using System.Text;
using System.Text.Json.Serialization;
using HtmlAgilityPack;

namespace CheatEngineForumScraper
{
    public class Profile
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("user_name")]
        public string UserName { get; set; } = string.Empty;
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;
        [JsonPropertyName("joined")]
        public DateOnly DateJoined { get; set; }
        [JsonPropertyName("posts")]
        public int TotalPosts { get; set; }

        static StringBuilder _defaultBuilder = new();
        private static string ClouldFlareDecodeEmail(string encodedString)
        {
            _defaultBuilder.Clear();
            if (string.IsNullOrWhiteSpace(encodedString)) return string.Empty;
            byte[] hexValue = Convert.FromHexString(encodedString);
            for (int i = 1; i < hexValue.Length; i++)
                _defaultBuilder.Append((char)(hexValue[i] ^ hexValue[0]));
            return _defaultBuilder.ToString();
        }

        public static Profile? FromHtml(HtmlNode root)
        {
            const string XFORUM_LINE = "/html/body/table/tr/td/table[3]";
            const string USER_NAME = "tr[2]/td[2]/b/span"; // "All about {USER_NAME}"
            const string JOIN_DATE = "tr[3]/td[2]/table/tr[1]/td[2]/b/span";
            const string POST_COUNT = "tr[3]/td[2]/table/tr[2]/td[2]/b/span";
            const string EMAIL_URI = "tr[5]/td/table/tr[1]/td[2]/b/span/a";

            const string USER_NAME_PREFIX = "All about ";

            var infoTable = root.SelectSingleNode(XFORUM_LINE);
            if (infoTable is null) return null;

            HtmlNode? userNode = infoTable.SelectSingleNode(USER_NAME);
            if (userNode is null) return null;
            string username, email;
            if (userNode.ChildNodes.Count > 1)
            {
                username = string.Empty;

                // User signup using email as username
                HtmlNode? emailNode = userNode.ChildNodes.FindFirst("a");
                if (emailNode is not null)
                {
                    string emailEncrypted = emailNode.GetAttributeValue("data-cfemail", string.Empty);
                    email = ClouldFlareDecodeEmail(emailEncrypted);
                }
                else
                {
                    email = string.Empty;
                }
            }
            else
            {
                username = userNode.InnerText.Substring(USER_NAME_PREFIX.Length) ?? string.Empty;

                var emailNode = infoTable.SelectSingleNode(EMAIL_URI);
                if (emailNode is not null)
                {
                    email = emailNode.GetAttributeValue("href", string.Empty);
                    if (string.IsNullOrEmpty(email)) return null;
                    email = ClouldFlareDecodeEmail(email.Substring(email.IndexOf('#') + 1));
                }
                else
                {
                    email = string.Empty;
                }
            }

            bool parseSuccess = DateOnly.TryParse(infoTable.SelectSingleNode(JOIN_DATE)?.InnerText, out DateOnly joinDate);
            if (!parseSuccess) return null;

            parseSuccess = int.TryParse(infoTable.SelectSingleNode(POST_COUNT)?.InnerText, out int postCount);
            if (!parseSuccess) return null;

            return new Profile()
            {
                UserName = username,
                DateJoined = joinDate,
                TotalPosts = postCount,
                Email = email,
            };
        }
        public void Log()
        {
            Console.Write($"{Id}:[");
            WriteToConsole(UserName, ConsoleColor.Blue);
            WriteToConsole(Email, ConsoleColor.Yellow);
            WriteToConsole(DateJoined.ToString("MM-yyyy"), ConsoleColor.Cyan);
            WriteToConsole(TotalPosts.ToString(), ConsoleColor.Gray, "]\n");
            Console.ResetColor();
        }
        private static void WriteToConsole(string str, ConsoleColor fgColor, string delimiter = ", ")
        {
            if (string.IsNullOrEmpty(str)) return;
            Console.ForegroundColor = fgColor;
            Console.Write(str + delimiter);
        }
    }
}
