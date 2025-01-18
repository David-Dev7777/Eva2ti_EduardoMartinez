using Microsoft.Identity.Client;
using System.IO;
using System.Security.Cryptography;
using System.Text;


namespace Eva2ti_EduardoMartinez.Models
{
    public class encriptacion
    {

        public string encriptarTexto(string texto) {

            string hash = "divide y venceras"; //frase de crifrado
            byte[] data = Encoding.UTF8.GetBytes(texto);//bytes del texto a encriptar

            MD5 md5 = MD5.Create();//instancia md5
            TripleDES tripleDES = TripleDES.Create();

            tripleDES.Key = md5.ComputeHash(UTF32Encoding.UTF8.GetBytes(hash));//codificacion de nuestro hash
            tripleDES.Mode = CipherMode.ECB;//cifrado

            ICryptoTransform transformar = tripleDES.CreateEncryptor();//el transform recibe todas la configuraciones del tripleDES
            byte[] resultado = transformar.TransformFinalBlock(data, 0, data.Length);



            return Convert.ToBase64String(resultado);
        }

        public string desEncriptacion(string textoEncrip) {

            string hash = "divide y venceras"; //frase de crifrado
            byte[] data = Convert.FromBase64String(textoEncrip);//convertimos el texto que ya esta encriptado

            MD5 md5 = MD5.Create();//instancia md5
            TripleDES tripleDES = TripleDES.Create();

            tripleDES.Key = md5.ComputeHash(UTF32Encoding.UTF8.GetBytes(hash));//codificacion de nuestro hash
            tripleDES.Mode = CipherMode.ECB;//cifrado

            ICryptoTransform transformar = tripleDES.CreateDecryptor();//el transform recibe todas la configuraciones del tripleDES
            byte[] resultado = transformar.TransformFinalBlock(data, 0, data.Length);



            return UTF8Encoding.UTF8.GetString(resultado);
        }

    }
  
    
}
