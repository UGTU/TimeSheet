using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UOW
{
    public static class EnumerableAsyncExtensions
    {
        internal static Task<T> StartTask<T>(Func<T> action)
        {
            return Task.Factory.StartNew<T>(action);
        }
        public static Task<List<T>> ToListAsync<T>(this IEnumerable<T> source)
        {
            if (source == null) throw new ArgumentNullException("source");
            return StartTask(source.ToList);
        }
        public static Task<T[]> ToArrayAsync<T>(this IEnumerable<T> source)
        {
            if (source == null) throw new ArgumentNullException("source");
            return StartTask(source.ToArray);
        }
        public static Task<Dictionary<TK, TV>> ToDictionaryAsync<TK, TV>(this IEnumerable<TV> source, 
            Func<TV, TK> keySelector)
        {
            if (source == null) throw new ArgumentNullException("source");
            return StartTask(()=>source.ToDictionary(keySelector));
        }
    }
}