// ======================================
// Author: Ebenezer Monney
// Email:  info@ebenmonney.com
// Copyright (c) 2017 www.ebenmonney.com
// 
// ==> Gun4Hire: contact@ebenmonney.com
// ======================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InvestorDashboard.Backend.Core.Interfaces;
using InvestorDashboard.Backend.Database;

namespace InvestorDashboard.Backend.Core
{
    public class UnitOfWork : IUnitOfWork
    {
        readonly ApplicationDbContext _context;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }
        public int SaveChanges()
        {
            return _context.SaveChanges();
        }
    }
}
