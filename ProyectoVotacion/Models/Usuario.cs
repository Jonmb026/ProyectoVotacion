using System;
using System.ComponentModel.DataAnnotations;

namespace ProyectoVotacion.Models
{
    public class Usuario
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Cedula { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; }

        [Required]
        [StringLength(100)]
        public string PrimerApellido { get; set; }

        [StringLength(100)]
        public string SegundoApellido { get; set; }

        [Required]
        public DateTime FechaNacimiento { get; set; }

        [Required]
        [StringLength(100)]
        public string Provincia { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100)]
        public string Password { get; set; }

        [Required]
        [StringLength(50)]
        public string Rol { get; set; }  // "Admin" o "Votante"

        // Método calculado para obtener la edad
        public int ObtenerEdad()
        {
            int edad = DateTime.Now.Year - FechaNacimiento.Year;
            if (FechaNacimiento > DateTime.Now.AddYears(-edad)) edad--;
            return edad;
        }
    }
}
