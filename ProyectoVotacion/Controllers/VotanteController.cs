using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoVotacion.Data;
using ProyectoVotacion.Models;
using System.Linq;
using System.Threading.Tasks;

namespace ProyectoVotacion.Controllers
{
    [Authorize(Roles = "Votante")]
    public class VotanteController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VotanteController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Muestra la lista de candidatos para votar
        public async Task<IActionResult> Votar()
        {
            var candidatos = await _context.Candidatos.ToListAsync();
            return View(candidatos);
        }

        // Procesa el voto del votante
        [HttpPost]
        public async Task<IActionResult> Votar(int candidatoId)
        {
            var usuarioId = ObtenerUsuarioAutenticadoId();
            var yaVoto = await _context.Votos.AnyAsync(v => v.UsuarioId == usuarioId);

            if (yaVoto)
            {
                TempData["Error"] = "Ya has votado.";
                return RedirectToAction(nameof(Votar));
            }

            var voto = new Voto
            {
                CandidatoId = candidatoId,
                UsuarioId = usuarioId
            };

            _context.Votos.Add(voto);
            await _context.SaveChangesAsync();

            TempData["Success"] = "¡Voto registrado con éxito!";
            return RedirectToAction(nameof(Votar));
        }

        // Muestra el resultado final de la votación para los votantes
        public async Task<IActionResult> Resultado()
        {
            var totalVotos = await _context.Votos.CountAsync();
            var resultados = await _context.Votos
                .GroupBy(v => v.CandidatoId)
                .Select(g => new ResultadoVoto
                {
                    Candidato = _context.Candidatos.Where(c => c.Id == g.Key).Select(c => c.Nombre + " " + c.PrimerApellido).FirstOrDefault(),
                    Partido = _context.Candidatos.Where(c => c.Id == g.Key).Select(c => c.Partido).FirstOrDefault(),
                    Votos = g.Count(),
                    Porcentaje = (double)g.Count() * 100 / totalVotos
                })
                .OrderByDescending(r => r.Votos)
                .ToListAsync();

            ViewBag.MensajeResultado = TempData["MensajeResultado"];
            return View(resultados);
        }

        // Método auxiliar para obtener el ID del usuario autenticado
        private int ObtenerUsuarioAutenticadoId()
        {
            return int.Parse(User.FindFirst("Id").Value);
        }
    }
}
