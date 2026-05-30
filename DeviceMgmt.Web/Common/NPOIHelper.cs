using System.Data;
using System.Text;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;

namespace DeviceMgmt.Web.Common;

public static class NPOIHelper
{
    /// <summary>生成只含表头的导入模板（.xls）</summary>
    public static byte[] BuildTemplate(IEnumerable<string> headers, string sheetName = "Sheet1")
    {
        IWorkbook book = new HSSFWorkbook();
        var sheet = book.CreateSheet(string.IsNullOrEmpty(sheetName) ? "Sheet1" : sheetName);
        var header = sheet.CreateRow(0);
        var c = 0;
        foreach (var h in headers) header.CreateCell(c++).SetCellValue(h);
        using var ms = new MemoryStream();
        book.Write(ms, false);
        return ms.ToArray();
    }

    /// <summary>读取上传文件为「表头 + 行字典」。支持 .xls/.xlsx/.csv</summary>
    public static (List<string> headers, List<Dictionary<string, string>> rows) ReadRows(Stream stream, string fileName)
    {
        var headers = new List<string>();
        var rows = new List<Dictionary<string, string>>();
        var ext = (Path.GetExtension(fileName) ?? string.Empty).ToLowerInvariant();

        if (ext == ".csv")
        {
            using var reader = new StreamReader(stream, Encoding.UTF8, true);
            string? line;
            var isFirst = true;
            while ((line = reader.ReadLine()) != null)
            {
                var cells = SplitCsvLine(line);
                if (isFirst) { headers.AddRange(cells.Select(x => x.Trim())); isFirst = false; continue; }
                if (cells.All(string.IsNullOrWhiteSpace)) continue;
                var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                for (var i = 0; i < headers.Count && i < cells.Count; i++) dict[headers[i]] = cells[i].Trim();
                rows.Add(dict);
            }
            return (headers, rows);
        }

        IWorkbook book = WorkbookFactory.Create(stream);
        var sheet = book.GetSheetAt(0);
        if (sheet == null) return (headers, rows);
        var headerRow = sheet.GetRow(sheet.FirstRowNum);
        if (headerRow == null) return (headers, rows);
        for (var i = 0; i < headerRow.LastCellNum; i++)
            headers.Add(GetCellString(headerRow.GetCell(i)).Trim());

        for (var r = sheet.FirstRowNum + 1; r <= sheet.LastRowNum; r++)
        {
            var row = sheet.GetRow(r);
            if (row == null) continue;
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var hasValue = false;
            for (var i = 0; i < headers.Count; i++)
            {
                var v = GetCellString(row.GetCell(i)).Trim();
                if (!string.IsNullOrEmpty(v)) hasValue = true;
                dict[headers[i]] = v;
            }
            if (hasValue) rows.Add(dict);
        }
        return (headers, rows);
    }

    private static string GetCellString(ICell? cell)
    {
        if (cell == null) return string.Empty;
        switch (cell.CellType)
        {
            case CellType.String: return cell.StringCellValue ?? string.Empty;
            case CellType.Numeric:
                return DateUtil.IsCellDateFormatted(cell)
                    ? cell.DateCellValue?.ToString("yyyy-MM-dd HH:mm:ss") ?? string.Empty
                    : cell.NumericCellValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
            case CellType.Boolean: return cell.BooleanCellValue ? "true" : "false";
            case CellType.Formula:
                try { return cell.StringCellValue ?? string.Empty; }
                catch { return cell.NumericCellValue.ToString(System.Globalization.CultureInfo.InvariantCulture); }
            default: return string.Empty;
        }
    }

    private static List<string> SplitCsvLine(string line)
    {
        var result = new List<string>();
        var sb = new StringBuilder();
        var inQuotes = false;
        for (var i = 0; i < line.Length; i++)
        {
            var ch = line[i];
            if (inQuotes)
            {
                if (ch == '"')
                {
                    if (i + 1 < line.Length && line[i + 1] == '"') { sb.Append('"'); i++; }
                    else inQuotes = false;
                }
                else sb.Append(ch);
            }
            else
            {
                if (ch == '"') inQuotes = true;
                else if (ch == ',') { result.Add(sb.ToString()); sb.Clear(); }
                else sb.Append(ch);
            }
        }
        result.Add(sb.ToString());
        return result;
    }

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
