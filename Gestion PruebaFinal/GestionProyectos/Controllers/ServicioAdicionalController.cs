using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace prueba.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServicioAdicionalController : ControllerBase
    {
        private readonly AppDBContext _appDbContext;

        public ServicioAdicionalController(AppDBContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetServiciosAdicionales()
        {
            var servicios = await _appDbContext.ServiciosAdicionales
                .Include(s => s.Reserva)
                .ThenInclude(r => r.Cliente)
                .Include(s => s.Reserva.Habitacion)
                .ToListAsync();
            return Ok(servicios);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetServicioAdicional(int id)
        {
            var servicio = await _appDbContext.ServiciosAdicionales
                .Include(s => s.Reserva)
                .ThenInclude(r => r.Cliente)
                .Include(s => s.Reserva.Habitacion)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (servicio == null) return Ok("El servicio no existe");

            return Ok(servicio);
        }

        [HttpPost]
        public async Task<IActionResult> CreateServicioAdicional(ServicioAdicional servicio)
        {
            if (string.IsNullOrWhiteSpace(servicio.Descripcion))
            {
                return Ok("La descripcion del servicio es obligatoria");
            }

            if (!servicio.Descripcion.All(char.IsLetter))
            {
                return Ok("La descripcion solo debe contener letras");
            }

            if (servicio.Costo <= 0 || servicio.Costo > 200)
            {
                return Ok("El costo debe ser entre 0 y 200");
            }

            var reserva = await _appDbContext.Reservas.FindAsync(servicio.Reserva.Id);
            if (reserva == null)
            {
                return Ok("La reserva asociada no existe");
            }

            servicio.Reserva = reserva;

            _appDbContext.ServiciosAdicionales.Add(servicio);
            await _appDbContext.SaveChangesAsync();

            return Ok(servicio);

        }

        [HttpPut("{id}")]
        public async Task<IActionResult> EditServicioAdicional(int id, ServicioAdicional servicio)
        {
            if (id != servicio.Id) return Ok("El ID del servicio adicional no coincide");

            var servicioExistente = await _appDbContext.ServiciosAdicionales
                .Include(s => s.Reserva)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (servicioExistente == null) return Ok("El servicio adicional no existe");

            if (string.IsNullOrWhiteSpace(servicio.Descripcion))
            {
                return Ok("La descripcion del servicio es obligatoria");
            }

            if (servicio.Costo <= 0 || servicio.Costo > 200)
            {
                return Ok("El costo debe ser entre 0 y 200");
            }

            var reserva = await _appDbContext.Reservas.FindAsync(servicio.Reserva.Id);
            if (reserva == null)
            {
                return Ok("La reserva asociada no existe.");
            }

            servicioExistente.Descripcion = servicio.Descripcion;
            servicioExistente.Costo = servicio.Costo;
            servicioExistente.Reserva = reserva;

            _appDbContext.ServiciosAdicionales.Update(servicioExistente);
            await _appDbContext.SaveChangesAsync();

            return Ok(servicioExistente);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteServicioAdicional(int id)
        {
            var servicio = await _appDbContext.ServiciosAdicionales.FindAsync(id);
            if (servicio == null) return Ok("El servicio adicional no existe");

            _appDbContext.ServiciosAdicionales.Remove(servicio);
            await _appDbContext.SaveChangesAsync();

            return Ok(new { message = $"Servicio adicional con ID {id} eliminado correctamente" });
        }
    }
}
