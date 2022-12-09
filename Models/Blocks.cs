using Arcanoid_SFML.Interfaces;
using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Arcanoid_SFML
{
    internal class CollisionEventArgs : EventArgs
    {
        internal IColliding EncounteredObject;
        public CollisionEventArgs(IColliding gameObject)
        {
            EncounteredObject = gameObject;
        }
    }
    internal abstract class Block : IGameObject, IDrawable, IMovable, IDestroyable, IColliding, IInteractive
    {
        protected Clock _destroyTimer;
        private float _speed;
        
        public bool IsDestroyMode { get; protected set; }        
        public Texture BlockTexture { get; set; }
        public Sprite BlockSprite { get; set; } = new Sprite();
        public bool AllowToDestroy { get; set; }
        public event EventHandler IsCollisionEvent;

        public Block() => InitializeBlock();

        public virtual void InitializeBlock()
        {
            IsDestroyMode = false;
            _destroyTimer = new Clock();
            BlockTexture = new Texture(@"./res/Block.png");
            BlockSprite.Texture = BlockTexture;
        }
        public virtual void Draw() => Controller.View.Draw(BlockSprite);
        public virtual void CheckCollision(IColliding withObject)
        {
            if (HasCollision(withObject))
            {
                NotifyAboutCollision(withObject);
                Destroy();
            }                
        }
        
        public virtual bool HasCollision(IColliding withObject)
        {
            bool isCollision = !IsDestroyMode && BlockSprite.GetGlobalBounds().Intersects(withObject.GetSpriteOfObject().GetGlobalBounds());

            return isCollision;
        }
        
        public virtual void NotifyAboutCollision(IColliding withObject) => IsCollisionEvent?.Invoke(this, new CollisionEventArgs(withObject)); 
        
        public Sprite GetSpriteOfObject() => BlockSprite;

        public virtual void Destroy()
        {
            if (!IsDestroyMode)
            {
                BlockTexture = new Texture(@"./res/Explosion.png");                
                Vector2f savedPos = BlockSprite.Position;
                BlockSprite = new Sprite(BlockTexture);
                BlockSprite.Position = new Vector2f(savedPos.X, savedPos.Y - 15);
                IsDestroyMode = true;
                _destroyTimer.Restart();
            }
        } 

        public virtual void Move()
        {
            Vector2f _direction = new Vector2f(0, 1);
            
            if (Controller.LevelNumber > 2)
                _speed = 0.02f;
            else
                _speed = 0.04f;

            BlockSprite.Position += _direction * _speed;
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
    }    

    class ExplosiveBlock : Block
    {
        public ExplosiveBlock(Block baseBlock) : base() 
        {
            this.BlockSprite.Position = baseBlock.BlockSprite.Position;
            this.BlockSprite.Texture = new Texture(@"./res/ExplosiveBlock.png");
        }                        
    }

    class GlassBlock : Block
    {
        private Texture _textureCrack = new Texture(@"./res/CrackedBlock_Wrecked.png");
        private int _collisionCounter = 0;

        public GlassBlock(Block baseBlock) : base()
        {
            this.BlockSprite.Position = baseBlock.BlockSprite.Position;
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

                base.NotifyAboutCollision(withObject);
            }
        }       
    }
    class HardGlassBlock : Block
    {
        private Texture _textureCrack1 = new Texture(@"./res/CrackedBlock2_Wrecked.png");
        private Texture _textureCrack2 = new Texture(@"./res/CrackedBlock2_Wrecked2.png");
        private int _collisionCounter = 0;

        public HardGlassBlock(Block baseBlock) : base()
        {
            this.BlockSprite.Position = baseBlock.BlockSprite.Position;
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

                base.NotifyAboutCollision(withObject);
            }
        }        
    }

    abstract class ButtonBlock : Block
    {
        public ButtonBlock() : base() { }
        public override void CheckCollision(IColliding withObject)
        {
            base.CheckCollision(withObject);            
        }
            
        // Button-blocks aren't moving.
        public override void Move() {}
    }

    class PlayBlock : ButtonBlock
    {
        public PlayBlock() : base()
        {
            Texture playTexture = new Texture(@"./res/PlayButton.png");
            this.BlockSprite = new Sprite(playTexture);            
        }
    }

    class ExitBlock : ButtonBlock
    {
        public ExitBlock() : base()
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
                    if (Controller.LevelNumber == 5)
                    {
                        ExplosiveBlock explosiveBlock = new ExplosiveBlock(new SimpleBlock());
                        explosiveBlock.IsCollisionEvent += CollisionHandler;
                        BlockList.AddLast(explosiveBlock);
                    }                        
                    else
                    {
                        SimpleBlock simpleBlock = new SimpleBlock();
                        simpleBlock.IsCollisionEvent += CollisionHandler;
                        BlockList.AddLast(simpleBlock);
                    }                        

                BlocksSetStartPosition(numberOfColumns);
                _timerForGeneratingBonusBlocks.Restart();
            }                
            else if (Settings.GameMode == GameMode.StartScreen || Settings.GameMode == GameMode.EndGame || Settings.GameMode == GameMode.WinGame)
            {
                PlayBlock playBlock = new PlayBlock();
                SetBlockPosition(playBlock, 0.2f, 2, 75);           
                playBlock.IsCollisionEvent += CollisionHandler;
                BlockList.AddLast(playBlock);

                ExitBlock exitBlock = new ExitBlock();
                SetBlockPosition(exitBlock, 2.8f, 2, 75);      
                exitBlock.IsCollisionEvent += CollisionHandler;
                BlockList.AddLast(exitBlock);
            }            
        }
        
        internal void CollisionHandler(object sender, EventArgs e)
        {
            IsCollision?.Invoke(sender, e);
        }
        
        public void BlocksSetStartPosition(int numberOfColumns)
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
        }

        public void CheckCollision(IColliding withObject)
        {
            // First of all - checking collision with another object.
            foreach (Block block in BlockList)                
                    block.CheckCollision(withObject);

            DeleteDestructedBlocks();

            // Player has broke all blocks. It means - Level complete! Sending notification to the controller.
            if (BlockList.Count() == 0)
                BlocksAreOver?.Invoke(this, new EventArgs());
        }
        
        private void DeleteDestructedBlocks()
        {
            // Iterating all blocks, if some block is destroyable it has a flag "Allow to Destroy",
            // that means "You can destroy me", in this case we are kicking it from the collection.
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
        }

        // For compatibility with interface.
        public Sprite GetSpriteOfObject() => new Sprite();

        public void Move()
        {
            foreach (Block block in BlockList)
                block.Move();
        }

        public void Interact()
        {
            // Changing random block one time in 5 second.
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
            // Generating new kind of blocks in condition where this is the playing game-mode or count of blocks > 3
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
                
                // If we've enumerated all elements in the list and haven't had any result we need to stop. It was a bad try.
                if (_currentNode == null)
                    _continueEnumeration = false;
                else
                {
                    // If we found our random block and it isn't destroying right now.
                    if (_counter >= _rndNumber && !_currentNode.Value.IsDestroyMode)
                    {
                        // Change this block to another random block.
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

            // Subscribing to collision event, because this is a new block and we aren't subscribed yet..
            randomBlock.IsCollisionEvent += CollisionHandler;
            return randomBlock;
        }
    }
}
