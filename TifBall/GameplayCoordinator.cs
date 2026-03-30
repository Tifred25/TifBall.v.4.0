using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace TifBall;

internal sealed class GameplayCoordinator
{
    private const float FloatingScoreDurationSeconds = 2f;
    private const float FloatingScoreSpeed = 120f;
    private const int MaxBalls = 50;
    private const float InitialBallSpeed = 240f;
    private const float PowerUpBallSpeedUpMultiplier = 1.5f;
    private const float BallSpeedDownMultiplier = 0.75f;
    private const float BrickHitSpeedUpMultiplier = 1.05f;
    private const int BrickHitsPerSpeedIncrease = 5;
    private static readonly Dictionary<PowerUpType, float> DefaultPowerUpWeights = new()
    {
        [PowerUpType.BallSpeedUp] = 80f,
        [PowerUpType.BallSpeedDown] = 120f,
        [PowerUpType.PaddleGrow] = 140f,
        [PowerUpType.PaddleShrink] = 100f,
        [PowerUpType.Sticky] = 160f,
        [PowerUpType.ThreeBalls] = 160f,
        [PowerUpType.BallGrow] = 120f,
        [PowerUpType.BallShrink] = 80f,
        [PowerUpType.MachineGun] = 80f,
        [PowerUpType.ExtraLife] = 20f,
        [PowerUpType.InvincibilityBar] = 60f,
        [PowerUpType.InvincibleBall] = 30f,
        [PowerUpType.Drunk] = 40f,
        [PowerUpType.Pineapple] = 10f,
        [PowerUpType.Banana] = 8f,
        [PowerUpType.Cherries] = 5f,
        [PowerUpType.Lemon] = 10f,
        [PowerUpType.Strawberry] = 10f,
        [PowerUpType.Raspberry] = 6f,
        [PowerUpType.Kiwi] = 8f,
        [PowerUpType.Apple] = 5f
    };

    private readonly GameplayState _state;
    private readonly RoundManager _roundManager;
    private readonly Random _random;
    private readonly int _gameAreaX;
    private readonly int _gameAreaY;
    private readonly int _gameAreaWidth;
    private readonly int _gameAreaHeight;
    private readonly float _powerUpSpawnChance;
    private readonly float _shotPowerUpSpawnChance;
    private readonly float _powerUpFallSpeed;
    private readonly int _minPaddleWidth;
    private readonly int _maxPaddleWidth;
    private readonly int _paddleWidthStep;
    private readonly int _minBallSize;
    private readonly int _maxBallSize;
    private readonly int _ballSizeStep;
    private readonly float _invincibilityBarDurationSeconds;
    private readonly int _invincibilityBarHeight;
    private readonly int _indestructibleBrickHitPoints;
    private readonly int _standardBrickScore;
    private readonly int _threeHitBrickTouchScore;
    private readonly int _indestructibleBrickTouchScore;
    private readonly int _threeHitBrickDestroyScore;
    private readonly int _indestructibleBrickDestroyScore;
    private readonly List<(PowerUpType Type, float Weight)> _powerUpWeights;
    private readonly float _powerUpWeightTotal;

    public GameplayCoordinator(
        GameplayState state,
        RoundManager roundManager,
        Random random,
        int gameAreaX,
        int gameAreaY,
        int gameAreaWidth,
        int gameAreaHeight,
        float powerUpSpawnChance,
        float shotPowerUpSpawnChance,
        float powerUpFallSpeed,
        int minPaddleWidth,
        int maxPaddleWidth,
        int paddleWidthStep,
        int minBallSize,
        int maxBallSize,
        int ballSizeStep,
        float invincibilityBarDurationSeconds,
        int invincibilityBarHeight,
        int indestructibleBrickHitPoints,
        int standardBrickScore,
        int threeHitBrickTouchScore,
        int indestructibleBrickTouchScore,
        int threeHitBrickDestroyScore,
        int indestructibleBrickDestroyScore,
        PowerUpTestSettings? testSettings)
    {
        _state = state;
        _roundManager = roundManager;
        _random = random;
        _gameAreaX = gameAreaX;
        _gameAreaY = gameAreaY;
        _gameAreaWidth = gameAreaWidth;
        _gameAreaHeight = gameAreaHeight;
        _powerUpSpawnChance = powerUpSpawnChance;
        _shotPowerUpSpawnChance = shotPowerUpSpawnChance;
        _powerUpFallSpeed = powerUpFallSpeed;
        _minPaddleWidth = minPaddleWidth;
        _maxPaddleWidth = maxPaddleWidth;
        _paddleWidthStep = paddleWidthStep;
        _minBallSize = minBallSize;
        _maxBallSize = maxBallSize;
        _ballSizeStep = ballSizeStep;
        _invincibilityBarDurationSeconds = invincibilityBarDurationSeconds;
        _invincibilityBarHeight = invincibilityBarHeight;
        _indestructibleBrickHitPoints = indestructibleBrickHitPoints;
        _standardBrickScore = standardBrickScore;
        _threeHitBrickTouchScore = threeHitBrickTouchScore;
        _indestructibleBrickTouchScore = indestructibleBrickTouchScore;
        _threeHitBrickDestroyScore = threeHitBrickDestroyScore;
        _indestructibleBrickDestroyScore = indestructibleBrickDestroyScore;
        _powerUpWeights = BuildPowerUpWeights(testSettings);
        float total = 0f;
        foreach ((PowerUpType _, float weight) in _powerUpWeights)
        {
            total += weight;
        }

        _powerUpWeightTotal = total;
    }

