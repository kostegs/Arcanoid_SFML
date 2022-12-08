using SFML.Graphics;

namespace Arcanoid_SFML.Interfaces
{
    internal interface IColliding
    {
        void CheckCollision(IColliding withObject);
        Sprite GetSpriteOfObject();
    }
}
