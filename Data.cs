using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CheatEngineForumScraper
{
    public class Data
    {
        // Storing deleted profile is useless but it's good when you need to total the profiles scraped
        [JsonPropertyName("errors")]
        public List<int> ErrorIDs { get; set; } = new();

        [JsonPropertyName("profiles")]
        public List<Profile> Profiles { get; set; } = new();

        [JsonIgnore]
        public bool Running { get; set; } = true;

        public void LogLastSuccess()
        {
            int count = Profiles.Count;
            if (count == 0) return;
            var last = Profiles[count - 1];
            last.Log();
        }
        public void LogLastFailure()
        {
            int count = ErrorIDs.Count;
            if (count == 0) return;
            var lastID = ErrorIDs[count - 1];
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{lastID}:[ERROR]");
            Console.ResetColor();
        }
    }
}
