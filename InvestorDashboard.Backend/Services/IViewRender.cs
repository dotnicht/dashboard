using System;
using System.Collections.Generic;
using System.Text;

namespace InvestorDashboard.Backend.Services
{
    public interface IViewRenderService
    {
        string Render<TModel>(string name, TModel model);
    }
}
