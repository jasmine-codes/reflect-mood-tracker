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
    }
}