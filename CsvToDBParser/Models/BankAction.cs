using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvToDBParser.Models
{
    class BankAction
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }

        public int BankThemeId { get; set; }
        public BankTheme BankTheme { get; set; }

        public ICollection<BankProcess> BankProcesses { get; set; }
    }
}
