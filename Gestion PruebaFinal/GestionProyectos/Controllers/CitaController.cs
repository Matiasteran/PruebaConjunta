using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prueba;

namespace Prueba.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CitasController : ControllerBase
    {
        private readonly AppDBContext _appDbContext;

        public CitasController(AppDBContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetCitas()
        {
            var citas = await _appDbContext.Citas
                                           .Include(c => c.Paciente)
                                           .Include(c => c.Medico)
                                           .Include(c => c.Consultorio)
                                           .ToListAsync();
            return Ok(citas);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCita(int id)
        {
            var cita = await _appDbContext.Citas
                                          .Include(c => c.Paciente)
                                          .Include(c => c.Medico)
                                          .Include(c => c.Consultorio)
                                          .FirstOrDefaultAsync(c => c.Id == id);
            if (cita == null) return Ok("La cita no existe");
            return Ok(cita);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCita([FromBody] Cita cita)
        {
            var paciente = await _appDbContext.Pacientes.FindAsync(cita.Paciente.Id);
            if (paciente == null) return Ok("El paciente no existe");

            var medico = await _appDbContext.Medicos.FindAsync(cita.Medico.Id);
            if (medico == null) return Ok("El médico no existe");

            var consultorio = await _appDbContext.Consultorios.FindAsync(cita.Consultorio.Id);
            if (consultorio == null) return Ok("El consultorio no existe");

            // Asociar entidades relacionadas
            cita.Paciente = paciente;
            cita.Medico = medico;
            cita.Consultorio = consultorio;

            var validationResult = ValidarCita(cita);
            if (!string.IsNullOrEmpty(validationResult))
            {
                return Ok(validationResult);
            }

            _appDbContext.Citas.Add(cita);
            await _appDbContext.SaveChangesAsync();

            var citaConDetalles = await _appDbContext.Citas
                                                     .Include(c => c.Paciente)
                                                     .Include(c => c.Medico)
                                                     .Include(c => c.Consultorio)
                                                     .FirstOrDefaultAsync(c => c.Id == cita.Id);

            return Ok(citaConDetalles);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> EditCita(int id, [FromBody] Cita cita)
        {
            if (id != cita.Id) return Ok("El ID de la cita no coincide");

            var citaExistente = await _appDbContext.Citas
                                                   .Include(c => c.Paciente)
                                                   .Include(c => c.Medico)
                                                   .Include(c => c.Consultorio)
                                                   .FirstOrDefaultAsync(c => c.Id == id);
            if (citaExistente == null) return Ok("La cita no existe");

            var paciente = await _appDbContext.Pacientes.FindAsync(cita.Paciente.Id);
            if (paciente == null) return Ok("El paciente no existe");

            var medico = await _appDbContext.Medicos.FindAsync(cita.Medico.Id);
            if (medico == null) return Ok("El médico no existe");

            var consultorio = await _appDbContext.Consultorios.FindAsync(cita.Consultorio.Id);
            if (consultorio == null) return Ok("El consultorio no existe");

            citaExistente.Paciente = paciente;
            citaExistente.Medico = medico;
            citaExistente.Consultorio = consultorio;
            citaExistente.Fecha = cita.Fecha;
            citaExistente.Hora = cita.Hora;

            var validationResult = ValidarCita(citaExistente);
            if (!string.IsNullOrEmpty(validationResult))
            {
                return Ok(validationResult);
            }

            _appDbContext.Citas.Update(citaExistente);
            await _appDbContext.SaveChangesAsync();

            var citaConDetalles = await _appDbContext.Citas
                                                     .Include(c => c.Paciente)
                                                     .Include(c => c.Medico)
                                                     .Include(c => c.Consultorio)
                                                     .FirstOrDefaultAsync(c => c.Id == citaExistente.Id);

            return Ok(citaConDetalles);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCita(int id)
        {
            var cita = await _appDbContext.Citas.FindAsync(id);
            if (cita == null) return Ok("La cita no existe");

            _appDbContext.Citas.Remove(cita);
            await _appDbContext.SaveChangesAsync();
            return Ok(new { message = $"Cita con ID {id} ha sido eliminada correctamente" });
        }

        private static string? ValidarCita(Cita cita)
        {
            if (cita.Fecha == default)
                return "La fecha es obligatoria";

            if (cita.Fecha < DateTime.Now.Date)
                return "La fecha no puede ser anterior a la fecha actual";

            if (cita.Hora == default)
                return "La hora es obligatoria";

            if (cita.Hora < TimeSpan.FromHours(0) || cita.Hora > TimeSpan.FromHours(23.99))
                return "La hora debe estar entre las 00:00 y las 23:59";

            if (cita.Paciente == null)
                return "El paciente es obligatorio";

            if (cita.Medico == null)
                return "El médico es obligatorio";

            if (cita.Consultorio == null)
                return "El consultorio es obligatorio";

            return null;
        }
    }
}