    public BallState HandleBrickCollisions(BallState ball, Action advanceToNextLevel, Action refreshHudTitle, Action<string, float> playSound)
    {
        if (_state.Bricks.Count == 0)
        {
            return ball;
        }

        Rectangle ballBounds = new Rectangle((int)ball.Position.X, (int)ball.Position.Y, _state.CurrentBallSize, _state.CurrentBallSize);
        List<int> collidedIndices = new();
        int leftCollisions = 0;
        int rightCollisions = 0;
        int topCollisions = 0;
        int bottomCollisions = 0;

        for (int i = 0; i < _state.Bricks.Count; i++)
        {
            BrickState brick = _state.Bricks[i];
            if (!ballBounds.Intersects(brick.Bounds))
            {
                continue;
            }

            collidedIndices.Add(i);
            if (!_state.InvincibleBalls)
            {
                if (ball.Position.Y < brick.Bounds.Top && ball.Position.Y + _state.CurrentBallSize > brick.Bounds.Top && ball.Velocity.Y > 0f)
                {
                    topCollisions++;
                }

                if (ball.Position.Y < brick.Bounds.Bottom && ball.Position.Y + _state.CurrentBallSize > brick.Bounds.Bottom && ball.Velocity.Y < 0f)
                {
                    bottomCollisions++;
                }

                if (ball.Position.X < brick.Bounds.Left && ball.Position.X + _state.CurrentBallSize > brick.Bounds.Left && ball.Velocity.X > 0f)
                {
                    leftCollisions++;
                }

                if (ball.Position.X < brick.Bounds.Right && ball.Position.X + _state.CurrentBallSize > brick.Bounds.Right && ball.Velocity.X < 0f)
                {
                    rightCollisions++;
                }
            }
        }

        if (collidedIndices.Count == 0)
        {
            return ball;
        }

        if (!_state.InvincibleBalls && collidedIndices.Count == 1)
        {
            AdjustSingleBrickCornerCollision(
                ball,
                _state.Bricks[collidedIndices[0]],
                ref leftCollisions,
                ref rightCollisions,
                ref topCollisions,
                ref bottomCollisions);
        }

        for (int i = collidedIndices.Count - 1; i >= 0; i--)
        {
            int brickIndex = collidedIndices[i];
            BrickState brick = _state.Bricks[brickIndex];
            BrickState updatedBrick = brick with { HitPoints = GetRemainingHitPointsAfterBallCollision(brick) };
            _state.Score += GetScoreForHit(updatedBrick.Type, updatedBrick.HitPoints);

            if (updatedBrick.HitPoints <= 0)
            {
                if (updatedBrick.Type != 9)
                {
                    TrySpawnPowerUp(updatedBrick.Bounds, fromShot: false);
                }

                _state.Bricks.RemoveAt(brickIndex);
            }
            else
            {
                _state.Bricks[brickIndex] = updatedBrick;
            }
        }

        Vector2 velocity = ball.Velocity;
        int brickHitCount = ball.BrickHitCount + collidedIndices.Count;
        playSound("Pong", 1f);

        if (!_state.InvincibleBalls)
        {
            ApplyBrickBounce(ref velocity, leftCollisions, rightCollisions, topCollisions, bottomCollisions);
        }

        if (brickHitCount >= BrickHitsPerSpeedIncrease)
        {
            brickHitCount = 0;
            velocity *= BrickHitSpeedUpMultiplier;
        }

        refreshHudTitle();

        if (CountRemainingDestructibleBricks() == 0)
        {
            advanceToNextLevel();
        }

        return ball with { Velocity = velocity, BrickHitCount = brickHitCount };
    }

    private int GetRemainingHitPointsAfterBallCollision(BrickState brick)
    {
        if (_state.InvincibleBalls)
        {
            return 0;
        }

        return brick.HitPoints - 1;
    }

