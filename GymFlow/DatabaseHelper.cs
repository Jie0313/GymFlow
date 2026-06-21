using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Xml.Linq;

namespace GymFlow
{
    public static class DatabaseHelper
    {
        private static readonly string DataFolder = "GymFlowData";

        private static string FilePath(string table) =>
            Path.Combine(DataFolder, table + ".xml");

        public static void InitializeDatabase()
        {
            if (!Directory.Exists(DataFolder))
                Directory.CreateDirectory(DataFolder);

            // 建立預設資料
            if (!File.Exists(FilePath("Plans")))
            {
                var plans = new XElement("Plans",
                    NewRow(1, "PlanID,PlanName,Price,DurationDays", "1,月費方案,800,30"),
                    NewRow(2, "PlanID,PlanName,Price,DurationDays", "2,季費方案,2000,90"),
                    NewRow(3, "PlanID,PlanName,Price,DurationDays", "3,年費方案,6000,365")
                );
                plans.Save(FilePath("Plans"));
            }
            if (!File.Exists(FilePath("Members")))
            {
                var members = new XElement("Members",
                    MakeRow("MemberID,Name,Phone,Email,JoinDate", new[] { "1", "王小明", "0912-345678", "ming@example.com", "2025-01-10" }),
                    MakeRow("MemberID,Name,Phone,Email,JoinDate", new[] { "2", "林美玲", "0923-456789", "mei@example.com", "2025-03-05" }),
                    MakeRow("MemberID,Name,Phone,Email,JoinDate", new[] { "3", "張大偉", "0934-567890", "wei@example.com", "2025-05-20" })
                );
                members.Save(FilePath("Members"));
            }
            if (!File.Exists(FilePath("Payments")))
            {
                var payments = new XElement("Payments",
                    MakeRow("PaymentID,MemberID,PlanID,PayDate,ExpireDate", new[] { "1", "1", "2", "2025-01-10", "2026-07-10" }),
                    MakeRow("PaymentID,MemberID,PlanID,PayDate,ExpireDate", new[] { "2", "2", "1", "2026-06-01", "2026-07-01" }),
                    MakeRow("PaymentID,MemberID,PlanID,PayDate,ExpireDate", new[] { "3", "3", "3", "2025-05-20", "2026-05-20" })
                );
                payments.Save(FilePath("Payments"));
            }
            if (!File.Exists(FilePath("Classes")))
            {
                var classes = new XElement("Classes",
                    MakeRow("ClassID,ClassName,Coach,Schedule,MaxCapacity", new[] { "1", "瑜伽入門", "陳教練", "週一 09:00", "15" }),
                    MakeRow("ClassID,ClassName,Coach,Schedule,MaxCapacity", new[] { "2", "有氧舞蹈", "林教練", "週三 18:00", "20" }),
                    MakeRow("ClassID,ClassName,Coach,Schedule,MaxCapacity", new[] { "3", "重訓基礎", "黃教練", "週五 19:00", "12" })
                );
                classes.Save(FilePath("Classes"));
            }
            if (!File.Exists(FilePath("Enrollments")))
            {
                var enroll = new XElement("Enrollments",
                    MakeRow("EnrollID,MemberID,ClassID,EnrollDate", new[] { "1", "1", "1", "2025-01-11" }),
                    MakeRow("EnrollID,MemberID,ClassID,EnrollDate", new[] { "2", "2", "2", "2025-03-06" }),
                    MakeRow("EnrollID,MemberID,ClassID,EnrollDate", new[] { "3", "3", "3", "2025-05-21" })
                );
                enroll.Save(FilePath("Enrollments"));
            }
            if (!File.Exists(FilePath("CheckIns")))
                new XElement("CheckIns").Save(FilePath("CheckIns"));
        }

        private static XElement MakeRow(string fields, string[] values)
        {
            var row = new XElement("Row");
            string[] cols = fields.Split(',');
            for (int i = 0; i < cols.Length; i++)
                row.Add(new XElement(cols[i], i < values.Length ? values[i] : ""));
            return row;
        }

        private static XElement NewRow(int id, string fields, string csv)
            => MakeRow(fields, csv.Split(','));

        // ── 讀取整張表為 DataTable ────────────────────────────────────────────
        public static DataTable LoadTable(string table)
        {
            var dt = new DataTable(table);
            if (!File.Exists(FilePath(table))) return dt;
            var root = XElement.Load(FilePath(table));
            foreach (var row in root.Elements("Row"))
            {
                if (dt.Columns.Count == 0)
                    foreach (var el in row.Elements())
                        dt.Columns.Add(el.Name.LocalName);
                var dr = dt.NewRow();
                foreach (DataColumn col in dt.Columns)
                    dr[col.ColumnName] = row.Element(col.ColumnName)?.Value ?? "";
                dt.Rows.Add(dr);
            }
            return dt;
        }

        public static void SaveTable(string table, DataTable dt)
        {
            var root = new XElement(table);
            foreach (DataRow dr in dt.Rows)
            {
                var row = new XElement("Row");
                foreach (DataColumn col in dt.Columns)
                    row.Add(new XElement(col.ColumnName, dr[col.ColumnName]?.ToString() ?? ""));
                root.Add(row);
            }
            root.Save(FilePath(table));
        }

        public static int GetNextId(string table, string idCol)
        {
            var dt = LoadTable(table);
            int max = 0;
            foreach (DataRow dr in dt.Rows)
                if (int.TryParse(dr[idCol]?.ToString(), out int v) && v > max) max = v;
            return max + 1;
        }

        public static void InsertRow(string table, string idCol, Dictionary<string, string> values)
        {
            var dt = LoadTable(table);
            // 如果是空表，建立所有欄位
            if (dt.Columns.Count == 0)
            {
                dt.Columns.Add(idCol);
                foreach (var k in values.Keys) dt.Columns.Add(k);
            }
            else
            {
                // 動態新增缺少的欄位（例如 CheckOutTime）
                if (!dt.Columns.Contains(idCol)) dt.Columns.Add(idCol);
                foreach (var k in values.Keys)
                    if (!dt.Columns.Contains(k)) dt.Columns.Add(k);
                // 補齊舊資料缺少的欄位值
                foreach (DataRow existRow in dt.Rows)
                    foreach (var k in values.Keys)
                        if (existRow[k] == DBNull.Value) existRow[k] = "";
            }
            int newId = GetNextId(table, idCol);
            var dr = dt.NewRow();
            foreach (DataColumn col in dt.Columns)
                dr[col.ColumnName] = values.ContainsKey(col.ColumnName) ? values[col.ColumnName] : "";
            dr[idCol] = newId.ToString(); // 最後設定，確保不被覆蓋
            dt.Rows.Add(dr);
            SaveTable(table, dt);
        }

        public static void DeleteRow(string table, string idCol, string idValue)
        {
            var dt = LoadTable(table);
            for (int i = dt.Rows.Count - 1; i >= 0; i--)
                if (dt.Rows[i][idCol]?.ToString() == idValue)
                    dt.Rows.RemoveAt(i);
            SaveTable(table, dt);
        }
    }
}
