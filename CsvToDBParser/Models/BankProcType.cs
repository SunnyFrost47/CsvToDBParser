using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvToDBParser.Models
{
    class BankProcType
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public ICollection<BankTheme> BankThemes { get; set; }
    }
}
