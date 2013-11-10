using System;

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
            using (FractalGame game = new FractalGame())
#else
            using (Game_Editor game = new Game_Editor())            
#endif
			{
                game.Run();
            }
        }
    }
}

