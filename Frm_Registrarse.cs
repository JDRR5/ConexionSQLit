using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Windows.Forms;

namespace ConexionSQLite
{
    public partial class Frm_Registrarse : Form
    {
        private Frm_ProcesarSolicitud context;
        private bool modoRegistro = false;
        
        private Point btnIniciarOriginalPos;
        private Point btnRegistrarOriginalPos;
        private Point btnCancelarOriginalPos;
        private int alturaOriginalFormulario;

        public Frm_Registrarse()
        {
            InitializeComponent();
            alturaOriginalFormulario = this.Height;

            context = new Frm_ProcesarSolicitud(this);
            
            btnIniciarOriginalPos = btnIniciar.Location;
            btnRegistrarOriginalPos = btnRegistrar.Location;
            btnCancelarOriginalPos = btnCancelar.Location;
            
            btnIniciar.Click += BtnIniciar_Click;     
            btnRegistrar.Click += BtnRegistrar_Click; 
            btnCancelar.Click += BtnCancelar_Click;   

            CargarRoles();

            ActualizarInterfazDesdeContext();
        }

        private void CargarRoles()
        {
            using (var conexion = Frm_BasedeDatos.Instance.CrearConexion())
            {
                try
                {
                    conexion.Open();
                    using (var cmd = new SQLiteCommand("SELECT id, nombre FROM roles ORDER BY id", conexion))
                    {
                        var dt = new DataTable();
                        var adapter = new SQLiteDataAdapter(cmd);
                        adapter.Fill(dt);

                        if (dt.Rows.Count > 0)
                        {
                            cmbRol.DisplayMember = "nombre";
                            cmbRol.ValueMember = "id";
                            cmbRol.DataSource = dt;
                            cmbRol.SelectedIndex = -1; 
                        }
                        else
                        {
                            MessageBox.Show("No se encontraron roles en la base de datos.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al cargar roles: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        public void ActualizarInterfazDesdeContext()
        {
            string tituloActual = context.GetTitulo();
            modoRegistro = (tituloActual == "Registro de Usuario");
            
            lblTitulo.Text = tituloActual;
            
            btnIniciar.Text = context.GetBotonIniciarTexto();
            btnRegistrar.Text = context.GetBotonRegistrarTexto();
            btnCancelar.Text = modoRegistro ? "Cancelar" : "Salir";
            
            lblRol.Visible = modoRegistro;
            cmbRol.Visible = modoRegistro;
            
            SuspendLayout();

            if (modoRegistro)
            {
                this.Height = alturaOriginalFormulario + 120;
                
                int buttonY = cmbRol.Bottom + 20;
                
                btnIniciar.Location = new Point(btnIniciarOriginalPos.X, buttonY);
                btnRegistrar.Location = new Point(btnRegistrarOriginalPos.X, buttonY - 66); 
                btnCancelar.Location = new Point(btnCancelarOriginalPos.X, buttonY);
            }
            else
            {
                this.Height = alturaOriginalFormulario;
                
                btnIniciar.Location = btnIniciarOriginalPos;
                btnRegistrar.Location = btnRegistrarOriginalPos;
                btnCancelar.Location = btnCancelarOriginalPos;
            }
            
            btnIniciar.Visible = true;
            btnRegistrar.Visible = true;
            btnCancelar.Visible = true;

            ResumeLayout(true);
            Refresh();
        }
        
        private void BtnIniciar_Click(object sender, EventArgs e)
        {
            ProcesarAccionPrincipal();
        }
        
        private void BtnRegistrar_Click(object sender, EventArgs e)
        {
            CambiarModo();
        }
        
        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("¿Desea salir de la aplicación?", "Salir",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private void ProcesarAccionPrincipal()
        {
            string username = txtUsuario.Text.Trim();
            string password = txtContraseña.Text.Trim();
            int? idRol = null;

            if (modoRegistro)
            {
                if (cmbRol.SelectedIndex == -1 || cmbRol.SelectedValue == null)
                {
                    MessageBox.Show("Debe seleccionar un rol para registrarse.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                idRol = Convert.ToInt32(cmbRol.SelectedValue);
            }
            context.ProcesarSolicitud(username, password, idRol);
        }

        private void CambiarModo()
        {
            if (!modoRegistro)
            {
                context.CambiarEstado(new Frm_EstadoRegistro());
            }
            else
            {
                context.CambiarEstado(new Frm_ValidarCredenciales());
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Enter)
            {
                var focusedControl = this.ActiveControl;

                if (focusedControl is TextBox)
                {
                    this.SelectNextControl(focusedControl, true, true, true, true);
                    return true;
                }
                else if (focusedControl is Button || focusedControl == cmbRol)
                {
                    ProcesarAccionPrincipal();
                    return true;
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        public void MostrarFormulario()
        {
            txtUsuario.Clear();
            txtContraseña.Clear();
            context.CambiarEstado(new Frm_ValidarCredenciales());
            
            this.Show();
        }
    }
}
