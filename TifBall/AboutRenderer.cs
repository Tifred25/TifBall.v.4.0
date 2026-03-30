using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TifBall;

internal sealed class AboutRenderer : IDisposable
{
    private const int PanelWidth = 328;
    private const int PanelHeight = 158;

    private readonly Texture2D _pixel;
    private readonly HudTextRenderer _textRenderer;

    public AboutRenderer(GraphicsDevice graphicsDevice)
    {
        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
        _textRenderer = new HudTextRenderer(graphicsDevice);
    }

    public void Draw(SpriteBatch spriteBatch, Rectangle gameArea, Texture2D ballTexture, string versionText)
    {
        Rectangle panel = new(
            gameArea.X + ((gameArea.Width - PanelWidth) / 2),
            gameArea.Y + ((gameArea.Height - PanelHeight) / 2),
            PanelWidth,
            PanelHeight);
        Rectangle contentPanel = new(panel.X, panel.Y, PanelWidth, 120);
        Rectangle buttonBounds = new(panel.Right - 96, panel.Bottom - 30, 75, 23);
        Rectangle ballBounds = new(panel.X + 18, panel.Y + 18, 96, 96);

        spriteBatch.Draw(_pixel, gameArea, Color.Black * 0.45f);
        spriteBatch.Draw(_pixel, panel, new Color(216, 216, 216));
        spriteBatch.Draw(_pixel, contentPanel, Color.Black);
        spriteBatch.Draw(_pixel, new Rectangle(panel.X, panel.Y, panel.Width, 1), Color.White);
        spriteBatch.Draw(_pixel, new Rectangle(panel.X, panel.Y, 1, panel.Height), Color.White);
        spriteBatch.Draw(_pixel, new Rectangle(panel.X, panel.Bottom - 1, panel.Width, 1), Color.DimGray);
        spriteBatch.Draw(_pixel, new Rectangle(panel.Right - 1, panel.Y, 1, panel.Height), Color.DimGray);
        spriteBatch.Draw(_pixel, buttonBounds, new Color(232, 232, 232));
        spriteBatch.Draw(_pixel, new Rectangle(buttonBounds.X, buttonBounds.Y, buttonBounds.Width, 1), Color.White);
        spriteBatch.Draw(_pixel, new Rectangle(buttonBounds.X, buttonBounds.Y, 1, buttonBounds.Height), Color.White);
        spriteBatch.Draw(_pixel, new Rectangle(buttonBounds.X, buttonBounds.Bottom - 1, buttonBounds.Width, 1), Color.DimGray);
        spriteBatch.Draw(_pixel, new Rectangle(buttonBounds.Right - 1, buttonBounds.Y, 1, buttonBounds.Height), Color.DimGray);

        spriteBatch.Draw(ballTexture, ballBounds, Color.White);

        _textRenderer.DrawText(spriteBatch, "TIFBALL", new Vector2(panel.X + 138, panel.Y + 18), 4f, Color.White);
        _textRenderer.DrawText(spriteBatch, "VERSION " + versionText, new Vector2(panel.X + 146, panel.Y + 72), 1.15f, Color.White);
        _textRenderer.DrawText(spriteBatch, "COPYRIGHT (C) 2007 TIFRED", new Vector2(panel.X + 146, panel.Y + 98), 1.15f, Color.White);
        _textRenderer.DrawText(spriteBatch, "OK", new Vector2(buttonBounds.X + 24, buttonBounds.Y + 6), 1.6f, Color.Black);
    }

    public void Dispose()
    {
        _textRenderer.Dispose();
        _pixel.Dispose();
    }
}
