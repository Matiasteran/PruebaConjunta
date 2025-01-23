using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace prueba.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClienteController : ControllerBase
    {
        private readonly AppDBContext _appDbContext;

        public ClienteController(AppDBContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetClientes()
        {
            var clientes = await _appDbContext.Clientes.ToListAsync();
            return Ok(clientes);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCliente(int id)
        {
            var cliente = await _appDbContext.Clientes.FindAsync(id);
            if (cliente == null) return Ok("El cliente no existe");
            return Ok(cliente);
        }

        private static string ValidarCliente(Cliente cliente)
        {
            if (string.IsNullOrWhiteSpace(cliente.Nombre))
            {
                return "El nombre es obligatorio";
            }
            if (!cliente.Nombre.All(char.IsLetter))
            {
                return "El nombre solo debe contener letras";
            }

            if (string.IsNullOrWhiteSpace(cliente.Apellido))
            {
                return "El apellido es obligatorio";
            }
            if (!cliente.Apellido.All(char.IsLetter))
            {
                return "El apellido solo debe contener letras";
            }

            if (string.IsNullOrWhiteSpace(cliente.Cedula))
            {
                return "La cédula es obligatoria";
            }
            if (!cliente.Cedula.All(char.IsDigit))
            {
                return "La cédula solo debe contener números";
            }
            if (!EsCedulaValida(cliente.Cedula))
            {
                return "La cédula ingresada no es válida";
            }

            if (cliente.Edad < 18 || cliente.Edad > 70)
            {
                return "La edad debe ser mayor o igual a 18 años y menor o igual a 70";
            }

            return "ok"; 
        }

        [HttpPost]
        public async Task<IActionResult> CreateCliente(Cliente cliente)
        {

            var error = ValidarCliente(cliente);
            if (error != "ok")
            {
                return Ok(error);
            }

            var clienteExistente = await _appDbContext.Clientes.FirstOrDefaultAsync(c => c.Cedula == cliente.Cedula);
            if (clienteExistente != null)
            {
                return Ok("Ya existe un cliente con la misma cédula");
            }

            _appDbContext.Clientes.Add(cliente);
            await _appDbContext.SaveChangesAsync();
            return Ok(cliente);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> EditCliente(int id, Cliente cliente)
        {
            if (id != cliente.Id) return Ok("El ID del cliente no coincide");

            var clienteExistente = await _appDbContext.Clientes.FindAsync(id);
            if (clienteExistente == null) return Ok("El cliente no existe");


            var error = ValidarCliente(cliente);
            if (error != "ok")
            {
                return Ok(error);
            }

            var clienteConCedula = await _appDbContext.Clientes.FirstOrDefaultAsync(c => c.Cedula == cliente.Cedula && c.Id != id);
            if (clienteConCedula != null)
            {
                return Ok("Ya existe otro cliente con la misma cédula");
            }

           
            clienteExistente.Nombre = cliente.Nombre;
            clienteExistente.Apellido = cliente.Apellido; 
            clienteExistente.Edad = cliente.Edad;
            clienteExistente.Cedula = cliente.Cedula;

            _appDbContext.Clientes.Update(clienteExistente);
            await _appDbContext.SaveChangesAsync();

            return Ok(clienteExistente);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCliente(int id)
        {
            var cliente = await _appDbContext.Clientes.FindAsync(id);
            if (cliente == null) return Ok("El cliente no existe");

            var tieneReservaciones = await _appDbContext.Reservas.AnyAsync(r => r.Cliente.Id == id);
            if (tieneReservaciones)
            {
                return Ok("No se puede eliminar al cliente porque tiene reservaciones asociadas.");
            }

            _appDbContext.Clientes.Remove(cliente);
            await _appDbContext.SaveChangesAsync();

            return Ok(new { message = $"Cliente con ID {id} ha sido eliminado correctamente" });
        }



        private static bool EsCedulaValida(string cedula)
        {
            if (cedula.Length != 10 || !cedula.All(char.IsDigit))
            {
                return false;
            }

            int provincia = int.Parse(cedula.Substring(0, 2));

            if ((provincia < 1 || provincia > 24) && provincia != 30)
            {
                return false;
            }


            int[] coeficientes = { 2, 1, 2, 1, 2, 1, 2, 1, 2 };
            int suma = 0;

            for (int i = 0; i < coeficientes.Length; i++)
            {
                int valor = coeficientes[i] * int.Parse(cedula[i].ToString());
                suma += valor >= 10 ? valor - 9 : valor;
            }

            int digitoVerificador = int.Parse(cedula[9].ToString());
            int residuo = suma % 10;
            int resultado = residuo == 0 ? 0 : 10 - residuo;

            return resultado == digitoVerificador;
        }

    }
}
