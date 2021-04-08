using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvToDBParser.Models
{
    class BankTheme
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }

        public int BankProcTypeId { get; set; }
        public BankProcType BankProcType { get; set; }

        public ICollection<BankAction> BankActions { get; set; }
    }
}
