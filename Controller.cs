using Arcanod_SFML_HomeWork.Interfaces;
using Arcanod_SFML_HomeWork.Models;
using SFML.Audio;
using SFML.System;
using SFML.Window;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Arcanod_SFML_HomeWork
{
    public class ResourceList<T> : IEnumerable where T : class 
    {
        private Dictionary<string, object> list;

        public ResourceList(int amountResource)
        {
            list = new Dictionary<string, object>(amountResource);
        }

        public ResourceList()
        {
            list = new Dictionary<string, object>();
        }

        public T Get(string name)
        {
            if (!list.TryGetValue(name, out var value))
            {
                //Console.WriteLine("Resource with name {0} not found!", name);
                return (T)value;
            }

            return (T)value;
        }

        public T Add(string name, T resource)
        {
            if (list.TryGetValue(name, out var value))
            {
                return (T)value;
            }

            list.Add(name, resource);
            return resource;
        }

        public IEnumerator GetEnumerator()
        {
            return list.GetEnumerator();
        }
    }

    internal static class Controller
    {
        static Random s_random;
        static GameMode _lastGameMode;
        private static Queue<IGameObject> s_queueAddObjects = new Queue<IGameObject>();
        private static string _explosionSound;
        private static string _startScreenMusic;
        private static string _playGameMusic;
        private static string _endGameMusic;
        private static string _ballSound;
        private static string _crackedBlockSound;
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
            _lastGameMode = GameMode.StartScreen;
            View.InitializationGameModeSwitched();
            View.IsKeyPressed += View_KeyPressed;
            View.WindowLostFocus_Event += View_WindowLostFocus_Event;
            Hp = Settings.DefaultHp;
            _explosionSound = LoadSound(@"./res/Explosion.ogg");
            _ballSound = LoadSound(@"./res/BallSound.ogg");
            _crackedBlockSound = LoadSound(@"./res/CrackedBlock_Sound.ogg");

            _startScreenMusic = LoadMusic(@"./res/StartScreen_Music.ogg");
            _playGameMusic = LoadMusic(@"./res/PlayGame_Music.ogg");
            _endGameMusic = LoadMusic(@"./res/EndGame_Music.ogg");

            PlayMusic(_startScreenMusic, 45);
        }

        public static void InitializateController()
        {
            InitializeGameObjects();            
        }
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

            Ball mainBall = new Ball();
            mainBall.BallDropped += BallDroppedHandler;
            mainBall.IsCollision += BallCollisionHandler;

            s_GameObjects.AddLast(mainBall);
            s_GameObjects.AddLast(new Platform());

            Blocks blocks = CreateBlocksObject();
            blocks.BlocksAreOver += Blocks_BlocksAreOver;
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
                CheckEvents();
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
                    case (GameMode.Pause):
                        View.DrawPauseScreen();
                        break;
                }

                // Display window
                View.Display();
            }
        }     

        private static void CheckEvents()
        {
            

        }

        public static void CheckGameModeSwitched()
        {
            if (Settings.GameMode != _lastGameMode)
            {
                if (_lastGameMode == GameMode.EndGame)
                    RestartGame();

                if (Settings.GameMode == GameMode.Play)
                {
                    InitializateController();
                    foreach (KeyValuePair<string, object> music in Musics)
                        (music.Value as Music).Stop();
                    //StopMusic(_startScreenMusic);
                    PlayMusic(_playGameMusic, 45);                    
                }
                else if (Settings.GameMode == GameMode.EndGame)
                {
                    InitializateController();
                    foreach (KeyValuePair<string, object> music in Musics)
                        (music.Value as Music).Stop();
                    //StopMusic(_playGameMusic);
                    PlayMusic(_endGameMusic, 45);
                }
                
                View.InitializationGameModeSwitched();
                _lastGameMode = Settings.GameMode;
            }
        }
        public static void PlayGameActions()
        {
            // Checking queue
            while (s_queueAddObjects.Count() != 0)
            {
                s_GameObjects.AddFirst(s_queueAddObjects.Dequeue());
            }

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

            // Remove destructed objects
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

            // Clear window
            View.Clear();
            View.DrawBackground();

            // Draw objects
            foreach (IGameObject gameObject in s_GameObjects)
                if (gameObject is IDrawable)
                    ((IDrawable)gameObject).Draw();

            View.DisplayStats();
        }

        public static void StartScreenActions() => ChoosePlayOrExitActions();

        public static void EndGameActions() => ChoosePlayOrExitActions();
        public static void ChoosePlayOrExitActions()
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
                    PlaySound(_ballSound, 25);

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
                    PlaySound(_crackedBlockSound, 45);
                else
                    PlaySound(_explosionSound, 85);

                // if block is in collision with platform or below - it's EndGame
                if (e is CollisionEventArgs &&
                    ((CollisionEventArgs)e).EncounteredObject is Platform || ((CollisionEventArgs)e).EncounteredObject is BorderUnderPlatform)
                    Settings.GameMode = GameMode.EndGame;
            }
        }

        private static void Blocks_BlocksAreOver(object sender, EventArgs e)
        {
            if (sender is Blocks && Settings.GameMode == GameMode.Play)
            {
                LevelNumber++;
                LevelNumber = LevelNumber > 5 ? 1 : LevelNumber;
                Settings.GameMode = GameMode.ShowingLevelNumber;
                CheckGameModeSwitched();
            }
                
        }
        private static void View_KeyPressed(object sender, KeyEventArgs e)
        {
            if (e.Code == Keyboard.Key.Escape)
            {
                if (Settings.GameMode == GameMode.Play)
                    Settings.GameMode = GameMode.Pause;
                else if (Settings.GameMode == GameMode.Pause)
                    Settings.GameMode = GameMode.Play;

                _lastGameMode = Settings.GameMode;
            }            
        }
        private static void View_WindowLostFocus_Event(object sender, EventArgs e)
        {
            if (Settings.GameMode == GameMode.Play)
                Settings.GameMode = _lastGameMode = GameMode.Pause;            
        }

        public static void AddExplosiveBall(float xPos, float yPos, float width, float height)
        {
            IGameObject explosiveBall = new ExplosiveObject(new Vector2f(xPos, yPos), width, height);
            s_queueAddObjects.Enqueue(explosiveBall);            
        }

        public static string LoadSound(string path)
        {
            SoundBuffer buffer = SoundsBuffers.Add(path, new SoundBuffer(path));
            Sounds.Add(path, new Sound(buffer));
            return path;
        }

        public static void PlaySound(string name)
        {
            Sound sound = Sounds.Get(name);
            if (sound.Status != SoundStatus.Playing)
            {
                sound.Play();
            }
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

        public static void PlayMusic(string name)
        {
            Music music = Musics.Get(name);
            if (music.Status != SoundStatus.Playing)
            {
                music.Loop = true;
                music.Play();
            }
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

        public static void StopMusic(string name)
        {
            Music music = Musics.Get(name);

            if (music.Status == SoundStatus.Playing)
                music.Stop();
        }
    }
}
