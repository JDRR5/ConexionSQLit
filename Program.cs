using System;
using System.Windows.Forms;

namespace ConexionSQLite
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
            
            // Create database tables if they don't exist
            Frm_BasedeDatos.Instance.InicializarBaseDeDatos();
            
            // Start with the login/register form
            Application.Run(new Frm_Registrarse());
        }
    }
}
