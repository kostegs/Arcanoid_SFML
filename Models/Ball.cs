using Arcanoid_SFML.Interfaces;
using Arcanoid_SFML.Models;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;

namespace Arcanoid_SFML
{
    internal class Ball : IGameObject, IInteractive, IMovable, IDrawable, IColliding
    {
        private Texture _ballTexture;
        private float _speed;
        private Vector2i _mousePosition;
        private Vector2f _direction;

        public Sprite BallSprite { get; protected set; }
        public event EventHandler BallDropped;
        public event EventHandler IsCollision;

        public Ball()
        {
            _ballTexture = new Texture(@"./res/Ball.png");
            BallSprite = new Sprite(_ballTexture);
            SetStartPosition();
        }     
        public void SetStartPosition()
        {
            _speed = 0;

            // Stick the ball to the mouse pointer until the player click the left button.
            BallSprite.Position = new Vector2f(0, 500 - BallSprite.TextureRect.Height);
            _mousePosition = Mouse.GetPosition(Controller.View);
            BallSprite.Position = new Vector2f(_mousePosition.X - (BallSprite.TextureRect.Width * 0.5f), BallSprite.Position.Y);
        }
        public void Interact()
        {
            if (_speed != 0)
                return;

            _mousePosition = Mouse.GetPosition(Controller.View);

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
                // To avoid exit from borders, in case where ball is on the platform and move with this
                float xPos = _mousePosition.X - (BallSprite.TextureRect.Width * 0.5f) < 0 ? 0 : _mousePosition.X - (BallSprite.TextureRect.Width * 0.5f);
                xPos = xPos + BallSprite.TextureRect.Width > 800 ? 800 - BallSprite.TextureRect.Width : xPos;
                BallSprite.Position = new Vector2f(xPos, BallSprite.Position.Y);
                return;
            }                

            BallSprite.Position += _direction * _speed;           
        }

        public void Move() => Move(new Vector2i(0, 0), new Vector2i(800, 600));
        
        public void Draw() => Controller.View.Draw(BallSprite);

        public virtual void CheckCollision(IColliding withObject)
        {
            bool hasCollision = false;

            if (withObject is Blocks)
            {
                foreach (Block block in ((Blocks)withObject).BlockList)
                {
                    if (block.IsDestroyMode)
                        continue;

                    Sprite withObjectSprite = block.GetSpriteOfObject();

                    // If we have collision, we don't need continue the cycle.
                    if (BallSprite.GetGlobalBounds().Intersects(withObjectSprite.GetGlobalBounds()))
                    {
                        hasCollision = true;
                        _direction.Y *= -1;
                        break;
                    }                                        
                }
            }
            else if (withObject is Border)
            {
                Sprite withObjectSprite = ((IColliding)withObject).GetSpriteOfObject();
                if (BallSprite.GetGlobalBounds().Intersects(withObjectSprite.GetGlobalBounds()))
                {
                    hasCollision = true;

                    if (withObject is SideBorder)
                        _direction.X *= -1;
                    else if (withObject is TopBorder)
                        _direction.Y *= -1;
                    else if (withObject is BottomBorder)
                        BallDropped?.Invoke(this, new EventArgs());
                }                    
            }
            else
            {
                Sprite withObjectSprite = ((IColliding)withObject).GetSpriteOfObject();

                if (BallSprite.GetGlobalBounds().Intersects(withObjectSprite.GetGlobalBounds()))
                {
                    hasCollision = true;

                    if (withObject is Platform)
                    {
                        _direction.Y = -1;
                        float f = ((BallSprite.Position.X + BallSprite.TextureRect.Width * 0.5f) - (withObjectSprite.Position.X + withObjectSprite.TextureRect.Width * 0.5f)) / withObjectSprite.TextureRect.Width;
                        _direction.X = f * 1.5f;
                    }
                }
            }

            if (hasCollision)
                IsCollision?.Invoke(this, new CollisionEventArgs(withObject));

        }
        public Sprite GetSpriteOfObject() => BallSprite;        
    }    
}
