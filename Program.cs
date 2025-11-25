using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;

public class JournalEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Date { get; set; } = DateTime.Today;
    public int Mood { get; set; }
    public string Title { get; set; }
    public string Notes { get; set; }
    public List<string> Tags { get; set; } = new List<string>();
}

class Program
{
    static string filePath = "entries.json";
    static List<JournalEntry> entries = new List<JournalEntry>();
    static string? input;

    static void Main()
    {
        Console.Clear();
        entries = LoadEntries();
        Console.WriteLine("Welcome to Reflect - Mood Journal");
        Console.WriteLine();

        while (true)
        {
            Console.WriteLine("Main Menu:");
            Console.WriteLine("1) Add Entry");
            Console.WriteLine("2)View Entries");
            Console.WriteLine("3) View Entries - by Date");
            Console.WriteLine("4)View Entries - by Tag");
            Console.WriteLine("5) Edit Entry");
            Console.WriteLine("6) Delete Entry");
            Console.WriteLine("7) Statistics");
            Console.WriteLine("8) Export Entries (text)");
            Console.WriteLine("9) Reset Storage (delete all)");
            Console.WriteLine("0) Exit");
            Console.Write("Choose an option: ");
            input = Console.ReadLine();

            Console.WriteLine();

            switch (input)
            {
                case "1": AddEntry(); break;
                case "2": ViewAllEntries(); break;
                case "3": ViewByDate(); break;
                case "4": ViewByTag(); break;
                case "5": EditEntry(); break;
                case "6": DeleteEntry(); break;
                case "7": ShowStats(); break;
                case "8": ExportEntries(); break;
                case "9": ResetStorage(); break;
                case "0": SaveEntries(); Console.WriteLine("Saved. Bye!"); return;
                default: Console.WriteLine("Invalid choice."); break;
            }

            Console.WriteLine("");
        }
    }

    #region Persistence

    static List<JournalEntry> LoadEntries()
    {
        try
        {
            if (!File.Exists(filePath)) return new List<JournalEntry>();
            string json = File.ReadAllText(filePath);
            var list = JsonSerializer.Deserialize<List<JournalEntry>>(json);
            return list ?? new List<JournalEntry>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading entries: {ex.Message}");
            return new List<JournalEntry>();
        }
    }

    static void SaveEntries()
    {
        try
        {
            var json = JsonSerializer.Serialize(entries, new JsonSerializerOptions { writeIndented = true });
            File.WriteAllText(filePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving entries: {ex.Message}");
        }
    }


    static void ResetStorage()
    {
        Console.Write("Are you sure you want to delete all entries? (y/n): ");
        var confirm = Console.ReadLine()?.ToLower();

        if (confirm == "y")
        {
            if (File.Exists(filePath)) File.Delete(filePath);
            entries.Clear();
            Console.WriteLine("All entries deleted.");
        }
        else
        {
            Console.WriteLine("Reset cancelled.");
        }
    }

    #endregion

    #region CRUD and View

    static void AddEntry()
    {
        var e = new JournalEntry();

        //Date - optional
        Console.Write($"Date YYYY-MM-DD [default {DateTime.Today:YYYY-MM-DD}]: ");
        var dateInput = Console.ReadLine() ?;

        if (!string.IsNullOrWhiteSpace(dateInput))
        {
            if (DateTime.TryParse(dateInput, out DateTime parsedDate))
            {
                e.Date = parsedDate;
            }
            else
            {
                Console.WriteLine("Invalid date - using today");
            }
        }

        //Mood
        while (true)
        {
            Console.Write("Mood (1-10): ");
            var moodInput = Console.ReadLine() ?;

            if (int.TryParse(moodInput, out int mood) && mood >= 1 && mood <= 10)
            {
                e.Mood = mood;
                break;
            }

            Console.WriteLine("Please enter a number from 1 to 10.");
        }

        //Title
        Console.Write("Title (short): ");
        e.Title = Console.ReadLine() ?? "";

        //Notes
        Console.Write("Notes (you can hit Enter to skip): ");
        e.Notes = Console.Readline() ?? "";

        //Tags
        Console.Write("Tags (comma separated, optional): ");
        var tags = Console.ReadLine() ?;

        if (!string.IsNullOrWhiteSpace(tags)) e.Tags = tags.Split(',').Select(t => t.Trim()).Where(t => t.Length > 0).ToList();

        entries.Add(e);
        SaveEntries();
        Console.WriteLine("Entry added!");
        Console.WriteLine("Press Enter to continue");
        Console.ReadLine();
    }

    static void ViewAllEntries()
    {
        if (!entries.Any())
        {
            Console.WriteLine("No entries yet.");
            Console.WriteLine();
            Console.ReadLine();
            return;
        }

        var sorted = entries.OrderByDescending(x => x.Date).ThenByDescending(x => x.Id).ToList();
        PrintEntriesList(sorted);
    }

    static void ViewByDate()
    {
        Console.Write("Enter date (YYYY-MM-DD) or range (YYYY-MM-DD to YYYY-MM-DD): ");
        var s = Console.ReadLine()?;

        if (string.IsNullOrWhiteSpace(s))
        {
            Console.WriteLine("Nothing entered.");
            Console.WriteLine();
            Console.ReadLine();
            return;
        }

        if (s.Contains("to"))
        {
            var parts = s.Split("to", StringSplitOptions.RemoveEmptyEntries).Select(parts => parts.Trim()).ToArray();

            if (parts.Length == 2 &&
            DateTime.TryParse(parts[0], out DateTime d1) &&
            DateTime.TryParse(parts[1], out DateTime d2))
            {
                var list = entries.Where(en => en.Date.Date >= d1.Date && en.Date.Date <= d2.Date).OrderByDescending(e => e.Date).ToList();
                PrintEntriesList(list);
                return;
            }

            Console.WriteLine("Invalid range format.");
            Console.WriteLine();
            Console.ReadLine();
            return;
        }
        else
        {
            if (DateTime.TryParse(s, out DateTime d))
            {
                var list = entries.Where(en => en.Date.Date == d.Date).OrderByDescending(e => e.Date).ToList();
                PrintEntriesList(list);
                Console.WriteLine();
                Console.ReadLine();
                return;
            }

            Console.WriteLine("Invalid date format.");
            Console.WriteLine();
            Console.ReadLine();
            return;
        }
    }

    static void ViewByTag()
    {
        Console.Write("Enter tag to search: ");
        var tag = Console.ReadLine()?.Trim();
    }

    #endregion
}