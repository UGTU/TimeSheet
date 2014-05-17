using System;

namespace UOW
{
    
    /// <summary>
    /// Определяет интерфейс доступа к репозитариям с поддержкой транзакционности
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Получает контекст данных, связанный с экземпляром Unit of Work
        /// </summary>
        IDataContext Context { get; }
        /// <summary>
        /// Получает репозитарий объектов заданного типа
        /// </summary>
        /// <typeparam name="T">Тип объекта репозитария</typeparam>
        /// <returns>Объект репозитария</returns>
        IRepository<T> Repository<T>() where T : class; 
        /// <summary>
        /// Применяет изменения, выполненные в репозитариях
        /// </summary>
        void Commit();

    }
}