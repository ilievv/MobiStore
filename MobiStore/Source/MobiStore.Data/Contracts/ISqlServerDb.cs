﻿using MobiStore.Models;
using MobiStore.Models.MobileDevices;
using MobiStore.Models.MobileDevices.Components;
using MobiStore.Models.Reports;

namespace MobiStore.Data.Contracts
{
    public interface ISqlServerDb
    {
        ISqlServerContext Context { get; }

        IRepository<Battery> Batteries { get; }

        IRepository<Display> Displays { get; }

        IRepository<Processor> Processors { get; }

        IRepository<MobileDevice> MobileDevices { get; }
        
        IRepository<Shop> Shops { get; }

        IRepository<Employee> Employees { get; }

        IRepository<Sale> Sales { get; }

        IRepository<SalesReport> SalesReports { get; }

        void SaveChanges();

        void Dispose();
    }
}