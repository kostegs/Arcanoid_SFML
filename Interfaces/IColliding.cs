using SFML.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcanod_SFML_HomeWork.Interfaces
{
    internal interface IColliding
    {
        void CheckCollision(IColliding withObject);
        Sprite GetSpriteOfObject();
    }
}
