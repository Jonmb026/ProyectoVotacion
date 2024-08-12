using System;
using System.ComponentModel.DataAnnotations;

namespace ProyectoVotacion.Models
{
    public class Voto
    {
        public int Id { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        [Required]
        public int CandidatoId { get; set; }

        [Required]
        public DateTime FechaVoto { get; set; }

        public Usuario Usuario { get; set; }
        public Candidato Candidato { get; set; }
    }
}
