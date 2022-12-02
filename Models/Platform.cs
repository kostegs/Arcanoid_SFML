using Arcanod_SFML_HomeWork.Interfaces;
using Arcanod_SFML_HomeWork.Models;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace Arcanod_SFML_HomeWork
{
    internal class Platform : IGameObject, IInteractive, IMovable, IDrawable, IColliding
    {
        private Texture platformTexture;
        public Sprite PlatformSprite { get; private set; }        
        private static Vector2i s_mousePosition;
        public Platform()
        {
            platformTexture = new Texture(@"./res/Stick.png");
            PlatformSprite = new Sprite(platformTexture);
            
            // Stick the platform to the mouse pointer.
            PlatformSprite.Position = new Vector2f(0, 500);
            s_mousePosition = Mouse.GetPosition(Controller.View);
            PlatformSprite.Position = new Vector2f(s_mousePosition.X - (PlatformSprite.TextureRect.Width * 0.5f), PlatformSprite.Position.Y);
        }

        public void Interact() => s_mousePosition = Mouse.GetPosition(Controller.View);

        public void Move()
        {
            // To avoid leaving from screen-borders
            float xPos = s_mousePosition.X - (PlatformSprite.TextureRect.Width * 0.5f) < 0 ? 0 : s_mousePosition.X - (PlatformSprite.TextureRect.Width * 0.5f);
            xPos = xPos + PlatformSprite.TextureRect.Width > 800 ? 800 - PlatformSprite.TextureRect.Width : xPos;

            PlatformSprite.Position = new Vector2f(xPos, PlatformSprite.Position.Y);            
        }

        public void Draw() => Controller.View.Draw(PlatformSprite); 

        // Platform do nothing if has collision, but inherits IColliding interface to be checked by another objects.
        public void CheckCollision(IColliding withObject) {}

        public Sprite GetSpriteOfObject() => PlatformSprite;
    }
}
