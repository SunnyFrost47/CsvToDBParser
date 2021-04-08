using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvToDBParser.Models
{
    class BankProcessContext : DbContext
    {
        public BankProcessContext() 
            :base("BankProcessDB")
        { }

        public DbSet<BankProcType> BankProcTypes { get; set; }
        public DbSet<BankTheme> BankThemes { get; set; }
        public DbSet<BankAction> BankActions { get; set; }
        public DbSet<BankProcess> BankProcesses { get; set; }
        public DbSet<BankDepartment> BankDepartments { get; set; }
    }
}
