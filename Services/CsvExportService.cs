using System;
using System.Data;
using System.IO;
using System.Text;

namespace sberbank.Services
{
    public static class CsvExportService
    {
        public static void ExportDataTable(DataTable table, string filePath)
        {
            var builder = new StringBuilder();

            for (int i = 0; i < table.Columns.Count; i++)
            {
                if (i > 0)
                {
                    builder.Append(";");
                }
                builder.Append(Escape(table.Columns[i].ColumnName));
            }
            builder.AppendLine();

            foreach (DataRow row in table.Rows)
            {
                if (row.RowState == DataRowState.Deleted)
                {
                    continue;
                }

                for (int i = 0; i < table.Columns.Count; i++)
                {
                    if (i > 0)
                    {
                        builder.Append(";");
                    }
                    builder.Append(Escape(Convert.ToString(row[i])));
                }
                builder.AppendLine();
            }

            File.WriteAllText(filePath, builder.ToString(), Encoding.UTF8);
        }

        private static string Escape(string value)
        {
            value = value ?? string.Empty;
            if (value.Contains(";") || value.Contains("\"") || value.Contains("\r") || value.Contains("\n"))
            {
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            }
            return value;
        }
    }
}
