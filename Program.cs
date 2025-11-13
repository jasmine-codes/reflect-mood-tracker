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
                case "6": DeleteEntry(); brea;
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

    #endregion
}