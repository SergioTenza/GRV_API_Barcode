namespace GRV_API_Barcode.Domain.Modelos
{
    public sealed class Peticion
    {
        public Guid Guid { get; set; } = Guid.NewGuid();
        public string ApiKey { get; set; }
        public string ReportName { get; set; }
        public string Empresa { get; set; }
        public int ExportType { get; set; }
        public Parametro[] Parametros { get; set; }
        public string Base64 { get; set; }
    }
}
