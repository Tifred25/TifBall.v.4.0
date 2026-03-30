using System.Globalization;

namespace TifBall;

internal readonly record struct HudState(
    int Level,
    int Score,
    int Lives,
    int Framerate,
    bool IsGameOver,
    bool IsAwaitingStart,
    bool IsLevelTransitioning,
    float LevelTransitionY,
    bool IsPaused,
    bool IsShowingHighScores,
    bool IsEnteringHighScore,
    bool IsShowingPresentation,
    string? PowerUpLabel,
    bool BackgroundMusicPlaying,
    string AudioStatus,
    bool MachineGunActive,
    bool InvincibilityBarActive)
{
    public string BuildWindowTitle(string baseTitle)
    {
        string title = string.Format(
            CultureInfo.InvariantCulture,
            "{0} - {1} fps - Level {2} - Score {3} - Vies {4}",
            baseTitle,
            Framerate,
            Level,
            Score,
            Lives);

        if (IsEnteringHighScore)
        {
            title += " - NOUVEAU HIGH SCORE";
        }
        else if (IsShowingPresentation)
        {
            title += " - PRESENTATION";
        }
        else if (IsGameOver)
        {
            title += " - GAME OVER - Entree ou clic pour recommencer";
        }
        else if (IsShowingHighScores)
        {
            title += " - HIGHSCORES";
        }
        else if (IsAwaitingStart)
        {
            title += " - CLIC POUR COMMENCER";
        }
        else if (IsPaused)
        {
            title += " - PAUSE - P POUR REPRENDRE";
        }

        if (!string.IsNullOrEmpty(PowerUpLabel))
        {
            title += " - Bonus " + PowerUpLabel;
        }

        if (BackgroundMusicPlaying)
        {
            title += " - Musique";
        }
        else
        {
            title += " - Audio " + AudioStatus;
        }

        if (MachineGunActive)
        {
            title += " - MG";
        }

        if (InvincibilityBarActive)
        {
            title += " - Barre";
        }

        return title;
    }

    public string? BuildCenterMessage()
    {
        if (IsEnteringHighScore)
        {
            return null;
        }

        if (IsShowingPresentation)
        {
            return null;
        }

        if (IsGameOver)
        {
            return "PARTIE TERMINEE";
        }

        if (IsShowingHighScores)
        {
            return null;
        }

        if (IsLevelTransitioning)
        {
            return null;
        }

        if (IsAwaitingStart)
        {
            return "CLIC POUR COMMENCER";
        }

        if (IsPaused)
        {
            return "PAUSE";
        }

        return null;
    }
}
