using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoVotacion.Data;
using ProyectoVotacion.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProyectoVotacion.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var candidatos = await _context.Candidatos.ToListAsync();
            return View(candidatos);
        }

        public IActionResult CreateCandidato()
        {
            ViewBag.Provincias = GetProvincias();
            ViewBag.Cargos = GetCargos();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateCandidato(Candidato candidato)
        {
            if (ModelState.IsValid)
            {
                _context.Candidatos.Add(candidato);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Provincias = GetProvincias();
            ViewBag.Cargos = GetCargos();
            return View(candidato);
        }

        public async Task<IActionResult> EditCandidato(int id)
        {
            var candidato = await _context.Candidatos.FindAsync(id);
            if (candidato == null)
            {
                return NotFound();
            }
            ViewBag.Provincias = GetProvincias();
            ViewBag.Cargos = GetCargos();
            return View(candidato);
        }

        [HttpPost]
        public async Task<IActionResult> EditCandidato(Candidato candidato)
        {
            if (ModelState.IsValid)
            {
                _context.Update(candidato);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Provincias = GetProvincias();
            ViewBag.Cargos = GetCargos();
            return View(candidato);
        }

        public IActionResult CreateUsuario()
        {
            ViewBag.Provincias = GetProvincias();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateUsuario(Usuario usuario)
        {
            if (ModelState.IsValid)
            {
                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ManageUsers));
            }
            ViewBag.Provincias = GetProvincias();
            return View(usuario);
        }

        public async Task<IActionResult> ManageUsers()
        {
            var usuarios = await _context.Usuarios.ToListAsync();
            return View(usuarios);
        }

        public async Task<IActionResult> EditUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }
            ViewBag.Provincias = GetProvincias();
            return View(usuario);
        }

        [HttpPost]
        public async Task<IActionResult> EditUsuario(Usuario usuario)
        {
            if (ModelState.IsValid)
            {
                _context.Update(usuario);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ManageUsers));
            }
            ViewBag.Provincias = GetProvincias();
            return View(usuario);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCandidato(int id)
        {
            var candidato = await _context.Candidatos.FindAsync(id);
            if (candidato != null)
            {
                _context.Candidatos.Remove(candidato);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario != null)
            {
                _context.Usuarios.Remove(usuario);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(ManageUsers));
        }

        [HttpPost]
        public async Task<IActionResult> GenerateResult()
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

            TempData["Resultados"] = resultados;
            TempData["MensajeResultado"] = resultados.Any()
                ? $"El ganador es {resultados.First().Candidato} con {resultados.First().Votos} votos ({resultados.First().Porcentaje:F2}%). ¡Felicitaciones!"
                : "No se encontraron votos.";

            return RedirectToAction(nameof(ResultadoFinal));
        }

        [Authorize(Roles = "Admin, Votante")]
        public IActionResult ResultadoFinal()
        {
            var resultados = TempData["Resultados"] as List<ResultadoVoto>;
            var mensajeResultado = TempData["MensajeResultado"] as string;

            if (resultados == null)
            {
                mensajeResultado = "No se encontraron resultados o no se han generado aún.";
            }

            ViewBag.MensajeResultado = mensajeResultado;
            return View(resultados);
        }

        private List<string> GetProvincias()
        {
            return new List<string>
            {
                "San José",
                "Alajuela",
                "Cartago",
                "Heredia",
                "Guanacaste",
                "Puntarenas",
                "Limón"
            };
        }

        private List<string> GetCargos()
        {
            return new List<string>
            {
                "Presidente",
                "Alcalde",
                "Diputado"
            };
        }
    }
}
