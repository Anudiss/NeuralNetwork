namespace NeuralNet.GameLogic
{
    public class Field
    {
        public int Width { get; }
        public int Height { get; }

        public Snake Snake { get; private set; }
        public Apple Apple { get; private set; }

        public Field(int width, int height)
        {
            Width = width;
            Height = height;

            Snake = new(this);
            Apple = new();
        }

        public void Restart()
        {
            Snake = new(this);
            Apple = new();
        }

        public void Draw()
        {
            // Fill background
            Console.BackgroundColor = ConsoleColor.White;

            for (int y = 0; y < Height; y++)
            {
                Console.SetCursorPosition(0, y);
                Console.WriteLine(string.Join("", Enumerable.Repeat(" ", Width)));
            }

            Console.ResetColor();

            Apple.Draw();
            Snake.Draw();

        }
    }
}
