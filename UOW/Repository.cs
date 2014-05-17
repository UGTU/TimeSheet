using System;
using System.Collections.Generic;
using System.Linq;

namespace UOW
{
    /// <summary>
    /// Базовая реализация IRepository
    /// </summary>
    /// <typeparam name="T">Тип элемента коллекции</typeparam>
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly IDataContext _ctx;

        public Repository(IDataContext ctx)
        {
            if (ctx == null) throw new ArgumentNullException("ctx");
            _ctx = ctx;
        }

        public IQueryable<T> AsQueryable()
        {
            return _ctx.AsQueryable<T>();
        }

        public void Add(T item)
        {
            _ctx.Add(item);
        }

        public void Update(T item)
        {
            _ctx.Update(item);
        }

        public void Delete(T item)
        {
            _ctx.Delete(item);
        }
    }
}