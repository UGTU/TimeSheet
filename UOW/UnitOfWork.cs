using System.Collections;

namespace UOW
{
    public class UnitOfWork : IUnitOfWork
    {
        readonly Hashtable _repositories = new Hashtable();
        private readonly IDataContext _context;
        private readonly IRepositoryFactory _factory;
        
        public UnitOfWork(IDataContext context, IRepositoryFactory factory)
        {
            _context = context;
            _factory = factory;
        }

        public IDataContext Context
        {
            get { return _context; }
        }

        public IRepository<T> Repository<T>() where T : class
        {
            if (!_repositories.ContainsKey(typeof (T)))
            {
                _repositories.Add(typeof(T), _factory.CreateRepository<T>(_context));
            }
            return _repositories[typeof (T)] as IRepository<T>;
        }

        public void Commit()
        {
            Context.Save();
        }

        public void Dispose()
        {
            Context.Dispose();
        }
    }
}