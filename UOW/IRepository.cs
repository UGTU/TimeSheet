using System.Data;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace UOW
{
    /// <summary>
    /// Интерфейс типов, реализующих базовые CRUD операции
    /// </summary>
    /// <typeparam name="T">Тип элемента репозитория</typeparam>
    public interface IRepository<T>
    {
        /// <summary>
        /// Получает интерфейс IQueryable репозитория для выполнения запросов
        /// </summary>
        /// <returns></returns>
        IQueryable<T> AsQueryable();
        /// <summary>
        /// Добавление элемента в репозиторий
        /// </summary>
        /// <param name="item">Объект для добавления в репозиторий</param>
        void Add(T item);
        /// <summary>
        /// Добавляет изменённый элемент в репозиторий
        /// </summary>
        /// <param name="item">Изменённый элемент</param>
        void Update(T item);
        /// <summary>
        /// Удаляет элемент из репозитория
        /// </summary>
        /// <param name="item">Элемент для удаления</param>
        void Delete(T item);
    }

    public interface IQueryableRepository<out T> : IQueryable<T>
    {
        
    }
}