    public void UpdateShots(float deltaSeconds, Action advanceToNextLevel, Action refreshHudTitle, Action<string, float> playSound)
    {
        if (_state.Shots.Count == 0)
        {
            return;
        }

        const float machineGunShotSpeed = 420f;

        for (int i = _state.Shots.Count - 1; i >= 0; i--)
        {
            ShotState shot = _state.Shots[i] with
            {
                Position = _state.Shots[i].Position + new Vector2(0f, -machineGunShotSpeed * deltaSeconds)
            };

            int levelBeforeUpdate = _state.CurrentLevel;
            int shotCountBeforeUpdate = _state.Shots.Count;

            if (shot.Position.Y + shot.Height < _gameAreaY)
            {
                _state.Shots.RemoveAt(i);
                continue;
            }

            bool consumed = false;
            Rectangle shotBounds = new Rectangle((int)shot.Position.X, (int)shot.Position.Y, shot.Width, shot.Height);
            for (int j = 0; j < _state.Bricks.Count; j++)
            {
                if (!shotBounds.Intersects(_state.Bricks[j].Bounds))
                {
                    continue;
                }

                BrickState brick = _state.Bricks[j];
                BrickState updatedBrick = brick with { HitPoints = brick.HitPoints - 1 };
                _state.Score += GetScoreForHit(updatedBrick.Type, updatedBrick.HitPoints);

                if (updatedBrick.HitPoints <= 0)
                {
                    if (updatedBrick.Type != 9)
                    {
                        TrySpawnPowerUp(updatedBrick.Bounds, fromShot: true);
                    }

                    _state.Bricks.RemoveAt(j);
                }
                else
                {
                    _state.Bricks[j] = updatedBrick;
                }

                playSound("Pong", 0.8f);
                if (CountRemainingDestructibleBricks() == 0)
                {
                    advanceToNextLevel();
                }

                if (levelBeforeUpdate != _state.CurrentLevel || shotCountBeforeUpdate != _state.Shots.Count)
                {
                    consumed = false;
                    break;
                }

                refreshHudTitle();
                consumed = true;
                break;
            }

            if (levelBeforeUpdate != _state.CurrentLevel || shotCountBeforeUpdate != _state.Shots.Count)
            {
                break;
            }

            if (consumed)
            {
                _state.Shots.RemoveAt(i);
            }
            else
            {
                _state.Shots[i] = shot;
            }
        }
    }

    public void UpdatePowerUps(float deltaSeconds, Rectangle paddleBounds, Vector2 paddlePosition, Action refreshHudTitle, Action<string, float> playSound)
    {
        if (_state.PowerUps.Count == 0)
        {
            return;
        }

        for (int i = _state.PowerUps.Count - 1; i >= 0; i--)
        {
            FallingPowerUpState powerUp = _state.PowerUps[i];
            powerUp = powerUp with { Position = powerUp.Position + (powerUp.Velocity * deltaSeconds) };

            Rectangle powerUpBounds = new Rectangle((int)powerUp.Position.X, (int)powerUp.Position.Y, powerUp.Width, powerUp.Height);
            if (powerUpBounds.Intersects(paddleBounds))
            {
                ApplyPowerUp(powerUp.Type, paddlePosition, powerUp.Position.X, paddlePosition.Y, playSound);
                _state.PowerUps.RemoveAt(i);
                refreshHudTitle();
                continue;
            }

            if (powerUp.Position.Y > _gameAreaY + _gameAreaHeight)
            {
                _state.PowerUps.RemoveAt(i);
                continue;
            }

            _state.PowerUps[i] = powerUp;
        }
    }

    public void UpdateFloatingScores(float deltaSeconds)
    {
        for (int i = _state.FloatingScores.Count - 1; i >= 0; i--)
        {
            FloatingScoreState score = _state.FloatingScores[i] with
            {
                Position = _state.FloatingScores[i].Position + new Vector2(0f, -FloatingScoreSpeed * deltaSeconds),
                RemainingSeconds = _state.FloatingScores[i].RemainingSeconds - deltaSeconds
            };

            if (score.RemainingSeconds <= 0f)
            {
                _state.FloatingScores.RemoveAt(i);
                continue;
            }

            _state.FloatingScores[i] = score;
        }
    }

