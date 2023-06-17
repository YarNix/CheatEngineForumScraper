using System;
using System.IO;
using System.Net.Http;
using HtmlAgilityPack;

namespace CheatEngineForumScraper
{
    internal static class Scraper
    {
        public static Data ScrapeProfiles(int startID, int endID, Data? appendData = null, bool continueScrape = false)
        {
            const string PROFILE_URL = "https://forum.cheatengine.org/profile.php?mode=viewprofile&u=";

            Data data = appendData ?? new();
            if (data.Profiles.Count > 0 && continueScrape)
                startID = data.Profiles[data.Profiles.Count - 1].Id + 1;

            void OnCancelRequested(object? sender, ConsoleCancelEventArgs e)
            {
                data.Running = false;
                e.Cancel = true;
            }

            Console.CancelKeyPress += OnCancelRequested;

            HtmlDocument document = new();
            HttpClient client = new();

            for (int i = startID; i <= endID; i++)
            {
                if (!data.Running) break;
                Stream? pageStream = null;
                try
                {
                    pageStream = client.GetStreamAsync(PROFILE_URL + i.ToString()).Result;
                    document.Load(pageStream);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    break;
                }
                finally
                {
                    pageStream?.Dispose();
                }

                var newProfile = Profile.FromHtml(document.DocumentNode);
                if (newProfile is null)
                {
                    data.ErrorIDs.Add(i);
                    data.LogLastFailure();
                }
                else
                {
                    newProfile.Id = i;
                    data.Profiles.Add(newProfile);
                    data.LogLastSuccess();
                }
            }

            Console.CancelKeyPress -= OnCancelRequested;

            return data;
        }
    }
}
