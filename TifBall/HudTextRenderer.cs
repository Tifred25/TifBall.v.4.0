using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace TifBall;

internal sealed class HudTextRenderer
{
    private const int GlyphWidth = 5;
    private const int GlyphHeight = 7;
    private readonly Texture2D _pixel;

    public HudTextRenderer(GraphicsDevice graphicsDevice)
    {
        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
    }

    public void DrawText(SpriteBatch spriteBatch, string text, Vector2 position, float scale, Color color)
    {
        string normalized = Normalize(text);
        float x = position.X;
        foreach (char character in normalized)
        {
            DrawGlyph(spriteBatch, character, new Vector2(x, position.Y), scale, color);
            x += (GlyphWidth + 1) * scale;
        }
    }

    public int MeasureWidth(string text, float scale)
    {
        string normalized = Normalize(text);
        return normalized.Length == 0 ? 0 : (int)MathF.Ceiling((((normalized.Length * (GlyphWidth + 1)) - 1) * scale));
    }

    public int MeasureHeight(float scale)
    {
        return (int)MathF.Ceiling(GlyphHeight * scale);
    }

    public void Dispose()
    {
        _pixel.Dispose();
    }

    private void DrawGlyph(SpriteBatch spriteBatch, char character, Vector2 position, float scale, Color color)
    {
        string[] rows = Glyphs.TryGetValue(character, out string[]? glyphRows) ? glyphRows : Glyphs[' '];
        for (int row = 0; row < rows.Length; row++)
        {
            string rowPattern = rows[row];
            int column = 0;
            while (column < rowPattern.Length)
            {
                if (rowPattern[column] != '#')
                {
                    column++;
                    continue;
                }

                int runStart = column;
                while (column < rowPattern.Length && rowPattern[column] == '#')
                {
                    column++;
                }

                int runEnd = column;
                int x = (int)MathF.Round(position.X + (runStart * scale));
                int y = (int)MathF.Round(position.Y + (row * scale));
                int right = (int)MathF.Round(position.X + (runEnd * scale));
                int bottom = (int)MathF.Round(position.Y + ((row + 1) * scale));
                Rectangle destination = new(
                    x,
                    y,
                    Math.Max(1, right - x),
                    Math.Max(1, bottom - y));
                spriteBatch.Draw(_pixel, destination, color);
            }
        }
    }

    private static string Normalize(string text)
    {
        return text
            .ToUpperInvariant()
            .Replace("É", "E")
            .Replace("È", "E")
            .Replace("Ê", "E")
            .Replace("À", "A")
            .Replace("Ù", "U")
            .Replace("Ç", "C")
            .Replace("'", " ")
            .Replace("-", " ");
    }

    private static readonly Dictionary<char, string[]> Glyphs = new()
    {
        [' '] = Rows(".....",".....",".....",".....",".....",".....","....."),
        ['.'] = Rows(".....",".....",".....",".....",".....","..#..","..#.."),
        [':'] = Rows(".....","..#..",".....",".....","..#..",".....","....."),
        ['0'] = Rows(".###.","#...#","#..##","#.#.#","##..#","#...#",".###."),
        ['1'] = Rows("..#..",".##..","..#..","..#..","..#..","..#..",".###."),
        ['2'] = Rows(".###.","#...#","....#","...#.","..#..",".#...","#####"),
        ['3'] = Rows("####.","....#","...#.","..##.","....#","#...#",".###."),
        ['4'] = Rows("...#.","..##.",".#.#.","#..#.","#####","...#.","...#."),
        ['5'] = Rows("#####","#....","####.","....#","....#","#...#",".###."),
        ['6'] = Rows(".###.","#...#","#....","####.","#...#","#...#",".###."),
        ['7'] = Rows("#####","....#","...#.","..#..",".#...",".#...",".#..."),
        ['8'] = Rows(".###.","#...#","#...#",".###.","#...#","#...#",".###."),
        ['9'] = Rows(".###.","#...#","#...#",".####","....#","#...#",".###."),
        ['A'] = Rows(".###.","#...#","#...#","#####","#...#","#...#","#...#"),
        ['B'] = Rows("####.","#...#","#...#","####.","#...#","#...#","####."),
        ['C'] = Rows(".####","#....","#....","#....","#....","#....",".####"),
        ['D'] = Rows("####.","#...#","#...#","#...#","#...#","#...#","####."),
        ['E'] = Rows("#####","#....","#....","####.","#....","#....","#####"),
        ['F'] = Rows("#####","#....","#....","####.","#....","#....","#...."),
        ['G'] = Rows(".####","#....","#....","#.###","#...#","#...#",".###."),
        ['H'] = Rows("#...#","#...#","#...#","#####","#...#","#...#","#...#"),
        ['I'] = Rows("#####","..#..","..#..","..#..","..#..","..#..","#####"),
        ['J'] = Rows("#####","...#.","...#.","...#.","...#.","#..#.",".##.."),
        ['K'] = Rows("#...#","#..#.","#.#..","##...","#.#..","#..#.","#...#"),
        ['L'] = Rows("#....","#....","#....","#....","#....","#....","#####"),
        ['M'] = Rows("#...#","##.##","#.#.#","#.#.#","#...#","#...#","#...#"),
        ['N'] = Rows("#...#","##..#","##..#","#.#.#","#..##","#..##","#...#"),
        ['O'] = Rows(".###.","#...#","#...#","#...#","#...#","#...#",".###."),
        ['P'] = Rows("####.","#...#","#...#","####.","#....","#....","#...."),
        ['Q'] = Rows(".###.","#...#","#...#","#...#","#.#.#","#..#.",".##.#"),
        ['R'] = Rows("####.","#...#","#...#","####.","#.#..","#..#.","#...#"),
        ['S'] = Rows(".####","#....","#....",".###.","....#","....#","####."),
        ['T'] = Rows("#####","..#..","..#..","..#..","..#..","..#..","..#.."),
        ['U'] = Rows("#...#","#...#","#...#","#...#","#...#","#...#",".###."),
        ['V'] = Rows("#...#","#...#","#...#","#...#",".#.#.",".#.#.","..#.."),
        ['W'] = Rows("#...#","#...#","#...#","#.#.#","#.#.#","##.##","#...#"),
        ['X'] = Rows("#...#","#...#",".#.#.","..#..",".#.#.","#...#","#...#"),
        ['Y'] = Rows("#...#","#...#",".#.#.","..#..","..#..","..#..","..#.."),
        ['Z'] = Rows("#####","....#","...#.","..#..",".#...","#....","#####"),
    };

    private static string[] Rows(params string[] rows)
    {
        return rows;
    }
}