    public void UpdateBalls(
        float deltaSeconds,
        Action advanceToNextLevel,
        Action refreshHudTitle,
        Action<string, float> playSound,
        Action onBallsDrained,
        Rectangle paddleBounds,
        Vector2 paddlePosition)
    {
        for (int i = _state.Balls.Count - 1; i >= 0; i--)
        {
            BallState ball = _state.Balls[i];
            int levelBeforeUpdate = _state.CurrentLevel;
            int ballCountBeforeUpdate = _state.Balls.Count;
            if (!ball.IsLaunched)
            {
                continue;
            }

            ball = ball with
            {
                PreviousPosition = ball.Position,
                Position = ball.Position + (ball.Velocity * deltaSeconds)
            };
            ball = HandleWallBounce(ball);
            ball = HandlePaddleBounce(ball, paddleBounds, paddlePosition, playSound);
            ball = HandleBrickCollisions(ball, advanceToNextLevel, refreshHudTitle, playSound);

            if (levelBeforeUpdate != _state.CurrentLevel || ballCountBeforeUpdate != _state.Balls.Count)
            {
                break;
            }

            if (ball.Position.Y >= _gameAreaY + _gameAreaHeight)
            {
                _state.Balls.RemoveAt(i);
                continue;
            }

            _state.Balls[i] = ball;
        }

        if (_state.Balls.Count == 0)
        {
            onBallsDrained();
        }
    }

    public void HandleMachineGunInput(
        bool fireRequested,
        Vector2 leftShotOrigin,
        Vector2 rightShotOrigin,
        int shotWidth,
        int shotHeight,
        Action<string, float> playSound)
    {
        if (!_state.MachineGunActive || !fireRequested || _state.MachineGunCooldownRemaining > 0f)
        {
            return;
        }

        _state.Shots.Add(new ShotState(leftShotOrigin, shotWidth, shotHeight));
        _state.Shots.Add(new ShotState(rightShotOrigin, shotWidth, shotHeight));
        _state.MachineGunCooldownRemaining = 0.18f;
        playSound("Laser", 0.35f);
    }

    public void ClampPaddleInsidePlayfield(ref Vector2 paddlePosition)
    {
        float minX = _gameAreaX;
        float maxX = _gameAreaX + _gameAreaWidth - _state.CurrentPaddleWidth;
        paddlePosition = new Vector2(MathHelper.Clamp(paddlePosition.X, minX, maxX), paddlePosition.Y);

        if (_roundManager.HasAnyStuckBall())
        {
            _roundManager.RepositionStuckBalls(paddlePosition);
        }
    }

    public void DuplicateBalls()
    {
        List<BallState> snapshot = new List<BallState>(_state.Balls);
        foreach (BallState ball in snapshot)
        {
            if (_state.Balls.Count >= MaxBalls)
            {
                break;
            }

            Vector2 baseVelocity = ball.IsLaunched ? ball.Velocity : new Vector2(InitialBallSpeed * 0.35f, -InitialBallSpeed);
            _state.Balls.Add(new BallState(ball.Position, ball.Position, new Vector2(-baseVelocity.X, baseVelocity.Y), ball.IsLaunched, ball.StuckPaddleOffset, 0));
            if (_state.Balls.Count >= MaxBalls)
            {
                break;
            }

            _state.Balls.Add(new BallState(ball.Position, ball.Position, new Vector2(baseVelocity.X, -baseVelocity.Y), ball.IsLaunched, ball.StuckPaddleOffset, 0));
        }
    }

    public static int GetInitialHitPoints(int brickType)
    {
        return brickType switch
        {
            6 => 3,
            9 => 20,
            _ => 1
        };
    }

    private void TrySpawnPowerUp(Rectangle brickBounds, bool fromShot)
    {
        float threshold = fromShot ? _shotPowerUpSpawnChance : _powerUpSpawnChance;
        if (_random.NextDouble() >= threshold)
        {
            return;
        }

        PowerUpType type = RollPowerUpType();
        (int width, int height) = GetPowerUpSize(type);
        float fallSpeed = GetPowerUpFallSpeed(type);
        Vector2 position = new Vector2(
            brickBounds.X + ((brickBounds.Width - width) / 2f),
            brickBounds.Y + ((brickBounds.Height - height) / 2f));

        _state.PowerUps.Add(new FallingPowerUpState(
            type,
            position,
            new Vector2(0f, fallSpeed),
            width,
            height));
    }

    private PowerUpType RollPowerUpType()
    {
        if (_powerUpWeights.Count == 0 || _powerUpWeightTotal <= 0f)
        {
            return PowerUpType.Apple;
        }

        float roll = (float)(_random.NextDouble() * _powerUpWeightTotal);
        float cumulative = 0f;
        foreach ((PowerUpType type, float weight) in _powerUpWeights)
        {
            cumulative += weight;
            if (roll < cumulative)
            {
                return type;
            }
        }

        return _powerUpWeights[^1].Type;
    }

