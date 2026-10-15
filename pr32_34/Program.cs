using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace pr32_34
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //строка для перехода на главную форму при бездействии более 30 секунд
            Application.AddMessageFilter(new GlobalInputFilter());
            Application.Run(new Form1());
        }
    }
}
