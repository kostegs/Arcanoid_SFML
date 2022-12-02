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
        public static View View { get; private set; } = new View();
        public static LinkedList<IGameObject> s_GameObjects { get; private set; }
        public static void GlobalInitialization()
        {
            View.Initialize();
            InitializateController();
        }
        public static void InitializateController()
        {
            InitializeGameObjects();
        }
        public static void InitializeGameObjects()
        {
            s_GameObjects = new LinkedList<IGameObject>();

            s_GameObjects.AddLast(new Ball());
            s_GameObjects.AddLast(new Platform());
            s_GameObjects.AddLast(new Blocks());

            // Left Border
            s_GameObjects.AddLast(new SideBorder(-5, 0, 5, 600));
            // Top Border
            s_GameObjects.AddLast(new TopBorder(0, -5, 800, 5));
            // Right Border
            s_GameObjects.AddLast(new SideBorder(800, 0, 5, 600));
            // Bottom Border
            s_GameObjects.AddLast(new BottomWall(0, 600, 5, 800));
                        
        }
        public static void Play()
        {
            while (View.IsOpen)
            {
                View.DispatchEvents();

                // Checking interact
                foreach(IGameObject gameObject in s_GameObjects)                
                    if (gameObject is IInteractive)
                        ((IInteractive)gameObject).Interact();                

                // Moving objects
                foreach (IGameObject gameObject in s_GameObjects)                
                    if (gameObject is IMovable)
                        ((IMovable)gameObject).Move();                

                // Checking Collision
                foreach (IGameObject gameObject in s_GameObjects)
                {
                    if (gameObject is IColliding)
                    {
                        foreach (IGameObject anotherObject in s_GameObjects)
                        {
                            if ((anotherObject is IColliding) && (anotherObject != gameObject))
                                ((IColliding)gameObject).CheckCollision((IColliding)anotherObject);
                        }
                    }                        
                }

                // Clear window
                View.Clear();

                // Draw objects
                foreach (IGameObject gameObject in s_GameObjects)                
                    if (gameObject is IDrawable)
                        ((IDrawable)gameObject).Draw();                

                // Display window
                View.Display();
            }
        }        
    }
}
