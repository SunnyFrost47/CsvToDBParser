using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;

namespace CsvToDBParser.Models
{
    public class BankProcCSV
    {
        [Index(0)]
        public string Id { get; set; }
        [Index(1)]
        public string Name { get; set; }
        [Index(2)]
        public string Department { get; set; }
    }
}
