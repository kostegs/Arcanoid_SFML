using Arcanod_SFML_HomeWork.Interfaces;
using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcanod_SFML_HomeWork.Models
{
    internal class Border : IGameObject, IColliding
    {
        public Sprite BorderSprite { get; private set; }
        
        public Border(float xPos, float yPos, uint width, uint height)
        {
            BorderSprite = new Sprite(new Texture(width, height));
            BorderSprite.Position = new Vector2f(xPos, yPos);
        }
        
        // Borders do nothing if has collision, but inherits IColliding interface to be checked by another objects.
        public void CheckCollision(IColliding withObject) { }

        public Sprite GetSpriteOfObject() => BorderSprite;
    }
    
    // There are particular classes of walls, because collision with this has another consequences. 
    internal class TopBorder : Border
    {
        public TopBorder(float xPos, float yPos, uint width, uint height) : base(xPos, yPos, width, height) { }
    }
    internal class SideBorder : Border
    {
        public SideBorder(float xPos, float yPos, uint width, uint height) : base(xPos, yPos, width, height) { }
    }
    
    internal class BottomBorder : Border
    {
        public BottomBorder(float xPos, float yPos, uint width, uint height) : base(xPos, yPos, width, height) {}
    }
}
