using System;
using System.Data.Linq;
using System.Linq;
using UOW;

namespace TimeSheetMvc4WebApplication.UOWRepositories
{
    public class LinqContext:IDataContext
    {
        private readonly DataContext _context;

        public LinqContext(DataContext context)
        {
            _context = context;
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
            _context.Dispose();
        }

        public bool IsModified { get { return false; } }

        public void Add<T>(T item) where T : class
        {
           _context.GetTable<T>().InsertOnSubmit(item);
        }

        public void Update<T>(T item) where T : class
        {
            _context.GetTable<T>().Attach(item);
        }

        public void Delete<T>(T item) where T : class
        {
            _context.GetTable<T>().DeleteOnSubmit(item);
        }

        public IQueryable<T> AsQueryable<T>() where T : class
        {
            _context.GetTable<T>();
            throw new NotImplementedException();
        }

        public void Save()
        {
            _context.SubmitChanges();
        }
    }
}