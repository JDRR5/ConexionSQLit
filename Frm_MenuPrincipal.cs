using System;
using System.Data.SQLite;
using System.Windows.Forms;

namespace ConexionSQLite
{
    public partial class Frm_MenuPrincipal : Form
    {
        private int userId;
        private int rolId;
        private string rolNombre;

        public Frm_MenuPrincipal(int userId, int rolId)
        {
            InitializeComponent();
            this.userId = userId;
            this.rolId = rolId;
            
            // Get role name
            using (var conexion = Frm_BasedeDatos.Instance.CrearConexion())
            {
                conexion.Open();
                using (var cmd = new SQLiteCommand("SELECT nombre FROM roles WHERE id = @id", conexion))
                {
                    cmd.Parameters.AddWithValue("@id", rolId);
                    rolNombre = (string)cmd.ExecuteScalar();
                }
            }
            
            ConfigurarMenuSegunRol();
        }

        private void ConfigurarMenuSegunRol()
        {
            // Set form title with role
            this.Text = $"Menú Principal - {rolNombre}";
            
            // Configure menu items based on role
            switch (rolId)
            {
                case 1: // Administrador
                    // Admin can access all forms
                    break;
                case 2: // Almacenista
                    // Warehouse worker can only access Products
                    menuClientes.Visible = false;
                    menuRoles.Visible = false;
                    break;
                case 3: // Vendedor
                    // Seller can only access Clients
                    menuProductos.Visible = false;
                    menuRoles.Visible = false;
                    break;
            }
        }

        private void menuClientes_Click(object sender, EventArgs e)
        {
            Frm_Clientes formClientes = new Frm_Clientes();
            formClientes.ShowDialog();
        }

        private void menuProductos_Click(object sender, EventArgs e)
        {
            Frm_Productos formProductos = new Frm_Productos();
            formProductos.ShowDialog();
        }

        private void menuRoles_Click(object sender, EventArgs e)
        {
            Frm_Roles formRoles = new Frm_Roles();
            formRoles.ShowDialog();
        }

        private void menuSalir_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("¿Desea cerrar sesión?", "Cerrar Sesión", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                // Show login form again
                Form loginForm = Application.OpenForms["Frm_Registrarse"];
                if (loginForm != null)
                {
                    loginForm.Show();
                }
                
                this.Close();
            }
        }
    }
}
