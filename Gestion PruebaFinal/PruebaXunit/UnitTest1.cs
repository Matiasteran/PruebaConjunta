using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using prueba;
using Prueba;
public class CitasControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public CitasControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDBContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<AppDBContext>(options =>
                    options.UseSqlServer("Server=LAPTOPMATIAS;Database=PruebaConjunta_DB;User ID=sa;Password=ma2604;TrustServerCertificate=true; MultipleActiveResultSets=true"));
            });
        });
    }

    [Fact]
    public async Task VerificarRelacionesEntreTablas()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDBContext>();

        var citas = await context.Citas
            .Include(c => c.Paciente)
            .Include(c => c.Medico)
            .Include(c => c.Consultorio)
            .ToListAsync();

        Assert.NotNull(citas);

        foreach (var cita in citas)
        {
            Assert.NotNull(cita.Paciente);
            Assert.True(cita.Paciente.Id > 0, "El ID del paciente no es válido.");

            Assert.NotNull(cita.Medico);
            Assert.True(cita.Medico.Id > 0, "El ID del médico no es válido.");

            Assert.NotNull(cita.Consultorio);
            Assert.True(cita.Consultorio.Id > 0, "El ID del consultorio no es válido.");
        }
    }
}