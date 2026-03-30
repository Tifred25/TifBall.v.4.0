using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace TifBall;

internal sealed class HighScoreRenderer : IDisposable
{
    private const int PanelX = 96;
    private const int PanelY = 64;
    private const int PanelWidth = 800;
    private const int PanelHeight = 600;
    private const float TitleScale = 5.1f;
    private const float IndexScale = 3.9f;
    private const float NameScale = 3.8f;
    private const float ScoreScale = 3.9f;

    private readonly HudTextRenderer _textRenderer;
    private readonly Texture2D _background;

    public HighScoreRenderer(GraphicsDevice graphicsDevice)
    {
        _textRenderer = new HudTextRenderer(graphicsDevice);
        _background = EmbeddedAssetLoader.LoadTexture(graphicsDevice, "Assets/Images/Background2.jpg");
    }

    public void Draw(SpriteBatch spriteBatch, IReadOnlyList<HighScoreEntry> entries)
    {
        spriteBatch.Draw(_background, new Rectangle(PanelX, PanelY, PanelWidth, PanelHeight), Color.White);
        DrawCentered(spriteBatch, "MEILLEURS SCORES", PanelX + (PanelWidth / 2f), PanelY + 16, TitleScale, Color.White);

        for (int i = 0; i < entries.Count; i++)
        {
            HighScoreEntry entry = entries[i];
            float rowBaseY = PanelY + 102 + (i * 48);
            Color color = GetRowColor(i);

            DrawRightAligned(
                spriteBatch,
                (i + 1).ToString(CultureInfo.InvariantCulture) + ".",
                PanelX + 90,
                rowBaseY,
                IndexScale,
                color);

            DrawCenteredInWidth(
                spriteBatch,
                entry.Name,
                PanelX + 110,
                PanelY + 104 + (i * 48),
                350,
                NameScale,
                color);

            _textRenderer.DrawText(spriteBatch, ":", new Vector2(PanelX + 490, rowBaseY), IndexScale, color);

            DrawRightAligned(
                spriteBatch,
                entry.Score.ToString(CultureInfo.InvariantCulture),
                PanelX + 760,
                rowBaseY,
                ScoreScale,
                color);
        }
    }

    public void Dispose()
    {
        _textRenderer.Dispose();
        _background.Dispose();
    }

    private void DrawCentered(SpriteBatch spriteBatch, string text, float centerX, float y, float scale, Color color)
    {
        float x = centerX - (_textRenderer.MeasureWidth(text, scale) / 2f);
        _textRenderer.DrawText(spriteBatch, text, new Vector2(x, y), scale, color);
    }

    private void DrawCenteredInWidth(SpriteBatch spriteBatch, string text, float left, float y, float width, float scale, Color color)
    {
        float x = left + ((width - _textRenderer.MeasureWidth(text, scale)) / 2f);
        _textRenderer.DrawText(spriteBatch, text, new Vector2(x, y), scale, color);
    }

    private void DrawRightAligned(SpriteBatch spriteBatch, string text, float right, float y, float scale, Color color)
    {
        float x = right - _textRenderer.MeasureWidth(text, scale);
        _textRenderer.DrawText(spriteBatch, text, new Vector2(x, y), scale, color);
    }

    private static Color GetRowColor(int index)
    {
        return (index % 4) switch
        {
            0 => Color.LightBlue,
            1 => Color.LightGreen,
            2 => Color.LightSalmon,
            _ => Color.LightYellow
        };
    }
}
