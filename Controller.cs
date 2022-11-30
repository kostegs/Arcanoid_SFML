using Arcanod_SFML_HomeWork.Interfaces;
using System;
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
            s_GameObjects.AddLast(new SideWalls());
            s_GameObjects.AddLast(new TopWall());
            s_GameObjects.AddLast(new BottomWall());
            s_GameObjects.AddLast(new Blocks());
        }
        public static void Play()
        {
            while (View.IsOpen)
            {
                View.DispatchEvents();

                // Checking interact

                // Moving objects

                // Checking Collision

                View.Clear();

                // Draw objects

                View.Display();
            }
        }
    }
}
