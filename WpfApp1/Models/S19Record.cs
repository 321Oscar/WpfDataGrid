using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERad5TestGUI.Models
{
    public class S19Record
    {
        public string Address { get; set; }
        public string Data { get; set; }

        public override string ToString()
        {
            return $"{Address} {Data}";
        }
    }

    public class S19RecordFile
    {
        private readonly ObservableCollection<S19Record> s19Records = new ObservableCollection<S19Record>();
        public ObservableCollection<S19Record> S19Records => s19Records;
        public static void ParseS19File(string filePath, ObservableCollection<S19Record> s19Records)
        {
            s19Records.Clear();
            string[] lines = File.ReadAllLines(filePath);
            foreach (var line in lines)
            {
                if (line.StartsWith("S")) // 确保是有效的 S19 记录
                {
                    var record = ParseS19Line(line);
                    if (record != null)
                    {
                        s19Records.Add(record);
                    }
                }
            }
        }
        public void ParseS19File(string filePath)
        {
            ParseS19File(filePath, this.s19Records);
        }

        private static S19Record ParseS19Line(string line)
        {
            if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("S"))
            {
                return null; // 忽略空行或无效行
            }

            char recordType = line[1];
            if (recordType == '0' || recordType == '9') // S0, S9 records are usually ignored for data
            {
                return null;
            }

            int byteCount = Convert.ToInt32(line.Substring(2, 2), 16);

            // S-record format has different address lengths based on the type:
            // S1 - 16-bit address (6 hex digits)
            // S2 - 24-bit address (8 hex digits)
            // S3 - 32-bit address (10 hex digits)
            string address;
            switch (recordType)
            {
                case '1':
                    address = line.Substring(4, 6); // 16-bit address
                    break;
                case '2':
                    address = line.Substring(4, 8); // 24-bit address
                    break;
                case '3':
                    address = line.Substring(4, 10); // 32-bit address
                    break;
                default:
                    return null; // Ignore other types
            }

            // Data starts after the address and before the checksum
            string data = line.Substring(address.Length + 4, line.Length - address.Length - 6).TrimEnd(); // Remove checksum

            // Format data with 4 spaces between each data byte
            string formattedData = string.Join("    ", Enumerable.Range(0, data.Length / 2).Select(i => data.Substring(i * 2, 2)));

            return new S19Record() { Address = $"0x{address.ToUpper()}", Data = formattedData.ToUpper() };
        }
    }
}
