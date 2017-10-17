
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Core.Interfaces
{
    public interface IUnitOfWork
    {
        int SaveChanges();
    }
}