    private void ApplyPowerUp(PowerUpType type, Vector2 paddlePosition, float powerUpX, float paddleY, Action<string, float> playSound)
    {
        if (_state.DrunkRemaining != 0f && type != PowerUpType.Drunk)
        {
            _state.DrunkRemaining = 0f;
        }

        switch (type)
        {
            case PowerUpType.PaddleGrow:
                ReleaseStickyBallsIfNeeded();
                _state.CurrentPaddleWidth = Math.Min(_maxPaddleWidth, _state.CurrentPaddleWidth + _paddleWidthStep);
                _state.MachineGunActive = false;
                ClampPaddleInsidePlayfield(ref paddlePosition);
                _state.LastPowerUpLabel = "Raquette +";
                playSound("Pop", 1f);
                break;

            case PowerUpType.PaddleShrink:
                ReleaseStickyBallsIfNeeded();
                _state.CurrentPaddleWidth = Math.Max(_minPaddleWidth, _state.CurrentPaddleWidth - _paddleWidthStep);
                _state.MachineGunActive = false;
                ClampPaddleInsidePlayfield(ref paddlePosition);
                _state.LastPowerUpLabel = "Raquette -";
                playSound("Pop", 1f);
                break;

            case PowerUpType.BallSpeedUp:
                for (int i = 0; i < _state.Balls.Count; i++)
                {
                    if (_state.Balls[i].IsLaunched)
                    {
                        _state.Balls[i] = _state.Balls[i] with { Velocity = _state.Balls[i].Velocity * PowerUpBallSpeedUpMultiplier };
                    }
                }
                _state.LastPowerUpLabel = "Balle +";
                playSound("Pop", 1f);
                break;

            case PowerUpType.BallSpeedDown:
                for (int i = 0; i < _state.Balls.Count; i++)
                {
                    if (_state.Balls[i].IsLaunched)
                    {
                        _state.Balls[i] = _state.Balls[i] with { Velocity = _state.Balls[i].Velocity * BallSpeedDownMultiplier };
                    }
                }
                _state.LastPowerUpLabel = "Balle -";
                playSound("Pop", 1f);
                break;

            case PowerUpType.ExtraLife:
                _state.Lives++;
                _state.LastPowerUpLabel = "Vie +";
                playSound("Woohoo", 1f);
                break;

            case PowerUpType.Sticky:
                _state.StickyMode = true;
                _state.MachineGunActive = false;
                _state.LastPowerUpLabel = "Colle";
                playSound("Pop", 1f);
                break;

            case PowerUpType.ThreeBalls:
                ReleaseStickyBallsIfNeeded();
                DuplicateBalls();
                _state.LastPowerUpLabel = "3 Balles";
                playSound("Pop", 1f);
                break;

            case PowerUpType.InvincibleBall:
                _state.InvincibleBalls = true;
                _state.LastPowerUpLabel = "Invincible";
                playSound("Pop", 1f);
                break;

            case PowerUpType.BallGrow:
                ReleaseStickyBallsIfNeeded();
                _state.CurrentBallSize = Math.Min(_maxBallSize, _state.CurrentBallSize + _ballSizeStep);
                _roundManager.RepositionStuckBalls(paddlePosition);
                _state.LastPowerUpLabel = "Taille Balle +";
                playSound("Pop", 1f);
                break;

            case PowerUpType.BallShrink:
                ReleaseStickyBallsIfNeeded();
                _state.CurrentBallSize = Math.Max(_minBallSize, _state.CurrentBallSize - _ballSizeStep);
                _roundManager.RepositionStuckBalls(paddlePosition);
                _state.LastPowerUpLabel = "Taille Balle -";
                playSound("Pop", 1f);
                break;

            case PowerUpType.MachineGun:
                ReleaseStickyBallsIfNeeded();
                _state.MachineGunActive = true;
                _state.StickyMode = false;
                _state.LastPowerUpLabel = "Mitraillette";
                playSound("Recharge", 1f);
                break;

            case PowerUpType.InvincibilityBar:
                _state.InvincibilityBarRemaining = _invincibilityBarDurationSeconds;
                _state.LastPowerUpLabel = "Barre";
                playSound("Pop", 1f);
                break;

            case PowerUpType.Drunk:
                _state.DrunkRemaining = -1f;
                _state.LastPowerUpLabel = "Hic";
                _state.Score += 100000;
                AddFloatingScore(100000, powerUpX, paddleY);
                playSound("Hiccup", 1f);
                break;

            case PowerUpType.Pineapple:
                _state.Score += 30000;
                _state.LastPowerUpLabel = "Ananas";
                AddFloatingScore(30000, powerUpX, paddleY);
                playSound("Crunch", 1f);
                break;

            case PowerUpType.Banana:
                _state.Score += 70000;
                _state.LastPowerUpLabel = "Banane";
                AddFloatingScore(70000, powerUpX, paddleY);
                playSound("Crunch", 1f);
                break;

            case PowerUpType.Cherries:
                _state.Score += 200000;
                _state.LastPowerUpLabel = "Cerises";
                AddFloatingScore(200000, powerUpX, paddleY);
                playSound("Crunch", 1f);
                break;

            case PowerUpType.Lemon:
                _state.Score += 50000;
                _state.LastPowerUpLabel = "Citron";
                AddFloatingScore(50000, powerUpX, paddleY);
                playSound("Crunch", 1f);
                break;

            case PowerUpType.Strawberry:
                _state.Score += 60000;
                _state.LastPowerUpLabel = "Fraise";
                AddFloatingScore(60000, powerUpX, paddleY);
                playSound("Crunch", 1f);
                break;

            case PowerUpType.Raspberry:
                _state.Score += 90000;
                _state.LastPowerUpLabel = "Framboise";
                AddFloatingScore(90000, powerUpX, paddleY);
                playSound("Crunch", 1f);
                break;

            case PowerUpType.Kiwi:
                _state.Score += 40000;
                _state.LastPowerUpLabel = "Kiwi";
                AddFloatingScore(40000, powerUpX, paddleY);
                playSound("Crunch", 1f);
                break;

            case PowerUpType.Apple:
                _state.Score += 120000;
                _state.LastPowerUpLabel = "Pomme";
                AddFloatingScore(120000, powerUpX, paddleY);
                playSound("Crunch", 1f);
                break;
        }
    }

