using DeviceMgmt.Repository.Core;
using Infrastructure;

namespace DeviceMgmt.Repository.Interface;

public interface IRepository<T> where T : Entity
{
    T? FindSingle(long id);
    T? FindSingle(string whereSql, object? param = null);
    IEnumerable<T> Find(string? whereSql = null, object? param = null, string? orderBy = null);
    (IEnumerable<T> data, int total) FindPaged(Filter[] filters, int page, int limit, string? orderBy = null);
    long Insert(T entity);
    int Update(T entity);
    int Delete(long id);
    int Delete(IEnumerable<long> ids);
    int ExecuteSql(string sql, object? param = null);
    IEnumerable<TResult> Query<TResult>(string sql, object? param = null);
    int Count(string? whereSql = null, object? param = null);
}
