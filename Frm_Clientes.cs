using System;
using System.Data;
using System.Data.SQLite;
using System.Windows.Forms;

namespace ConexionSQLite
{
    public partial class Frm_Clientes : Form
    {
        private DataTable dtClientes;
        private int currentPosition = 0;
        private int totalRecords = 0;

        public Frm_Clientes()
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
                    string query = "SELECT * FROM clientes ORDER BY id";
                    
                    using (var adapter = new SQLiteDataAdapter(query, conexion))
                    {
                        dtClientes = new DataTable();
                        adapter.Fill(dtClientes);
                        
                        totalRecords = dtClientes.Rows.Count;
                        
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
            if (dtClientes.Rows.Count > 0 && position >= 0 && position < dtClientes.Rows.Count)
            {
                DataRow row = dtClientes.Rows[position];
                
                txtId.Text = row["id"].ToString();
                txtNombre.Text = row["nombre"].ToString();
                txtApellido.Text = row["apellido"].ToString();
                txtTelefono.Text = row["telefono"].ToString();
                txtEmail.Text = row["email"].ToString();
                txtDireccion.Text = row["direccion"].ToString();
                
                currentPosition = position;
                lblPosicion.Text = $"Registro {position + 1} de {totalRecords}";
            }
        }

        private void LimpiarCampos()
        {
            txtId.Text = "";
            txtNombre.Text = "";
            txtApellido.Text = "";
            txtTelefono.Text = "";
            txtEmail.Text = "";
            txtDireccion.Text = "";
            
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
            btnEliminar.Enabled = totalRecords > 0;
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
            if (string.IsNullOrWhiteSpace(txtNombre.Text) || string.IsNullOrWhiteSpace(txtApellido.Text))
            {
                MessageBox.Show("Nombre y apellido son campos obligatorios.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (var conexion = Frm_BasedeDatos.Instance.CrearConexion())
            {
                try
                {
                    conexion.Open();
                    
                    if (txtId.Text == "Nuevo")
                    {
                        // Insert new record
                        string query = @"INSERT INTO clientes (nombre, apellido, telefono, email, direccion) 
                                        VALUES (@nombre, @apellido, @telefono, @email, @direccion)";
                        
                        using (var cmd = new SQLiteCommand(query, conexion))
                        {
                            cmd.Parameters.AddWithValue("@nombre", txtNombre.Text);
                            cmd.Parameters.AddWithValue("@apellido", txtApellido.Text);
                            cmd.Parameters.AddWithValue("@telefono", txtTelefono.Text);
                            cmd.Parameters.AddWithValue("@email", txtEmail.Text);
                            cmd.Parameters.AddWithValue("@direccion", txtDireccion.Text);
                            
                            int rowsAffected = cmd.ExecuteNonQuery();
                            
                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Cliente agregado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                CargarDatos();
                                MostrarRegistro(totalRecords - 1);
                            }
                        }
                    }
                    else
                    {
                        // Update existing record
                        string query = @"UPDATE clientes SET nombre = @nombre, apellido = @apellido, 
                                        telefono = @telefono, email = @email, direccion = @direccion 
                                        WHERE id = @id";
                        
                        using (var cmd = new SQLiteCommand(query, conexion))
                        {
                            cmd.Parameters.AddWithValue("@nombre", txtNombre.Text);
                            cmd.Parameters.AddWithValue("@apellido", txtApellido.Text);
                            cmd.Parameters.AddWithValue("@telefono", txtTelefono.Text);
                            cmd.Parameters.AddWithValue("@email", txtEmail.Text);
                            cmd.Parameters.AddWithValue("@direccion", txtDireccion.Text);
                            cmd.Parameters.AddWithValue("@id", int.Parse(txtId.Text));
                            
                            int rowsAffected = cmd.ExecuteNonQuery();
                            
                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Cliente actualizado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                CargarDatos();
                                MostrarRegistro(currentPosition);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al guardar cliente: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (txtId.Text == "Nuevo" || string.IsNullOrEmpty(txtId.Text))
            {
                MessageBox.Show("No hay un cliente seleccionado para eliminar.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (MessageBox.Show("¿Está seguro de eliminar este cliente?", "Confirmar", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                using (var conexion = Frm_BasedeDatos.Instance.CrearConexion())
                {
                    try
                    {
                        conexion.Open();
                        string query = "DELETE FROM clientes WHERE id = @id";
                        
                        using (var cmd = new SQLiteCommand(query, conexion))
                        {
                            cmd.Parameters.AddWithValue("@id", int.Parse(txtId.Text));
                            
                            int rowsAffected = cmd.ExecuteNonQuery();
                            
                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Cliente eliminado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                
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
                        MessageBox.Show($"Error al eliminar cliente: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}
