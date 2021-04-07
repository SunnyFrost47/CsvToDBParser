using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CsvHelper;
using System.Collections;
using System.IO;
using System.Globalization;
using CsvHelper.Configuration;
using CsvToDBParser.Models;

namespace CsvToDBParser
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            openFileDialog1.Filter = "CSV files(*.csv)|*.csv|Text files(*.txt)|*.txt";
        }

        private void cSVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.Cancel) return;
            string fileName = openFileDialog1.FileName;

            var bankProcs = new List<BankProcCSV>();

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                //HasHeaderRecord = false,
                Delimiter = ";",
            };

            using (StreamReader streamReader = new StreamReader(fileName, Encoding.GetEncoding(1251)))
            {
                using (CsvReader csvReader = new CsvReader(streamReader, config))
                {
                    while (csvReader.Read())
                    {
                        bankProcs.Add(csvReader.GetRecord<BankProcCSV>());
                    }

                    //var bankProcs = csvReader.GetRecords<BankProcCSV>();
                    dataGridView1.DataSource = bankProcs;
                }
            }
        }
    }
}
