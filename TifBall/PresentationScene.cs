using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace TifBall;

internal sealed class PresentationScene : IDisposable
{
    private const string TifredText = "TIFRED";
    private const string PresenteText = "PRESENTE";
    private const string ClickText = "CLIC POUR CONTINUER";
    private const float PhysicsStepSeconds = 1f / 60f;
    private const float GravityPerStep = 0.4f;
    private const float Damping = 0.85f;
    private const float TifredScale = 6.0f;
    private const float PresenteScale = 4.2f;
    private const float Idle1Duration = 2f;
    private const float Idle2Duration = 2f;
    private const float Idle4Duration = 1f;
    private const float BallDuration = 10f;
    private const int MaxTifredY = 150;
    private const int MaxPresenteY = 300;

    private readonly HudTextRenderer _textRenderer;
    private readonly Texture2D _backgroundTexture;
    private readonly Texture2D _ballTexture;
    private readonly Texture2D _tifBallTexture;

    private PresentationMode _mode = PresentationMode.Initial;
    private float _modeElapsedSeconds;
    private float _ballPhysicsAccumulator;
    private int _tifredY = -200;
    private int _presenteY = 600;
    private int _tifBallX = 800;
    private float _ballX = 1007f;
    private float _ballY = 156f;
    private float _ballVelocityX = 6f;
    private float _ballVelocityY;

    public PresentationScene(GraphicsDevice graphicsDevice, Texture2D backgroundTexture, Texture2D ballTexture, Texture2D tifBallTexture)
    {
        _textRenderer = new HudTextRenderer(graphicsDevice);
        _backgroundTexture = backgroundTexture;
        _ballTexture = ballTexture;
        _tifBallTexture = tifBallTexture;
    }

    public bool CanContinue => _mode == PresentationMode.Finish;

    public void Reset()
    {
        _mode = PresentationMode.Initial;
        _modeElapsedSeconds = 0f;
        _ballPhysicsAccumulator = 0f;
        _tifredY = -200;
        _presenteY = 600;
        _tifBallX = 800;
        _ballX = 1007f;
        _ballY = 156f;
        _ballVelocityX = 6f;
        _ballVelocityY = 0f;
    }

    public void Update(float deltaSeconds, Action<string, float> playSound)
    {
        _modeElapsedSeconds += deltaSeconds;

        switch (_mode)
        {
            case PresentationMode.Initial:
                SwitchMode(PresentationMode.Idle1);
                break;

            case PresentationMode.Idle1:
                if (_modeElapsedSeconds >= Idle1Duration)
                {
                    SwitchMode(PresentationMode.Tifred);
                }
                break;

            case PresentationMode.Tifred:
                _tifredY = Math.Min(MaxTifredY, _tifredY + (int)MathF.Round(4f * 60f * deltaSeconds));
                if (_tifredY >= MaxTifredY)
                {
                    SwitchMode(PresentationMode.Presente);
                }
                break;

            case PresentationMode.Presente:
                _presenteY = Math.Max(MaxPresenteY, _presenteY - (int)MathF.Round(4f * 60f * deltaSeconds));
                if (_presenteY <= MaxPresenteY)
                {
                    SwitchMode(PresentationMode.Idle2);
                }
                break;

            case PresentationMode.Idle2:
                if (_modeElapsedSeconds >= Idle2Duration)
                {
                    SwitchMode(PresentationMode.TifBallArrives);
                }
                break;

            case PresentationMode.TifBallArrives:
                if (_tifBallX > 0)
                {
                    int offset = (int)MathF.Round(8f * 60f * deltaSeconds);
                    _tifBallX = Math.Max(0, _tifBallX - offset);
                    _ballX -= offset;
                }

                if (_tifBallX <= 0)
                {
                    _tifBallX = 0;
                    SwitchMode(PresentationMode.Idle4);
                }
                break;

            case PresentationMode.Idle4:
                if (_modeElapsedSeconds >= Idle4Duration)
                {
                    SwitchMode(PresentationMode.Ball);
                }
                break;

            case PresentationMode.Ball:
                UpdateBall(deltaSeconds, playSound);
                if (_modeElapsedSeconds >= BallDuration)
                {
                    SwitchMode(PresentationMode.Finish);
                }
                break;

            case PresentationMode.Finish:
                UpdateBall(deltaSeconds, playSound);
                break;
        }
    }

