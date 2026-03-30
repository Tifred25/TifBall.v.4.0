using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace TifBall;

internal sealed class TifBallGame : Game
{
    private const float LevelTransitionSpeed = 300f;
    private const float LevelTransitionMaxY = 360f;
    private const float LevelTransitionDurationSeconds = 3f;
    private const int WindowedBackBufferWidth = 992;
    private const int WindowedBackBufferHeight = 716;
    private const int GameAreaX = 96;
    private const int GameAreaY = 64;
    private const int GameAreaWidth = 800;
    private const int GameAreaHeight = 600;
    private const int BrickWidth = 50;
    private const int BrickHeight = 20;
    private const int PaddleY = 530;
    private const float InitialBallSpeed = 240f;
    private const int InitialLives = 2;
    private const float PowerUpSpawnChance = 0.15f;
    private const float ShotPowerUpSpawnChance = 0.10f;
    private const float PowerUpFallSpeed = 120f;
    private const int DefaultPaddleWidth = 120;
    private const int MinPaddleWidth = 40;
    private const int MaxPaddleWidth = 240;
    private const int PaddleWidthStep = 20;
    private const int DefaultBallSize = 20;
    private const int MinBallSize = 10;
    private const int MaxBallSize = 60;
    private const int BallSizeStep = 10;
    private const float MachineGunShotSpeed = 420f;
    private const float MachineGunShotCooldownSeconds = 0.18f;
    private const float InvincibilityBarDurationSeconds = 10f;
    private const float InvincibilityBarFadeSeconds = 2f;
    private const int InvincibilityBarHeight = 20;
    private const int MachineGunWidth = 40;
    private const int MachineGunHeight = 20;
    private const int StandardBrickScore = 500;
    private const int ThreeHitBrickTouchScore = 200;
    private const int IndestructibleBrickTouchScore = 20;
    private const int ThreeHitBrickDestroyScore = 1000;
    private const int IndestructibleBrickDestroyScore = 4000;
    private const int IndestructibleBrickHitPoints = 20;
    private const float BallTrailSpacingPixels = 5f;
    private const string ApplicationVersion = "3.0.0";
    private const string WindowBaseTitle = "TifBall v3.0";
    private static readonly string PowerUpTestSettingsPath = Path.Combine(AppContext.BaseDirectory, "power-up-test-settings.local.json");

    private readonly GraphicsDeviceManager _graphics;
    private readonly AudioManager _audioManager = new();
    private readonly GameplayState _state = new(InitialLives, DefaultPaddleWidth, DefaultBallSize);
    private readonly PowerUpTestSettings? _powerUpTestSettings = PowerUpTestSettings.TryLoad(PowerUpTestSettingsPath);
    private readonly RoundManager _roundManager;
    private readonly GameplayCoordinator _gameplayCoordinator;
    private readonly LegacyHighScoreStore _highScoreStore = new(Path.Combine(AppContext.BaseDirectory, "Scores.dat"));
    private HudRenderer? _hudRenderer;
    private HighScoreRenderer? _highScoreRenderer;
    private ScoreEntryRenderer? _scoreEntryRenderer;
    private PresentationScene? _presentationScene;
    private FloatingScoreRenderer? _floatingScoreRenderer;
    private AboutRenderer? _aboutRenderer;
    private NativeOptionsMenu? _nativeOptionsMenu;
    private SpriteBatch? _spriteBatch;
    private Texture2D? _backgroundPixel;
    private Texture2D? _borderPixel;
    private Texture2D? _paddleTexture;
    private Texture2D? _ballTexture;
    private Texture2D? _invincibleBallTexture;
    private Texture2D? _shotTexture;
    private Texture2D? _machineGunTexture;
    private Texture2D? _invincibilityBarActiveTexture;
    private Texture2D? _invincibilityBarTexture;
    private readonly Dictionary<int, Texture2D> _brickTextures = new();
    private readonly Dictionary<PowerUpType, Texture2D> _powerUpTextures = new();
    private readonly List<Texture2D> _gameplayBackgroundTextures = new();
    private Texture2D? _currentGameplayBackground;
    private Vector2 _paddlePosition;
    private MouseState _previousMouseState;
    private KeyboardState _previousKeyboardState;
    private readonly Random _random = new();
    private bool _backgroundMusicPlaying;
    private float _levelTransitionRemaining;
    private double _framerateAccumulatedSeconds;
    private int _framerateFrameCount;
    private int _currentFramerate;
    private bool _resumePauseAfterHighScoresDismiss;
    private bool _restartGameAfterHighScoresDismiss;
    private bool _previousDrunkState;
    private Vector2 _renderOffset;
    private bool _musicEnabled = true;
    private bool _soundsEnabled = true;
    private bool _backgroundImagesEnabled = true;
    private bool _isShowingAbout;
    private string _applicationVersionText = ApplicationVersion;

    public TifBallGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        _roundManager = new RoundManager(
            _state,
            InitialLives,
            DefaultPaddleWidth,
            DefaultBallSize,
            InitialBallSpeed,
            GetInitialBallSpeedMultiplier(),
            GameAreaX,
            GameAreaY,
            GameAreaWidth,
            PaddleY,
            () => _levelTransitionRemaining = LevelTransitionDurationSeconds);
        _gameplayCoordinator = new GameplayCoordinator(
            _state,
            _roundManager,
            _random,
            GameAreaX,
            GameAreaY,
            GameAreaWidth,
            GameAreaHeight,
            _powerUpTestSettings?.BallSpawnChance ?? PowerUpSpawnChance,
            _powerUpTestSettings?.ShotSpawnChance ?? ShotPowerUpSpawnChance,
            PowerUpFallSpeed,
            MinPaddleWidth,
            MaxPaddleWidth,
            PaddleWidthStep,
            MinBallSize,
            MaxBallSize,
            BallSizeStep,
            InvincibilityBarDurationSeconds,
            InvincibilityBarHeight,
            IndestructibleBrickHitPoints,
            StandardBrickScore,
            ThreeHitBrickTouchScore,
            IndestructibleBrickTouchScore,
            ThreeHitBrickDestroyScore,
            IndestructibleBrickDestroyScore,
            _powerUpTestSettings);
        _graphics.PreferredBackBufferWidth = WindowedBackBufferWidth;
        _graphics.PreferredBackBufferHeight = WindowedBackBufferHeight;
        _graphics.SynchronizeWithVerticalRetrace = true;
        Window.AllowUserResizing = true;
        Window.ClientSizeChanged += (_, _) => UpdateRenderOffset();

