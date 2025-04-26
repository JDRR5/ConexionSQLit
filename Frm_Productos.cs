using System;
using System.Data;
using System.Data.SQLite;
using System.Windows.Forms;

namespace ConexionSQLite
{
    public partial class Frm_Productos : Form
    {
        private DataTable dtProductos;
        private int currentPosition = 0;
        private int totalRecords = 0;

        public Frm_Productos()
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
                    string query = "SELECT * FROM productos ORDER BY id";
                    
                    using (var adapter = new SQLiteDataAdapter(query, conexion))
                    {
                        dtProductos = new DataTable();
                        adapter.Fill(dtProductos);
                        
                        totalRecords = dtProductos.Rows.Count;
                        
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
            if (dtProductos.Rows.Count > 0 && position >= 0 && position < dtProductos.Rows.Count)
            {
                DataRow row = dtProductos.Rows[position];
                
                txtId.Text = row["id"].ToString();
                txtNombre.Text = row["nombre"].ToString();
                txtDescripcion.Text = row["descripcion"].ToString();
                txtPrecio.Text = row["precio"].ToString();
                txtStock.Text = row["stock"].ToString();
                
                currentPosition = position;
                lblPosicion.Text = $"Registro {position + 1} de {totalRecords}";
            }
        }

        private void LimpiarCampos()
        {
            txtId.Text = "";
            txtNombre.Text = "";
            txtDescripcion.Text = "";
            txtPrecio.Text = "";
            txtStock.Text = "";
            
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
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("El nombre del producto es obligatorio.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            decimal precio;
            if (!decimal.TryParse(txtPrecio.Text, out precio) || precio < 0)
            {
                MessageBox.Show("El precio debe ser un número válido mayor o igual a cero.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int stock;
            if (!int.TryParse(txtStock.Text, out stock) || stock < 0)
            {
                MessageBox.Show("El stock debe ser un número entero válido mayor o igual a cero.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                        string query = @"INSERT INTO productos (nombre, descripcion, precio, stock) 
                                        VALUES (@nombre, @descripcion, @precio, @stock)";
                        
                        using (var cmd = new SQLiteCommand(query, conexion))
                        {
                            cmd.Parameters.AddWithValue("@nombre", txtNombre.Text);
                            cmd.Parameters.AddWithValue("@descripcion", txtDescripcion.Text);
                            cmd.Parameters.AddWithValue("@precio", precio);
                            cmd.Parameters.AddWithValue("@stock", stock);
                            
                            int rowsAffected = cmd.ExecuteNonQuery();
                            
                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Producto agregado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                CargarDatos();
                                MostrarRegistro(totalRecords - 1);
                            }
                        }
                    }
                    else
                    {
                        // Update existing record
                        string query = @"UPDATE productos SET nombre = @nombre, descripcion = @descripcion, 
                                        precio = @precio, stock = @stock 
                                        WHERE id = @id";
                        
                        using (var cmd = new SQLiteCommand(query, conexion))
                        {
                            cmd.Parameters.AddWithValue("@nombre", txtNombre.Text);
                            cmd.Parameters.AddWithValue("@descripcion", txtDescripcion.Text);
                            cmd.Parameters.AddWithValue("@precio", precio);
                            cmd.Parameters.AddWithValue("@stock", stock);
                            cmd.Parameters.AddWithValue("@id", int.Parse(txtId.Text));
                            
                            int rowsAffected = cmd.ExecuteNonQuery();
                            
                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Producto actualizado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                CargarDatos();
                                MostrarRegistro(currentPosition);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al guardar producto: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (txtId.Text == "Nuevo" || string.IsNullOrEmpty(txtId.Text))
            {
                MessageBox.Show("No hay un producto seleccionado para eliminar.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (MessageBox.Show("¿Está seguro de eliminar este producto?", "Confirmar", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                using (var conexion = Frm_BasedeDatos.Instance.CrearConexion())
                {
                    try
                    {
                        conexion.Open();
                        string query = "DELETE FROM productos WHERE id = @id";
                        
                        using (var cmd = new SQLiteCommand(query, conexion))
                        {
                            cmd.Parameters.AddWithValue("@id", int.Parse(txtId.Text));
                            
                            int rowsAffected = cmd.ExecuteNonQuery();
                            
                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Producto eliminado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                
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
                        MessageBox.Show($"Error al eliminar producto: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}
