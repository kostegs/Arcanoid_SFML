using Arcanoid_SFML.Interfaces;
using SFML.Graphics;
using SFML.System;

namespace Arcanoid_SFML.Models
{
    internal class ExplosiveObject : IGameObject, IDrawable, IColliding, IDestroyable
    {
        private Texture _objectTexture;
        public Sprite ObjectSprite { get; protected set; }
        public bool AllowToDestroy { get; set; }        

        public ExplosiveObject(Vector2f startPosition, float width, float height)
        {
            _objectTexture = new Texture(10, 10);
            _objectTexture.Repeated = true;
            ObjectSprite = new Sprite(_objectTexture, new IntRect((int)startPosition.X, (int)startPosition.Y, (int)width, (int)height));
            ObjectSprite.Position = startPosition;            
        }
        public void Draw() => Controller.View.Draw(ObjectSprite);
        
        public Sprite GetSpriteOfObject() => ObjectSprite;
        public void CheckCollision(IColliding withObject)
        {
            // We should destroy object in any case since another object already have checked their collisions with this object. 
            // Yet this object is needed us for destroy another objects.
            Destroy();
        }
        public void Destroy()
        {
            AllowToDestroy = true;            
        }        
    }
}
