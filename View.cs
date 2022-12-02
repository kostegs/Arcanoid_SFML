using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.Reflection.Emit;

namespace Arcanod_SFML_HomeWork
{
    internal class View : RenderWindow
    {
        private Font _currentFont;
        private Text _lable = new Text();
        private Color _fillColor;

        public View() : base(new VideoMode(800, 600), "Arcanoid") { }

        public void Initialize()
        {
            SetFont(@"./res/comic.ttf");
            SetFramerateLimit(60);
            Closed += Window_Closed;
            SetMouseCursorVisible(false);
        }
        public void SetFont(string path)
        {
            _currentFont = new Font(path);
            _lable = new Text();
            _lable.Font = _currentFont;
        }
        public void SetFillColor(byte red, byte green, byte blue)
        {
            _fillColor = new Color(red, green, blue);
        }
        public void DrawText(int x, int y, string text, uint size = 12u)
        {
            _lable.DisplayedString = text;
            _lable.CharacterSize = size;
            _lable.Position = new Vector2f(x, y);
            _lable.FillColor = _fillColor;
            Draw(_lable);
        }
        public void DisplayStats()
        {
            SetFillColor(255, 255, 255);
            DrawText(5, 5, $"HP: {Controller.Hp}");
        }
        public void DrawEndGameWIndow()
        {
            SetFillColor(255, 255, 255);
            DrawText(350, 300, "End Game :(", 30);
        }
        private void Window_Closed(object sender, EventArgs e) => Close();
    }
}
