using Arcanod_SFML_HomeWork.Interfaces;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Arcanod_SFML_HomeWork
{
    internal class Ball : IGameObject, IInteractive, IMovable, IDrawable, IColliding
    {
        private Texture _ballTexture;
        private float _speed;
        private Vector2f _direction;
        public Sprite Sprite { get; private set; }

        public Ball()
        {
            _ballTexture = new Texture(@"./res/Ball.png");
            Sprite = new Sprite(_ballTexture);
            Sprite.Position = new Vector2f(375, 400);
        }

        public void Interact()
        {
            if (Mouse.IsButtonPressed(Mouse.Button.Left))
                Start(5, new Vector2f(0, -1));
        }
        public void Start(float speed, Vector2f direction)
        {
            if (this._speed != 0)
                return;

            this._speed = speed;
            this._direction = direction;
        }
        public void Move(Vector2i boundsPos, Vector2i boundsSize)
        {
            Sprite.Position += _direction * _speed;
            if (Sprite.Position.X > boundsSize.X - Sprite.TextureRect.Width || Sprite.Position.X < boundsPos.X)
                _direction.X *= -1;
            if (Sprite.Position.Y < boundsPos.Y)
                _direction.Y *= -1;
        }

        public void Move()
        {
            Move(new Vector2i(0, 0), new Vector2i(800, 600));
        }

        public void Draw()
        {
            Controller.View.Draw(Sprite);
        }

        public void CheckCollision(IColliding withObject)
        {
            if (withObject is Blocks)
            {
                foreach (Block block in ((Blocks)withObject).BlockList)
                {
                    if (block is IColliding)
                    {
                        Sprite withObjectSprite = block.GetSpriteOfObject();

                        // If we have collision, we don't need continue the cycle.
                        if (Sprite.GetGlobalBounds().Intersects(withObjectSprite.GetGlobalBounds()))
                        {
                            _direction.Y *= -1;
                            break;
                        }
                    }                    
                }
            }
            else
            {
                Sprite withObjectSprite = ((IColliding)withObject).GetSpriteOfObject();

                if (Sprite.GetGlobalBounds().Intersects(withObjectSprite.GetGlobalBounds()))
                {
                    if (withObject is Platform)
                    {
                        _direction.Y = -1;
                        float f = ((Sprite.Position.X + Sprite.TextureRect.Width * 0.5f) - (withObjectSprite.Position.X + withObjectSprite.TextureRect.Width * 0.5f)) / withObjectSprite.TextureRect.Width;
                        _direction.X = f * 1.5f;
                    }
                }
            }
        }

        public Sprite GetSpriteOfObject()
        {
            return Sprite;
        }
    }
}
