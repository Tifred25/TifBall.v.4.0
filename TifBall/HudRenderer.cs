using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Globalization;

namespace TifBall;

internal sealed class HudRenderer : IDisposable
{
    private const int Margin = 18;
    private const int TopY = 64 + 8;
    private const float HeaderScale = 2.6f;
    private const float GameOverScale = 4.6f;
    private const float OverlayScale = 3.6f;
    private const float StartPromptScale = 2.8f;
    private const float TransitionScale = 8.2f;

    private readonly HudTextRenderer _textRenderer;

    public HudRenderer(GraphicsDevice graphicsDevice)
    {
        _textRenderer = new HudTextRenderer(graphicsDevice);
    }

    public void Draw(SpriteBatch spriteBatch, HudState state)
    {
        string scoreText = "SCORE : " + state.Score.ToString(CultureInfo.InvariantCulture);
        string levelText = "LEVEL : " + state.Level.ToString(CultureInfo.InvariantCulture);
        string livesText = "VIES : " + state.Lives.ToString(CultureInfo.InvariantCulture);

        _textRenderer.DrawText(spriteBatch, scoreText, new Vector2(96 + Margin, TopY), HeaderScale, Color.White);
        _textRenderer.DrawText(
            spriteBatch,
            levelText,
            new Vector2(96 + ((800 - _textRenderer.MeasureWidth(levelText, HeaderScale)) / 2f), TopY),
            HeaderScale,
            Color.White);
        _textRenderer.DrawText(
            spriteBatch,
            livesText,
            new Vector2(96 + 800 - Margin - _textRenderer.MeasureWidth(livesText, HeaderScale), TopY),
            HeaderScale,
            Color.White);

        string? centerMessage = state.BuildCenterMessage();
        if (!string.IsNullOrEmpty(centerMessage))
        {
            float messageScale = state.IsGameOver
                ? GameOverScale
                : state.IsAwaitingStart
                    ? StartPromptScale
                    : OverlayScale;
            float centerX = 96 + ((800 - _textRenderer.MeasureWidth(centerMessage, messageScale)) / 2f);
            float centerY = state.IsGameOver
                ? 64 + ((2f * 600) / 3f)
                : state.IsAwaitingStart
                    ? 64 + ((3f * 600) / 4f)
                    : 64 + ((2f * 600) / 3f);
            _textRenderer.DrawText(spriteBatch, centerMessage, new Vector2(centerX, centerY), messageScale, Color.White);
        }
    }

    public void DrawLevelTransition(SpriteBatch spriteBatch, int level, float y)
    {
        string transitionText = "LEVEL " + level.ToString(CultureInfo.InvariantCulture);
        float x = 96 + ((800 - _textRenderer.MeasureWidth(transitionText, TransitionScale)) / 2f);
        float drawY = 64 + y;
        _textRenderer.DrawText(spriteBatch, transitionText, new Vector2(x, drawY), TransitionScale, Color.White);
    }

    public void Dispose()
    {
        _textRenderer.Dispose();
    }
}
