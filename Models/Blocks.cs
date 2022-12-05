using Arcanod_SFML_HomeWork.Interfaces;
using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Arcanod_SFML_HomeWork
{
    internal abstract class Block : IGameObject, IDrawable, IMovable, IDestroyable, IColliding, IInteractive
    {
        public bool IsDestroyMode { get; private set; }
        private Clock _destroyTimer;
        public Texture BlockTexture { get; set; }
        public Sprite BlockSprite { get; set; } = new Sprite();
        public bool AllowToDestroy { get; set; }        

        public Block() => InitializeBlock();

        public virtual void InitializeBlock()
        {
            IsDestroyMode = false;
            _destroyTimer = new Clock();
            BlockTexture = new Texture(@"./res/Block.png");
            BlockSprite.Texture = BlockTexture;
        }
        public virtual void Draw() => Controller.View.Draw(BlockSprite);
        public abstract void CheckCollision(IColliding withObject);        
        public Sprite GetSpriteOfObject() => BlockSprite;
        public virtual void Destroy()
        {
            if (!IsDestroyMode)
            {
                BlockTexture = new Texture(@"./res/DestroyingBlock.png");
                BlockSprite.Texture = BlockTexture;
                IsDestroyMode = true;
                _destroyTimer.Restart();
            }
        } 
        public virtual void Move()
        {
            Vector2f _direction = new Vector2f(0, 1);
            BlockSprite.Position += _direction * 0.02f;
        }

        public void Interact()
        {
            if (IsDestroyMode && _destroyTimer.ElapsedTime.AsMilliseconds() >= 300)
                AllowToDestroy = true;
        }
    }
    internal class SimpleBlock : Block
    {
        public SimpleBlock() : base() {}
        public override void CheckCollision(IColliding withObject)
        {
            // Don't check collisions where block is destroyed
            if (IsDestroyMode)
                return;

            Sprite spriteObject = withObject.GetSpriteOfObject();

            if (BlockSprite.GetGlobalBounds().Intersects(spriteObject.GetGlobalBounds()))
                Destroy();
        }
    }
    abstract class Decorator : Block
    {
        protected Block _block;
        public Decorator(Block block)
        {
            this._block = block;
            InitializeBlock();
        }
        
        public override void CheckCollision(IColliding withObject)
        {
            // Don't check collisions where block is destroyed
            if (IsDestroyMode)
                return;

            if (_block != null)
                _block.CheckCollision(withObject);
        }
    }
    class ExplosiveBlock : Decorator
    {
        public ExplosiveBlock(Block block) : base(block) 
        {
            this.BlockSprite.Position = _block.BlockSprite.Position;
            this.BlockSprite.Texture = new Texture(@"./res/ExplosiveBlock.png");
        }       
         
        public override void CheckCollision(IColliding withObject)
        {
            // Call method check collisions from parent
            base.CheckCollision(withObject);

            // Don't check collisions where block is destroyed
            if (IsDestroyMode)
                return;

            Sprite spriteObject = withObject.GetSpriteOfObject();

            if (BlockSprite.GetGlobalBounds().Intersects(spriteObject.GetGlobalBounds()))
            {
                // Creating Explosive ball 3*3 simple block's size to destroy neighbours.
                float spriteWidth = BlockSprite.TextureRect.Width * 3;
                float spriteHeight = BlockSprite.TextureRect.Height * 3;
                float xPos = BlockSprite.Position.X - BlockSprite.TextureRect.Width;
                float yPos = BlockSprite.Position.Y - BlockSprite.TextureRect.Height;

                // Ask controller to put explosive ball on the screen.
                Controller.AddExplosiveBall(xPos, yPos, spriteWidth, spriteHeight);
                
                // Destroy ourselves.
                Destroy();
            }            
        }
    }
    
    internal class Blocks : IGameObject, IDrawable, IMovable, IColliding, IInteractive
    {
        public LinkedList<Block> BlockList { get; private set; }
        Clock _clock = new Clock();
        
        public Blocks()
        {
            BlockList = new LinkedList<Block>();
            for (int i = 0; i < 100; i++)
                BlockList.AddLast(new SimpleBlock());
                //BlockList.AddLast(new ExplosiveBlock(new SimpleBlock()));

            SetStartPosition();
            _clock.Restart();
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
            foreach (Block block in BlockList)
                block.Move();
        }

        public void Interact()
        {
            if (_clock.ElapsedTime.AsSeconds() >= 5)
            {
                ChangeRandomBlock();
                _clock.Restart();
            }
            foreach (Block block in BlockList)
                block.Interact();
                
        }
        private void ChangeRandomBlock()
        {
            if (BlockList.Count < 3)
                return;

            int _rndNumber = Controller.RandomNumber(1, BlockList.Count());
            var _currentNode = BlockList.First;
            int _counter = 0;
            bool _continueEnumeration = true;

            while (_continueEnumeration)
            {
                _counter++;                
                _currentNode = _currentNode.Next;
                
                if (_currentNode == null)
                    _continueEnumeration = false;
                else
                {
                    if (_counter >= _rndNumber && !_currentNode.Value.IsDestroyMode)
                    {
                        _currentNode.Value = new ExplosiveBlock(_currentNode.Value);
                        _continueEnumeration = false;
                    }
                }
                    
            }
        }
    }
}
