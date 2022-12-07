using Arcanod_SFML_HomeWork.Interfaces;
using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Linq;

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
        
        public bool HasCollision(IColliding withObject) => !IsDestroyMode && BlockSprite.GetGlobalBounds().Intersects(withObject.GetSpriteOfObject().GetGlobalBounds());        

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
            if (HasCollision(withObject))
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

        public void SetComponentPosition() => _block.BlockSprite.Position = BlockSprite.Position;
        public override void CheckCollision(IColliding withObject)
        {
            if (_block != null)
                _block.CheckCollision(withObject);
        }
        public override void Move()
        {
            base.Move();    
            _block.BlockSprite.Position = BlockSprite.Position;
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

            if (HasCollision(withObject))            
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
    class GlassBlock : Decorator
    {
        
        private Texture _textureCrack = new Texture(@"./res/CrackedBlock1.png");
        private int _collisionCounter = 0;

        public GlassBlock(Block block) : base(block)
        {
            this.BlockSprite.Position = _block.BlockSprite.Position;
            this.BlockSprite.Texture = new Texture(@"./res/CrackedBlockDefault.png");
        }  
                
        public override void CheckCollision(IColliding withObject)
        {
            if (HasCollision(withObject))
            {
                _collisionCounter++;
                
                if (_collisionCounter == 1)
                    BlockSprite.Texture = _textureCrack;
                else
                    Destroy();
            }
        }        
    }
    class HardGlassBlock : Decorator
    {
        private Texture _textureCrack1 = new Texture(@"./res/CrackedBlock1.png");
        private Texture _textureCrack2 = new Texture(@"./res/CrackedBlock2.png");
        private int _collisionCounter = 0;

        public HardGlassBlock(Block block) : base(block)
        {
            this.BlockSprite.Position = _block.BlockSprite.Position;
            this.BlockSprite.Texture = new Texture(@"./res/CrackedBlock2Default.png");
        }

        public override void CheckCollision(IColliding withObject)
        {
            if (HasCollision(withObject))
            {
                _collisionCounter++;

                switch (_collisionCounter)
                {
                    case 1:
                        BlockSprite.Texture = _textureCrack1;
                        break;
                    case 2:
                        BlockSprite.Texture = _textureCrack2;
                        break;
                    default:
                        Destroy();
                        break;
                }                
            }
        }
    }
    abstract class ButtonBlock : Block
    {
        public event EventHandler IsCollision;

        public ButtonBlock(EventHandler collisionHandler)
        {
            IsCollision += collisionHandler;
        }
        public override void CheckCollision(IColliding withObject)
        {
            if (HasCollision(withObject))            
                IsCollision?.Invoke(this, new EventArgs());            
        }
        public override void Move() {}
    }
    class PlayBlock : ButtonBlock
    {
        public PlayBlock(EventHandler collisionHandler) : base(collisionHandler)
        {
            Texture playTexture = new Texture(@"./res/PlayButton.png");
            this.BlockSprite = new Sprite(playTexture);
            
        }
    }
    class ExitBlock : ButtonBlock
    {
        public ExitBlock(EventHandler collisionHandler) : base(collisionHandler)
        {
            Texture playTexture = new Texture(@"./res/ExitButton.png");
            this.BlockSprite = new Sprite(playTexture);
        }                
    }
    internal class Blocks : IGameObject, IDrawable, IMovable, IColliding, IInteractive
    {
        public LinkedList<Block> BlockList { get; private set; }
        public event EventHandler IsCollision;
        public event EventHandler BlocksAreOver;        
        Clock _timerForGeneratingBonusBlocks = new Clock();
        
        public Blocks(int countOfBlocks, int numberOfColumns)
        {
            BlockList = new LinkedList<Block>();

            if (Settings.GameMode == GameMode.Play)
            {
                for (int i = 1; i <= countOfBlocks; i++)
                    BlockList.AddLast(new SimpleBlock());
                SetStartPosition(numberOfColumns);
                _timerForGeneratingBonusBlocks.Restart();
            }                
            else if (Settings.GameMode == GameMode.StartScreen)
            {
                PlayBlock playBlock = new PlayBlock(CollisionHandler);
                SetBlockPosition(playBlock, 0.5f, 1, 75);                
                BlockList.AddLast(playBlock);

                ExitBlock exitBlock = new ExitBlock(CollisionHandler);
                SetBlockPosition(exitBlock, 2.5f, 1, 75);                
                BlockList.AddLast(exitBlock);
            }            
        }
        
        internal void CollisionHandler(object sender, EventArgs e)
        {
            IsCollision?.Invoke(sender, e);
        }
        public void SetStartPosition(int numberOfColumns)
        {
            int x = 0;
            int y = 0;
            
            // Offset from screen left border. It works only If blocks have width = 15.
            int xSreenOffset = 75;

            if (numberOfColumns == 5)
                xSreenOffset = 227;

                foreach (Block block in BlockList)
            {
                if (x == numberOfColumns)
                {
                    x = 0;
                    y++;
                }                

                SetBlockPosition(block, x, y, xSreenOffset);
                x++;
            }            
        }
        public void SetBlockPosition(Block block, float x, float y, int xSreenOffset)
        {
            block.BlockSprite.Position = new Vector2f(x * (block.BlockSprite.TextureRect.Width + 15) + xSreenOffset,
                        y * (block.BlockSprite.TextureRect.Height + 15) + 50);
            // Set position for incapsulated inside decorator block. In case where we start game with decorated blocks.
            if (block is Decorator)
                ((Decorator)block).SetComponentPosition();
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

            if (BlockList.Count() == 0)
                BlocksAreOver?.Invoke(this, new EventArgs());
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
            if (_timerForGeneratingBonusBlocks.ElapsedTime.AsSeconds() >= 5)
            {
                ChangeRandomBlock();
                _timerForGeneratingBonusBlocks.Restart();
            }
            foreach (Block block in BlockList)
                block.Interact();
                
        }
        private void ChangeRandomBlock()
        {
            // Generate new kind of blocks only in playing-mode or If count of blocks > 3.
            if ((Settings.GameMode != GameMode.Play) || (BlockList.Count < 3))
                return;

            int _rndNumber = Controller.RandomNumber(1, BlockList.Count());
            var _currentNode = BlockList.First;
            int _counter = 0;
            bool _continueEnumeration = true;

            while (_continueEnumeration)
            {
                _counter++;                
                _currentNode = _currentNode.Next;
                
                // If we've enumerated all elements in the list we need to stop
                if (_currentNode == null)
                    _continueEnumeration = false;
                else
                {
                    // If we found our random block and it isn't destroying 
                    if (_counter >= _rndNumber && !_currentNode.Value.IsDestroyMode)
                    {
                        _currentNode.Value = GetRandomBonusBlock(_currentNode.Value);
                        _continueEnumeration = false;
                    }
                }                    
            }
        }

        private Block GetRandomBonusBlock(Block sourceBlock)
        {
            int rndNumber = Controller.RandomNumber(1, 4);
            Block randomBlock;

            switch (rndNumber)
            {
                case 1:
                    randomBlock = new ExplosiveBlock(sourceBlock);
                    break;
                case 2:
                    randomBlock = new GlassBlock(sourceBlock);
                    break;
                case 3:
                    randomBlock = new HardGlassBlock(sourceBlock);
                    break;
                default:
                    randomBlock = new ExplosiveBlock(sourceBlock);
                    break;
            }

            return randomBlock;
        }
    }
}
