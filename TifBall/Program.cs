using System;
using System.IO;

try
{
    using var game = new TifBall.TifBallGame();
    game.Run();
}
catch (Exception ex)
{
    string logPath = Path.Combine(AppContext.BaseDirectory, "monogame-host-error.log");
    File.WriteAllText(logPath, ex.ToString());
    throw;
}
