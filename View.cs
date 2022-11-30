using SFML.Graphics;
using SFML.Window;
using System;

namespace Arcanod_SFML_HomeWork
{
    internal class View : RenderWindow
    {
        public View() : base(new VideoMode(800, 600), "Arcanoid") { }
                    
        
        public void Initialize()
        {
            SetFramerateLimit(60);
            Closed += Window_Closed;
            SetMouseCursorVisible(false);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Close();
        }
    }
}
