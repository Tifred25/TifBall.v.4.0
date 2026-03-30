using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Globalization;

namespace TifBall;

internal sealed class FloatingScoreRenderer : IDisposable
{
    private readonly HudTextRenderer _textRenderer;

    public FloatingScoreRenderer(GraphicsDevice graphicsDevice)
    {
        _textRenderer = new HudTextRenderer(graphicsDevice);
    }

    public void Draw(SpriteBatch spriteBatch, IReadOnlyList<FloatingScoreState> scores)
    {
        const float scale = 2.0f;
        foreach (FloatingScoreState score in scores)
        {
            string text = (score.Score / 1000).ToString(CultureInfo.InvariantCulture) + "K";
            _textRenderer.DrawText(spriteBatch, text, score.Position, scale, Color.White);
        }
    }

    public void Dispose()
    {
        _textRenderer.Dispose();
    }
}
