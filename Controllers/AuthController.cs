using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using OtpNet;
using Microsoft.AspNetCore.Http;
using System;
using Eva2ti_EduardoMartinez.Models;

namespace Eva2ti_EduardoMartinez.Controllers
{
    public class AuthController : Controller
    {
        [HttpPost]
        public IActionResult ValidarCredenciales(string rut, string Clave)
        {
            encriptacion encriptador = new encriptacion();
            string claveEncriptada = encriptador.encriptarTexto(Clave);//encriptamos la clave del usuario

            
                
            SqlConnection con = new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=bbd_ejemplo");
            var sentencia = new SqlCommand();
            SqlDataReader dr;
            ViewBag.Error = "";
            sentencia.Connection = con;
            sentencia.CommandText = "SELECT RUT, NOMBRE, ROL FROM CONTACTOS WHERE RUT = @RUT AND CLAVE = @Clave";
            sentencia.Parameters.AddWithValue("@RUT", rut);
            sentencia.Parameters.AddWithValue("@Clave", claveEncriptada);// aqui consultamos que si la clave encriptada coincide con la clave de la tabla


            con.Open();
            dr = sentencia.ExecuteReader();

            if (dr.Read())
            {
                // Guardar los datos del usuario en la sesión
                HttpContext.Session.SetString("RUT", dr["RUT"].ToString());
                HttpContext.Session.SetString("Nombre", dr["NOMBRE"].ToString());
                HttpContext.Session.SetString("ROL", dr["ROL"].ToString().Trim());
                dr.Close();
                con.Close();
                return RedirectToAction("Googleauth", "Auth");
            }
            else
            {
                // Credenciales inválidas
                ViewBag.Error = "Rut o clave incorrectos.";
                dr.Close();
                con.Close();
                return View("/Views/formularios/vista_login.cshtml");
            }
        }

        public IActionResult Googleauth()
        {
            string rut = HttpContext.Session.GetString("RUT");

            if (string.IsNullOrEmpty(rut))
            {
                ModelState.AddModelError("", "No se pudo recuperar el RUT del usuario.");
                return RedirectToAction("Login", "Auth");
            }

            // Recuperar la clave secreta del usuario desde la base de datos
            string secretKeyBase32 = GetSecretKeyForUser(rut);

            if (string.IsNullOrEmpty(secretKeyBase32))
            {
                // Si no hay clave, generamos una nueva y la almacenamos en la base de datos
                var secretBytes = KeyGeneration.GenerateRandomKey(20); // Generar clave secreta
                secretKeyBase32 = Base32Encoding.ToString(secretBytes); // Convertir a Base32
                SaveSecretKeyToDatabase(rut, secretKeyBase32); // Guardar en la base de datos
            }

            // Pasar el código manual a la vista
            ViewBag.ManualCode = secretKeyBase32;

            return View("/Views/formularios/googleaut.cshtml");
        }

        [HttpPost]
        public IActionResult VerifyCode(string authCode)
        {
            string rut = HttpContext.Session.GetString("RUT");

            if (string.IsNullOrEmpty(rut))
            {
                ModelState.AddModelError("", "Usuario no autenticado.");
                return RedirectToAction("Login", "Auth");
            }

            string secretKey = GetSecretKeyForUser(rut);

            if (string.IsNullOrEmpty(secretKey))
            {
                ModelState.AddModelError("", "Clave secreta no encontrada para el usuario.");
                return RedirectToAction("Login", "Auth");
            }

            var secretBytes = Base32Encoding.ToBytes(secretKey);
            var totp = new Totp(secretBytes);

            if (totp.VerifyTotp(authCode, out _, VerificationWindow.RfcSpecifiedNetworkDelay))
            {
                return RedirectToAction("Ventas", "Home"); // Código válido
            }
            else
            {
                ModelState.AddModelError("", "El código ingresado no es válido.");
                return View("/Views/formularios/googleaut.cshtml");
            }
        }

        private void SaveSecretKeyToDatabase(string rut, string secretKeyBase32)
        {
            if (string.IsNullOrEmpty(rut))
            {
                throw new ArgumentException("El RUT no puede ser nulo o vacío.");
            }

            using (SqlConnection con = new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=bbd_ejemplo"))
            {
                var sentencia = new SqlCommand();
                sentencia.Connection = con;
                sentencia.CommandText = "UPDATE CONTACTOS SET KeyGoogle = @KeyGoogle WHERE RUT = @rut";
                sentencia.Parameters.AddWithValue("@KeyGoogle", secretKeyBase32);
                sentencia.Parameters.AddWithValue("@rut", rut);

                con.Open();
                sentencia.ExecuteNonQuery();
                con.Close();
            }
        }

        private string GetSecretKeyForUser(string rut)
        {
            string secretKey = null;
            SqlConnection con = new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=bbd_ejemplo");
            var sentencia = new SqlCommand();

            sentencia.Connection = con;
            sentencia.CommandText = "SELECT KeyGoogle FROM CONTACTOS WHERE RUT = @RUT";
            sentencia.Parameters.AddWithValue("@RUT", rut);

            try
            {
                con.Open();
                var result = sentencia.ExecuteScalar();
                if (result != null)
                {
                    secretKey = result.ToString();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener la clave secreta: {ex.Message}");
            }
            finally
            {
                con.Close();
            }

            return secretKey;
        }

        public IActionResult Login()
        {
            return View("/Views/formularios/vista_login.cshtml");
        }

        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Auth");
        }
    }
}
