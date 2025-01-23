using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace prueba
{
    public class AppDBContext : DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options) { }

        public DbSet<Paciente> Pacientes { get; set; }
        public DbSet<Medico> Medicos { get; set; }
        public DbSet<Cita> Citas { get; set; }
        public DbSet<Consultorio> Consultorios { get; set; }
    }
        public class Paciente
    {
        public required int Id { get; set; }
        public required string Nombre { get; set; }
        public required string Apellido { get; set; }
        [JsonRequired] public DateTime FechaNacimiento { get; set; }
        public required string Email { get; set; }
    }

    public class Medico
    {
        public required int Id { get; set; }
        public required string Nombre { get; set; }
        public required string Apellido { get; set; }
        public required string Especialidad { get; set; }
    }
    public class Consultorio
    {
        public required int Id { get; set; }
        public required int Numero { get; set; }
        public required int Piso { get; set; }
    }

    public class Cita
    {
        public required int Id { get; set; }
        public required int PacienteId { get; set; }
        public required Paciente Paciente { get; set; } = null!;
        public required int MedicoId { get; set; }
        public required Medico Medico { get; set; } = null!;
        [JsonRequired] public DateTime Fecha { get; set; }
        [JsonRequired] public TimeSpan Hora { get; set; }
        public required int ConsultorioId { get; set; }
        public required Consultorio Consultorio { get; set; } = null!;
    }

    
}