        Window.Title = WindowBaseTitle;
        IsMouseVisible = true;
    }

    protected override void LoadContent()
    {
        UpdateRenderOffset();
        InitializeNativeOptionsMenu();
        _audioManager.LoadEmbeddedSounds(
            "Crunch.wav",
            "Boing.wav",
            "Hiccup.wav",
            "Laser.wav",
            "Tick.wav",
            "Ping.wav",
            "Pop.wav",
            "Splat.wav",
            "Pong.wav",
            "Recharge.wav",
            "Clack.wav",
            "LaserSaber.wav",
            "Woohoo.wav");
        _backgroundMusicPlaying = _audioManager.TryLoadBackgroundMusic("music.ogg")
            && _audioManager.TryPlayBackgroundMusic();

        _hudRenderer = new HudRenderer(GraphicsDevice);
        _highScoreRenderer = new HighScoreRenderer(GraphicsDevice);
        _scoreEntryRenderer = new ScoreEntryRenderer(GraphicsDevice);
        _floatingScoreRenderer = new FloatingScoreRenderer(GraphicsDevice);
        _aboutRenderer = new AboutRenderer(GraphicsDevice);
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _backgroundPixel = CreateSolidTexture(Color.Black);
        _borderPixel = CreateSolidTexture(new Color(244, 208, 63));
        _paddleTexture = LoadTexture("Assets/Paddle.png", transparentBlack: true);
        _ballTexture = LoadTexture("Assets/Ball.png", transparentBlack: true);
        _invincibleBallTexture = LoadTexture("Assets/InvincibleBall.png", transparentBlack: true);
        _shotTexture = LoadTexture("Assets/Shot.png", transparentBlack: true);
        _machineGunTexture = LoadTexture("Assets/MachineGun.png", transparentBlack: true);
        _invincibilityBarActiveTexture = LoadTexture("Assets/InvincibilityBar.png", transparentBlack: true);
        _invincibilityBarTexture = LoadTexture("Assets/InvincibilityBar2.png", transparentBlack: true);
        _presentationScene = LoadPresentationScene();
        _applicationVersionText = GetApplicationVersionText();
        LoadGameplayBackgroundTextures();
        LoadBrickTextures();
        LoadPowerUpTextures();

        _roundManager.CenterPaddle(ref _paddlePosition);
        int initialLevel = GetInitialLevelFromTestSettings();
        LoadLevel(initialLevel);
        ApplyStartupTestSettings();
        UpdateWindowTitle();
    }

    protected override void Update(GameTime gameTime)
    {
        InitializeNativeOptionsMenu();
        UpdateFramerate(gameTime);

        KeyboardState keyboardState = Keyboard.GetState();
        MouseState mouseState = Mouse.GetState();
        if (keyboardState.IsKeyDown(Keys.Escape))
        {
            if (_isShowingAbout)
            {
                _isShowingAbout = false;
            }
            else
            {
                Exit();
            }
        }

        if (_isShowingAbout)
        {
            if (IsAboutDismissRequested(mouseState, keyboardState))
            {
                _isShowingAbout = false;
            }

            _previousMouseState = mouseState;
            _previousKeyboardState = keyboardState;
            base.Update(gameTime);
            return;
        }

        if (_state.IsEnteringHighScore)
        {
            HandleHighScoreEntry(keyboardState, mouseState);
            _previousMouseState = mouseState;
            _previousKeyboardState = keyboardState;
            base.Update(gameTime);
            return;
        }

        if (_state.IsShowingPresentation)
        {
            float presentationDeltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _presentationScene?.Update(presentationDeltaSeconds, PlaySoundEffect);
            bool clickPressed = mouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released;
            if (clickPressed || (_presentationScene?.CanContinue == true && IsOverlayDismissRequested(mouseState, keyboardState)))
            {
                _state.IsShowingPresentation = false;
                ShowHighScores();
                UpdateWindowTitle();
            }

            _previousMouseState = mouseState;
            _previousKeyboardState = keyboardState;
            base.Update(gameTime);
            return;
        }

        if (_state.IsGameOver)
        {
            if (IsRestartRequested(mouseState, keyboardState))
            {
                RestartGame();
            }

            _previousMouseState = mouseState;
            _previousKeyboardState = keyboardState;
            base.Update(gameTime);
            return;
        }

        if (_state.IsShowingHighScores)
        {
            if (IsOverlayDismissRequested(mouseState, keyboardState))
            {
                DismissHighScores();
            }

            _previousMouseState = mouseState;
            _previousKeyboardState = keyboardState;
            base.Update(gameTime);
            return;
        }

        if (_state.IsLevelTransitioning)
        {
            UpdateLevelTransition((float)gameTime.ElapsedGameTime.TotalSeconds);
            _previousMouseState = mouseState;
            _previousKeyboardState = keyboardState;
            base.Update(gameTime);
            return;
        }

        if (IsHighScoreToggleRequested(keyboardState))
        {
            ShowHighScores(_state.IsPaused);
            UpdateWindowTitle();
            _previousMouseState = mouseState;
            _previousKeyboardState = keyboardState;
            base.Update(gameTime);
            return;
        }

        if (IsPauseToggleRequested(keyboardState))
        {
            _state.IsPaused = !_state.IsPaused;
            UpdateWindowTitle();
        }

        if (IsFullscreenToggleRequested(keyboardState))
        {
            ToggleFullscreen();
            mouseState = Mouse.GetState();
        }

        SyncDrunkCursorIfNeeded(ref mouseState);
        _previousDrunkState = IsDrunk;

        if (_paddleTexture != null && !_state.IsPaused)
        {
            float paddleX = (mouseState.X - _renderOffset.X) - (_state.CurrentPaddleWidth / 2f);
            float minX = GameAreaX;
            float maxX = GameAreaX + GameAreaWidth - _state.CurrentPaddleWidth;
            if (IsDrunk)
            {
                float relative = paddleX - GameAreaX;
                paddleX = GameAreaX + (GameAreaWidth - _state.CurrentPaddleWidth) - relative;
            }
            paddleX = MathHelper.Clamp(paddleX, minX, maxX);
            _paddlePosition = new Vector2(paddleX, GameAreaY + PaddleY);

            if (_roundManager.HasAnyStuckBall())
            {
                _roundManager.RepositionStuckBalls(_paddlePosition);
            }

            if (IsStartRequested(mouseState, keyboardState))
            {
                _state.IsAwaitingStart = false;
                UpdateWindowTitle();
            }

            if (!_state.IsAwaitingStart && mouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released && _roundManager.HasAnyStuckBall())
            {
                _roundManager.LaunchStuckBalls();
                _audioManager.Play("Ping", 0.4f);
            }
        }

        if (_state.IsAwaitingStart || _state.IsPaused)
        {
            _previousMouseState = mouseState;
            _previousKeyboardState = keyboardState;
            base.Update(gameTime);
            return;
        }

        float deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (_state.MachineGunCooldownRemaining > 0f)
        {
            _state.MachineGunCooldownRemaining = Math.Max(0f, _state.MachineGunCooldownRemaining - deltaSeconds);
        }

        if (_state.InvincibilityBarRemaining > 0f)
        {
            _state.InvincibilityBarRemaining = Math.Max(0f, _state.InvincibilityBarRemaining - deltaSeconds);
        }

        HandleMachineGunInput();

        if (_ballTexture != null && _state.Balls.Count > 0)
        {
            UpdateBalls(deltaSeconds);
        }

        UpdateShots(deltaSeconds);
        UpdatePowerUps(deltaSeconds);
        _gameplayCoordinator.UpdateFloatingScores(deltaSeconds);

        _previousMouseState = mouseState;
        _previousKeyboardState = keyboardState;

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(new Color(70, 70, 70));

        if (_spriteBatch != null && _backgroundPixel != null && _borderPixel != null)
        {
            _spriteBatch.Begin(transformMatrix: Matrix.CreateTranslation(_renderOffset.X, _renderOffset.Y, 0f));

            if (_state.IsShowingPresentation && _presentationScene != null)
            {
                _spriteBatch.End();
                Rectangle previousPresentationScissor = GraphicsDevice.ScissorRectangle;
                GraphicsDevice.ScissorRectangle = new Rectangle(
                    (int)_renderOffset.X + GameAreaX,
                    (int)_renderOffset.Y + GameAreaY,
                    GameAreaWidth,
                    GameAreaHeight);
                _spriteBatch.Begin(
                    transformMatrix: Matrix.CreateTranslation(_renderOffset.X, _renderOffset.Y, 0f),
                    rasterizerState: new RasterizerState { ScissorTestEnable = true });
                _presentationScene.Draw(_spriteBatch, new Rectangle(GameAreaX, GameAreaY, GameAreaWidth, GameAreaHeight));
                DrawAboutOverlay();
                _spriteBatch.End();
                GraphicsDevice.ScissorRectangle = previousPresentationScissor;
                base.Draw(gameTime);
                return;
            }

            if (_state.IsEnteringHighScore && _scoreEntryRenderer != null)
            {
                _scoreEntryRenderer.Draw(_spriteBatch, _state.Score, _state.PendingHighScoreName, _state.SelectedHighScoreCharIndex);
                DrawAboutOverlay();
                _spriteBatch.End();
                base.Draw(gameTime);
                return;
            }

            if (_state.IsShowingHighScores && _highScoreRenderer != null)
            {
                _highScoreRenderer.Draw(_spriteBatch, _highScoreStore.Entries);
                DrawAboutOverlay();
                _spriteBatch.End();
                base.Draw(gameTime);
                return;
            }

            const int borderThickness = 8;
            Color frameColor = new Color(176, 176, 176);
            if (_backgroundImagesEnabled && _currentGameplayBackground != null)
            {
                _spriteBatch.Draw(_currentGameplayBackground, new Rectangle(GameAreaX, GameAreaY, GameAreaWidth, GameAreaHeight), Color.White);
            }
            else
            {
                _spriteBatch.Draw(_backgroundPixel, new Rectangle(GameAreaX, GameAreaY, GameAreaWidth, GameAreaHeight), Color.White);
            }
            _spriteBatch.Draw(_borderPixel, new Rectangle(GameAreaX - borderThickness, GameAreaY - borderThickness, GameAreaWidth + (2 * borderThickness), borderThickness), frameColor);
            _spriteBatch.Draw(_borderPixel, new Rectangle(GameAreaX - borderThickness, GameAreaY + GameAreaHeight, GameAreaWidth + (2 * borderThickness), borderThickness), frameColor);
            _spriteBatch.Draw(_borderPixel, new Rectangle(GameAreaX - borderThickness, GameAreaY, borderThickness, GameAreaHeight), frameColor);
            _spriteBatch.Draw(_borderPixel, new Rectangle(GameAreaX + GameAreaWidth, GameAreaY, borderThickness, GameAreaHeight), frameColor);
            _spriteBatch.End();

            Rectangle previousScissor = GraphicsDevice.ScissorRectangle;
            GraphicsDevice.ScissorRectangle = new Rectangle(
                (int)_renderOffset.X + GameAreaX,
                (int)_renderOffset.Y + GameAreaY,
                GameAreaWidth,
                GameAreaHeight);
            _spriteBatch.Begin(
                transformMatrix: Matrix.CreateTranslation(_renderOffset.X, _renderOffset.Y, 0f),
                rasterizerState: new RasterizerState { ScissorTestEnable = true });
            if (_paddleTexture != null)
            {
                Rectangle paddleTarget = new Rectangle((int)_paddlePosition.X, (int)_paddlePosition.Y, _state.CurrentPaddleWidth, _paddleTexture.Height);
                _spriteBatch.Draw(_paddleTexture, paddleTarget, Color.White);

                if (_state.MachineGunActive && _machineGunTexture != null)
                {
                    Rectangle leftGunTarget = GetLeftMachineGunTarget();
                    Rectangle rightGunTarget = GetRightMachineGunTarget();
                    _spriteBatch.Draw(_machineGunTexture, leftGunTarget, Color.White);
                    _spriteBatch.Draw(_machineGunTexture, rightGunTarget, Color.White);
                }
            }

            if (_ballTexture != null)
            {
                foreach (BallState ball in _state.Balls)
                {
                    Texture2D texture = _state.InvincibleBalls && _invincibleBallTexture != null ? _invincibleBallTexture : _ballTexture;
                    Vector2 ballScale = new Vector2(
                        _state.CurrentBallSize / (float)texture.Width,
                        _state.CurrentBallSize / (float)texture.Height);
                    DrawBallTrail(texture, ball, ballScale);
                    _spriteBatch.Draw(
                        texture,
                        ball.Position,
                        null,
                        Color.White,
                        0f,
                        Vector2.Zero,
                        ballScale,
                        SpriteEffects.None,
                        0f);
                }
            }

            foreach (BrickState brick in _state.Bricks)
            {
                if (_brickTextures.TryGetValue(brick.Type, out Texture2D? brickTexture))
                {
                    _spriteBatch.Draw(brickTexture, brick.Bounds, Color.White);
                }
            }

            foreach (FallingPowerUpState powerUp in _state.PowerUps)
            {
                if (_powerUpTextures.TryGetValue(powerUp.Type, out Texture2D? powerUpTexture))
                {
                    Rectangle powerUpTarget = new Rectangle((int)powerUp.Position.X, (int)powerUp.Position.Y, powerUp.Width, powerUp.Height);
                    _spriteBatch.Draw(powerUpTexture, powerUpTarget, Color.White);
                }
            }

            if (_shotTexture != null)
            {
                foreach (ShotState shot in _state.Shots)
                {
                    Rectangle shotTarget = new Rectangle((int)shot.Position.X, (int)shot.Position.Y, shot.Width, shot.Height);
                    _spriteBatch.Draw(_shotTexture, shotTarget, Color.White);
                }
            }

            if (_invincibilityBarTexture != null && _invincibilityBarActiveTexture != null && IsInvincibilityBarActive)
            {
                Texture2D barTexture = _state.InvincibilityBarRemaining <= InvincibilityBarFadeSeconds
                    ? _invincibilityBarTexture
                    : _invincibilityBarActiveTexture;
                Rectangle barTarget = new Rectangle(GameAreaX, GameAreaY + GameAreaHeight - barTexture.Height, GameAreaWidth, barTexture.Height);
                _spriteBatch.Draw(barTexture, barTarget, Color.White);
            }

            if (_floatingScoreRenderer != null)
            {
                _floatingScoreRenderer.Draw(_spriteBatch, _state.FloatingScores);
            }
            _spriteBatch.End();
            GraphicsDevice.ScissorRectangle = previousScissor;

            _spriteBatch.Begin(transformMatrix: Matrix.CreateTranslation(_renderOffset.X, _renderOffset.Y, 0f));
            DrawHud();
            DrawAboutOverlay();
            _spriteBatch.End();

            if (_state.IsLevelTransitioning && _hudRenderer != null)
            {
                Rectangle previousTransitionScissor = GraphicsDevice.ScissorRectangle;
                GraphicsDevice.ScissorRectangle = new Rectangle(
                    (int)_renderOffset.X + GameAreaX,
                    (int)_renderOffset.Y + GameAreaY,
                    GameAreaWidth,
                    GameAreaHeight);
                _spriteBatch.Begin(
                    transformMatrix: Matrix.CreateTranslation(_renderOffset.X, _renderOffset.Y, 0f),
                    rasterizerState: new RasterizerState { ScissorTestEnable = true });
                _hudRenderer.DrawLevelTransition(_spriteBatch, _state.CurrentLevel, _state.LevelTransitionY);
                _spriteBatch.End();
                GraphicsDevice.ScissorRectangle = previousTransitionScissor;
            }
        }

        base.Draw(gameTime);
    }

    private Texture2D CreateSolidTexture(Color color)
    {
        Texture2D texture = new Texture2D(GraphicsDevice, 1, 1);
        texture.SetData(new[] { color });
        return texture;
    }

    private Texture2D LoadTexture(string relativePath, bool transparentBlack = false)
    {
        return EmbeddedAssetLoader.LoadTexture(GraphicsDevice, relativePath, transparentBlack);
    }

    private bool TryLoadTexture(string relativePath, out Texture2D? texture, bool transparentBlack = false)
    {
        try
        {
            texture = LoadTexture(relativePath, transparentBlack);
            return true;
        }
        catch (Exception ex)
        {
            WriteHostDiagnosticLog("Texture load failed for " + relativePath + Environment.NewLine + ex);
            texture = null;
            return false;
        }
    }

    private static void WriteHostDiagnosticLog(string message)
    {
        string logPath = Path.Combine(AppContext.BaseDirectory, "monogame-host-error.log");
        string logEntry =
            "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) + "] "
            + message
            + Environment.NewLine;

        System.IO.File.AppendAllText(logPath, logEntry);
    }

    private void LoadBrickTextures()
    {
        _brickTextures.Clear();
        _brickTextures[1] = LoadTexture("Assets/Brick1.png", transparentBlack: true);
        _brickTextures[2] = LoadTexture("Assets/Brick2.png", transparentBlack: true);
        _brickTextures[3] = LoadTexture("Assets/Brick3.png", transparentBlack: true);
        _brickTextures[4] = LoadTexture("Assets/Brick4.png", transparentBlack: true);
        _brickTextures[5] = LoadTexture("Assets/Brick5.png", transparentBlack: true);
        _brickTextures[6] = LoadTexture("Assets/Brick6.png", transparentBlack: true);
        _brickTextures[9] = LoadTexture("Assets/Brick9.png", transparentBlack: true);
    }

    private void LoadPowerUpTextures()
    {
        _powerUpTextures.Clear();
        _powerUpTextures[PowerUpType.PaddleGrow] = LoadTexture("Assets/PowerUpPaddleGrow.png", transparentBlack: true);
        _powerUpTextures[PowerUpType.PaddleShrink] = LoadTexture("Assets/PowerUpPaddleShrink.png", transparentBlack: true);
        _powerUpTextures[PowerUpType.BallSpeedUp] = LoadTexture("Assets/PowerUpBallSpeedUp.png", transparentBlack: true);
        _powerUpTextures[PowerUpType.BallSpeedDown] = LoadTexture("Assets/PowerUpBallSpeedDown.png", transparentBlack: true);
        _powerUpTextures[PowerUpType.ExtraLife] = LoadTexture("Assets/PowerUpExtraLife.png", transparentBlack: true);
        _powerUpTextures[PowerUpType.Sticky] = LoadTexture("Assets/PowerUpSticky.png", transparentBlack: true);
        _powerUpTextures[PowerUpType.ThreeBalls] = LoadTexture("Assets/PowerUpThreeBalls.png", transparentBlack: true);
        _powerUpTextures[PowerUpType.InvincibleBall] = LoadTexture("Assets/PowerUpInvincibleBall.png", transparentBlack: true);
        _powerUpTextures[PowerUpType.BallGrow] = LoadTexture("Assets/PowerUpBallGrow.png", transparentBlack: true);
        _powerUpTextures[PowerUpType.BallShrink] = LoadTexture("Assets/PowerUpBallShrink.png", transparentBlack: true);
        _powerUpTextures[PowerUpType.MachineGun] = LoadTexture("Assets/PowerUpMachineGun.png", transparentBlack: true);
        _powerUpTextures[PowerUpType.InvincibilityBar] = LoadTexture("Assets/PowerUpInvincibilityBar.png", transparentBlack: true);
        _powerUpTextures[PowerUpType.Drunk] = LoadTexture("Assets/PowerUpDrunk.png", transparentBlack: true);
        _powerUpTextures[PowerUpType.Pineapple] = LoadTexture("Assets/Pineapple.png", transparentBlack: true);
        _powerUpTextures[PowerUpType.Banana] = LoadTexture("Assets/Banana.png", transparentBlack: true);
        _powerUpTextures[PowerUpType.Cherries] = LoadTexture("Assets/Cherries.png", transparentBlack: true);
        _powerUpTextures[PowerUpType.Lemon] = LoadTexture("Assets/Lemon.png", transparentBlack: true);
        _powerUpTextures[PowerUpType.Strawberry] = LoadTexture("Assets/Strawberry.png", transparentBlack: true);
        _powerUpTextures[PowerUpType.Raspberry] = LoadTexture("Assets/Raspberry.png", transparentBlack: true);
        _powerUpTextures[PowerUpType.Kiwi] = LoadTexture("Assets/Kiwi.png", transparentBlack: true);
        _powerUpTextures[PowerUpType.Apple] = LoadTexture("Assets/Apple.png", transparentBlack: true);
    }

    private bool LoadLevel(int levelNumber)
    {
        _state.Bricks.Clear();

        string[] lines = File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "Level.txt"));
        string header = "LEVEL" + levelNumber;
        int levelStartIndex = Array.FindIndex(lines, line => string.Equals(line.Trim(), header, StringComparison.OrdinalIgnoreCase));
        if (levelStartIndex < 0)
        {
            return false;
        }

        int startX = GameAreaX;
        int startY = GameAreaY + 50;

        for (int row = 0; row < 20; row++)
        {
            string rowText = lines[levelStartIndex + row + 1].Trim();
            if (rowText == "-")
            {
                continue;
            }

            string[] cells = rowText.Split(',');
            for (int column = 0; column < 16; column++)
            {
                if (column >= cells.Length)
                {
                    continue;
                }

                if (!int.TryParse(cells[column].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int brickType))
                {
                    continue;
                }

                if (brickType == 0)
                {
                    continue;
                }

                _state.Bricks.Add(new BrickState(
                    new Rectangle(
                        startX + (column * BrickWidth),
                        startY + (row * BrickHeight),
                        BrickWidth,
                        BrickHeight),
                    brickType,
                    GameplayCoordinator.GetInitialHitPoints(brickType)));
            }
        }

        _roundManager.InitializeLevel(levelNumber, ref _paddlePosition);
        SelectRandomGameplayBackground();
        UpdateWindowTitle();
        return true;
    }

    private void UpdateBalls(float deltaSeconds)
    {
        if (_paddleTexture == null)
        {
            return;
        }

        Rectangle paddleBounds = new Rectangle((int)_paddlePosition.X, (int)_paddlePosition.Y, _state.CurrentPaddleWidth, _paddleTexture.Height);
        _gameplayCoordinator.UpdateBalls(
            deltaSeconds,
            AdvanceToNextLevel,
            UpdateWindowTitle,
            PlaySoundEffect,
            HandleBallDrain,
            paddleBounds,
            _paddlePosition);
    }

    private void HandleMachineGunInput()
    {
        if (_shotTexture == null || !_state.MachineGunActive)
        {
            return;
        }

        KeyboardState keyboardState = Keyboard.GetState();
        MouseState mouseState = Mouse.GetState();
        bool leftClickPressed = mouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released;
        bool rightClickPressed = mouseState.RightButton == ButtonState.Pressed && _previousMouseState.RightButton == ButtonState.Released;
        bool fireRequested = leftClickPressed || rightClickPressed || keyboardState.IsKeyDown(Keys.Space);
        Rectangle leftGunTarget = GetLeftMachineGunTarget();
        Rectangle rightGunTarget = GetRightMachineGunTarget();
        Vector2 leftShotOrigin = GetMachineGunShotOrigin(leftGunTarget, _shotTexture.Width, _shotTexture.Height);
        Vector2 rightShotOrigin = GetMachineGunShotOrigin(rightGunTarget, _shotTexture.Width, _shotTexture.Height);
        _gameplayCoordinator.HandleMachineGunInput(
            fireRequested,
            leftShotOrigin,
            rightShotOrigin,
            _shotTexture.Width,
            _shotTexture.Height,
            PlaySoundEffect);
    }

    private void UpdateShots(float deltaSeconds)
    {
        _gameplayCoordinator.UpdateShots(deltaSeconds, AdvanceToNextLevel, UpdateWindowTitle, PlaySoundEffect);
    }

    private void UpdatePowerUps(float deltaSeconds)
    {
        if (_paddleTexture == null)
        {
            return;
        }

        Rectangle paddleBounds = new Rectangle((int)_paddlePosition.X, (int)_paddlePosition.Y, _state.CurrentPaddleWidth, _paddleTexture.Height);
        _gameplayCoordinator.UpdatePowerUps(deltaSeconds, paddleBounds, _paddlePosition, UpdateWindowTitle, PlaySoundEffect);
    }

    private void ClampPaddleInsidePlayfield()
    {
        _gameplayCoordinator.ClampPaddleInsidePlayfield(ref _paddlePosition);
    }

    private void AdvanceToNextLevel()
    {
        int nextLevel = _state.CurrentLevel + 1;
        if (!LoadLevel(nextLevel))
        {
            LoadLevel(1);
        }
    }

    private void HandleBallDrain()
    {
        _roundManager.HandleBallDrain(ref _paddlePosition);
        SelectRandomGameplayBackground();
        if (_state.IsGameOver && _highScoreStore.Qualifies(_state.Score))
        {
            _state.IsEnteringHighScore = true;
            _state.IsShowingHighScores = false;
            _state.IsPaused = false;
            _state.PendingHighScoreName = string.Empty;
            _state.SelectedHighScoreCharIndex = 0;
        }
        else if (_state.IsGameOver)
        {
            ShowHighScores(restartGameAfterDismiss: true);
        }

        UpdateWindowTitle();
    }

    private void RestartGame()
    {
        _roundManager.RestartGame(ref _paddlePosition);
        SelectRandomGameplayBackground();
        _state.IsShowingHighScores = false;
        _state.IsEnteringHighScore = false;
        _state.PendingHighScoreName = string.Empty;
        _state.SelectedHighScoreCharIndex = 0;
        _state.IsShowingPresentation = false;
        _resumePauseAfterHighScoresDismiss = false;
        _restartGameAfterHighScoresDismiss = false;
        UpdateWindowTitle();
    }

    private bool IsRestartRequested(MouseState mouseState, KeyboardState keyboardState)
    {
        bool enterPressed = keyboardState.IsKeyDown(Keys.Enter) && !_previousKeyboardState.IsKeyDown(Keys.Enter);
        bool clickPressed = mouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released;
        return enterPressed || clickPressed;
    }

    private bool IsOverlayDismissRequested(MouseState mouseState, KeyboardState keyboardState)
    {
        bool enterPressed = keyboardState.IsKeyDown(Keys.Enter) && !_previousKeyboardState.IsKeyDown(Keys.Enter);
        bool hPressed = keyboardState.IsKeyDown(Keys.H) && !_previousKeyboardState.IsKeyDown(Keys.H);
        bool clickPressed = mouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released;
        return enterPressed || hPressed || clickPressed;
    }

    private bool IsStartRequested(MouseState mouseState, KeyboardState keyboardState)
    {
        if (!_state.IsAwaitingStart)
        {
            return false;
        }

        bool enterPressed = keyboardState.IsKeyDown(Keys.Enter) && !_previousKeyboardState.IsKeyDown(Keys.Enter);
        bool clickPressed = mouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released;
        return enterPressed || clickPressed;
    }

    private bool IsPauseToggleRequested(KeyboardState keyboardState)
    {
        return !_state.IsAwaitingStart
            && !_state.IsGameOver
            && !_state.IsShowingHighScores
            && keyboardState.IsKeyDown(Keys.P)
            && !_previousKeyboardState.IsKeyDown(Keys.P);
    }

    private bool IsHighScoreToggleRequested(KeyboardState keyboardState)
    {
        bool hPressed = keyboardState.IsKeyDown(Keys.H) && !_previousKeyboardState.IsKeyDown(Keys.H);
        return hPressed && (_state.IsAwaitingStart || _state.IsPaused || _state.IsGameOver);
    }

    private void UpdateFramerate(GameTime gameTime)
    {
        _framerateAccumulatedSeconds += gameTime.ElapsedGameTime.TotalSeconds;
        _framerateFrameCount++;

        if (_framerateAccumulatedSeconds < 1.0)
        {
            return;
        }

        _currentFramerate = (int)Math.Round(_framerateFrameCount / _framerateAccumulatedSeconds, MidpointRounding.AwayFromZero);
        _framerateAccumulatedSeconds = 0.0;
        _framerateFrameCount = 0;
        UpdateWindowTitle();
    }

    private void UpdateWindowTitle()
    {
        Window.Title = string.Format(CultureInfo.InvariantCulture, "{0} - {1} fps", WindowBaseTitle, _currentFramerate);
        UpdateNativeOptionsMenuState();
    }

    private int GetInitialLevelFromTestSettings()
    {
        if (_powerUpTestSettings?.StartLevel is int configuredLevel && configuredLevel > 0)
        {
            return configuredLevel;
        }

        return _state.CurrentLevel;
    }

    private float GetInitialBallSpeedMultiplier()
    {
        if (_powerUpTestSettings?.InitialBallSpeedMultiplier is float configuredMultiplier && configuredMultiplier > 0f)
        {
            return configuredMultiplier;
        }

        return 1f;
    }

    private void ApplyStartupTestSettings()
    {
        if (_powerUpTestSettings == null)
        {
            return;
        }

        if (_powerUpTestSettings.SkipPresentation == true)
        {
            _state.IsShowingPresentation = false;
        }

        if (_powerUpTestSettings.SkipInitialHighScores == true)
        {
            _state.IsShowingHighScores = false;
        }

        if (_powerUpTestSettings.AutoStartBall == true)
        {
            _state.IsShowingPresentation = false;
            _state.IsShowingHighScores = false;
            _state.IsAwaitingStart = false;
            _roundManager.LaunchStuckBalls();
        }
    }

    private void DrawHud()
    {
        if (_spriteBatch == null || _hudRenderer == null)
        {
            return;
        }

        _hudRenderer.Draw(_spriteBatch, CreateHudState());
    }

    private HudState CreateHudState()
    {
        return new HudState(
            _state.CurrentLevel,
            _state.Score,
            _state.Lives,
            _currentFramerate,
            _state.IsGameOver,
            _state.IsAwaitingStart,
            _state.IsLevelTransitioning,
            _state.LevelTransitionY,
            _state.IsPaused,
            _state.IsShowingHighScores,
            _state.IsEnteringHighScore,
            _state.IsShowingPresentation,
            _state.LastPowerUpLabel,
            _backgroundMusicPlaying,
            _audioManager.BackgroundMusicStatus,
            _state.MachineGunActive,
            IsInvincibilityBarActive);
    }

    private bool IsInvincibilityBarActive
    {
        get { return _state.InvincibilityBarRemaining > 0f; }
    }

    private bool IsDrunk
    {
        get { return _state.DrunkRemaining != 0f; }
    }

    private bool IsFullscreenToggleRequested(KeyboardState keyboardState)
    {
        bool f11Pressed = keyboardState.IsKeyDown(Keys.F11) && _previousKeyboardState.IsKeyUp(Keys.F11);
        bool altEnterPressed =
            keyboardState.IsKeyDown(Keys.Enter)
            && _previousKeyboardState.IsKeyUp(Keys.Enter)
            && (keyboardState.IsKeyDown(Keys.LeftAlt) || keyboardState.IsKeyDown(Keys.RightAlt));
        return f11Pressed || altEnterPressed;
    }

    private void ToggleFullscreen()
    {
        if (_graphics.IsFullScreen)
        {
            _graphics.IsFullScreen = false;
            _graphics.PreferredBackBufferWidth = WindowedBackBufferWidth;
            _graphics.PreferredBackBufferHeight = WindowedBackBufferHeight;
        }
        else
        {
            DisplayMode displayMode = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
            _graphics.PreferredBackBufferWidth = displayMode.Width;
            _graphics.PreferredBackBufferHeight = displayMode.Height;
            _graphics.IsFullScreen = true;
        }

        _graphics.ApplyChanges();
        UpdateRenderOffset();
        UpdateNativeOptionsMenuState();
    }

    private void UpdateRenderOffset()
    {
        float offsetX = Math.Max(0f, (GraphicsDevice.Viewport.Width - WindowedBackBufferWidth) / 2f);
        float offsetY = Math.Max(0f, (GraphicsDevice.Viewport.Height - WindowedBackBufferHeight) / 2f);
        _renderOffset = new Vector2(offsetX, offsetY);
    }

    private void SyncDrunkCursorIfNeeded(ref MouseState mouseState)
    {
        bool isDrunk = IsDrunk;
        if (_paddleTexture == null || isDrunk == _previousDrunkState)
        {
            return;
        }

        int targetX = isDrunk
            ? GetMirroredCursorXForCurrentPaddle()
            : GetNormalCursorXForCurrentPaddle();
        int targetY = Math.Clamp(mouseState.Y, 0, GraphicsDevice.Viewport.Height - 1);
        Mouse.SetPosition(targetX, targetY);
        mouseState = Mouse.GetState();
    }

    private int GetNormalCursorXForCurrentPaddle()
    {
        float paddleCenter = _paddlePosition.X + (_state.CurrentPaddleWidth / 2f);
        float screenX = _renderOffset.X + paddleCenter;
        return Math.Clamp((int)MathF.Round(screenX), 0, GraphicsDevice.Viewport.Width - 1);
    }

    private int GetMirroredCursorXForCurrentPaddle()
    {
        float mirroredCenter = GameAreaX + GameAreaWidth - (_paddlePosition.X - GameAreaX) - (_state.CurrentPaddleWidth / 2f);
        float screenX = _renderOffset.X + mirroredCenter;
        return Math.Clamp((int)MathF.Round(screenX), 0, GraphicsDevice.Viewport.Width - 1);
    }

    private void PlaySoundEffect(string soundName, float volume)
    {
        _audioManager.Play(soundName, volume);
    }

    private void UpdateLevelTransition(float deltaSeconds)
    {
        _levelTransitionRemaining = Math.Max(0f, _levelTransitionRemaining - deltaSeconds);

        if (_state.LevelTransitionY < LevelTransitionMaxY)
        {
            _state.LevelTransitionY = Math.Min(LevelTransitionMaxY, _state.LevelTransitionY + (LevelTransitionSpeed * deltaSeconds));
        }

        if (_state.LevelTransitionY >= LevelTransitionMaxY && _levelTransitionRemaining == 0f)
        {
            _state.IsLevelTransitioning = false;
            _state.LevelTransitionY = -100f;
        }
    }

    private void InitializeNativeOptionsMenu()
    {
        if (!OperatingSystem.IsWindows() || _nativeOptionsMenu != null)
        {
            return;
        }

        IntPtr nativeWindowHandle = NativeWindowHandleResolver.TryFindCurrentProcessMainWindow(WindowBaseTitle);
        if (nativeWindowHandle == IntPtr.Zero)
        {
            return;
        }

        _nativeOptionsMenu = new NativeOptionsMenu(
            nativeWindowHandle,
            TogglePauseFromMenu,
            ToggleMusicFromMenu,
            ToggleSoundsFromMenu,
            ToggleBackgroundImagesFromMenu,
            ShowAboutFromMenu);
        UpdateNativeOptionsMenuState();
    }

    private void LoadGameplayBackgroundTextures()
    {
        _gameplayBackgroundTextures.Clear();
        for (int i = 1; i <= 30; i++)
        {
            string relativePath = "Assets/Images/Image" + i.ToString(CultureInfo.InvariantCulture) + ".jpg";
            if (TryLoadTexture(relativePath, out Texture2D? texture) && texture != null)
            {
                _gameplayBackgroundTextures.Add(texture);
            }
        }

        SelectRandomGameplayBackground();
    }

    private void SelectRandomGameplayBackground()
    {
        if (_gameplayBackgroundTextures.Count == 0)
        {
            _currentGameplayBackground = null;
            return;
        }

        _currentGameplayBackground = _gameplayBackgroundTextures[_random.Next(_gameplayBackgroundTextures.Count)];
    }

    private void TogglePauseFromMenu()
    {
        if (!CanTogglePauseFromMenu())
        {
            UpdateNativeOptionsMenuState();
            return;
        }

        _state.IsPaused = !_state.IsPaused;
        UpdateWindowTitle();
    }

    private void ToggleMusicFromMenu()
    {
        _musicEnabled = !_musicEnabled;
        _audioManager.SetMusicEnabled(_musicEnabled);
        _backgroundMusicPlaying = _musicEnabled && _audioManager.TryPlayBackgroundMusic();
        UpdateNativeOptionsMenuState();
    }

    private void ToggleSoundsFromMenu()
    {
        _soundsEnabled = !_soundsEnabled;
        _audioManager.SetSoundsEnabled(_soundsEnabled);
        UpdateNativeOptionsMenuState();
    }

    private void ToggleBackgroundImagesFromMenu()
    {
        _backgroundImagesEnabled = !_backgroundImagesEnabled;
        if (_backgroundImagesEnabled && _currentGameplayBackground == null)
        {
            SelectRandomGameplayBackground();
        }

        UpdateNativeOptionsMenuState();
    }

    private void ShowAboutFromMenu()
    {
        _applicationVersionText = GetApplicationVersionText();
        _isShowingAbout = true;
    }

    private static string GetApplicationVersionText()
    {
        return ApplicationVersion;
    }

    private void UpdateNativeOptionsMenuState()
    {
        _nativeOptionsMenu?.UpdateState(
            _state.IsPaused,
            _musicEnabled,
            _soundsEnabled,
            _backgroundImagesEnabled,
            CanTogglePauseFromMenu(),
            !_graphics.IsFullScreen);
    }

    private bool CanTogglePauseFromMenu()
    {
        return !_state.IsShowingPresentation
            && !_state.IsShowingHighScores
            && !_state.IsEnteringHighScore
            && !_state.IsGameOver;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _nativeOptionsMenu?.Dispose();
            _audioManager.Dispose();
            _hudRenderer?.Dispose();
            _highScoreRenderer?.Dispose();
            _scoreEntryRenderer?.Dispose();
            _presentationScene?.Dispose();
            _floatingScoreRenderer?.Dispose();
            _aboutRenderer?.Dispose();
            _spriteBatch?.Dispose();
            _backgroundPixel?.Dispose();
            _borderPixel?.Dispose();
            _paddleTexture?.Dispose();
            _ballTexture?.Dispose();
            _invincibleBallTexture?.Dispose();
            _shotTexture?.Dispose();
            _machineGunTexture?.Dispose();
            _invincibilityBarActiveTexture?.Dispose();
            _invincibilityBarTexture?.Dispose();

            foreach (Texture2D texture in _brickTextures.Values)
            {
                texture.Dispose();
            }

            foreach (Texture2D texture in _powerUpTextures.Values)
            {
                texture.Dispose();
            }

            foreach (Texture2D texture in _gameplayBackgroundTextures)
            {
                texture.Dispose();
            }

            _brickTextures.Clear();
            _powerUpTextures.Clear();
            _gameplayBackgroundTextures.Clear();
        }

        base.Dispose(disposing);
    }

    private void HandleHighScoreEntry(KeyboardState keyboardState, MouseState mouseState)
    {
        bool didChange = false;
        bool leftClickPressed = mouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released;
        Point logicalMousePosition = new(
            (int)MathF.Round(mouseState.X - _renderOffset.X),
            (int)MathF.Round(mouseState.Y - _renderOffset.Y));

        if (WasKeyPressed(Keys.Left, keyboardState))
        {
            _state.SelectedHighScoreCharIndex =
                _state.SelectedHighScoreCharIndex == 0
                    ? 35
                    : _state.SelectedHighScoreCharIndex - 1;
            _audioManager.Play("Tick", 1f);
            didChange = true;
        }

        if (WasKeyPressed(Keys.Right, keyboardState))
        {
            _state.SelectedHighScoreCharIndex =
                _state.SelectedHighScoreCharIndex == 35
                    ? 0
                    : _state.SelectedHighScoreCharIndex + 1;
            _audioManager.Play("Tick", 1f);
            didChange = true;
        }

        if (leftClickPressed && ScoreEntryRenderer.GetLeftArrowBounds().Contains(logicalMousePosition))
        {
            _state.SelectedHighScoreCharIndex =
                _state.SelectedHighScoreCharIndex == 0
                    ? 35
                    : _state.SelectedHighScoreCharIndex - 1;
            _audioManager.Play("Tick", 1f);
            didChange = true;
        }

        if (leftClickPressed && ScoreEntryRenderer.GetRightArrowBounds().Contains(logicalMousePosition))
        {
            _state.SelectedHighScoreCharIndex =
                _state.SelectedHighScoreCharIndex == 35
                    ? 0
                    : _state.SelectedHighScoreCharIndex + 1;
            _audioManager.Play("Tick", 1f);
            didChange = true;
        }

        if (WasKeyPressed(Keys.Space, keyboardState) && _state.PendingHighScoreName.Length < 10)
        {
            _state.PendingHighScoreName += GetSelectedHighScoreCharacter();
            _audioManager.Play("Clack", 1f);
            didChange = true;
        }

        if (leftClickPressed
            && ScoreEntryRenderer.GetCurrentLetterBounds().Contains(logicalMousePosition)
            && _state.PendingHighScoreName.Length < 10)
        {
            _state.PendingHighScoreName += GetSelectedHighScoreCharacter();
            _audioManager.Play("Clack", 1f);
            didChange = true;
        }

        if (WasKeyPressed(Keys.Back, keyboardState) && _state.PendingHighScoreName.Length > 0)
        {
            _state.PendingHighScoreName = _state.PendingHighScoreName[..^1];
            didChange = true;
        }

        if (leftClickPressed
            && ScoreEntryRenderer.GetEffBounds().Contains(logicalMousePosition)
            && _state.PendingHighScoreName.Length > 0)
        {
            _state.PendingHighScoreName = _state.PendingHighScoreName[..^1];
            didChange = true;
        }

        foreach (char character in GetTypedHighScoreCharacters(keyboardState))
        {
            if (_state.PendingHighScoreName.Length >= 10)
            {
                break;
            }

            _state.PendingHighScoreName += character;
            SyncSelectedHighScoreCharacter(character);
            _audioManager.Play("Clack", 1f);
            didChange = true;
        }

        if ((WasKeyPressed(Keys.Enter, keyboardState)
                || (leftClickPressed && ScoreEntryRenderer.GetOkBounds().Contains(logicalMousePosition)))
            && _state.PendingHighScoreName.Length > 0)
        {
            _highScoreStore.SaveScore(_state.PendingHighScoreName, _state.Score);
            _state.IsEnteringHighScore = false;
            ShowHighScores(restartGameAfterDismiss: true);
            UpdateWindowTitle();
            return;
        }

        if (didChange)
        {
            UpdateWindowTitle();
        }
    }

    private bool WasKeyPressed(Keys key, KeyboardState keyboardState)
    {
        return keyboardState.IsKeyDown(key) && !_previousKeyboardState.IsKeyDown(key);
    }

    private bool IsAboutDismissRequested(MouseState mouseState, KeyboardState keyboardState)
    {
        bool enterPressed = keyboardState.IsKeyDown(Keys.Enter) && !_previousKeyboardState.IsKeyDown(Keys.Enter);
        bool clickPressed = mouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released;
        return enterPressed || clickPressed;
    }

    private IEnumerable<char> GetTypedHighScoreCharacters(KeyboardState keyboardState)
    {
        foreach (Keys key in keyboardState.GetPressedKeys())
        {
            if (_previousKeyboardState.IsKeyDown(key))
            {
                continue;
            }

            char? character = ConvertKeyToHighScoreCharacter(key);
            if (character.HasValue)
            {
                yield return character.Value;
            }
        }
    }

    private static char? ConvertKeyToHighScoreCharacter(Keys key)
    {
        if (key >= Keys.A && key <= Keys.Z)
        {
            int offset = key - Keys.A;
            return (char)('A' + offset);
        }

        if (key >= Keys.D0 && key <= Keys.D9)
        {
            int offset = key - Keys.D0;
            return (char)('0' + offset);
        }

        if (key >= Keys.NumPad0 && key <= Keys.NumPad9)
        {
            int offset = key - Keys.NumPad0;
            return (char)('0' + offset);
        }

        return null;
    }

    private char GetSelectedHighScoreCharacter()
    {
        const string allowedCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return allowedCharacters[_state.SelectedHighScoreCharIndex];
    }

    private void SyncSelectedHighScoreCharacter(char character)
    {
        const string allowedCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        int index = allowedCharacters.IndexOf(character);
        if (index >= 0)
        {
            _state.SelectedHighScoreCharIndex = index;
        }
    }

    private PresentationScene LoadPresentationScene()
    {
        Texture2D backgroundTexture = LoadTexture("Assets/Images/Background2.jpg");
        Texture2D ballTexture = LoadTexture("Assets/Ball.png", transparentBlack: true);
        Texture2D tifBallTexture = LoadTexture("Assets/Presentation/TifBall.png", transparentBlack: true);
        return new PresentationScene(GraphicsDevice, backgroundTexture, ballTexture, tifBallTexture);
    }

    private void ShowHighScores(bool resumePauseAfterDismiss = false, bool restartGameAfterDismiss = false)
    {
        _state.IsShowingHighScores = true;
        _state.IsPaused = false;
        _resumePauseAfterHighScoresDismiss = resumePauseAfterDismiss;
        _restartGameAfterHighScoresDismiss = restartGameAfterDismiss;
    }

    private void DismissHighScores()
    {
        _state.IsShowingHighScores = false;

        if (_restartGameAfterHighScoresDismiss)
        {
            RestartGame();
            return;
        }

        if (_resumePauseAfterHighScoresDismiss)
        {
            _state.IsPaused = true;
        }

        _resumePauseAfterHighScoresDismiss = false;
        _restartGameAfterHighScoresDismiss = false;
        UpdateWindowTitle();
    }

    private void DrawAboutOverlay()
    {
        if (!_isShowingAbout || _aboutRenderer == null || _ballTexture == null || _spriteBatch == null)
        {
            return;
        }

        _aboutRenderer.Draw(
            _spriteBatch,
            new Rectangle(GameAreaX, GameAreaY, GameAreaWidth, GameAreaHeight),
            _ballTexture,
            _applicationVersionText);
    }

    private void DrawBallTrail(Texture2D texture, BallState ball, Vector2 ballScale)
    {
        if (_spriteBatch == null || !ball.IsLaunched)
        {
            return;
        }

        float speed = ball.Velocity.Length();
        if (speed <= 0.01f)
        {
            return;
        }

        Vector2 direction = Vector2.Normalize(ball.Velocity);
        DrawBallGhost(texture, ball.Position - (direction * BallTrailSpacingPixels), ballScale, 0.18f);
        DrawBallGhost(texture, ball.Position - (direction * BallTrailSpacingPixels * 2f), ballScale, 0.10f);
    }

    private void DrawBallGhost(Texture2D texture, Vector2 position, Vector2 ballScale, float alpha)
    {
        if (_spriteBatch == null)
        {
            return;
        }

        _spriteBatch.Draw(
            texture,
            position,
            null,
            Color.White * alpha,
            0f,
            Vector2.Zero,
            ballScale,
            SpriteEffects.None,
            0f);
    }

    private Rectangle GetLeftMachineGunTarget()
    {
        return new Rectangle(
            (int)_paddlePosition.X,
            (int)(_paddlePosition.Y - (MachineGunHeight / 2f)),
            MachineGunWidth,
            MachineGunHeight);
    }

    private Rectangle GetRightMachineGunTarget()
    {
        return new Rectangle(
            (int)(_paddlePosition.X + _state.CurrentPaddleWidth - MachineGunWidth),
            (int)(_paddlePosition.Y - (MachineGunHeight / 2f)),
            MachineGunWidth,
            MachineGunHeight);
    }

    private static Vector2 GetMachineGunShotOrigin(Rectangle gunTarget, int shotWidth, int shotHeight)
    {
        float shotX = gunTarget.X + ((gunTarget.Width - shotWidth) / 2f);
        float shotY = gunTarget.Y - shotHeight + 1f;
        return new Vector2(shotX, shotY);
    }
}
