using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvToDBParser.Models
{
    class BankProcess
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }

        public int BankDepartmentId { get; set; }
        public BankDepartment BankDepartment { get; set; }

        public int BankActionId { get; set; }
        public BankAction BankAction { get; set; }
    }
}
