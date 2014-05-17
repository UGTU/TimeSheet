using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace UOW
{
    /// <summary>
    /// Определяет методы контекста данных
    /// </summary>
    public interface IDataContext : IDisposable
    {
        bool IsModified { get; }
        /// <summary>
        /// Добавляет новый  элемент в коллекцию заданного типа
        /// </summary>
        /// <typeparam name="T">Тип элемента коллекции</typeparam>
        /// <param name="item">Элемент</param>
        void Add<T>(T item) where T : class;
        /// <summary>
        /// Обновляет указанный элемент коллекции заданного типа
        /// </summary>
        /// <typeparam name="T">Тип элемента коллекции</typeparam>
        /// <param name="item">Элемент</param>
        void Update<T>(T item) where T : class;
        /// <summary>
        /// Удаляет указанный элемент из коллекции заданного типа
        /// </summary>
        /// <typeparam name="T">Тип элемента коллекции</typeparam>
        /// <param name="item">Элемент</param>
        void Delete<T>(T item) where T : class;
        /// <summary>
        /// Получает доступ к интерфейсу IQueryable коллекции заданного типа
        /// </summary>
        /// <typeparam name="T">Тип элемента коллекции</typeparam>
        /// <returns>Объект типа IQueryable</returns>
        IQueryable<T> AsQueryable<T>() where T : class;
        /// <summary>
        /// Сохраняет изменения, внесённые в коллекции
        /// </summary>
        void Save();
    }
}