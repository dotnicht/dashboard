using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvestorDashboard.Web.Server.Models.DashboardModels
{
  public class IcoInfoModel
  {
    public int TotalInvestors { get; set; }
    public double TotalUsd { get; set; }
    public int TotalCoins { get; set; }
  }
}
