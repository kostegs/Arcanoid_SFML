using Arcanod_SFML_HomeWork.Interfaces;
using Arcanod_SFML_HomeWork.Models;
using SFML.System;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcanod_SFML_HomeWork
{
    internal static class Controller
    {
        static Random s_random;
        static GameMode _lastGameMode;
        private static Queue<IGameObject> s_queueAddObjects = new Queue<IGameObject>();        
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
        }
        public static void InitializateController()
        {
            InitializeGameObjects();
            Hp = Settings.DefaultHp;            
        }
        public static int RandomNumber(int minBound, int maxBound) => s_random.Next(minBound, maxBound);
        
        private static void InitializeGameObjects()
        {
            s_GameObjects = new LinkedList<IGameObject>();

            Ball mainBall = new Ball();
            mainBall.BallDropped += BallDroppedHandler;

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
        }

        private static Blocks CreateBlocksObject()
        {
            switch (Settings.GameMode)
            {
                case GameMode.Play:
                    return CreateBlocksObject_PlayMode();                    
                case GameMode.StartScreen:
                    return CreateBlocksObject_StartScreenMode();                    
                default:
                    return CreateBlocksObject_PlayMode();
            }
        }

        private static Blocks CreateBlocksObject_PlayMode()
        {
            switch (LevelNumber)
            {
                case 1:
                    return new Blocks(25, 5);                    
                case 2:
                    return new Blocks(50, 10);                    
                case 3:
                    return new Blocks(80, 8);                    
                case 4:
                case 5:
                    return new Blocks(100, 10);                    
                default:
                    return new Blocks(25, 5);                    
            }
        }

        private static Blocks CreateBlocksObject_StartScreenMode()
        {
            Blocks blocks = new Blocks(3, 2);
            blocks.IsCollision += ButtonBlock_Collision;
            return blocks;            
        }

        private static void ButtonBlock_Collision(object sender, EventArgs e)
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
                }

                // Display window
                View.Display();
            }
        }     

        public static void CheckGameModeSwitched()
        {
            if (Settings.GameMode != _lastGameMode)
            {
                if (Settings.GameMode == GameMode.Play)                
                    InitializateController();                
                
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

        public static void StartScreenActions()
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

        public static void EndGameActions()
        {
            View.Clear();
            View.DrawEndGameWIndow();
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
        public static void AddExplosiveBall(float xPos, float yPos, float width, float height)
        {
            IGameObject explosiveBall = new ExplosiveObject(new Vector2f(xPos, yPos), width, height);
            s_queueAddObjects.Enqueue(explosiveBall);            
        }
    }
}
