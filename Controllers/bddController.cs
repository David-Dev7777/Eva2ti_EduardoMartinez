using Microsoft.AspNetCore.Mvc;
using System;
using System.Data.SqlClient;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Drawing;
using Eva2ti_EduardoMartinez.Models;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Eva2ti_EduardoMartinez.Controllers
{
    public class bddController : Controller
    {
        public IActionResult ver_formulario()
        {
            return View("/Views/bbd/vista_bbd.cshtml");
        }

        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult mostrar()
        {
            // Verificar si la sesión está activa
            if (HttpContext.Session.GetString("RUT") == null)
            {
                return RedirectToAction("Login", "Auth");
            }
            else
            {
                SqlConnection con = new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=bbd_ejemplo");
                var sentencia = new SqlCommand();
                SqlDataReader dr;

                sentencia.Connection = con;
                sentencia.CommandText = "SELECT RUT, NOMBRE, APELLIDO, CORREO, ROL FROM CONTACTOS";
                sentencia.CommandType = System.Data.CommandType.Text;

                var tablaUsuarios = "";

                con.Open();
                dr = sentencia.ExecuteReader();

                while (dr.Read())
                {
                    var rut = dr["RUT"].ToString(); // Extraer RUT para generar enlaces
                    tablaUsuarios += "<tr>";
                    tablaUsuarios += $"<td>{dr["RUT"]}</td>";
                    tablaUsuarios += $"<td>{dr["NOMBRE"]}</td>";
                    tablaUsuarios += $"<td>{dr["APELLIDO"]}</td>";
                    tablaUsuarios += $"<td>{dr["CORREO"]}</td>";
                    tablaUsuarios += $"<td>{dr["ROL"]}</td>";
                    tablaUsuarios += $"<td><a href='/bdd/modificar?rut={rut}' class='btn btn-warning btn-sm'>Modificar</a> " +
                                     $"<a href='/bdd/eliminar?rut={rut}' class='btn btn-danger btn-sm'>Eliminar</a></td>";
                    tablaUsuarios += "</tr>";
                }

                dr.Close();
                con.Close();

                ViewBag.TablaUsuarios = tablaUsuarios;

                return View("~/Views/formularios/Admin_usu.cshtml");
            }
        }
        
        public IActionResult modificar(string rut)
        {
            SqlConnection con = new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=bbd_ejemplo");
            var sentencia = new SqlCommand();
            SqlDataReader dr;

            sentencia.Connection = con;
            sentencia.CommandText = "SELECT RUT, NOMBRE, APELLIDO, CORREO, ROL FROM CONTACTOS WHERE RUT = @RUT";
            sentencia.Parameters.AddWithValue("@RUT", rut);
            sentencia.CommandType = System.Data.CommandType.Text;

            Usuario usuario = null;

            con.Open();
            dr = sentencia.ExecuteReader();

            if (dr.Read())
            {
                usuario = new Usuario
                {
                    RUT = dr["RUT"].ToString(),
                    Nombre = dr["NOMBRE"].ToString(),
                    Apellido = dr["APELLIDO"].ToString(),
                    Correo = dr["CORREO"].ToString(),
                    Rol = dr["ROL"].ToString()
                };
            }

            dr.Close();
            con.Close();

            if (usuario == null)
            {
                return NotFound("Usuario no encontrado.");
            }

            return View("~/Views/formularios/Edit_usu.cshtml", usuario);
        }

        
        public IActionResult guardarModificacion(Usuario usuario)
        {
            SqlConnection con = new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=bbd_ejemplo");
            var sentencia = new SqlCommand();

            sentencia.Connection = con;
            sentencia.CommandText = "UPDATE CONTACTOS SET NOMBRE = UPPER(@NOMBRE), APELLIDO = UPPER(@APELLIDO), CORREO = @CORREO, ROL = @ROL WHERE RUT = @RUT";
            sentencia.Parameters.AddWithValue("@RUT", usuario.RUT);
            sentencia.Parameters.AddWithValue("@NOMBRE", usuario.Nombre);
            sentencia.Parameters.AddWithValue("@APELLIDO", usuario.Apellido);
            sentencia.Parameters.AddWithValue("@CORREO", usuario.Correo);
            sentencia.Parameters.AddWithValue("@ROL", usuario.Rol);

            sentencia.CommandType = System.Data.CommandType.Text;

            con.Open();
            sentencia.ExecuteNonQuery();
            con.Close();

            return RedirectToAction("mostrar");
        }


        [HttpPost]
        public IActionResult guardarNuevoUsuario(string RUT, string Nombre, string Apellido, string Correo, string Rol, string Clave)
        {

            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
            // Asegurar que el RUT tenga el formato correcto
            if (RUT.Length == 9)
            {
                RUT = "0" + RUT;
            }

            if (RUT.Length != 10 || RUT[8] != '-')
            {
                ViewBag.Mensaje = "El RUT debe tener el formato correcto (ejemplo: 12345678-9).";
                return View("~/Views/formularios/Agregar_usuario.cshtml");
            }

            string parteAntesDelGuion = RUT.Substring(0, 8);
            string digitoVerificador = RUT[^1].ToString().ToUpper();

            if (!EsNumero(parteAntesDelGuion))
            {
                ViewBag.Mensaje = "La parte antes del guion debe contener solo números.";
                return View("~/Views/formularios/Agregar_usuario.cshtml");
            }

            // Validar el dígito verificador
            if (!ValidarDigitoVerificador(parteAntesDelGuion, digitoVerificador))
            {
                ViewBag.Mensaje = "RUT inválido. El dígito verificador no coincide.";
                return View("~/Views/formularios/Agregar_usuario.cshtml");
            }

            // Verificar existencia del RUT en la base de datos
            string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=bbd_ejemplo";
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT COUNT(*) FROM CONTACTOS WHERE RUT = @RUT";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@RUT", RUT);

                con.Open();
                int count = (int)cmd.ExecuteScalar();
                con.Close();

                if (count > 0)
                {
                    ViewBag.Mensaje = "El RUT ya está registrado en la base de datos.";
                    return View("~/Views/formularios/Agregar_usuario.cshtml");
                }
            }

            // Generar una clave secreta aleatoria en formato Base32
            var secretBytes = OtpNet.KeyGeneration.GenerateRandomKey(20); // Generar clave secreta de 20 bytes
            var KeyGoogle = OtpNet.Base32Encoding.ToString(secretBytes); // Convertir clave secreta a formato Base32

            encriptacion encriptador = new encriptacion();//creamos un objecto tipo encriptacion
            string claveEncriptada = encriptador.encriptarTexto(Clave); // encriptamos el texto con ayuda del metodo encriptarTexto

            using (SqlConnection con = new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=bbd_ejemplo"))
            {
                var sentencia = new SqlCommand();
                sentencia.Connection = con;
                sentencia.CommandText = @"
            INSERT INTO CONTACTOS (RUT, NOMBRE, APELLIDO, CORREO, ROL, CLAVE, KeyGoogle) 
            VALUES (@RUT, UPPER(@NOMBRE), UPPER(@APELLIDO), @CORREO, @ROL, @CLAVE, @KeyGoogle)";
                sentencia.Parameters.AddWithValue("@RUT", RUT);
                sentencia.Parameters.AddWithValue("@NOMBRE", Nombre);
                sentencia.Parameters.AddWithValue("@APELLIDO", Apellido);
                sentencia.Parameters.AddWithValue("@CORREO", Correo);
                sentencia.Parameters.AddWithValue("@ROL", Rol);
                sentencia.Parameters.AddWithValue("@CLAVE", claveEncriptada);
                sentencia.Parameters.AddWithValue("@KeyGoogle", KeyGoogle);

                con.Open();
                sentencia.ExecuteNonQuery();
                con.Close();
            }

            
            // Redirigir a la vista de administración para confirmar la adición
            return RedirectToAction("mostrar");
        }

        private bool ValidarDigitoVerificador(string parteNumerica, string digitoVerificador)
        {
            int suma = 0;
            int[] multiplicadores = { 3, 2, 7, 6, 5, 4, 3, 2 };

            for (int i = 0; i < 8; i++)
            {
                suma += int.Parse(parteNumerica[i].ToString()) * multiplicadores[i];
            }

            int resto = suma % 11;
            int digito = 11 - resto;

            string digitoCalculado = digito switch
            {
                10 => "K",
                11 => "0",
                _ => digito.ToString()
            };

            return digitoCalculado == digitoVerificador;
        }

        private bool EsNumero(string str)
        {
            return str.All(char.IsDigit);
        }

        public IActionResult eliminar(string rut)
        {
            SqlConnection con = new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=bbd_ejemplo");
            var sentencia = new SqlCommand();

            sentencia.Connection = con;
            sentencia.CommandText = "DELETE FROM CONTACTOS WHERE RUT = @RUT";
            sentencia.CommandType = System.Data.CommandType.Text;

            sentencia.Parameters.AddWithValue("@RUT", rut);

            con.Open();
            sentencia.ExecuteNonQuery();
            con.Close();

            // Redirigir a la acción mostrar para actualizar la tabla
            return RedirectToAction("mostrar");
        }




    }
}



