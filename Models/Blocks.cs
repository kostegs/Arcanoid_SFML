using Arcanod_SFML_HomeWork.Interfaces;
using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Arcanod_SFML_HomeWork
{
    internal class Block : IGameObject, IDrawable, IMovable, IDestroyable, IColliding
    {
        protected Texture _blockTexture;
        internal Sprite BlockSprite { get; private set; }
        public bool AllowToDestroy { get; set ; }

        public Block()
        {
            _blockTexture = new Texture(@"./res/Block.png");
            BlockSprite = new Sprite(_blockTexture);
        }

        public void Destroy()
        {
            AllowToDestroy = true;
        }

        public void Draw()
        {
            Controller.View.Draw(BlockSprite);
        }

        public void Move()
        {
            
        }

        public void CheckCollision(IColliding withObject)
        {
            Sprite spriteObject = withObject.GetSpriteOfObject();

            if (BlockSprite.GetGlobalBounds().Intersects(spriteObject.GetGlobalBounds()))
                Destroy();
        }

        public Sprite GetSpriteOfObject()
        {
            return BlockSprite;
        }
    }
    internal class Blocks : IGameObject, IDrawable, IMovable, IColliding
    {
        public LinkedList<Block> BlockList { get; private set; }
        
        public Blocks()
        {
            BlockList = new LinkedList<Block>();
            for (int i = 0; i < 100; i++)
                BlockList.AddLast(new Block());

            SetStartPosition();
        }
        public void SetStartPosition()
        {
            int x = 0;
            int y = 0;

            foreach (Block block in BlockList)
            {
                if (x == 10)
                {
                    x = 0;
                    y++;
                }

                block.BlockSprite.Position = new Vector2f(x * (block.BlockSprite.TextureRect.Width + 15) + 75,
                        y * (block.BlockSprite.TextureRect.Height + 15) + 50);
                x++;
            }            
        }
        public void CheckCollision(IColliding withObject)
        {
            // First of all - checking collision with another object.
            foreach (Block block in BlockList)
                if (block is IColliding)
                    block.CheckCollision(withObject);

            // Iterating all blocks, if some block is destroyable and has a flag "You can destroy me" - we are kicking him from the collection.
            var currentNode = BlockList.First;
            LinkedListNode<Block> lastNode = null;

            while (currentNode != null)
            {
                Block currentBlock = currentNode.Value;
                lastNode = currentNode;
                currentNode = currentNode.Next;

                if (currentBlock is IDestroyable && currentBlock.AllowToDestroy)
                {
                    currentBlock = null;
                    BlockList.Remove(lastNode);
                }
            }
        }
        
        public void Draw()
        {
            foreach (Block block in BlockList)
                block.Draw();

            Console.SetCursorPosition(0, 0);
            Console.Write("   ");
            Console.SetCursorPosition(0, 0);
            Console.Write(BlockList.Count());
        }

        public Sprite GetSpriteOfObject()
        {
            return new Sprite();
        }

        public void Move()
        {
            
        }
    }
}
