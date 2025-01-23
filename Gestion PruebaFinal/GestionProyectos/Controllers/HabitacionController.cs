using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace prueba.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HabitacionController : ControllerBase
    {
        private readonly AppDBContext _appDbContext;

        public HabitacionController(AppDBContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetHabitaciones()
        {
            var habitaciones = await _appDbContext.Habitaciones.ToListAsync();
            return Ok(habitaciones);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetHabitacion(int id)
        {
            var habitacion = await _appDbContext.Habitaciones.FindAsync(id);
            if (habitacion == null) return Ok("La habitación no existe");
            return Ok(habitacion);
        }

        [HttpPost]
        public async Task<IActionResult> CreateHabitacion(Habitacion habitacion)
        {
            if (habitacion.Capacidad == null || habitacion.Capacidad <= 0 || habitacion.Capacidad >= 5)
            {
                return Ok("La capacidad debe ser entre 1 y 4");
            }

            if (habitacion.NumeroDeHabitacion == null || habitacion.NumeroDeHabitacion <= 0 || habitacion.NumeroDeHabitacion >300)
            {
                return Ok("El número de habitación debe ser mayor a 0 y menor a 300");
            }

            var habitacionExistente = await _appDbContext.Habitaciones.FirstOrDefaultAsync(h => h.NumeroDeHabitacion == habitacion.NumeroDeHabitacion);
            if (habitacionExistente != null)
            {
                return Ok("Ya existe una habitación con el mismo número.");
            }

            if (habitacion.Costo <= 10 || habitacion.Costo > 300)
            {
                return Ok("El costo debe ser entre 20 y 300");
            }


            if (habitacion.Piso != null && habitacion.Piso < 0 || habitacion.Piso > 10)
            {
                return Ok("El piso debe ser entre 0 y 10");
            }

            _appDbContext.Habitaciones.Add(habitacion);
            await _appDbContext.SaveChangesAsync();
            return Ok(habitacion);

        }

        [HttpPut("{id}")]
        public async Task<IActionResult> EditHabitacion(int id, Habitacion habitacion)
        {
            if (id != habitacion.Id) return Ok("El ID de la habitación no coincide");

            var habitacionExistente = await _appDbContext.Habitaciones.FindAsync(id);
            if (habitacionExistente == null) return Ok("La habitación no existe");

            if (habitacion.Capacidad == null || habitacion.Capacidad <= 0 || habitacion.Capacidad >= 5)
            {
                return Ok("La capacidad debe ser entre 1 y 4");
            }

            if (habitacion.NumeroDeHabitacion == null || habitacion.NumeroDeHabitacion <= 0 || habitacion.NumeroDeHabitacion > 300)
            {
                return Ok("El número de habitación debe ser mayor a 0 y menor a 300");
            }

            var otraHabitacion = await _appDbContext.Habitaciones.FirstOrDefaultAsync(h => h.NumeroDeHabitacion == habitacion.NumeroDeHabitacion && h.Id != id);
            if (otraHabitacion != null)
            {
                return Ok("Ya existe otra habitación con el mismo número.");
            }

            if (habitacion.Costo <= 10 || habitacion.Costo > 300)
            {
                return Ok("El costo debe ser entre 10 y 300");
            }

            if (habitacion.Piso != null && habitacion.Piso < 0 || habitacion.Piso > 10)
            {
                return Ok("El piso debe ser entre 0 y 10");
            }

            

            habitacionExistente.Capacidad = habitacion.Capacidad;
            habitacionExistente.NumeroDeHabitacion = habitacion.NumeroDeHabitacion;
            habitacionExistente.Costo = habitacion.Costo;
            habitacionExistente.Piso = habitacion.Piso;


            _appDbContext.Habitaciones.Update(habitacionExistente);
            await _appDbContext.SaveChangesAsync();

            return Ok(habitacionExistente);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHabitacion(int id)
        {
            var habitacion = await _appDbContext.Habitaciones.FindAsync(id);
            if (habitacion == null) return Ok("La habitación no existe");

            var tieneReservaciones = await _appDbContext.Reservas.AnyAsync(r => r.Habitacion.Id == id);
            if (tieneReservaciones)
            {
                return Ok("No se puede eliminar una habitacion porque ya tiene una reserva.");
            }

            _appDbContext.Habitaciones.Remove(habitacion);
            await _appDbContext.SaveChangesAsync();

            return Ok(new { message = $"Habitación con ID {id} eliminada correctamente" });
        }
    }
}
