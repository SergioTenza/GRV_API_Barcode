using GRV_API_Barcode.Domain.Modelos;
using GRV_API_Barcode.Infraestructura.Contracts.Repositorios;
using System;

namespace GRV_API_Barcode.Repositorios
{
    
    public class InMemoryRepository : IRepository<Peticion>
    {
        private readonly List<Peticion> _context;

        public InMemoryRepository()
        {
            _context = new List<Peticion>();
        }

        public void Delete(Guid guid)
        {
            var exists = _context.FirstOrDefault(x => x.Guid == guid);
            if (exists is not null) _context.Remove(exists);
        }

        public IEnumerable<Peticion> GetAll()
        {
            return _context;
        }

        public Peticion? GetById(Guid guid)
        {
            return _context.FirstOrDefault(x=> x.Guid == guid);
        }

        public void Insert(Peticion subject)
        {
            if (subject == null) throw new ArgumentNullException(nameof(subject));
            var exists = _context.FirstOrDefault(x => x.Guid == subject.Guid);
            if (exists is null) _context.Add(subject);
        }

        public void Update(Peticion subject)
        {
            if (subject == null) throw new ArgumentNullException(nameof(subject));
            var exists = _context.FirstOrDefault(x => x.Guid == subject.Guid);
            if (exists is not null) _context.Remove(subject);
            _context.Add(subject);
        }
    }
}
