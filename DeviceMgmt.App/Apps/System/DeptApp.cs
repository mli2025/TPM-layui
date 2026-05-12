using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.System;

public class DeptApp : BaseApp<Sys_Dept>
{
    public DeptApp(IUnitWork unitWork, IRepository<Sys_Dept> repository) : base(unitWork, repository)
    {
    }

    public long Save(Sys_Dept d)
    {
        if (d.Id == 0)
        {
            if (d.Status == 0) d.Status = 1;
            Repository.Insert(d);
        }
        else
        {
            Repository.Update(d);
        }
        return d.Id;
    }
}
