using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace prueba
{
    public class AppDBContext : DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options) { }

        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Habitacion> Habitaciones { get; set; }
        public DbSet<Reserva> Reservas { get; set; }
        public DbSet<ServicioAdicional> ServiciosAdicionales { get; set; }
    }

    public class Cliente
    {
        public required int Id { get; set; }
        public required string Nombre { get; set; }

        public required string Apellido { get; set; }

        public int? Edad { get; set; }
        public required string Cedula { get; set; }
    }

    public class Habitacion
    {
        public required int Id { get; set; }
        public int? Capacidad { get; set; }
        public int? NumeroDeHabitacion { get; set; }
        [JsonRequired] public decimal Costo { get; set; }
        public int? Piso { get; set; }
    }

    public class Reserva
    {
        public required int Id { get; set; }
        public required Cliente Cliente { get; set; } = null!;
        public required Habitacion Habitacion { get; set; } = null!; 
        [JsonRequired] public DateTime FechaDeReserva { get; set; }
        [JsonRequired] public DateTime FechaDeInicio { get; set; }
        [JsonRequired] public DateTime FechaDeFin { get; set; }
    }

    public class ServicioAdicional
    {
        public required int Id { get; set; }
        public required string Descripcion { get; set; } = null!;
        [JsonRequired] public decimal Costo { get; set; }
        public required Reserva Reserva { get; set; } = null!;
    }
}
