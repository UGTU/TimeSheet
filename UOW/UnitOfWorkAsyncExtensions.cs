using System;
using System.Threading.Tasks;

namespace UOW
{
    public static class UnitOfWorkAsyncExtensions
    {
        public static Task CommitAsync(this IUnitOfWork source)
        {
            if (source == null) throw new ArgumentNullException("source");
            var task = new Task(source.Commit);
            task.Start();
            return task;
        }
    }
}