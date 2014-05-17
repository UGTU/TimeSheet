using System;

namespace UOW
{
 
    public interface IRepositoryEntity<out T> where T : struct
    {
        T Id { get; }
    }
}