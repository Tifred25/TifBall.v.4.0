using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace TifBall;

internal sealed class ScoreEntryRenderer : IDisposable
{
    private const string AllowedCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    public const int PanelX = 96;
    public const int PanelY = 64;
    private const int PanelWidth = 800;
    private const int ScoreY = 44;
    private const int EnterNameY = 132;
    private const int PreviousX = 220;
    private const int PreviousY = 230;
    private const int PreviousWidth = 70;
    private const int PreviousHeight = 70;
    private const int CurrentX = 320;
    private const int CurrentY = 190;
    private const int CurrentWidth = 150;
    private const int CurrentHeight = 150;
    private const int NextX = 500;
    private const int NextY = 230;
    private const int NextWidth = 70;
    private const int NextHeight = 70;
    private const int NameY = 500;
    private const int LeftArrowX = 100;
    private const int LeftArrowY = 220;
    private const int RightArrowX = 590;
    private const int RightArrowY = 220;
    private const int ArrowWidth = 100;
    private const int ArrowHeight = 100;
    private const int OkX = 460;
    private const int OkY = 340;
    private const int EffX = 240;
    private const int EffY = 340;
    private const int ButtonWidth = 100;
    private const int ButtonHeight = 100;
    private const float ScoreScale = 4.0f;
    private const float PromptScale = 2.8f;

    private readonly HudTextRenderer _textRenderer;
    private readonly Texture2D _background;
    private readonly Texture2D _leftArrow;
    private readonly Texture2D _rightArrow;
    private readonly Texture2D _ok;
    private readonly Texture2D _eff;
    private readonly Dictionary<char, Texture2D> _largeRedLetters = new();
    private readonly Dictionary<char, Texture2D> _smallRedLetters = new();
    private readonly Dictionary<char, Texture2D> _smallBlueLetters = new();

    public ScoreEntryRenderer(GraphicsDevice graphicsDevice)
    {
        _textRenderer = new HudTextRenderer(graphicsDevice);
        _background = EmbeddedAssetLoader.LoadTexture(graphicsDevice, "Assets/Images/Background2.jpg");
        _leftArrow = EmbeddedAssetLoader.LoadTexture(graphicsDevice, "Assets/Letters/ArrowLeft_r.png", transparentBlack: true);
        _rightArrow = EmbeddedAssetLoader.LoadTexture(graphicsDevice, "Assets/Letters/ArrowRight_r.png", transparentBlack: true);
        _ok = EmbeddedAssetLoader.LoadTexture(graphicsDevice, "Assets/Letters/OK_r.png", transparentBlack: true);
        _eff = EmbeddedAssetLoader.LoadTexture(graphicsDevice, "Assets/Letters/Delete_r.png", transparentBlack: true);

        foreach (char character in AllowedCharacters)
        {
            _largeRedLetters[character] = EmbeddedAssetLoader.LoadTexture(graphicsDevice, $"Assets/Letters/{character}_r.png", transparentBlack: true);
            _smallRedLetters[character] = EmbeddedAssetLoader.LoadTexture(graphicsDevice, $"Assets/Letters/{character}_r.png", transparentBlack: true);
            _smallBlueLetters[character] = EmbeddedAssetLoader.LoadTexture(graphicsDevice, $"Assets/Letters/{character}_b.png", transparentBlack: true);
        }
    }

    public void Draw(SpriteBatch spriteBatch, int score, string currentName, int selectedCharacterIndex)
    {
        spriteBatch.Draw(_background, new Rectangle(PanelX, PanelY, PanelWidth, 600), Color.White);

        DrawCenteredText(spriteBatch, "SCORE : " + score.ToString(CultureInfo.InvariantCulture), ScoreY, ScoreScale, Color.White);
        DrawCenteredText(spriteBatch, "ENTREZ VOTRE NOM", EnterNameY, PromptScale, Color.White);

        int previousIndex = selectedCharacterIndex == 0 ? AllowedCharacters.Length - 1 : selectedCharacterIndex - 1;
        int nextIndex = selectedCharacterIndex == AllowedCharacters.Length - 1 ? 0 : selectedCharacterIndex + 1;
        char previousCharacter = AllowedCharacters[previousIndex];
        char currentCharacter = AllowedCharacters[selectedCharacterIndex];
        char nextCharacter = AllowedCharacters[nextIndex];

        DrawTexture(spriteBatch, _leftArrow, LeftArrowX, LeftArrowY, ArrowWidth, ArrowHeight);
        DrawTexture(spriteBatch, _rightArrow, RightArrowX, RightArrowY, ArrowWidth, ArrowHeight);
        DrawTexture(spriteBatch, _smallRedLetters[previousCharacter], PreviousX, PreviousY, PreviousWidth, PreviousHeight);
        DrawTexture(spriteBatch, _largeRedLetters[currentCharacter], CurrentX, CurrentY, CurrentWidth, CurrentHeight);
        DrawTexture(spriteBatch, _smallRedLetters[nextCharacter], NextX, NextY, NextWidth, NextHeight);
        DrawTexture(spriteBatch, _eff, EffX, EffY, ButtonWidth, ButtonHeight);
        DrawTexture(spriteBatch, _ok, OkX, OkY, ButtonWidth, ButtonHeight);

        for (int i = 0; i < currentName.Length; i++)
        {
            if (_smallBlueLetters.TryGetValue(currentName[i], out Texture2D? letterTexture))
            {
                DrawTexture(spriteBatch, letterTexture, 5 + (i * 80), NameY, PreviousWidth, PreviousHeight);
            }
        }
    }

    public void Dispose()
    {
        _textRenderer.Dispose();
        _background.Dispose();
        _leftArrow.Dispose();
        _rightArrow.Dispose();
        _ok.Dispose();
        _eff.Dispose();

        foreach (Texture2D texture in _largeRedLetters.Values)
        {
            texture.Dispose();
        }

        foreach (Texture2D texture in _smallRedLetters.Values)
        {
            texture.Dispose();
        }

        foreach (Texture2D texture in _smallBlueLetters.Values)
        {
            texture.Dispose();
        }
    }

    public static Rectangle GetLeftArrowBounds()
    {
        return new Rectangle(PanelX + LeftArrowX, PanelY + LeftArrowY, ArrowWidth, ArrowHeight);
    }

    public static Rectangle GetRightArrowBounds()
    {
        return new Rectangle(PanelX + RightArrowX, PanelY + RightArrowY, ArrowWidth, ArrowHeight);
    }

    public static Rectangle GetCurrentLetterBounds()
    {
        return new Rectangle(PanelX + CurrentX, PanelY + CurrentY, CurrentWidth, CurrentHeight);
    }

    public static Rectangle GetEffBounds()
    {
        return new Rectangle(PanelX + EffX, PanelY + EffY, ButtonWidth, ButtonHeight);
    }

    public static Rectangle GetOkBounds()
    {
        return new Rectangle(PanelX + OkX, PanelY + OkY, ButtonWidth, ButtonHeight);
    }

    private void DrawCenteredText(SpriteBatch spriteBatch, string text, float y, float scale, Color color)
    {
        float x = PanelX + (PanelWidth / 2f) - (_textRenderer.MeasureWidth(text, scale) / 2f);
        _textRenderer.DrawText(spriteBatch, text, new Vector2(x, PanelY + y), scale, color);
    }

    private static void DrawTexture(SpriteBatch spriteBatch, Texture2D texture, int x, int y, int width, int height)
    {
        spriteBatch.Draw(texture, new Rectangle(PanelX + x, PanelY + y, width, height), Color.White);
    }
}
