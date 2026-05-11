using System.Data;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;

namespace DeviceMgmt.Web.Common;

public static class NPOIHelper
{
    public static byte[] ExportToBytes(DataTable data, string sheetName = "Sheet1")
    {
        IWorkbook book = new HSSFWorkbook();
        var sheet = book.CreateSheet(string.IsNullOrEmpty(sheetName) ? "Sheet1" : sheetName);

        var header = sheet.CreateRow(0);
        for (var c = 0; c < data.Columns.Count; c++)
        {
            header.CreateCell(c).SetCellValue(data.Columns[c].ColumnName);
        }

        for (var r = 0; r < data.Rows.Count; r++)
        {
            var row = sheet.CreateRow(r + 1);
            for (var c = 0; c < data.Columns.Count; c++)
            {
                var v = data.Rows[r][c];
                if (v == null || v == DBNull.Value) { row.CreateCell(c).SetCellValue(string.Empty); continue; }
                switch (v)
                {
                    case DateTime dt:
                        row.CreateCell(c).SetCellValue(dt.ToString("yyyy-MM-dd HH:mm:ss"));
                        break;
                    case double d:
                        row.CreateCell(c).SetCellValue(d);
                        break;
                    case decimal m:
                        row.CreateCell(c).SetCellValue((double)m);
                        break;
                    case int i:
                        row.CreateCell(c).SetCellValue(i);
                        break;
                    case long l:
                        row.CreateCell(c).SetCellValue(l);
                        break;
                    case bool b:
                        row.CreateCell(c).SetCellValue(b);
                        break;
                    default:
                        row.CreateCell(c).SetCellValue(v.ToString());
                        break;
                }
            }
        }

        using var ms = new MemoryStream();
        book.Write(ms, false);
        return ms.ToArray();
    }

    public static DataTable LINQToDataTable<T>(IEnumerable<T> source)
    {
        var dt = new DataTable();
        var props = typeof(T).GetProperties();
        foreach (var p in props)
        {
            var t = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;
            dt.Columns.Add(p.Name, t);
        }
        foreach (var item in source)
        {
            var row = dt.NewRow();
            foreach (var p in props)
            {
                row[p.Name] = p.GetValue(item) ?? DBNull.Value;
            }
            dt.Rows.Add(row);
        }
        return dt;
    }
}
