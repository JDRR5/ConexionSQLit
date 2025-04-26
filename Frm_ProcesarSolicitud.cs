using System;
using System.Data.SQLite;
using System.Windows.Forms;

namespace ConexionSQLite
{
    // Context class for the State pattern
    public class Frm_ProcesarSolicitud
    {
        private IEstado estadoActual;
        private Frm_Registrarse formulario;

        public Frm_ProcesarSolicitud(Frm_Registrarse form)
        {
            formulario = form;
            estadoActual = new Frm_ValidarCredenciales();
        }

        public void CambiarEstado(IEstado nuevoEstado)
        {
            estadoActual = nuevoEstado;
            formulario.ActualizarInterfazDesdeContext();
        }

        public string GetTitulo()
        {
            return estadoActual.GetTitulo();
        }

        public string GetBotonIniciarTexto()
        {
            return estadoActual.GetBotonIniciarTexto();
        }

        public string GetBotonRegistrarTexto()
        {
            return estadoActual.GetBotonRegistrarTexto();
        }

        public void ProcesarSolicitud(string username, string password, int? idRol)
        {
            estadoActual.ProcesarSolicitud(this, username, password, idRol);
        }
    }

    // State interface
    public interface IEstado
    {
        string GetTitulo();
        string GetBotonIniciarTexto();
        string GetBotonRegistrarTexto();
        void ProcesarSolicitud(Frm_ProcesarSolicitud context, string username, string password, int? idRol);
    }

    // Concrete state for login
    public class Frm_ValidarCredenciales : IEstado
    {
        public string GetTitulo()
        {
            return "Inicio de Sesión";
        }

        public string GetBotonIniciarTexto()
        {
            return "Iniciar Sesión";
        }

        public string GetBotonRegistrarTexto()
        {
            return "Registrarse";
        }

        public void ProcesarSolicitud(Frm_ProcesarSolicitud context, string username, string password, int? idRol)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Por favor, ingrese usuario y contraseña.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (var conexion = Frm_BasedeDatos.Instance.CrearConexion())
            {
                try
                {
                    conexion.Open();
                    using (var cmd = new SQLiteCommand("SELECT id, id_rol FROM usuarios WHERE username = @username AND password = @password", conexion))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        cmd.Parameters.AddWithValue("@password", password);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int userId = reader.GetInt32(0);
                                int rolId = reader.GetInt32(1);
                                
                                // Login successful, open main menu
                                Frm_MenuPrincipal menuPrincipal = new Frm_MenuPrincipal(userId, rolId);
                                menuPrincipal.Show();
                                
                                // Hide login form
                                Form loginForm = Application.OpenForms["Frm_Registrarse"];
                                if (loginForm != null)
                                {
                                    loginForm.Hide();
                                }
                            }
                            else
                            {
                                MessageBox.Show("Usuario o contraseña incorrectos.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al iniciar sesión: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }

    // Concrete state for registration
    public class Frm_EstadoRegistro : IEstado
    {
        public string GetTitulo()
        {
            return "Registro de Usuario";
        }

        public string GetBotonIniciarTexto()
        {
            return "Registrar";
        }

        public string GetBotonRegistrarTexto()
        {
            return "Volver a Inicio";
        }

        public void ProcesarSolicitud(Frm_ProcesarSolicitud context, string username, string password, int? idRol)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || !idRol.HasValue)
            {
                MessageBox.Show("Por favor, complete todos los campos.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (var conexion = Frm_BasedeDatos.Instance.CrearConexion())
            {
                try
                {
                    conexion.Open();
                    
                    // Check if username already exists
                    using (var cmdCheck = new SQLiteCommand("SELECT COUNT(*) FROM usuarios WHERE username = @username", conexion))
                    {
                        cmdCheck.Parameters.AddWithValue("@username", username);
                        int count = Convert.ToInt32(cmdCheck.ExecuteScalar());
                        
                        if (count > 0)
                        {
                            MessageBox.Show("El nombre de usuario ya existe. Por favor, elija otro.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                    
                    // Insert new user
                    using (var cmd = new SQLiteCommand("INSERT INTO usuarios (username, password, id_rol) VALUES (@username, @password, @idRol)", conexion))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        cmd.Parameters.AddWithValue("@password", password);
                        cmd.Parameters.AddWithValue("@idRol", idRol.Value);
                        
                        int rowsAffected = cmd.ExecuteNonQuery();
                        
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Usuario registrado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            context.CambiarEstado(new Frm_ValidarCredenciales());
                        }
                        else
                        {
                            MessageBox.Show("No se pudo registrar el usuario.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al registrar usuario: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