    private void AddFloatingScore(int score, float x, float paddleY)
    {
        _state.FloatingScores.Add(new FloatingScoreState(
            score,
            new Vector2(x, paddleY),
            FloatingScoreDurationSeconds));
    }

    private int CountRemainingDestructibleBricks()
    {
        int count = 0;
        foreach (BrickState brick in _state.Bricks)
        {
            if (brick.Type != 9)
            {
                count++;
            }
        }

        return count;
    }

    private void ReleaseStickyBallsIfNeeded()
    {
        if (!_state.StickyMode)
        {
            return;
        }

        _state.StickyMode = false;
        _roundManager.ReleaseStuckBallsIfAny();
    }

    private int GetScoreForHit(int brickType, int remainingHitPoints)
    {
        if (remainingHitPoints <= 0)
        {
            return brickType switch
            {
                6 => _threeHitBrickDestroyScore,
                9 => _indestructibleBrickDestroyScore,
                _ => _standardBrickScore
            };
        }

        return brickType switch
        {
            6 => _threeHitBrickTouchScore,
            9 => _indestructibleBrickTouchScore,
            _ => 0
        };
    }

    private static (int Width, int Height) GetPowerUpSize(PowerUpType type)
    {
        return type switch
        {
            PowerUpType.Drunk => (18, 60),
            PowerUpType.Pineapple => (27, 60),
            PowerUpType.Banana => (32, 43),
            PowerUpType.Cherries => (40, 39),
            PowerUpType.Lemon => (40, 34),
            PowerUpType.Strawberry => (32, 43),
            PowerUpType.Raspberry => (32, 43),
            PowerUpType.Kiwi => (40, 42),
            PowerUpType.Apple => (40, 39),
            PowerUpType.ExtraLife => (40, 20),
            _ => (40, 20)
        };
    }

    private float GetPowerUpFallSpeed(PowerUpType type)
    {
        float multiplier = type switch
        {
            PowerUpType.Pineapple => 1.2f,
            PowerUpType.Banana => 1.7f,
            PowerUpType.Cherries => 2.5f,
            PowerUpType.Lemon => 1.3f,
            PowerUpType.Strawberry => 1.6f,
            PowerUpType.Raspberry => 1.8f,
            PowerUpType.Kiwi => 1.2f,
            PowerUpType.Apple => 2f,
            _ => 1f
        };

        return _powerUpFallSpeed * multiplier;
    }

    private static List<(PowerUpType Type, float Weight)> BuildPowerUpWeights(PowerUpTestSettings? testSettings)
    {
        Dictionary<string, float> multipliers =
            testSettings?.WeightMultipliers ?? new Dictionary<string, float>(System.StringComparer.OrdinalIgnoreCase);

        List<(PowerUpType Type, float Weight)> result = new();
        foreach ((PowerUpType type, float defaultWeight) in DefaultPowerUpWeights)
        {
            float multiplier = 1f;
            if (multipliers.TryGetValue(type.ToString(), out float configuredMultiplier))
            {
                multiplier = configuredMultiplier;
            }

            float weight = defaultWeight * multiplier;
            if (weight > 0f)
            {
                result.Add((type, weight));
            }
        }

        return result;
    }

