using System;
using Drawing;

namespace FractalGpu
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
#if WINDOWS
#if GAME
#else
        [STAThread]
#endif
#endif
        static void Main(string[] args) 
        {            
#if GAME
            using (Game1 game = new Game1())
#else
            using (Game_Editor game = new Game_Editor())            
#endif
			{
                game.Run();
            }
        }
    }
}

