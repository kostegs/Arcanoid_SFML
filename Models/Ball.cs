using Arcanod_SFML_HomeWork.Interfaces;
using Arcanod_SFML_HomeWork.Models;
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
        private Vector2i s_mousePosition;
        private Vector2f _direction;
        public Sprite BallSprite { get; private set; }

        public Ball()
        {
            _ballTexture = new Texture(@"./res/Ball.png");
            BallSprite = new Sprite(_ballTexture);
            SetStartPosition();
        }     
        public void SetStartPosition()
        {
            // Stick the ball to the mouse pointer until the player click the left button.
            BallSprite.Position = new Vector2f(0, 500 - BallSprite.TextureRect.Height);
            s_mousePosition = Mouse.GetPosition(Controller.View);
            BallSprite.Position = new Vector2f(s_mousePosition.X - (BallSprite.TextureRect.Width * 0.5f), BallSprite.Position.Y);
        }
        public void Interact()
        {
            if (_speed != 0)
                return;

            s_mousePosition = Mouse.GetPosition(Controller.View);

            if (Mouse.IsButtonPressed(Mouse.Button.Left))
                Start(Settings.BallSpeed, new Vector2f(0, -1));
        }
        public void Start(float speed, Vector2f direction)
        {
            if (_speed != 0)
                return;

            this._speed = speed;
            this._direction = direction;
        }
        public void Move(Vector2i boundsPos, Vector2i boundsSize)
        {
            if (_speed == 0)
            {
                // To avoid exit from borders
                float xPos = s_mousePosition.X - (BallSprite.TextureRect.Width * 0.5f) < 0 ? 0 : s_mousePosition.X - (BallSprite.TextureRect.Width * 0.5f);
                xPos = s_mousePosition.X - (BallSprite.TextureRect.Width * 0.5f) > 800 ? 800 - BallSprite.TextureRect.Width : xPos;
                BallSprite.Position = new Vector2f(xPos, BallSprite.Position.Y);
                return;
            }                

            BallSprite.Position += _direction * _speed;           
        }

        public void Move() => Move(new Vector2i(0, 0), new Vector2i(800, 600));
        
        public void Draw() => Controller.View.Draw(BallSprite);

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
                        if (BallSprite.GetGlobalBounds().Intersects(withObjectSprite.GetGlobalBounds()))
                        {
                            _direction.Y *= -1;
                            break;
                        }
                    }                    
                }
            }
            else if (withObject is Border)
            {
                Sprite withObjectSprite = ((IColliding)withObject).GetSpriteOfObject();
                if (BallSprite.GetGlobalBounds().Intersects(withObjectSprite.GetGlobalBounds()))
                {
                    if (withObject is SideBorder)
                        _direction.X *= -1;
                    else if (withObject is TopBorder)
                        _direction.Y *= -1;
                }
                    
            }
            else
            {
                Sprite withObjectSprite = ((IColliding)withObject).GetSpriteOfObject();

                if (BallSprite.GetGlobalBounds().Intersects(withObjectSprite.GetGlobalBounds()))
                {
                    if (withObject is Platform)
                    {
                        _direction.Y = -1;
                        float f = ((BallSprite.Position.X + BallSprite.TextureRect.Width * 0.5f) - (withObjectSprite.Position.X + withObjectSprite.TextureRect.Width * 0.5f)) / withObjectSprite.TextureRect.Width;
                        _direction.X = f * 1.5f;
                    }
                }
            }
        }

        public Sprite GetSpriteOfObject() => BallSprite;
        
    }
}
