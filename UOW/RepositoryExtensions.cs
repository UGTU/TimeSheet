
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace UOW
{
    public static class RepositoryExtensions
    {
        /// <summary>
        /// Удаляет элемент репозитория по заданному условию
        /// </summary>
        /// <typeparam name="T">Тип элемента коллекции</typeparam>
        /// <param name="sourceRepository">Репозиторий</param>
        /// <param name="predicate">Выражение фильтрации</param>
        public static void Delete<T>(this IRepository<T> sourceRepository, Expression<Func<T, bool>> predicate)
        {
            if (sourceRepository == null) throw new ArgumentNullException("sourceRepository");
            var item = Single(sourceRepository, predicate);
            sourceRepository.Delete(item);
        }
        
        public static T Single<T>(this IRepository<T> sourceRepository, Expression<Func<T, bool>> predicate)         
        {
            if (sourceRepository == null) throw new ArgumentNullException("sourceRepository");
            return sourceRepository.AsQueryable().Single(predicate);
        }

        public static IList<T> ToList<T>(this IRepository<T> sourceRepository)
        {
            if (sourceRepository == null) throw new ArgumentNullException("sourceRepository");
            return sourceRepository.AsQueryable().ToList();
        }
    }
}