    private void AdjustSingleBrickCornerCollision(
        BallState ball,
        BrickState brick,
        ref int leftCollisions,
        ref int rightCollisions,
        ref int topCollisions,
        ref int bottomCollisions)
    {
        int tolerance = Math.Max(1, _state.CurrentBallSize / 5);

        bool hitTop = topCollisions > 0;
        bool hitBottom = bottomCollisions > 0;
        bool hitLeft = leftCollisions > 0;
        bool hitRight = rightCollisions > 0;

        float ballLeft = ball.Position.X;
        float ballTop = ball.Position.Y;
        float ballRight = ballLeft + _state.CurrentBallSize;
        float ballBottom = ballTop + _state.CurrentBallSize;
        Rectangle bounds = brick.Bounds;

        if (hitTop && hitLeft)
        {
            int horizontalDelta = (int)(bounds.Left - ballLeft);
            int verticalDelta = (int)(bounds.Top - ballTop);
            ResolveCornerAmbiguity(horizontalDelta, verticalDelta, tolerance, ref topCollisions, ref leftCollisions);
        }
        else if (hitTop && hitRight)
        {
            int horizontalDelta = (int)(ballLeft - bounds.Right);
            int verticalDelta = (int)(bounds.Top - ballTop);
            ResolveCornerAmbiguity(horizontalDelta, verticalDelta, tolerance, ref topCollisions, ref rightCollisions);
        }
        else if (hitBottom && hitLeft)
        {
            int horizontalDelta = (int)(bounds.Left - ballLeft);
            int verticalDelta = (int)(ballBottom - bounds.Bottom);
            ResolveCornerAmbiguity(horizontalDelta, verticalDelta, tolerance, ref bottomCollisions, ref leftCollisions);
        }
        else if (hitBottom && hitRight)
        {
            int horizontalDelta = (int)(ballRight - bounds.Right);
            int verticalDelta = (int)(ballBottom - bounds.Bottom);
            ResolveCornerAmbiguity(horizontalDelta, verticalDelta, tolerance, ref bottomCollisions, ref rightCollisions);
        }
    }

    private static void ResolveCornerAmbiguity(int horizontalDelta, int verticalDelta, int tolerance, ref int verticalCollisions, ref int horizontalCollisions)
    {
        if (horizontalDelta - verticalDelta > tolerance)
        {
            verticalCollisions--;
        }
        else if (verticalDelta - horizontalDelta > tolerance)
        {
            horizontalCollisions--;
        }
    }

    private static void ApplyBrickBounce(ref Vector2 velocity, int leftCollisions, int rightCollisions, int topCollisions, int bottomCollisions)
    {
        if (leftCollisions > rightCollisions && leftCollisions > topCollisions && leftCollisions > bottomCollisions)
        {
            if (velocity.X > 0f)
            {
                velocity.X = -velocity.X;
            }
        }
        else if (rightCollisions > leftCollisions && rightCollisions > topCollisions && rightCollisions > bottomCollisions)
        {
            if (velocity.X < 0f)
            {
                velocity.X = -velocity.X;
            }
        }
        else if (topCollisions > leftCollisions && topCollisions > rightCollisions && topCollisions > bottomCollisions)
        {
            if (velocity.Y > 0f)
            {
                velocity.Y = -velocity.Y;
            }
        }
        else if (bottomCollisions > leftCollisions && bottomCollisions > rightCollisions && bottomCollisions > topCollisions)
        {
            if (velocity.Y < 0f)
            {
                velocity.Y = -velocity.Y;
            }
        }
        else if (leftCollisions == topCollisions && leftCollisions > rightCollisions && leftCollisions > bottomCollisions)
        {
            if (velocity.X > 0f)
            {
                velocity.X = -velocity.X;
            }

            if (velocity.Y > 0f)
            {
                velocity.Y = -velocity.Y;
            }
        }
        else if (leftCollisions == bottomCollisions && leftCollisions > rightCollisions && leftCollisions > topCollisions)
        {
            if (velocity.X > 0f)
            {
                velocity.X = -velocity.X;
            }

            if (velocity.Y < 0f)
            {
                velocity.Y = -velocity.Y;
            }
        }
        else if (rightCollisions == topCollisions && rightCollisions > leftCollisions && rightCollisions > bottomCollisions)
        {
            if (velocity.X < 0f)
            {
                velocity.X = -velocity.X;
            }

            if (velocity.Y > 0f)
            {
                velocity.Y = -velocity.Y;
            }
        }
        else if (rightCollisions == bottomCollisions && rightCollisions > leftCollisions && rightCollisions > topCollisions)
        {
            if (velocity.X < 0f)
            {
                velocity.X = -velocity.X;
            }

            if (velocity.Y < 0f)
            {
                velocity.Y = -velocity.Y;
            }
        }
    }

