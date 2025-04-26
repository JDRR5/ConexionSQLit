using System;
using System.Data;
using System.Data.SQLite;
using System.Windows.Forms;

namespace ConexionSQLite
{
    public partial class Frm_Roles : Form
    {
        private DataTable dtRoles;
        private int currentPosition = 0;
        private int totalRecords = 0;

        public Frm_Roles()
        {
            InitializeComponent();
            CargarDatos();
        }

        private void CargarDatos()
        {
            using (var conexion = Frm_BasedeDatos.Instance.CrearConexion())
            {
                try
                {
                    conexion.Open();
                    string query = "SELECT * FROM roles ORDER BY id";
                    
                    using (var adapter = new SQLiteDataAdapter(query, conexion))
                    {
                        dtRoles = new DataTable();
                        adapter.Fill(dtRoles);
                        
                        totalRecords = dtRoles.Rows.Count;
                        
                        if (totalRecords > 0)
                        {
                            MostrarRegistro(0);
                            ActualizarEstadoNavegacion();
                        }
                        else
                        {
                            LimpiarCampos();
                            ActualizarEstadoNavegacion();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al cargar datos: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void MostrarRegistro(int position)
        {
            if (dtRoles.Rows.Count > 0 && position >= 0 && position < dtRoles.Rows.Count)
            {
                DataRow row = dtRoles.Rows[position];
                
                txtId.Text = row["id"].ToString();
                txtNombre.Text = row["nombre"].ToString();
                
                currentPosition = position;
                lblPosicion.Text = $"Registro {position + 1} de {totalRecords}";
            }
        }

        private void LimpiarCampos()
        {
            txtId.Text = "";
            txtNombre.Text = "";
            
            lblPosicion.Text = "No hay registros";
        }

        private void ActualizarEstadoNavegacion()
        {
            btnPrimero.Enabled = totalRecords > 0 && currentPosition > 0;
            btnAnterior.Enabled = totalRecords > 0 && currentPosition > 0;
            btnSiguiente.Enabled = totalRecords > 0 && currentPosition < totalRecords - 1;
            btnUltimo.Enabled = totalRecords > 0 && currentPosition < totalRecords - 1;
            
            btnNuevo.Enabled = true;
            btnGuardar.Enabled = true;
            btnEliminar.Enabled = totalRecords > 0 && int.Parse(txtId.Text) > 3; // Prevent deletion of default roles
        }

        private void btnPrimero_Click(object sender, EventArgs e)
        {
            if (totalRecords > 0)
            {
                MostrarRegistro(0);
                ActualizarEstadoNavegacion();
            }
        }

        private void btnAnterior_Click(object sender, EventArgs e)
        {
            if (currentPosition > 0)
            {
                MostrarRegistro(currentPosition - 1);
                ActualizarEstadoNavegacion();
            }
        }

        private void btnSiguiente_Click(object sender, EventArgs e)
        {
            if (currentPosition < totalRecords - 1)
            {
                MostrarRegistro(currentPosition + 1);
                ActualizarEstadoNavegacion();
            }
        }

        private void btnUltimo_Click(object sender, EventArgs e)
        {
            if (totalRecords > 0)
            {
                MostrarRegistro(totalRecords - 1);
                ActualizarEstadoNavegacion();
            }
        }

        private void btnNuevo_Click(object sender, EventArgs e)
        {
            LimpiarCampos();
            txtId.Text = "Nuevo";
            txtNombre.Focus();
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("El nombre del rol es obligatorio.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (var conexion = Frm_BasedeDatos.Instance.CrearConexion())
            {
                try
                {
                    conexion.Open();
                    
                    // Check if role name already exists
                    using (var cmdCheck = new SQLiteCommand("SELECT COUNT(*) FROM roles WHERE nombre = @nombre AND id != @id", conexion))
                    {
                        cmdCheck.Parameters.AddWithValue("@nombre", txtNombre.Text);
                        cmdCheck.Parameters.AddWithValue("@id", txtId.Text == "Nuevo" ? -1 : int.Parse(txtId.Text));
                        
                        int count = Convert.ToInt32(cmdCheck.ExecuteScalar());
                        
                        if (count > 0)
                        {
                            MessageBox.Show("Ya existe un rol con ese nombre.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                    
                    if (txtId.Text == "Nuevo")
                    {
                        // Insert new record
                        string query = "INSERT INTO roles (nombre) VALUES (@nombre)";
                        
                        using (var cmd = new SQLiteCommand(query, conexion))
                        {
                            cmd.Parameters.AddWithValue("@nombre", txtNombre.Text);
                            
                            int rowsAffected = cmd.ExecuteNonQuery();
                            
                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Rol agregado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                CargarDatos();
                                MostrarRegistro(totalRecords - 1);
                            }
                        }
                    }
                    else
                    {
                        int id = int.Parse(txtId.Text);
                        
                        // Prevent modification of default roles
                        if (id <= 3)
                        {
                            MessageBox.Show("No se pueden modificar los roles predeterminados del sistema.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        
                        // Update existing record
                        string query = "UPDATE roles SET nombre = @nombre WHERE id = @id";
                        
                        using (var cmd = new SQLiteCommand(query, conexion))
                        {
                            cmd.Parameters.AddWithValue("@nombre", txtNombre.Text);
                            cmd.Parameters.AddWithValue("@id", id);
                            
                            int rowsAffected = cmd.ExecuteNonQuery();
                            
                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Rol actualizado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                CargarDatos();
                                MostrarRegistro(currentPosition);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al guardar rol: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (txtId.Text == "Nuevo" || string.IsNullOrEmpty(txtId.Text))
            {
                MessageBox.Show("No hay un rol seleccionado para eliminar.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int id = int.Parse(txtId.Text);
            
            // Prevent deletion of default roles
            if (id <= 3)
            {
                MessageBox.Show("No se pueden eliminar los roles predeterminados del sistema.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Check if role is in use
            using (var conexion = Frm_BasedeDatos.Instance.CrearConexion())
            {
                conexion.Open();
                using (var cmd = new SQLiteCommand("SELECT COUNT(*) FROM usuarios WHERE id_rol = @id", conexion))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    
                    if (count > 0)
                    {
                        MessageBox.Show("No se puede eliminar el rol porque está asignado a uno o más usuarios.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
            }

            if (MessageBox.Show("¿Está seguro de eliminar este rol?", "Confirmar", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                using (var conexion = Frm_BasedeDatos.Instance.CrearConexion())
                {
                    try
                    {
                        conexion.Open();
                        string query = "DELETE FROM roles WHERE id = @id";
                        
                        using (var cmd = new SQLiteCommand(query, conexion))
                        {
                            cmd.Parameters.AddWithValue("@id", id);
                            
                            int rowsAffected = cmd.ExecuteNonQuery();
                            
                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Rol eliminado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                
                                int newPosition = currentPosition;
                                if (newPosition >= totalRecords - 1)
                                {
                                    newPosition = totalRecords - 2;
                                }
                                
                                if (newPosition < 0)
                                {
                                    newPosition = 0;
                                }
                                
                                CargarDatos();
                                
                                if (totalRecords > 0)
                                {
                                    MostrarRegistro(newPosition);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al eliminar rol: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}
