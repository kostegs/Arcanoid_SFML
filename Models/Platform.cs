using Arcanod_SFML_HomeWork.Interfaces;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcanod_SFML_HomeWork
{
    internal class Platform : IGameObject, IInteractive, IMovable, IDrawable, IColliding
    {
        private Texture platformTexture;
        public Sprite platformSprite { get; private set; }
        private static Vector2i s_mousePosition;
        public Platform()
        {
            platformTexture = new Texture(@"./res/Stick.png");
            platformSprite = new Sprite(platformTexture);
            platformSprite.Position = new Vector2f(400, 500);
        }

        public void Interact()
        {
            s_mousePosition = Mouse.GetPosition(Controller.View);            
        }

        public void Move()
        {
            platformSprite.Position = new Vector2f(s_mousePosition.X - (platformSprite.TextureRect.Width * 0.5f), platformSprite.Position.Y);
        }

        public void Draw()
        {
            Controller.View.Draw(platformSprite);
        }

        // Platform do nothing if has collision
        public void CheckCollision(IColliding withObject) { }

        public Sprite GetSpriteOfObject()
        {
            return platformSprite;
        }
    }
}
