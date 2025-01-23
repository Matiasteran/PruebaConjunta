using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace prueba.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConsultorioController : ControllerBase
    {
        private readonly AppDBContext _appDbContext;

        public ConsultorioController(AppDBContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetConsultorios()
        {
            var consultorios = await _appDbContext.Consultorios.ToListAsync();
            return Ok(consultorios);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetConsultorio(int id)
        {
            var consultorio = await _appDbContext.Consultorios.FindAsync(id);

            if (consultorio == null) return Ok("El consultorio no existe");

            return Ok(consultorio);
        }

        [HttpPost]
        public async Task<IActionResult> CreateConsultorio(Consultorio consultorio)
        {
            var validationResult = ValidarConsultorio(consultorio);
            if (validationResult != null)
            {
                return Ok(validationResult);
            }

            _appDbContext.Consultorios.Add(consultorio);
            await _appDbContext.SaveChangesAsync();

            return Ok(consultorio);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> EditConsultorio(int id, Consultorio consultorio)
        {
            if (id != consultorio.Id) return Ok("El ID del consultorio no coincide");

            var consultorioExistente = await _appDbContext.Consultorios.FindAsync(id);
            if (consultorioExistente == null) return Ok("El consultorio no existe");

            var validationResult = ValidarConsultorio(consultorio);
            if (validationResult != null)
            {
                return Ok(validationResult);
            }

            consultorioExistente.Numero = consultorio.Numero;
            consultorioExistente.Piso = consultorio.Piso;

            _appDbContext.Consultorios.Update(consultorioExistente);
            await _appDbContext.SaveChangesAsync();

            return Ok(consultorioExistente);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteConsultorio(int id)
        {
            var consultorio = await _appDbContext.Consultorios.FindAsync(id);
            if (consultorio == null) return Ok("El consultorio no existe");

            _appDbContext.Consultorios.Remove(consultorio);
            await _appDbContext.SaveChangesAsync();

            return Ok(new { message = $"Consultorio con ID {id} eliminado correctamente" });
        }

        private static string? ValidarConsultorio(Consultorio consultorio)
        {
            if (consultorio.Numero <= 0)
            {
                return "El número del consultorio debe ser mayor a cero";
            }

            if (consultorio.Piso <= 0)
            {
                return "El piso del consultorio debe ser mayor a cero";
            }

            return null;
        }
    }
}
