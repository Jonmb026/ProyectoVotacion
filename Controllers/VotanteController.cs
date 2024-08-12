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

        public async Task<IActionResult> Votar()
        {
            var candidatos = await _context.Candidatos.ToListAsync();
            return View(candidatos);
        }

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

        [Authorize(Roles = "Admin, Votante")]
        public IActionResult ResultadoFinal()
        {
            var resultados = TempData["Resultados"] as List<ResultadoVoto>;
            var mensajeResultado = TempData["MensajeResultado"] as string;

            if (resultados == null)
            {
                return RedirectToAction("Index", "Home"); // Redirige si no hay resultados generados
            }

            ViewBag.MensajeResultado = mensajeResultado;
            return View(resultados);
        }

        private int ObtenerUsuarioAutenticadoId()
        {
            return int.Parse(User.FindFirst("Id").Value);
        }
    }
}
