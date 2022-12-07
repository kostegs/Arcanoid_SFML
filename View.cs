using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using ProjectSettings = Arcanod_SFML_HomeWork.Settings;
using System.Reflection.Emit;
using System.Threading;

namespace Arcanod_SFML_HomeWork
{
    internal class View : RenderWindow
    {
        private Font _currentFont;
        private Text _label = new Text();
        private Color _fillColor;
        private Texture _hpTexture = new Texture("./res/HP.png");
        private Texture _backgroundTexture; 
        private Sprite _backgroundImageSprite;
        private Texture _levelNumberTexture = new Texture("./res/LevelNumberTextures.png");

        public View() : base(new VideoMode(800, 600), "Arcanoid") { }

        public void Initialize()
        {
            SetFont(@"./res/comic.ttf");
            SetFramerateLimit(60);
            Closed += Window_Closed;
            SetMouseCursorVisible(false);
        }

        public void InitializationGameModeSwitched()
        {
            switch (ProjectSettings.GameMode)
            {
                case GameMode.Play:
                    _backgroundTexture = new Texture($"./res/Level{Controller.LevelNumber}_Background.png");
                    _backgroundImageSprite = new Sprite(_backgroundTexture);
                    break;
                case GameMode.StartScreen:
                    _backgroundTexture = new Texture("./res/BackgroundStartScreen.png");
                    _backgroundImageSprite = new Sprite(_backgroundTexture);
                    break;
                default:
                    _backgroundImageSprite = new Sprite();
                    break;
            }
        }

        public void DrawBackground()
        {
            Draw(_backgroundImageSprite);
        }
        public void SetFont(string path)
        {
            _currentFont = new Font(path);
            _label = new Text();
            _label.Font = _currentFont;
        }
        public void SetFillColor(byte red, byte green, byte blue)
        {
            _fillColor = new Color(red, green, blue);
        }
        public void DrawText(int x, int y, string text, uint size = 12u)
        {
            _label.DisplayedString = text;
            _label.CharacterSize = size;
            _label.Position = new Vector2f(x, y);
            _label.FillColor = _fillColor;
            Draw(_label);
        }
        public void DisplayStats()
        {
            float xPos, yPos;
            xPos = yPos = 5;

            for (int i = 1; i <= Controller.Hp; i++)
            {
                Sprite hpSprite = new Sprite(_hpTexture);
                hpSprite.Position = new Vector2f(xPos, yPos);                
                xPos += hpSprite.TextureRect.Width + 5;
                Draw(hpSprite);
            }

        }
        public void DrawEndGameWIndow()
        {
            SetFillColor(255, 255, 255);
            DrawText(350, 300, "End Game :(", 30);
        }
        public void DrawLevelNumber()
        {
            int levelNumber = Controller.LevelNumber;
            Clock delayTimer = new Clock();
            Sprite sprite = new Sprite(_levelNumberTexture);
            sprite.Position = new Vector2f(150, 200);            
            sprite.TextureRect = levelNumber == 5 ? new IntRect(0, 150 * (levelNumber - 1), 500, 320) : new IntRect(0, 150 * (levelNumber - 1), 500, 150);

            while (true)
            {
                Clear();
                SetFillColor(255, 255, 255);
                Draw(sprite);                
                Display();

                if (delayTimer.ElapsedTime.AsSeconds() >= 2)
                    break;
            }

        }
        private void Window_Closed(object sender, EventArgs e) => Close();
    }
}
