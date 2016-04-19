using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Shadows2
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            switch (1)
            {
                case 1:

                    Application.Run(new Form1());
                    break;
                case 2:
                    Application.Run(new TriangleTests());
                    break;
                case 3:
                    Application.Run(new RayTest());
                    break;
            }
        }
    }
}
