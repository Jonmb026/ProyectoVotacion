using System.ComponentModel.DataAnnotations;

namespace ProyectoVotacion.Models
{
    public class Candidato
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; }

        [Required]
        [StringLength(100)]
        public string PrimerApellido { get; set; }

        [StringLength(100)]
        public string SegundoApellido { get; set; }

        [Required]
        [StringLength(50)]
        public string Cedula { get; set; }

        [Required]
        [StringLength(50)]
        public string Cargo { get; set; }  // Ej: "Presidente", "Alcalde", "Diputado"

        [Required]
        [StringLength(100)]
        public string Provincia { get; set; }

        [Required]
        [StringLength(100)]
        public string Partido { get; set; }

        [StringLength(255)]
        public string Eslogan { get; set; }
    }
}
