using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using WebAPILogin.DTOs;
using WebAPILogin.Helpers;
using WebAPILogin.Models;

namespace WebAPILogin.Controllers
{
    [Route("api/[controller]")]
    [AllowAnonymous]
    [ApiController]
    public class AccesoController : ControllerBase
    {
        private readonly WebLoginAngularContext _context;
        private readonly Utilidades _utilidades;
        public AccesoController(WebLoginAngularContext context, Utilidades utilidades)
        {
            _context = context;
            _utilidades = utilidades;
        }


        [HttpPost]
        [Route("Registrarse")]
        public async Task<IActionResult> Registrarse(UsuarioDTO model)
        {

            var usuario = new Usuario
            {
                Nombre = model.Nombre,
                Correo = model.Correo,
                Clave = _utilidades.encriptarSHA256(model.Clave)
            };

            await _context.Usuarios.AddAsync(usuario);
            await _context.SaveChangesAsync();

            if (usuario.IdUsuario == 0)
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = true });
            else
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = false });
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login(UsuarioDTO model)
        {
            var usuarioEncontrado = await _context.Usuarios
                                           .Where(t =>
                                            t.Correo == model.Correo &&
                                            t.Clave == _utilidades.encriptarSHA256(model.Clave))
                                           .FirstOrDefaultAsync();

            if (usuarioEncontrado == null)
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = false, token = "" });
            else
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = true, token = _utilidades.GenerarJWT(usuarioEncontrado) });
        }

        [HttpGet]
        [Route("ValidarToken")]
        public IActionResult ValidarToken([FromQuery] string token)
        {
            bool respuesta = _utilidades.validarToken(token);
            return StatusCode(StatusCodes.Status200OK, new { isSuccess = respuesta});
        }
    }
}