using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;

class Program
{
    static async System.Threading.Tasks.Task Main(string[] args)
    {
        string url = "https://gist.githubusercontent.com/christianpanton/10d65ccef9f29de3acd49d97ed423736/raw/b09563bc0c4b318132c7a738e679d4f984ef0048/kings";

        using HttpClient client = new HttpClient();
        string jsonData;
        try
        {
            jsonData = await client.GetStringAsync(url);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching data: {ex.Message}");
            return;
        }

        // Deserialize the JSON data into a list of monarchs
        var monarchs = JsonConvert.DeserializeObject<List<Monarch>>(jsonData);

        if (monarchs == null || !monarchs.Any())
        {
            Console.WriteLine("Failed to load monarchs data or dataset is empty.");
            return;
        }

        // 1. How many monarchs are in the dataset?
        Console.WriteLine($"1. Number of monarchs: {monarchs.Count}");

        // 2. Which monarch ruled the longest and for how many years?
        var monarchWithLongestRule = monarchs
            .Select(m => new { Monarch = m, Years = CalculateYears(m.yrs) })
            .OrderByDescending(m => m.Years)
            .FirstOrDefault();
        if (monarchWithLongestRule != null)
        {
            Console.WriteLine($"2. Monarch with the longest reign: {monarchWithLongestRule.Monarch.nm} ({monarchWithLongestRule.Years} years)");
        }

        // 3. Which house ruled the longest and for how many years?
        var houseWithLongestRule = monarchs
            .GroupBy(m => m.hse)
            .Select(g => new { House = g.Key, TotalYears = g.Sum(m => CalculateYears(m.yrs)) })
            .OrderByDescending(h => h.TotalYears)
            .FirstOrDefault();
        if (houseWithLongestRule != null)
        {
            Console.WriteLine($"3. House with the longest rule: {houseWithLongestRule.House} ({houseWithLongestRule.TotalYears} years)");
        }

        // 4. What is the most common first name in the dataset?
        var mostCommonFirstName = monarchs
            .Select(m => m.nm?.Split(' ')[0])
            .Where(name => !string.IsNullOrEmpty(name))
            .GroupBy(name => name)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault();
        if (mostCommonFirstName != null)
        {
            Console.WriteLine($"4. Most common first name: {mostCommonFirstName.Key} ({mostCommonFirstName.Count()} occurrences)");
        }

        // 5. What is the house of the current monarch and for how many years did that house rule throughout history?
        var currentMonarch = monarchs.LastOrDefault();
        if (currentMonarch != null && !string.IsNullOrEmpty(currentMonarch.hse))
        {
            var currentHouseTotalYears = monarchs
                .Where(m => m.hse == currentMonarch.hse)
                .Sum(m => CalculateYears(m.yrs));
            Console.WriteLine($"5. Current monarch's house: {currentMonarch.hse} ({currentHouseTotalYears} years in total)");
        }
        else
        {
            Console.WriteLine("5. Current monarch or house information is not available.");
        }
    }

    // Helper method to calculate years from the "yrs" field
    static int CalculateYears(string? yrs)
    {
        if (string.IsNullOrEmpty(yrs)) return 0;

        var years = yrs.Split('-');

        // Parse the start year
        if (!int.TryParse(years[0], out int startYear))
        {
            return 0;
        }

        // Parse the end year or use the current year if it's ongoing
        int endYear;
        if (years.Length > 1 && int.TryParse(years[1], out var parsedEndYear))
        {
            endYear = parsedEndYear;
        }
        else if (years.Length == 1)
        {
            endYear = startYear; // Single year reign
        }
        else
        {
            endYear = DateTime.Now.Year; // Current monarch
        }

        return endYear - startYear;
    }


    // Model class for a monarch
    class Monarch
    {
        public int id { get; set; }
        public string? nm { get; set; }
        public string? cty { get; set; }
        public string? hse { get; set; }
        public string? yrs { get; set; }
    }
}
