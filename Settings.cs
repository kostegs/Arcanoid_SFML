
namespace Arcanod_SFML_HomeWork
{
    public enum GameMode : byte
    {
        StartScreen,
        Play,
        ShowingLevelNumber,
        EndLevel,
        EndGame
    }
    public struct Settings
    {
        public static int BallSpeed = 7;
        public static int DefaultHp = 3;
        public static GameMode GameMode;
    }
}
