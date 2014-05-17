using System;
using System.Collections;
using System.Collections.Generic;

namespace UOW
{
    
    /// <summary>
    /// Опредедяет фабрику классов репозитариев
    /// </summary>
    public interface IRepositoryFactory
    {
        /// <summary>
        /// Создаёт экземпляр репозитария заданного типа
        /// </summary>
        /// <typeparam name="T">Тип репозитария</typeparam>
        /// <param name="ctx">Объект контекста данных для репозитария</param>
        /// <returns>Репозитарий объектов заданного типа</returns>
        IRepository<T> CreateRepository<T>(IDataContext ctx) where T :class;
    }


 
    public class RepositoryFactory : IRepositoryFactory
    {
        readonly Dictionary<Type, Type> _bindings = new Dictionary<Type, Type>();
 
        public static readonly RepositoryFactory Default = new RepositoryFactory();

        public void Bind<T, TU>() where T : class where TU : class
        {
            _bindings.Add(typeof(T), typeof(TU));            
        }
        public IRepository<T> CreateRepository<T>(IDataContext ctx) where T : class
        {
            if (_bindings.ContainsKey(typeof (T)))
            {
                return Activator.CreateInstance(_bindings[typeof (T)]) as IRepository<T>;
            }
            return new Repository<T>(ctx);
        }
    }
}