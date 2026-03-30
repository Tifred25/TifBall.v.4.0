using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace TifBall;

internal sealed class LegacyHighScoreStore
{
    private const int ScoreCount = 10;
    private const string DefaultName = "TIFRED";
    private readonly string _scoreFilePath;
    private readonly List<HighScoreEntry> _entries = new();

    public LegacyHighScoreStore(string scoreFilePath)
    {
        _scoreFilePath = scoreFilePath;
        Load();
    }

    public IReadOnlyList<HighScoreEntry> Entries => _entries;

    public bool Qualifies(int score)
    {
        return score > _entries[^1].Score;
    }

    public void SaveScore(string name, int score)
    {
        HighScoreEntry entry = new(name, score);
        int index = ScoreCount - 1;
        while (index > 0 && entry.Score > _entries[index - 1].Score)
        {
            if (index < _entries.Count)
            {
                _entries[index] = _entries[index - 1];
            }

            index--;
        }

        _entries[index] = entry;
        Save();
    }

    private void Load()
    {
        try
        {
            using BinaryReader reader = new(File.Open(_scoreFilePath, FileMode.Open));
            _entries.Clear();
            for (int i = 0; i < ScoreCount; i++)
            {
                string name = reader.ReadString();
                int score = reader.ReadInt32();
                _entries.Add(new HighScoreEntry(name, score));
            }

            string expectedSignature = reader.ReadString();
            string computedSignature = ComputeSignature(_entries);
            if (!string.Equals(expectedSignature, computedSignature, StringComparison.Ordinal))
            {
                InitializeDefaults();
            }
        }
        catch
        {
            InitializeDefaults();
        }
    }

    private void InitializeDefaults()
    {
        _entries.Clear();
        for (int i = 0; i < ScoreCount; i++)
        {
            _entries.Add(new HighScoreEntry(DefaultName, 0));
        }

        Save();
    }

    private void Save()
    {
        string signature = ComputeSignature(_entries);
        using BinaryWriter writer = new(File.Open(_scoreFilePath, FileMode.Create));
        foreach (HighScoreEntry entry in _entries)
        {
            writer.Write(entry.Name);
            writer.Write(entry.Score);
        }

        writer.Write(signature);
    }

    private static string ComputeSignature(IReadOnlyList<HighScoreEntry> entries)
    {
        using MemoryStream stream = new();
        using BinaryWriter writer = new(stream, Encoding.UTF8, true);
        foreach (HighScoreEntry entry in entries)
        {
            writer.Write(entry.Name);
            writer.Write(entry.Score);
        }

        writer.Flush();
        stream.Position = 0;
        using MD5 md5 = MD5.Create();
        byte[] hash = md5.ComputeHash(stream);
        StringBuilder builder = new();
        foreach (byte value in hash)
        {
            builder.AppendFormat("{0:x2}", value);
        }

        return builder.ToString();
    }
}

internal readonly record struct HighScoreEntry(string Name, int Score);
