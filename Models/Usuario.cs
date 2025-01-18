
using System;

namespace Eva2ti_EduardoMartinez.Models
{
    public class Usuario
    {
        public string RUT { get; set; }
        
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Correo { get; set; }
        public string Rol { get; set; }

        public Usuario()
        {
    
        
        } // Constructor vacío, requerido para inicialización.
    }
}