using System.Data;

namespace DeviceMgmt.Repository.Interface;

public interface IUnitWork
{
    IDbConnection OpenConnection();
}
