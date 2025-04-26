using System;
using System.Data.SQLite;
using System.IO;
using System.Windows.Forms;

namespace ConexionSQLite
{
    public class Frm_BasedeDatos
    {
        private static Frm_BasedeDatos _instance;
        private string connectionString;

        // Singleton pattern
        public static Frm_BasedeDatos Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Frm_BasedeDatos();
                }
                return _instance;
            }
        }

        private Frm_BasedeDatos()
        {
            string dbPath = "database.db";
            connectionString = $"Data Source={dbPath};Version=3;";
        }

        public SQLiteConnection CrearConexion()
        {
            return new SQLiteConnection(connectionString);
        }

        public void InicializarBaseDeDatos()
        {
            bool dbExists = File.Exists("database.db");
            
            if (!dbExists)
            {
                SQLiteConnection.CreateFile("database.db");
            }

            using (var connection = CrearConexion())
            {
                connection.Open();

                // Create tables if they don't exist
                using (var command = new SQLiteCommand(connection))
                {
                    // Roles table
                    command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS roles (
                            id INTEGER PRIMARY KEY AUTOINCREMENT,
                            nombre TEXT NOT NULL UNIQUE
                        )";
                    command.ExecuteNonQuery();

                    // Users table
                    command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS usuarios (
                            id INTEGER PRIMARY KEY AUTOINCREMENT,
                            username TEXT NOT NULL UNIQUE,
                            password TEXT NOT NULL,
                            id_rol INTEGER,
                            FOREIGN KEY (id_rol) REFERENCES roles(id)
                        )";
                    command.ExecuteNonQuery();

                    // Clients table
                    command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS clientes (
                            id INTEGER PRIMARY KEY AUTOINCREMENT,
                            nombre TEXT NOT NULL,
                            apellido TEXT NOT NULL,
                            telefono TEXT,
                            email TEXT,
                            direccion TEXT
                        )";
                    command.ExecuteNonQuery();

                    // Products table
                    command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS productos (
                            id INTEGER PRIMARY KEY AUTOINCREMENT,
                            nombre TEXT NOT NULL,
                            descripcion TEXT,
                            precio REAL NOT NULL,
                            stock INTEGER NOT NULL
                        )";
                    command.ExecuteNonQuery();

                    // Insert default roles if the database was just created
                    if (!dbExists)
                    {
                        command.CommandText = "INSERT INTO roles (nombre) VALUES ('administrador')";
                        command.ExecuteNonQuery();
                        
                        command.CommandText = "INSERT INTO roles (nombre) VALUES ('almacenista')";
                        command.ExecuteNonQuery();
                        
                        command.CommandText = "INSERT INTO roles (nombre) VALUES ('vendedor')";
                        command.ExecuteNonQuery();
                        
                        // Create a default admin user
                        command.CommandText = "INSERT INTO usuarios (username, password, id_rol) VALUES ('admin', 'admin', 1)";
                        command.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}
