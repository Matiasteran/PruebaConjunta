using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace prueba.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PacienteController : ControllerBase
    {
        private readonly AppDBContext _appDbContext;

        public PacienteController(AppDBContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetPacientes()
        {
            var pacientes = await _appDbContext.Pacientes.ToListAsync();
            return Ok(pacientes);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPaciente(int id)
        {
            var paciente = await _appDbContext.Pacientes.FindAsync(id);
            if (paciente == null) return Ok("El paciente no existe");
            return Ok(paciente);
        }

        private static string ValidarPaciente(Paciente paciente)
        {
            if (string.IsNullOrWhiteSpace(paciente.Nombre))
            {
                return "El nombre es obligatorio";
            }
            if (!paciente.Nombre.All(char.IsLetter))
            {
                return "El nombre solo debe contener letras";
            }

            if (string.IsNullOrWhiteSpace(paciente.Apellido))
            {
                return "El apellido es obligatorio";
            }
            if (!paciente.Apellido.All(char.IsLetter))
            {
                return "El apellido solo debe contener letras";
            }

            if (paciente.FechaNacimiento == default)
            {
                return "La fecha de nacimiento es obligatoria";
            }
            if (paciente.FechaNacimiento > DateTime.Now)
            {
                return "La fecha de nacimiento no puede ser en el futuro";
            }

            if (string.IsNullOrWhiteSpace(paciente.Email) || !paciente.Email.Contains("@"))
            {
                return "El email es obligatorio y debe ser válido";
            }

            return "ok";
        }

        [HttpPost]
        public async Task<IActionResult> CreatePaciente(Paciente paciente)
        {
            var error = ValidarPaciente(paciente);
            if (error != "ok")
            {
                return Ok(error);
            }

            var pacienteExistente = await _appDbContext.Pacientes.FirstOrDefaultAsync(p => p.Email == paciente.Email);
            if (pacienteExistente != null)
            {
                return Ok("Ya existe un paciente con el mismo email");
            }

            _appDbContext.Pacientes.Add(paciente);
            await _appDbContext.SaveChangesAsync();
            return Ok(paciente);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> EditPaciente(int id, Paciente paciente)
        {
            if (id != paciente.Id) return Ok("El ID del paciente no coincide");

            var pacienteExistente = await _appDbContext.Pacientes.FindAsync(id);
            if (pacienteExistente == null) return Ok("El paciente no existe");

            var error = ValidarPaciente(paciente);
            if (error != "ok")
            {
                return Ok(error);
            }

            var pacienteConEmail = await _appDbContext.Pacientes.FirstOrDefaultAsync(p => p.Email == paciente.Email && p.Id != id);
            if (pacienteConEmail != null)
            {
                return Ok("Ya existe otro paciente con el mismo email");
            }

            pacienteExistente.Nombre = paciente.Nombre;
            pacienteExistente.Apellido = paciente.Apellido;
            pacienteExistente.FechaNacimiento = paciente.FechaNacimiento;
            pacienteExistente.Email = paciente.Email;

            _appDbContext.Pacientes.Update(pacienteExistente);
            await _appDbContext.SaveChangesAsync();

            return Ok(pacienteExistente);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePaciente(int id)
        {
            var paciente = await _appDbContext.Pacientes.FindAsync(id);
            if (paciente == null) return Ok("El paciente no existe");

            _appDbContext.Pacientes.Remove(paciente);
            await _appDbContext.SaveChangesAsync();

            return Ok(new { message = $"Paciente con ID {id} ha sido eliminado correctamente" });
        }
    }
}