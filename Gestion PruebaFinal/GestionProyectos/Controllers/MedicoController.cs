using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace prueba.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MedicoController : ControllerBase
    {
        private readonly AppDBContext _appDbContext;

        public MedicoController(AppDBContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetMedicos()
        {
            var medicos = await _appDbContext.Medicos.ToListAsync();
            return Ok(medicos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMedico(int id)
        {
            var medico = await _appDbContext.Medicos.FindAsync(id);
            if (medico == null) return Ok("El médico no existe");
            return Ok(medico);
        }

        private static string ValidarMedico(Medico medico)
        {
            if (string.IsNullOrWhiteSpace(medico.Nombre))
            {
                return "El nombre es obligatorio";
            }
            if (!medico.Nombre.All(char.IsLetter))
            {
                return "El nombre solo debe contener letras";
            }

            if (string.IsNullOrWhiteSpace(medico.Apellido))
            {
                return "El apellido es obligatorio";
            }
            if (!medico.Apellido.All(char.IsLetter))
            {
                return "El apellido solo debe contener letras";
            }

            if (string.IsNullOrWhiteSpace(medico.Especialidad))
            {
                return "La especialidad es obligatoria";
            }

            return "ok";
        }

        [HttpPost]
        public async Task<IActionResult> CreateMedico(Medico medico)
        {
            var error = ValidarMedico(medico);
            if (error != "ok")
            {
                return Ok(error);
            }

            var medicoExistente = await _appDbContext.Medicos.FirstOrDefaultAsync(m => m.Nombre == medico.Nombre && m.Apellido == medico.Apellido && m.Especialidad == medico.Especialidad);
            if (medicoExistente != null)
            {
                return Ok("Ya existe un médico con la misma especialidad y nombre");
            }

            _appDbContext.Medicos.Add(medico);
            await _appDbContext.SaveChangesAsync();
            return Ok(medico);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> EditMedico(int id, Medico medico)
        {
            if (id != medico.Id) return Ok("El ID del médico no coincide");

            var medicoExistente = await _appDbContext.Medicos.FindAsync(id);
            if (medicoExistente == null) return Ok("El médico no existe");

            var error = ValidarMedico(medico);
            if (error != "ok")
            {
                return Ok(error);
            }

            var medicoConEspecialidad = await _appDbContext.Medicos.FirstOrDefaultAsync(m => m.Especialidad == medico.Especialidad && m.Id != id);
            if (medicoConEspecialidad != null)
            {
                return Ok("Ya existe otro médico con la misma especialidad");
            }

            medicoExistente.Nombre = medico.Nombre;
            medicoExistente.Apellido = medico.Apellido;
            medicoExistente.Especialidad = medico.Especialidad;

            _appDbContext.Medicos.Update(medicoExistente);
            await _appDbContext.SaveChangesAsync();

            return Ok(medicoExistente);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMedico(int id)
        {
            var medico = await _appDbContext.Medicos.FindAsync(id);
            if (medico == null) return Ok("El médico no existe");

            _appDbContext.Medicos.Remove(medico);
            await _appDbContext.SaveChangesAsync();

            return Ok(new { message = $"Médico con ID {id} ha sido eliminado correctamente" });
        }
    }
}
