using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MecHub.Data;
using MecHub.Models;
using System.Security.Claims;

namespace MecHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class OrdensServicoApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrdensServicoApiController(AppDbContext context)
        {
            _context = context;
        }

        // 🚨 HELPER — extrai o MecanicoId do token JWT
        private int ObterMecanicoIdDoToken()
        {
            var claim = User.FindFirstValue("MecanicoId");
            if (string.IsNullOrWhiteSpace(claim))
                throw new UnauthorizedAccessException("MecanicoId não encontrado no token.");
            return int.Parse(claim);
        }

        // GET: api/ordensservico
        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            var mecanicoId = ObterMecanicoIdDoToken();

            var ordens = await _context.ordem_servico
                .Where(o => o.MecanicoId == mecanicoId)
                .Include(o => o.Veiculo)
                .Include(o => o.Cliente)
                .Include(o => o.Mecanico)
                    .ThenInclude(m => m!.Usuario)
                .OrderByDescending(o => o.DataCriacao)
                .Select(o => new
                {
                    o.Id,
                    Status = o.StatusOrdem.ToString(),
                    o.DataCriacao,
                    o.DataFechamento,
                    Veiculo = o.Veiculo != null ? o.Veiculo.Placa : null,
                    Cliente = o.Cliente != null ? o.Cliente.Nome : null,
                    Mecanico = o.Mecanico != null && o.Mecanico.Usuario != null
                                ? o.Mecanico.Usuario.Nome
                                : null
                })
                .ToListAsync();

            return Ok(ordens);
        }

        // GET: api/ordensservico/5
        [HttpGet("{id}")]
        public async Task<IActionResult> ObterPorId(int id)
        {
            var mecanicoId = ObterMecanicoIdDoToken();

            var ordem = await _context.ordem_servico
                .Where(o => o.Id == id && o.MecanicoId == mecanicoId)
                .Include(o => o.Veiculo)
                .Include(o => o.Cliente)
                .Include(o => o.Mecanico)
                    .ThenInclude(m => m!.Usuario)
                .Include(o => o.Itens)
                    .ThenInclude(i => i.Servico)
                .FirstOrDefaultAsync();

            if (ordem == null)
                return NotFound(new { mensagem = "Ordem de serviço não encontrada." });

            return Ok(new
            {
                ordem.Id,
                Status = ordem.StatusOrdem.ToString(),
                ordem.DataCriacao,
                ordem.DataFechamento,
                Veiculo = ordem.Veiculo != null ? new
                {
                    ordem.Veiculo.Id,
                    ordem.Veiculo.Placa,
                    ordem.Veiculo.Marca,
                    ordem.Veiculo.Modelo,
                    ordem.Veiculo.Cor
                } : null,
                Cliente = ordem.Cliente != null ? new
                {
                    ordem.Cliente.Id,
                    ordem.Cliente.Nome,
                    ordem.Cliente.Email,
                    ordem.Cliente.Telefone
                } : null,
                Mecanico = ordem.Mecanico?.Usuario != null ? new
                {
                    ordem.Mecanico.Id,
                    Nome = ordem.Mecanico.Usuario.Nome,
                    ordem.Mecanico.Usuario.Email
                } : null,
                Itens = ordem.Itens.Select(i => new
                {
                    i.Id,
                    Servico = i.Servico != null ? i.Servico.Descricao : null,
                    Tipo = i.Servico != null ? i.Servico.Tipo : null,
                    i.Quantidade,
                    ValorUnitario = i.Servico != null ? i.Servico.Valor : 0,
                    Subtotal = i.Servico != null ? i.Quantidade * i.Servico.Valor : 0
                }),
                Total = ordem.Itens
                    .Where(i => i.Servico != null)
                    .Sum(i => i.Quantidade * i.Servico!.Valor)
            });
        }

        // GET: api/ordensservico/status/Aberto
        // Valores válidos: Aberto, Em_andamento, AguardandoAprovacao, Fechada
        [HttpGet("status/{status}")]
        public async Task<IActionResult> ListarPorStatus(string status)
        {
            var mecanicoId = ObterMecanicoIdDoToken();

            if (!Enum.TryParse<StatusOrdemEnum>(status, true, out var statusEnum))
                return BadRequest(new
                {
                    mensagem = "Status inválido.",
                    valoresValidos = Enum.GetNames(typeof(StatusOrdemEnum))
                });

            var ordens = await _context.ordem_servico
                .Where(o => o.MecanicoId == mecanicoId && o.StatusOrdem == statusEnum)
                .Include(o => o.Veiculo)
                .Include(o => o.Cliente)
                .OrderByDescending(o => o.DataCriacao)
                .Select(o => new
                {
                    o.Id,
                    Status = o.StatusOrdem.ToString(),
                    o.DataCriacao,
                    o.DataFechamento,
                    Veiculo = o.Veiculo != null ? o.Veiculo.Placa : null,
                    Cliente = o.Cliente != null ? o.Cliente.Nome : null
                })
                .ToListAsync();

            return Ok(ordens);
        }

        // GET: api/ordensservico/veiculo/ABC1234
        [HttpGet("veiculo/{placa}")]
        public async Task<IActionResult> ListarPorPlaca(string placa)
        {
            var mecanicoId = ObterMecanicoIdDoToken();

            var ordens = await _context.ordem_servico
                .Where(o => o.MecanicoId == mecanicoId
                         && o.Veiculo != null
                         && o.Veiculo.Placa == placa)
                .Include(o => o.Veiculo)
                .OrderByDescending(o => o.DataCriacao)
                .Select(o => new
                {
                    o.Id,
                    Status = o.StatusOrdem.ToString(),
                    o.DataCriacao,
                    o.DataFechamento,
                    Veiculo = o.Veiculo != null ? o.Veiculo.Placa : null
                })
                .ToListAsync();

            if (!ordens.Any())
                return NotFound(new { mensagem = "Nenhuma ordem encontrada para essa placa." });

            return Ok(ordens);
        }
    
            // PUT: api/ordensservico/5/status
            [HttpPut("{id}/status")]
            public async Task<IActionResult> AtualizarStatus(int id, [FromBody] AtualizarStatusRequest request)
        {
            var mecanicoId = ObterMecanicoIdDoToken();

            // Valida se o status enviado é válido
            if (!Enum.IsDefined(typeof(StatusOrdemEnum), request.NovoStatus))
                return BadRequest(new
                {
                    mensagem = "Status inválido.",
                    valoresValidos = Enum.GetNames(typeof(StatusOrdemEnum))
                });

            var ordem = await _context.ordem_servico
                .FirstOrDefaultAsync(o => o.Id == id && o.MecanicoId == mecanicoId);

            if (ordem == null)
                return NotFound(new { mensagem = "Ordem de serviço não encontrada." });

            var statusAnterior = ordem.StatusOrdem;
            ordem.StatusOrdem = (StatusOrdemEnum)request.NovoStatus;

            // Se mudou pra "Fechada" e ainda não tinha data de fechamento, registra agora
            if (ordem.StatusOrdem == StatusOrdemEnum.Fechada && ordem.DataFechamento == null)
                ordem.DataFechamento = DateTime.Now;

            // Se saiu de "Fechada" pra outro status, limpa a data
            if (ordem.StatusOrdem != StatusOrdemEnum.Fechada)
                ordem.DataFechamento = null;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem = "Status atualizado com sucesso.",
                statusAnterior = statusAnterior.ToString(),
                statusNovo = ordem.StatusOrdem.ToString(),
                ordem.DataFechamento
            });
        }

        public class AtualizarStatusRequest
        {
            public int NovoStatus { get; set; }
         }
         // POST: api/ordensservico
        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] CriarOrdemRequest request)
        {
            var mecanicoId = ObterMecanicoIdDoToken();

            // Validações básicas
            if (request.ClienteId <= 0 || request.VeiculoId <= 0)
                return BadRequest(new { mensagem = "Cliente e veículo são obrigatórios." });

            if (request.Itens == null || !request.Itens.Any())
                return BadRequest(new { mensagem = "Adicione pelo menos um item." });

            // Valida se o cliente pertence ao mecânico
            var clienteExiste = await _context.cliente
                .AnyAsync(c => c.Id == request.ClienteId && c.MecanicoId == mecanicoId);

            if (!clienteExiste)
                return BadRequest(new { mensagem = "Cliente inválido." });

            // Valida se o veículo pertence ao mecânico
            var veiculoExiste = await _context.veiculo
                .AnyAsync(v => v.Id == request.VeiculoId && v.MecanicoId == mecanicoId);

            if (!veiculoExiste)
                return BadRequest(new { mensagem = "Veículo inválido." });

            // Cria a ordem
            var ordem = new OrdemServico
            {
                MecanicoId = mecanicoId,
                ClienteId = request.ClienteId,
                VeiculoId = request.VeiculoId,
                StatusOrdem = (StatusOrdemEnum)request.Status,
                DataCriacao = DateTime.Now,
                Itens = request.Itens
                    .Where(i => i.ServicoId > 0 && i.Quantidade > 0)
                    .Select(i => new ItemOrdemServico
                    {
                        ServicoId = i.ServicoId,
                        Quantidade = i.Quantidade
                    })
                    .ToList()
            };

            if (ordem.StatusOrdem == StatusOrdemEnum.Fechada)
                ordem.DataFechamento = DateTime.Now;

            _context.ordem_servico.Add(ordem);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem = "Ordem criada com sucesso.",
                id = ordem.Id
            });
        }

        public class CriarOrdemRequest
        {
            public int ClienteId { get; set; }
            public int VeiculoId { get; set; }
            public int Status { get; set; }
            public List<ItemRequest> Itens { get; set; } = new();
        }

        public class ItemRequest
        {
            public int ServicoId { get; set; }
            public int Quantidade { get; set; }
        }
                // GET: api/ordensservico/resumo
        [HttpGet("resumo")]
        public async Task<IActionResult> Resumo()
        {
            var mecanicoId = ObterMecanicoIdDoToken();

            var total = await _context.ordem_servico
                .CountAsync(o => o.MecanicoId == mecanicoId);

            var abertas = await _context.ordem_servico
                .CountAsync(o => o.MecanicoId == mecanicoId && o.StatusOrdem == StatusOrdemEnum.Aberto);

            var emAndamento = await _context.ordem_servico
                .CountAsync(o => o.MecanicoId == mecanicoId && o.StatusOrdem == StatusOrdemEnum.Em_andamento);

            var aguardando = await _context.ordem_servico
                .CountAsync(o => o.MecanicoId == mecanicoId && o.StatusOrdem == StatusOrdemEnum.AguardandoAprovacao);

            var fechadas = await _context.ordem_servico
                .CountAsync(o => o.MecanicoId == mecanicoId && o.StatusOrdem == StatusOrdemEnum.Fechada);

            return Ok(new
            {
                total,
                abertas,
                emAndamento,
                aguardando,
                fechadas
            });
        }
    }
}