    public void Draw(SpriteBatch spriteBatch, Rectangle presentationArea)
    {
        spriteBatch.Draw(_backgroundTexture, presentationArea, Color.White);

        switch (_mode)
        {
            case PresentationMode.Tifred:
                DrawCentered(spriteBatch, TifredText, presentationArea.X + (presentationArea.Width / 2f), presentationArea.Y + _tifredY, TifredScale);
                break;

            case PresentationMode.Presente:
            case PresentationMode.Idle2:
                DrawCentered(spriteBatch, TifredText, presentationArea.X + (presentationArea.Width / 2f), presentationArea.Y + _tifredY, TifredScale);
                DrawCentered(spriteBatch, PresenteText, presentationArea.X + (presentationArea.Width / 2f), presentationArea.Y + _presenteY, PresenteScale);
                break;

            case PresentationMode.TifBallArrives:
                DrawTifBall(spriteBatch, presentationArea, _tifBallTexture, _tifBallX);
                break;

            case PresentationMode.Idle4:
            case PresentationMode.Ball:
                DrawTifBall(spriteBatch, presentationArea, _tifBallTexture, _tifBallX);
                DrawBall(spriteBatch, presentationArea);
                break;

            case PresentationMode.Finish:
                DrawTifBall(spriteBatch, presentationArea, _tifBallTexture, _tifBallX);
                DrawBall(spriteBatch, presentationArea);
                DrawCentered(spriteBatch, ClickText, presentationArea.X + (presentationArea.Width / 2f), presentationArea.Y + 420, 2.1f);
                break;
        }
    }

    public void Dispose()
    {
        _textRenderer.Dispose();
        _backgroundTexture.Dispose();
        _ballTexture.Dispose();
        _tifBallTexture.Dispose();
    }

    private void UpdateBall(float deltaSeconds, Action<string, float> playSound)
    {
        _ballPhysicsAccumulator += deltaSeconds;
        while (_ballPhysicsAccumulator >= PhysicsStepSeconds)
        {
            _ballPhysicsAccumulator -= PhysicsStepSeconds;

            _ballX += _ballVelocityX;
            _ballY += _ballVelocityY;
            _ballVelocityY += GravityPerStep;

            if (_ballY > 600 - 40 && _ballVelocityY > 0f)
            {
                _ballY = 600 - 40;
                _ballVelocityY = -_ballVelocityY * Damping;
                playSound("Boing", 0.7f);
            }

            if (_ballX > 800 - 40 || _ballX < 0)
            {
                _ballX = Math.Clamp(_ballX, 0, 800 - 40);
                _ballVelocityX = -_ballVelocityX;
            }
        }
    }

    private void SwitchMode(PresentationMode mode)
    {
        _mode = mode;
        _modeElapsedSeconds = 0f;
    }

    private void DrawCentered(SpriteBatch spriteBatch, string text, float centerX, float y, float scale)
    {
        float x = centerX - (_textRenderer.MeasureWidth(text, scale) / 2f);
        _textRenderer.DrawText(spriteBatch, text, new Vector2(x, y), scale, Color.White);
    }

    private static void DrawTifBall(SpriteBatch spriteBatch, Rectangle presentationArea, Texture2D texture, int xOffset)
    {
        spriteBatch.Draw(texture, new Vector2(presentationArea.X + xOffset, presentationArea.Y), Color.White);
    }

    private void DrawBall(SpriteBatch spriteBatch, Rectangle presentationArea)
    {
        Rectangle target = new((int)MathF.Round(presentationArea.X + _ballX), (int)MathF.Round(presentationArea.Y + _ballY), 40, 40);
        spriteBatch.Draw(_ballTexture, target, Color.White);
    }

    private enum PresentationMode
    {
        Initial,
        Idle1,
        Tifred,
        Presente,
        Idle2,
        TifBallArrives,
        Idle4,
        Ball,
        Finish
    }
}
