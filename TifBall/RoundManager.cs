using Microsoft.Xna.Framework;
using System;

namespace TifBall;

internal sealed class RoundManager
{
    private readonly GameplayState _state;
    private readonly int _initialLives;
    private readonly int _defaultPaddleWidth;
    private readonly int _defaultBallSize;
    private readonly float _initialBallSpeed;
    private readonly int _gameAreaX;
    private readonly int _gameAreaY;
    private readonly int _gameAreaWidth;
    private readonly int _paddleY;
    private readonly Action _resetLevelTransition;
    private readonly float _initialBallSpeedMultiplier;

    public RoundManager(
        GameplayState state,
        int initialLives,
        int defaultPaddleWidth,
        int defaultBallSize,
        float initialBallSpeed,
        float initialBallSpeedMultiplier,
        int gameAreaX,
        int gameAreaY,
        int gameAreaWidth,
        int paddleY,
        Action resetLevelTransition)
    {
        _state = state;
        _initialLives = initialLives;
        _defaultPaddleWidth = defaultPaddleWidth;
        _defaultBallSize = defaultBallSize;
        _initialBallSpeed = initialBallSpeed;
        _initialBallSpeedMultiplier = initialBallSpeedMultiplier;
        _gameAreaX = gameAreaX;
        _gameAreaY = gameAreaY;
        _gameAreaWidth = gameAreaWidth;
        _paddleY = paddleY;
        _resetLevelTransition = resetLevelTransition;
    }

    public void InitializeLevel(int levelNumber, ref Vector2 paddlePosition)
    {
        _state.CurrentLevel = levelNumber;
        _state.IsGameOver = false;
        _state.IsAwaitingStart = true;
        _state.IsLevelTransitioning = true;
        _state.LevelTransitionY = -100f;
        _resetLevelTransition();
        _state.IsPaused = false;
        _state.ResetTransientRoundState(_defaultPaddleWidth, _defaultBallSize);
        CenterPaddle(ref paddlePosition);
        ResetBallsOnPaddle(paddlePosition);
    }

    public void HandleBallDrain(ref Vector2 paddlePosition)
    {
        if (_state.Lives == 0)
        {
            _state.IsGameOver = true;
            _state.IsPaused = false;
            _state.LastPowerUpLabel = "Game Over";
            return;
        }

        _state.Lives--;
        _state.IsAwaitingStart = true;
        _state.IsPaused = false;
        _state.ResetTransientRoundState(_defaultPaddleWidth, _defaultBallSize);
        CenterPaddle(ref paddlePosition);
        ResetBallsOnPaddle(paddlePosition);
    }

    public void RestartGame(ref Vector2 paddlePosition)
    {
        _state.Score = 0;
        _state.Lives = _initialLives;
        _state.CurrentLevel = 1;
        _state.LastPowerUpLabel = null;
        _state.IsGameOver = false;
        _state.IsAwaitingStart = true;
        _state.IsPaused = false;
        InitializeLevel(1, ref paddlePosition);
    }

    public void CenterPaddle(ref Vector2 paddlePosition)
    {
        paddlePosition = new Vector2(_gameAreaX + ((_gameAreaWidth - _state.CurrentPaddleWidth) / 2f), _gameAreaY + _paddleY);
    }

    public bool HasAnyStuckBall()
    {
        foreach (BallState ball in _state.Balls)
        {
            if (!ball.IsLaunched)
            {
                return true;
            }
        }

        return false;
    }

    public void RepositionStuckBalls(Vector2 paddlePosition)
    {
        for (int i = 0; i < _state.Balls.Count; i++)
        {
            if (!_state.Balls[i].IsLaunched)
            {
                _state.Balls[i] = _state.Balls[i] with
                {
                    PreviousPosition = new Vector2(
                        paddlePosition.X + _state.Balls[i].StuckPaddleOffset,
                        _gameAreaY + _paddleY - _state.CurrentBallSize),
                    Position = new Vector2(
                        paddlePosition.X + _state.Balls[i].StuckPaddleOffset,
                        _gameAreaY + _paddleY - _state.CurrentBallSize)
                };
            }
        }
    }

    public void LaunchStuckBalls()
    {
        for (int i = 0; i < _state.Balls.Count; i++)
        {
            if (!_state.Balls[i].IsLaunched)
            {
                BallState ball = _state.Balls[i];
                float paddleX = ball.Position.X - ball.StuckPaddleOffset;
                float paddleCenter = paddleX + (_state.CurrentPaddleWidth / 2f);
                float ballCenter = ball.Position.X + (_state.CurrentBallSize / 2f);
                float impact = MathHelper.Clamp((ballCenter - paddleCenter) / (_state.CurrentPaddleWidth / 2f), -1f, 1f);
                float launchSpeed = _initialBallSpeed * _initialBallSpeedMultiplier;
                _state.Balls[i] = _state.Balls[i] with
                {
                    PreviousPosition = ball.Position,
                    IsLaunched = true,
                    Velocity = new Vector2(
                        launchSpeed * impact,
                        -Math.Abs(launchSpeed * (1f - Math.Abs(impact) * 0.35f)))
                };
            }
        }
    }

    public void ReleaseStuckBallsIfAny()
    {
        if (HasAnyStuckBall())
        {
            LaunchStuckBalls();
        }
    }

    private void ResetBallsOnPaddle(Vector2 paddlePosition)
    {
        _state.Balls.Clear();
        float initialOffset = _state.CurrentPaddleWidth / 4f;
        Vector2 initialPosition = CreateInitialBallPosition(paddlePosition, initialOffset);
        _state.Balls.Add(new BallState(initialPosition, initialPosition, Vector2.Zero, false, initialOffset, 0));
    }

    private Vector2 CreateInitialBallPosition(Vector2 paddlePosition, float paddleOffset)
    {
        return new Vector2(
            paddlePosition.X + paddleOffset,
            _gameAreaY + _paddleY - _state.CurrentBallSize);
    }
}
