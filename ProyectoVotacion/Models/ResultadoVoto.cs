namespace ProyectoVotacion.Models
{
    public class ResultadoVoto
    {
        public string Candidato { get; set; }
        public string Partido { get; set; }
        public int Votos { get; set; }
        public double Porcentaje { get; set; }
    }
}
