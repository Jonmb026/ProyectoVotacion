using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoVotacion.Data;
using ProyectoVotacion.Models;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

public class AuthController : Controller
{
    private readonly ApplicationDbContext _context;

    public AuthController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(string email, string password)
    {
        var usuario = await _context.Usuarios
            .SingleOrDefaultAsync(u => u.Email == email && u.Password == password);

        if (usuario != null)
        {
            // Verificar si el usuario es mayor de edad
            if (usuario.ObtenerEdad() < 18)
            {
                ModelState.AddModelError("", "Debes ser mayor de 18 años para votar.");
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim("Id", usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Email),
                new Claim(ClaimTypes.Role, usuario.Rol)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties { IsPersistent = true };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError("", "Email o contraseña incorrectos");
        return View();
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login", "Auth");
    }

    public IActionResult Register()
    {
        ViewBag.Provincias = GetProvincias();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(Usuario usuario)
    {
        if (ModelState.IsValid)
        {
            // Calcular edad
            int edadUsuario = usuario.ObtenerEdad();

            if (edadUsuario >= 18)
            {
                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Login));
            }
            else
            {
                ModelState.AddModelError("", "Debe ser mayor de edad para registrarse.");
            }
        }
        ViewBag.Provincias = GetProvincias();
        return View(usuario);
    }

    // Nueva acción para manejar accesos denegados
    public IActionResult AccessDenied()
    {
        return View();
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
}
