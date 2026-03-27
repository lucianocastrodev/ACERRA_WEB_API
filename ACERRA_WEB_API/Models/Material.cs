namespace ACERRA_WEB_API.Models
{
    public class Material
    {
        public int Id { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public double PrecoCompra { get; set; }

        public double PrecoVenda { get; set; }
    }
}
