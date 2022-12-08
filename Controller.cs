using Arcanoid_SFML.Interfaces;
using Arcanoid_SFML.Models;
using SFML.Audio;
using SFML.System;
using SFML.Window;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Arcanoid_SFML
{
    internal class ResourceList<T> : IEnumerable where T : class 
    {
        private Dictionary<string, object> _list;

        internal ResourceList(int amountResource)
        {
            _list = new Dictionary<string, object>(amountResource);
        }

        internal ResourceList()
        {
            _list = new Dictionary<string, object>();
        }

        internal T Get(string name)
        {
            if (!_list.TryGetValue(name, out var value))
            {
                //Console.WriteLine("Resource with name {0} not found!", name);
                return (T)value;
            }

            return (T)value;
        }

        internal T Add(string name, T resource)
        {
            if (_list.TryGetValue(name, out var value))
            {
                return (T)value;
            }

            _list.Add(name, resource);
            return resource;
        }

        public IEnumerator GetEnumerator() => _list.GetEnumerator();        
    }

    internal static class Controller
    {
        private static Random s_random;
        private static GameMode s_lastGameMode;
        private static Queue<IGameObject> s_queueAddObjects = new Queue<IGameObject>();
        private static string s_explosionSound;
        private static string s_startScreenMusic;
        private static string s_playGameMusic;
        private static string s_endGameMusic;
        private static string s_winGameMusic;
        private static string s_ballSound;
        private static string s_crackedBlockSound;

        public static ResourceList<Sound> Sounds { get; set; } = new ResourceList<Sound>(10);
        public static ResourceList<Music> Musics { get; set; } = new ResourceList<Music>(10);
        public static ResourceList<SoundBuffer> SoundsBuffers { get; set; } = new ResourceList<SoundBuffer>(10);
        public static View View { get; private set; } = new View();
        public static LinkedList<IGameObject> s_GameObjects { get; private set; }
        public static int Hp { get; private set; }
        public static int LevelNumber { get; private set; } = 1;

        public static void GlobalInitialization()
        {
            View.Initialize();
            Settings.GameMode = GameMode.StartScreen;
            InitializateController();
            s_random = new Random();
            s_lastGameMode = GameMode.StartScreen;
            View.InitializationGameModeSwitched();
            View.IsKeyPressed += View_KeyPressedHandler;
            View.WindowLostFocus_Event += View_WindowLostFocusHandler;
            Hp = Settings.DefaultHp;

            s_explosionSound = LoadSound(@"./res/Explosion.ogg");
            s_ballSound = LoadSound(@"./res/BallSound.ogg");
            s_crackedBlockSound = LoadSound(@"./res/CrackedBlock_Sound.ogg");

            s_startScreenMusic = LoadMusic(@"./res/StartScreen_Music.ogg");
            s_playGameMusic = LoadMusic(@"./res/PlayGame_Music.ogg");
            s_endGameMusic = LoadMusic(@"./res/EndGame_Music.ogg");
            s_winGameMusic = LoadMusic(@"./res/WinGame_Music.ogg");

            PlayMusic(s_startScreenMusic, 45);
        }

        public static void InitializateController() => InitializeGameObjects();

        public static void RestartGame()
        {
            Hp = Settings.DefaultHp;
            Settings.GameMode = GameMode.ShowingLevelNumber;
            LevelNumber = 1;
            View.InitializationGameModeSwitched();            
        }

        public static int RandomNumber(int minBound, int maxBound) => s_random.Next(minBound, maxBound);
        
        private static void InitializeGameObjects()
        {
            s_GameObjects = new LinkedList<IGameObject>();

            // Ball
            Ball mainBall = new Ball();
            mainBall.BallDropped += BallDroppedHandler;
            mainBall.IsCollision += BallCollisionHandler;
            s_GameObjects.AddLast(mainBall);

            // Platform
            s_GameObjects.AddLast(new Platform());

            // Blocks
            Blocks blocks = CreateBlocksObject();
            blocks.BlocksAreOver += BlocksAreOverHandler;
            s_GameObjects.AddLast(blocks);

            // Left Border
            s_GameObjects.AddLast(new SideBorder(-5, 0, 5, 600));
            // Top Border
            s_GameObjects.AddLast(new TopBorder(0, -5, 800, 5));
            // Right Border
            s_GameObjects.AddLast(new SideBorder(800, 0, 5, 600));
            // Bottom Border
            s_GameObjects.AddLast(new BottomBorder(0, 600, 800, 5));
            // Border under platform for checking collision with blocks only
            s_GameObjects.AddLast(new BorderUnderPlatform(0, 520, 800, 80));
        }

        private static Blocks CreateBlocksObject()
        {
            switch (Settings.GameMode)
            {
                case GameMode.Play:
                    return CreateBlocksObject_PlayMode();                    
                case GameMode.StartScreen:
                case GameMode.EndGame:
                case GameMode.WinGame:
                    return CreateBlocksObject_StartScreenMode();                    
                default:
                    return CreateBlocksObject_PlayMode();
            }
        }

        private static Blocks CreateBlocksObject_PlayMode()
        {
            int countOfBlocks = 25;
            int numberOfColumns = 5;
            
            switch (LevelNumber)
            {
                case 1:
                    countOfBlocks = 25;
                    numberOfColumns = 5;
                    break;
                case 2:                    
                    countOfBlocks = 50;
                    numberOfColumns = 10;
                    break;
                case 3:
                    countOfBlocks = 80;
                    numberOfColumns = 8;
                    break;
                case 4:
                case 5:
                    countOfBlocks = 100;
                    numberOfColumns = 10;
                    break;
                default:
                    break;
            }

            Blocks blocks = new Blocks(countOfBlocks, numberOfColumns);
            blocks.IsCollision += BlockCollisionHandler;
            return blocks;
        }

        private static Blocks CreateBlocksObject_StartScreenMode()
        {
            Blocks blocks = new Blocks(3, 2);
            blocks.IsCollision += BlockCollisionHandler;
            return blocks;            
        }

        public static void Play()
        {
            while (View.IsOpen)
            {
                View.DispatchEvents();                
                CheckGameModeSwitched();

                switch (Settings.GameMode)
                {
                    case (GameMode.StartScreen):
                        StartScreenActions();
                        break;
                    case (GameMode.ShowingLevelNumber):
                        ShowLevelNumber();
                        break;
                    case (GameMode.Play):
                        PlayGameActions();
                        break;                        
                    case (GameMode.EndGame):
                        EndGameActions();
                        break;
                    case (GameMode.WinGame):
                        WinGameActions();
                        break;
                    case (GameMode.Pause):
                        View.DrawPauseScreen();
                        break;
                }
                
                View.Display();
            }
        }     

        public static void CheckGameModeSwitched()
        {
            if (Settings.GameMode != s_lastGameMode)
            {
                if (s_lastGameMode == GameMode.EndGame || s_lastGameMode == GameMode.WinGame)
                    RestartGame();

                if (Settings.GameMode == GameMode.Play)
                {
                    InitializateController();
                    foreach (KeyValuePair<string, object> music in Musics)
                        (music.Value as Music).Stop();
                    //StopMusic(_startScreenMusic);
                    PlayMusic(s_playGameMusic, 45);                    
                }
                else if (Settings.GameMode == GameMode.EndGame)
                {
                    InitializateController();
                    StopAnyMusic();
                    PlayMusic(s_endGameMusic, 45);
                }
                else if (Settings.GameMode == GameMode.WinGame)
                {
                    InitializateController();
                    StopAnyMusic();
                    PlayMusic(s_winGameMusic, 40);
                }

                View.InitializationGameModeSwitched();
                s_lastGameMode = Settings.GameMode;
            }
        }
       
        public static void PlayGameActions()
        {
            // Checking queue.
            while (s_queueAddObjects.Count() != 0)
            {
                s_GameObjects.AddFirst(s_queueAddObjects.Dequeue());
            }

            // Checking interact.
            foreach (IGameObject gameObject in s_GameObjects)
                if (gameObject is IInteractive)
                    ((IInteractive)gameObject).Interact();

            // Moving objects.
            foreach (IGameObject gameObject in s_GameObjects)
                if (gameObject is IMovable)
                    ((IMovable)gameObject).Move();

            // Checking Collision.
            foreach (IGameObject gameObject in s_GameObjects)            
                if (gameObject is IColliding)                
                    foreach (IGameObject anotherObject in s_GameObjects)                    
                        if ((anotherObject is IColliding) && (anotherObject != gameObject))
                            ((IColliding)gameObject).CheckCollision((IColliding)anotherObject);

            // Remove destructed objects.
            var currentNode = s_GameObjects.First;
            LinkedListNode<IGameObject> lastNode = null;

            while (currentNode != null)
            {
                IGameObject currentListElement= currentNode.Value;
                lastNode = currentNode;
                currentNode = currentNode.Next;

                if (currentListElement is IDestroyable && ((IDestroyable)currentListElement).AllowToDestroy)
                {
                    currentListElement = null;
                    s_GameObjects.Remove(lastNode);
                }
            }

            // Clear window.
            View.Clear();

            // Draw background picture.
            View.DrawBackground();

            // Draw objects.
            foreach (IGameObject gameObject in s_GameObjects)
                if (gameObject is IDrawable)
                    ((IDrawable)gameObject).Draw();

            // Show count of lives.
            View.DisplayStats();
        }

        public static void StartScreenActions() => SelectPlayOrExitAction();

        public static void EndGameActions() => SelectPlayOrExitAction();

        public static void WinGameActions() => SelectPlayOrExitAction();

        public static void SelectPlayOrExitAction()
        {
            // Checking interact
            foreach (IGameObject gameObject in s_GameObjects)
                if (gameObject is IInteractive)
                    ((IInteractive)gameObject).Interact();

            // Moving objects
            foreach (IGameObject gameObject in s_GameObjects)
                if (gameObject is IMovable)
                    ((IMovable)gameObject).Move();

            // Checking Collision
            foreach (IGameObject gameObject in s_GameObjects)
                if (gameObject is IColliding)
                    foreach (IGameObject anotherObject in s_GameObjects)
                        if ((anotherObject is IColliding) && (anotherObject != gameObject))
                            ((IColliding)gameObject).CheckCollision((IColliding)anotherObject);

            // Clear window
            View.Clear();
            View.DrawBackground();

            // Draw objects
            foreach (IGameObject gameObject in s_GameObjects)
                if (gameObject is IDrawable)
                    ((IDrawable)gameObject).Draw();
        }
        
        public static void ShowLevelNumber()
        {
            View.Clear();
            View.DrawLevelNumber();
            Settings.GameMode = GameMode.Play;
            CheckGameModeSwitched();
        }

        public static void AddExplosiveBall(float xPos, float yPos, float width, float height)
        {
            IGameObject explosiveBall = new ExplosiveObject(new Vector2f(xPos, yPos), width, height);
            s_queueAddObjects.Enqueue(explosiveBall);
        }

        public static void BallDroppedHandler(object sender, EventArgs e)
        {
            Hp--;

            if (Hp == 0)
                Settings.GameMode = GameMode.EndGame;
            
            if (sender is Ball)
            {
                Ball ball = sender as Ball;
                ball.SetStartPosition();                
            }            
        }

        public static void BallCollisionHandler(object sender, EventArgs e)
        {
            if ((sender is Ball) &&
                (e is CollisionEventArgs) &&
                ((CollisionEventArgs)e).EncounteredObject is Platform)
                    PlaySound(s_ballSound, 25);
        }

        private static void BlockCollisionHandler(object sender, EventArgs e)
        {
            if (sender is PlayBlock)
            {
                Settings.GameMode = GameMode.ShowingLevelNumber;
                CheckGameModeSwitched();
            }
            else if (sender is ExitBlock)
            {
                Environment.Exit(0);
            }
            else
            {
                if (sender is GlassBlock || sender is HardGlassBlock)
                {
                    if (((Block)sender).IsDestroyMode)
                        PlaySound(s_explosionSound, 85);
                    else
                        PlaySound(s_crackedBlockSound, 45);
                }                    
                else
                    PlaySound(s_explosionSound, 85);

                // if block is in collision with platform or below - it's EndGame
                if (e is CollisionEventArgs &&
                    ((CollisionEventArgs)e).EncounteredObject is Platform || ((CollisionEventArgs)e).EncounteredObject is BorderUnderPlatform)
                    Settings.GameMode = GameMode.EndGame;
            }
        }

        private static void BlocksAreOverHandler(object sender, EventArgs e)
        {
            if (sender is Blocks && Settings.GameMode == GameMode.Play)
            {
                LevelNumber++;

                if (LevelNumber > 5)
                    Settings.GameMode = GameMode.WinGame;
                else                
                    Settings.GameMode = GameMode.ShowingLevelNumber;

                CheckGameModeSwitched();
            }
                
        }
        
        private static void View_KeyPressedHandler(object sender, KeyEventArgs e)
        {
            if (e.Code == Keyboard.Key.Escape)
            {
                if (Settings.GameMode == GameMode.Play)
                    Settings.GameMode = GameMode.Pause;
                else if (Settings.GameMode == GameMode.Pause)
                    Settings.GameMode = GameMode.Play;

                s_lastGameMode = Settings.GameMode;
            }            
        }
        
        private static void View_WindowLostFocusHandler(object sender, EventArgs e)
        {
            if (Settings.GameMode == GameMode.Play)
                Settings.GameMode = s_lastGameMode = GameMode.Pause;            
        }

        public static string LoadSound(string path)
        {
            SoundBuffer buffer = SoundsBuffers.Add(path, new SoundBuffer(path));
            Sounds.Add(path, new Sound(buffer));
            return path;
        }

        public static void PlaySound(string name, float volume)
        {
            Sound sound = Sounds.Get(name);
            sound.Volume = volume;
            if (sound.Status == SoundStatus.Playing)
                sound.Stop();
            
            sound.Play();            
        }
        
        public static string LoadMusic(string path)
        {
            Musics.Add(path, new Music(path));
            return path;
        }

        public static void PlayMusic(string name, float volume)
        {
            Music music = Musics.Get(name);
            music.Volume = volume;            
            if (music.Status != SoundStatus.Playing)
            {
                music.Loop = true;
                music.Play();
            }
        }

        public static void StopAnyMusic()
        {
            foreach (KeyValuePair<string, object> music in Musics)
                (music.Value as Music).Stop();
        }
    }
}
