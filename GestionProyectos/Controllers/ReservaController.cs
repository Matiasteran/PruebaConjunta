using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace prueba.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservaController : ControllerBase
    {
        private readonly AppDBContext _appDbContext;

        public ReservaController(AppDBContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetReservas()
        {
            var reservas = await _appDbContext.Reservas
                .Include(r => r.Cliente)
                .Include(r => r.Habitacion)
                .ToListAsync();
            return Ok(reservas);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetReserva(int id)
        {
            var reserva = await _appDbContext.Reservas
                .Include(r => r.Cliente)
                .Include(r => r.Habitacion)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reserva == null) return Ok("La reserva no existe");

            return Ok(reserva);
        }

        [HttpPost]
        public async Task<IActionResult> CreateReserva(Reserva reserva)
        {
            var validationResult = await validar(reserva);
            if (validationResult != null)
            {
                return validationResult;
            }

            _appDbContext.Reservas.Add(reserva);
            await _appDbContext.SaveChangesAsync();

            return Ok(reserva);

        }

        [HttpPut("{id}")]
        public async Task<IActionResult> EditReserva(int id, Reserva reserva)
        {
            if (id != reserva.Id) return Ok("El ID de la reserva no coincide");

            var reservaExistente = await _appDbContext.Reservas
                .Include(r => r.Cliente)
                .Include(r => r.Habitacion)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservaExistente == null) return Ok("La reserva no existe");

            var validationResult = await validar(reserva, excludeReservaId: id);
            if (validationResult != null)
            {
                return validationResult;
            }

            reservaExistente.Cliente = reserva.Cliente;
            reservaExistente.Habitacion = reserva.Habitacion;
            reservaExistente.FechaDeReserva = reserva.FechaDeReserva;
            reservaExistente.FechaDeInicio = reserva.FechaDeInicio;
            reservaExistente.FechaDeFin = reserva.FechaDeFin;

            _appDbContext.Reservas.Update(reservaExistente);
            await _appDbContext.SaveChangesAsync();

            return Ok(reservaExistente);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReserva(int id)
        {
            var reserva = await _appDbContext.Reservas.FindAsync(id);
            if (reserva == null) return Ok("La reserva no existe");

            _appDbContext.Reservas.Remove(reserva);
            await _appDbContext.SaveChangesAsync();

            return Ok(new { message = $"Reserva con ID {id} eliminada correctamente" });
        }

        private async Task<IActionResult?> validar(Reserva reserva, int? excludeReservaId = null)
        { 
            var cliente = await _appDbContext.Clientes.FindAsync(reserva.Cliente.Id);
            var habitacion = await _appDbContext.Habitaciones.FindAsync(reserva.Habitacion.Id);

            if (habitacion == null)
            {
                return Ok("Habitacion no valida");
            }

            if (cliente == null)
            {
                return Ok("Cliente no valido");
            }

            if (reserva.FechaDeInicio < reserva.FechaDeReserva)
            {
                return Ok("La fecha de inicio no puede ser anterior a la fecha de reserva");
            }

            if (reserva.FechaDeFin <= reserva.FechaDeInicio)
            {
                return Ok("La fecha de fin debe ser posterior a la fecha de inicio");
            }

            var conflicto = await _appDbContext.Reservas.AnyAsync
                (r => r.Habitacion.Id == habitacion.Id &&
                (!excludeReservaId.HasValue || r.Id != excludeReservaId) &&
                ((reserva.FechaDeInicio >= r.FechaDeInicio && reserva.FechaDeInicio < r.FechaDeFin) ||
                (reserva.FechaDeFin > r.FechaDeInicio && reserva.FechaDeFin <= r.FechaDeFin)));

            if (conflicto)
            {
                return Ok("La habitacion ya esta reservada en las fechas seleccionadas.");
            }

            reserva.Cliente = cliente;
            reserva.Habitacion = habitacion;

            return null;
        }
    }
}