    private static float GetBallSpeed(BallState ball)
    {
        float speed = ball.Velocity.Length();
        return speed > 0f ? speed : InitialBallSpeed;
    }

    private BallState HandleWallBounce(BallState ball)
    {
        Vector2 position = ball.Position;
        Vector2 velocity = ball.Velocity;
        float protectionBarTop = _gameAreaY + _gameAreaHeight - _invincibilityBarHeight;

        if (position.X <= _gameAreaX)
        {
            position.X = _gameAreaX;
            velocity.X = Math.Abs(velocity.X);
        }
        else if (position.X + _state.CurrentBallSize >= _gameAreaX + _gameAreaWidth)
        {
            position.X = _gameAreaX + _gameAreaWidth - _state.CurrentBallSize;
            velocity.X = -Math.Abs(velocity.X);
        }

        if (position.Y <= _gameAreaY)
        {
            position.Y = _gameAreaY;
            velocity.Y = Math.Abs(velocity.Y);
        }
        else if (_state.InvincibilityBarRemaining > 0f && position.Y + _state.CurrentBallSize > protectionBarTop)
        {
            position.Y = protectionBarTop - _state.CurrentBallSize;
            velocity.Y = -Math.Abs(velocity.Y);
        }

        return ball with { Position = position, Velocity = velocity };
    }

    private BallState HandlePaddleBounce(BallState ball, Rectangle paddleBounds, Vector2 paddlePosition, Action<string, float> playSound)
    {
        Rectangle ballBounds = new Rectangle((int)ball.Position.X, (int)ball.Position.Y, _state.CurrentBallSize, _state.CurrentBallSize);
        if (!ballBounds.Intersects(paddleBounds))
        {
            return ball;
        }

        Vector2 position = ball.Position;
        Vector2 velocity = ball.Velocity;
        bool hitTop = ball.Position.Y + (_state.CurrentBallSize / 2f) < paddlePosition.Y + (paddleBounds.Height / 2f);
        bool invincibilityBarActive = _state.InvincibilityBarRemaining > 0f;
        float protectionBarTop = _gameAreaY + _gameAreaHeight - _invincibilityBarHeight;
        float gapBetweenPaddleAndBar = protectionBarTop - paddleBounds.Bottom;
        bool ballFitsBelowPaddle = _state.CurrentBallSize <= gapBetweenPaddleAndBar;

        if (!hitTop && invincibilityBarActive && !ballFitsBelowPaddle)
        {
            return ball;
        }

        if (_state.StickyMode
            && hitTop
            && velocity.Y > 0f
            && ball.Position.X > paddlePosition.X
            && ball.Position.X + _state.CurrentBallSize < paddlePosition.X + _state.CurrentPaddleWidth)
        {
            position.Y = paddlePosition.Y - _state.CurrentBallSize;
            playSound("Splat", 1f);
            return ball with
            {
                Position = position,
                Velocity = Vector2.Zero,
                IsLaunched = false,
                StuckPaddleOffset = ball.Position.X - paddlePosition.X
            };
        }

        playSound("Ping", 1f);

        if (hitTop)
        {
            position.Y = paddlePosition.Y - _state.CurrentBallSize;

            float paddleCenter = paddlePosition.X + (_state.CurrentPaddleWidth / 2f);
            float ballCenter = position.X + (_state.CurrentBallSize / 2f);
            float impact = MathHelper.Clamp((ballCenter - paddleCenter) / (_state.CurrentPaddleWidth / 2f), -1f, 1f);

            float speed = GetBallSpeed(ball);
            velocity.X = speed * impact;
            velocity.Y = -Math.Abs(speed * (1f - Math.Abs(impact) * 0.35f));
            return ball with { Position = position, Velocity = velocity, StuckPaddleOffset = ball.Position.X - paddlePosition.X };
        }

        if (ball.Position.X + (_state.CurrentBallSize / 2f) < paddlePosition.X)
        {
            position.X = paddlePosition.X - _state.CurrentBallSize;
            if (velocity.X > 0f)
            {
                velocity.X = -velocity.X;
            }
        }
        else if (ball.Position.X + (_state.CurrentBallSize / 2f) > paddlePosition.X + _state.CurrentPaddleWidth)
        {
            position.X = paddlePosition.X + _state.CurrentPaddleWidth;
            if (velocity.X < 0f)
            {
                velocity.X = -velocity.X;
            }
        }
        else
        {
            position.Y = paddleBounds.Bottom;
            if (velocity.Y < 0f)
            {
                velocity.Y = -velocity.Y;
            }
        }

        return ball with { Position = position, Velocity = velocity, StuckPaddleOffset = ball.Position.X - paddlePosition.X };
    }
}
