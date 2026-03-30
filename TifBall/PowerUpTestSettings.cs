using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace TifBall;

internal sealed class PowerUpTestSettings
{
    public int? StartLevel { get; init; }
    public bool? SkipPresentation { get; init; }
    public bool? SkipInitialHighScores { get; init; }
    public bool? AutoStartBall { get; init; }
    public float? BallSpawnChance { get; init; }
    public float? ShotSpawnChance { get; init; }
    public Dictionary<string, float>? WeightMultipliers { get; init; }

    public static PowerUpTestSettings? TryLoad(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<PowerUpTestSettings>(
                File.ReadAllText(filePath),
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true
                });
        }
        catch
        {
            return null;
        }
    }
}
