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
            var json = JsonSerializer.Serialize(entries, new JsonSerializerOptions { WriteIndented = true });
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
        string? dateInput = Console.ReadLine();

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
            string? moodInput = Console.ReadLine();

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
        e.Notes = Console.ReadLine() ?? "";

        //Tags
        Console.Write("Tags (comma separated, optional): ");
        string? tags = Console.ReadLine();

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
        string? s = Console.ReadLine();

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

        if (string.IsNullOrWhiteSpace(tag))
        {
            Console.WriteLine("No tag entered.");
            Console.WriteLine();
            Console.ReadLine();
            return;
        }

        var list = entries.Where(e => e.Tags.Any(t => string.Equals(t, tag, StringComparison.OrdinalIgnoreCase))).OrderByDescending(e => e.Date).ToList();
        PrintEntriesList(list);
        Console.WriteLine();
        Console.ReadLine();
    }

    static void EditEntry()
    {
        if (!entries.Any())
        {
            Console.WriteLine("No entries to edit.");
            Console.WriteLine();
            Console.ReadLine();
            return;
        }

        ViewAllEntries();

        Console.Write("Enter the index number of the entry to edit: ");
        string? s = Console.ReadLine();
        if (!int.TryParse(s, out int index) || index <= 0)
        {
            Console.WriteLine("Invalid index.");
            Console.WriteLine();
            Console.ReadLine();
            return;
        }

        //printed entries sorted, pick the same sorted list
        var sorted = entries.OrderByDescending(x => x.Date).ThenByDescending(x => x.Id).ToList();

        if (index > sorted.Count)
        {
            Console.WriteLine("Index out of range.");
            Console.WriteLine();
            Console.ReadLine();
            return;
        }

        var entry = sorted[index - 1];

        //edit date
        Console.WriteLine($"Editing entry: {entry.Title} ({entry.Date:yyyy-MM-dd})");
        Console.Write($"New date [current {entry.Date:yyyy-MM-dd}] (Enter to keep): ");
        string? dateInput = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(dateInput) && DateTime.TryParse(dateInput, out DateTime newDate)) entry.Date = newDate.Date;

        //edit mood
        Console.Write($"New mood [current {entry.Mood}] (1-10): ");
        string? moodInput = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(moodInput) && int.TryParse(moodInput, out int newMood) && newMood >= 1 && newMood <= 10) entry.Mood = newMood;

        //edit title
        Console.Write($"New title [current '{entry.Title}'] Enter to keep: ");
        string? titleInput = Console.ReadLine();
         if (!string.IsNullOrWhiteSpace(titleInput)) entry.Title = titleInput;

         //edit notes
         Console.Write("New notes (Enter to keep): ");
         string? notesInput = Console.ReadLine();
         if (!string.IsNullOrWhiteSpace(notesInput)) entry.Notes = notesInput;

         //edit tags
         Console.Write($"New tags (comma separated) [current {string.Join(", ", entry.Tags)}] (Enter to keep): ");
         string? tagsInput = Console.ReadLine();
         if (!string.IsNullOrWhiteSpace(tagsInput)) entry.Tags = tagsInput.Split(',').Select(t => t.Trim()).Where(t => t.Length > 0).ToList();

         //entry is reference to object in entries list (we are using sorted list) - ensure update by finding original by Id
         var originalIndex = entries.FindIndex(e => e.Id == entry.Id);
         if (originalIndex >= 0) entries[originalIndex] = entry;

         SaveEntries();
         Console.WriteLine("Entry updated.");
         Console.WriteLine();
         Console.ReadLine();
    }

    static void DeleteEntry()
    {
        if (!entries.Any())
        {
            Console.WriteLine("No entries to delete.");
            Console.WriteLine();
            Console.ReadLine();
            return;
        }

        ViewAllEntries();
        Console.Write("Enter the index number of the entry to delete: ");
        string? input = Console.ReadLine();

        if (!int.TryParse(input, out int idx) || idx <= 0)
        {
            Console.WriteLine("Invalid index.");
            Console.WriteLine();
            Console.ReadLine();
            return;
        }

        var sorted = entries.OrderByDescending(x => x.Date).ThenByDescending(x => x.Id).ToList();
        if (idx > sorted.Count)
        {
            Console.WriteLine("Index out of range.");
            Console.WriteLine();
            Console.ReadLine();
            return;
        }

        var entry = sorted[idx - 1];

        Console.Write($"Are you sure you want to delete '{entry.Title}' ({entry.Date:yyy-MM-dd})? (y/n): ");
        string? confirm = Console.ReadLine().ToLower();

        if (confirm == "y")
        {
            entries.RemoveAll(e => e.Id == entry.Id);
            SaveEntries();
            Console.WriteLine("Entry deleted.");
            Console.WriteLine();
            Console.ReadLine();
        }
        else
        {
            Console.WriteLine("Delete cancelled.");
            Console.WriteLine();
            Console.ReadLine();
        }
    }

    static void ExportEntries()
    {
        Console.Write("Export filename (e.g. my_export.txt): ");
        string? fn = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(fn))
        {
            Console.WriteLine("Invalid filename.");
            Console.WriteLine();
            Console.ReadLine();
            return;
        }

        try
        {
            using var sw = new StreamWriter(fn);
            var sorted = entries.OrderByDescending(e => e.Date).ToList();
            foreach (var e in sorted)
            {
                sw.WriteLine($"Date: {e.Date:yyyy-MM-dd}");
                sw.WriteLine($"Mood: {e.Mood}");
                sw.WriteLine($"Title: {e.Title}");
                sw.WriteLine($"Tags: {string.Join(", ", e.Tags)}");
                sw.WriteLine("Notes:");
                sw.WriteLine(e.Notes);
                sw.WriteLine(new string('-', 40) );
            }
            Console.WriteLine($"Exported {sorted.Count} entries to {fn}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exported failed: {ex.Message}");
        }
    }

    #endregion

    #region Helpers & Stats

    static void PrintEntriesList(List<JournalEntry> list)
    {
        if (!list.Any())
        {
            Console.WriteLine("No entries found.");
            Console.WriteLine();
            Console.ReadLine();
            return;
        }

        for (int i = 0; i < list.Count; i++)
        {
            var e = list[i];
            Console.WriteLine($"{i + 1}) [{e.Date:yyyy-MM-dd}] Mood: {e.Mood} - {e.Title} (Tags: {string.Join(", ", e.Tags)})");

            if (!string.IsNullOrWhiteSpace(e.Notes)) Console.WriteLine($"   {TrimTo(e.Notes, 150)}");
        }
    }

    static string TrimTo(string s, int len)
    {
        if (string.IsNullOrEmpty(s)) return "";
        if (s.Length <= len) return s;
        return s.Substring(0, len - 3) + "...";
    }

    static void ShowStats()
    {
        if (!entries.Any())
        {
            Console.WriteLine("No data for stats.");
            Console.WriteLine("");
            Console.ReadLine();
            return;
        }

        Console.Write("Enter date range (YYYY-MM-DD to YYYY-MM-DD) or press Enter for last 30 days: ");
        string? r = Console.ReadLine();

        DateTime start, end;
        if (string.IsNullOrWhiteSpace(r))
        {
            end = DateTime.Today;
            start = end.AddDays(-29); //last 30 days
        }
        else if (r.Contains("to"))
        {
            var parts = r.Split("to", StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
        }
    }

    #endregion
}