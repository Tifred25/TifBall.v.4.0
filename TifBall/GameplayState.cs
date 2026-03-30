using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace TifBall;

internal sealed class GameplayState
{
    public List<BrickState> Bricks { get; } = new();
    public List<FallingPowerUpState> PowerUps { get; } = new();
    public List<BallState> Balls { get; } = new();
    public List<ShotState> Shots { get; } = new();
    public List<FloatingScoreState> FloatingScores { get; } = new();

    public int CurrentLevel { get; set; } = 1;
    public int Score { get; set; }
    public int Lives { get; set; }
    public int CurrentPaddleWidth { get; set; }
    public int CurrentBallSize { get; set; }
    public string? LastPowerUpLabel { get; set; }
    public bool StickyMode { get; set; }
    public bool InvincibleBalls { get; set; }
    public bool MachineGunActive { get; set; }
    public float MachineGunCooldownRemaining { get; set; }
    public float InvincibilityBarRemaining { get; set; }
    public float DrunkRemaining { get; set; }
    public bool IsGameOver { get; set; }
    public bool IsAwaitingStart { get; set; } = true;
    public bool IsLevelTransitioning { get; set; }
    public float LevelTransitionY { get; set; } = -100f;
    public bool IsPaused { get; set; }
    public bool IsShowingHighScores { get; set; }
    public bool IsEnteringHighScore { get; set; }
    public bool IsShowingPresentation { get; set; } = true;
    public string PendingHighScoreName { get; set; } = string.Empty;
    public int SelectedHighScoreCharIndex { get; set; }

    public GameplayState(int initialLives, int defaultPaddleWidth, int defaultBallSize)
    {
        Lives = initialLives;
        CurrentPaddleWidth = defaultPaddleWidth;
        CurrentBallSize = defaultBallSize;
    }

    public void ResetTransientRoundState(int defaultPaddleWidth, int defaultBallSize)
    {
        PowerUps.Clear();
        Shots.Clear();
        FloatingScores.Clear();
        StickyMode = false;
        InvincibleBalls = false;
        MachineGunActive = false;
        MachineGunCooldownRemaining = 0f;
        InvincibilityBarRemaining = 0f;
        DrunkRemaining = 0f;
        CurrentPaddleWidth = defaultPaddleWidth;
        CurrentBallSize = defaultBallSize;
        LastPowerUpLabel = null;
    }
}

internal readonly record struct BrickState(Rectangle Bounds, int Type, int HitPoints);
internal readonly record struct FallingPowerUpState(PowerUpType Type, Vector2 Position, Vector2 Velocity, int Width, int Height);
internal readonly record struct BallState(Vector2 PreviousPosition, Vector2 Position, Vector2 Velocity, bool IsLaunched, float StuckPaddleOffset, int BrickHitCount);
internal readonly record struct ShotState(Vector2 Position, int Width, int Height);
internal readonly record struct FloatingScoreState(int Score, Vector2 Position, float RemainingSeconds);

internal enum PowerUpType
{
    BallSpeedUp,
    BallSpeedDown,
    PaddleGrow,
    PaddleShrink,
    ExtraLife,
    Sticky,
    ThreeBalls,
    InvincibleBall,
    BallGrow,
    BallShrink,
    MachineGun,
    InvincibilityBar,
    Drunk,
    Pineapple,
    Banana,
    Cherries,
    Lemon,
    Strawberry,
    Raspberry,
    Kiwi,
    Apple
}
