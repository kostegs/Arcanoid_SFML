
namespace Arcanoid_SFML
{
    public enum GameMode : byte
    {
        StartScreen,
        Play,
        ShowingLevelNumber,
        Pause,
        WinGame,
        EndGame
    }
    public struct Settings
    {
        public static int BallSpeed = 7;
        public static int DefaultHp = 3;
        public static GameMode GameMode;
    }
}
