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

        // Muestra la lista de candidatos
        public async Task<IActionResult> Index()
        {
            var candidatos = await _context.Candidatos.ToListAsync();
            return View(candidatos);
        }

        // Muestra el formulario para crear un nuevo candidato
        public IActionResult CreateCandidato()
        {
            ViewBag.Provincias = GetProvincias();
            ViewBag.Cargos = GetCargos();
            return View();
        }

        // Procesa el formulario de creación de candidato
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

        // Muestra el formulario para editar un candidato existente
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

        // Procesa el formulario de edición de candidato
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

        // Muestra el formulario para crear un nuevo usuario
        public IActionResult CreateUsuario()
        {
            ViewBag.Provincias = GetProvincias();
            return View();
        }

        // Procesa el formulario de creación de usuario
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

        // Muestra la lista de usuarios registrados
        public async Task<IActionResult> ManageUsers()
        {
            var usuarios = await _context.Usuarios.ToListAsync();
            return View(usuarios);
        }

        // Muestra el formulario para editar un usuario existente
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

        // Procesa el formulario de edición de usuario
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

        // Elimina un candidato
        [HttpPost]
        public async Task<IActionResult> DeleteCandidato(int id)
        {
            var candidato = await _context.Candidatos.FindAsync(id);
            if (candidato == null)
            {
                return NotFound();
            }

            var votos = await _context.Votos.Where(v => v.CandidatoId == id).ToListAsync();
            if (votos.Any())
            {
                TempData["Error"] = "No se puede eliminar un candidato que tiene votos asociados.";
                return RedirectToAction(nameof(Index));
            }

            _context.Candidatos.Remove(candidato);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Elimina un usuario
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

        // Genera el resultado de la votación
        [HttpPost]
        public async Task<IActionResult> GenerateResult()
        {
            var totalVotos = await _context.Votos.CountAsync();
            var resultados = await _context.Votos
                .GroupBy(v => v.CandidatoId)
                .Select(g => new ResultadoVoto
                {
                    Candidato = _context.Candidatos
                                       .Where(c => c.Id == g.Key)
                                       .Select(c => c.Nombre + " " + c.PrimerApellido)
                                       .FirstOrDefault(),
                    Partido = _context.Candidatos
                                     .Where(c => c.Id == g.Key)
                                     .Select(c => c.Partido)
                                     .FirstOrDefault(),
                    Votos = g.Count(),
                    Porcentaje = (double)g.Count() * 100 / totalVotos
                })
                .OrderByDescending(r => r.Votos)
                .ToListAsync();

            if (resultados.Any())
            {
                var candidatoGanador = resultados.First();
                TempData["MensajeResultado"] = $"El ganador es {candidatoGanador.Candidato} " +
                                                $"con {candidatoGanador.Votos} votos ({candidatoGanador.Porcentaje:F2}%). ¡Felicitaciones!";
            }
            else
            {
                TempData["MensajeResultado"] = "No se encontraron votos.";
            }

            // No guardar objetos complejos en TempData
            TempData["ResultadosGenerados"] = true;

            // Guardar los resultados en una lista dentro del ViewBag
            ViewBag.Resultados = resultados;

            return View("Resultado", resultados);
        }

        // Muestra el resultado final de la votación para Admin y Votantes
        [Authorize(Roles = "Admin,Votante")]
        public async Task<IActionResult> Resultado()
        {
            // Cargar resultados si ya han sido generados, de lo contrario mostrar resultados actuales
            List<ResultadoVoto> resultados;
            if (TempData["ResultadosGenerados"] != null)
            {
                resultados = ViewBag.Resultados;
            }
            else
            {
                var totalVotos = await _context.Votos.CountAsync();
                resultados = await _context.Votos
                    .GroupBy(v => v.CandidatoId)
                    .Select(g => new ResultadoVoto
                    {
                        Candidato = _context.Candidatos
                                           .Where(c => c.Id == g.Key)
                                           .Select(c => c.Nombre + " " + c.PrimerApellido)
                                           .FirstOrDefault(),
                        Partido = _context.Candidatos
                                         .Where(c => c.Id == g.Key)
                                         .Select(c => c.Partido)
                                         .FirstOrDefault(),
                        Votos = g.Count(),
                        Porcentaje = (double)g.Count() * 100 / totalVotos
                    })
                    .OrderByDescending(r => r.Votos)
                    .ToListAsync();
            }

            ViewBag.MensajeResultado = TempData["MensajeResultado"] as string ?? "Resultados actuales de la votación";
            return View(resultados);
        }

        // Métodos auxiliares para obtener listas de provincias y cargos
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
