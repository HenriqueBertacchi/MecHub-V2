using MecHub.Models;

namespace MecHub.ViewModel
{
    public class UsuarioListViewModel
    {
        public int Id { get; set; }

        public required string Nome { get; set; }

        public required string Email { get; set; }

        public TipoLoginEnum TipoLogin { get; set; }

    }
}
