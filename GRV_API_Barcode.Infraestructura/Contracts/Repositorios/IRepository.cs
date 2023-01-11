namespace GRV_API_Barcode.Infraestructura.Contracts.Repositorios
{
    public interface IRepository<T> where T : class
    {
        IEnumerable<T> GetAll();
        T? GetById(Guid guid);
        void Insert(T subject);
        void Update(T subject);
        void Delete(Guid guid);
    }
}
