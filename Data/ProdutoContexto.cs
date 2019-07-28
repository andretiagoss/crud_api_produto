
using Microsoft.EntityFrameworkCore;

namespace ATSS.API.Data
{
    public class ProdutoContexto : DbContext
    {   
        //Propriedades que representam as tabela do banco de dados.
        public DbSet<Produto> Produtos { get; set; }

        //Construtor herdando da classe base passando o "options" como parametro
        public ProdutoContexto(DbContextOptions options)
            :base(options)
        {
            
        }
    }
}