using System;
using System.IO;
using System.Text.Json;

namespace CheatEngineForumScraper
{
    internal static class Program
    {
        const string SAVE_NAME = "userData.json";

        static void Main()
        {
            
        }

        public static void ScrapeAndStore(int start, int end)
        {
            string jsonData;
            if (File.Exists(SAVE_NAME))
            {
                jsonData = File.ReadAllText(SAVE_NAME);
                Data? oldData = JsonSerializer.Deserialize<Data>(jsonData);
                if (oldData is not null)
                {
                    Scraper.ScrapeProfiles(start, end, oldData, true);
                    jsonData = JsonSerializer.Serialize(oldData);
                    File.WriteAllText(SAVE_NAME, jsonData);
                    return;
                }
            }
            Data newData = Scraper.ScrapeProfiles(start, end);
            jsonData = JsonSerializer.Serialize(newData);
            File.WriteAllText(SAVE_NAME, jsonData);
        }
        public static void QueryStoredData()
        {
            string jsonData = File.ReadAllText(SAVE_NAME);
            Data data = JsonSerializer.Deserialize<Data>(jsonData) ?? throw new Exception();
            data.ErrorIDs.Clear(); // Don't need this anymore
            data.Profiles.RemoveAll(o => !Predicate(o));
            data.Profiles.Sort(Comparer);
            Console.WriteLine($"Found {data.Profiles.Count} result(s):");
            for (int i = 0; i < data.Profiles.Count; i++)
                data.Profiles[i].Log();
            Console.Write("Do you want to save this query (y/n): ");
            string? res = Console.ReadLine();
            if (res is not null && res.Contains('y'))
            {
                int index = 1;
                string fileName;
                do { fileName = $"userDataQuery{index++}.json"; } while (File.Exists(fileName));
                jsonData = JsonSerializer.Serialize(data);
                File.WriteAllText(fileName, jsonData);
            }
        }
        // Join date in the year 2019
        // Email contain the "gmail" domain
        // Posts > 0
        private static bool Predicate(Profile profile) => profile.DateJoined.Year == 2019 && profile.TotalPosts > 0
                                                          && (string.IsNullOrEmpty(profile.Email) || profile.Email.Contains("@gmail.com"));
        // Sort by HasEmail descending
        // Sort by Posts ascending
        // Sort by JoinDate ascending
        private static int Comparer(Profile l, Profile r)
        {
            bool lEmail = !string.IsNullOrEmpty(l.Email),
                 rEmail = !string.IsNullOrEmpty(r.Email);
            if (lEmail ^ rEmail)
                // Either one is false.
                return lEmail ? -1 : 1;
            // Both is false or both is true

            int cmpResult = l.TotalPosts.CompareTo(r.TotalPosts);
            if (cmpResult != 0) return cmpResult;
            // They are equals

            return l.DateJoined.CompareTo(r.DateJoined);
        }
    }
}