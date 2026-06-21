using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace GymFlow
{
    public partial class frmMemberDetail : Form
    {
        static readonly Color ClrBg      = Color.FromArgb(18,  18,  35);
        static readonly Color ClrCard    = Color.FromArgb(28,  28,  52);
        static readonly Color ClrText    = Color.FromArgb(240, 240, 255);
        static readonly Color ClrSub     = Color.FromArgb(140, 140, 180);
        static readonly Color ClrAccent  = Color.FromArgb(255, 107, 107);
        static readonly Color ClrGreen   = Color.FromArgb(78,  205, 196);
        static readonly Color ClrYellow  = Color.FromArgb(255, 209,  92);
        static readonly Color ClrPurple  = Color.FromArgb(162,  89, 255);

        private string _memberId;

        public frmMemberDetail(string memberId) { _memberId = memberId; InitializeComponent(); BuildUI(); }

        private void BuildUI()
        {
            Text            = "🔍 會員詳細資料";
            ClientSize      = new Size(700, 620);
            BackColor       = ClrBg;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            StartPosition   = FormStartPosition.CenterParent;
            Font            = new Font("Microsoft JhengHei UI", 10f);

            var members     = DatabaseHelper.LoadTable("Members");
            var payments    = DatabaseHelper.LoadTable("Payments");
            var plans       = DatabaseHelper.LoadTable("Plans");
            var checkins    = DatabaseHelper.LoadTable("CheckIns");
            var enrollments = DatabaseHelper.LoadTable("Enrollments");
            var classes     = DatabaseHelper.LoadTable("Classes");

            var member = members.AsEnumerable().FirstOrDefault(r => r["MemberID"].ToString() == _memberId);
            if (member == null) { Close(); return; }

            // ── 基本資料卡片 ──────────────────────────────────────────────────
            var pnlInfo = new Panel { Size = new Size(640, 120), Location = new Point(30, 20), BackColor = ClrCard };
            pnlInfo.Paint += (s, e) =>
            {
                using (var pen = new System.Drawing.Pen(Color.FromArgb(60, 60, 100), 1.5f))
                    e.Graphics.DrawRectangle(pen, 0, 0, pnlInfo.Width - 1, pnlInfo.Height - 1);
                using (var br = new System.Drawing.SolidBrush(ClrAccent))
                    e.Graphics.FillRectangle(br, 0, 0, pnlInfo.Width, 4);
            };

            // 取得最新到期日
            string expire = payments.AsEnumerable()
                .Where(p => p["MemberID"].ToString() == _memberId)
                .OrderByDescending(p => p["ExpireDate"].ToString())
                .Select(p => p["ExpireDate"].ToString()).FirstOrDefault() ?? "尚未繳費";
            bool isActive = string.Compare(expire, DateTime.Today.ToString("yyyy-MM-dd")) >= 0;

            pnlInfo.Controls.Add(new Label { Text = member["Name"].ToString(), Font = new Font("Microsoft JhengHei UI", 18f, FontStyle.Bold), ForeColor = ClrAccent, Size = new Size(300, 45), Location = new Point(20, 15), BackColor = Color.Transparent });
            pnlInfo.Controls.Add(new Label { Text = "ID: " + _memberId, Font = new Font("Microsoft JhengHei UI", 9f), ForeColor = ClrSub, Size = new Size(200, 22), Location = new Point(22, 55), BackColor = Color.Transparent });
            pnlInfo.Controls.Add(new Label { Text = "📞 " + member["Phone"], Font = new Font("Microsoft JhengHei UI", 10f), ForeColor = ClrText, Size = new Size(250, 24), Location = new Point(22, 80), BackColor = Color.Transparent });

            var lblStatus = new Label { Text = isActive ? "✅ 有效會員" : "❌ 會籍已過期", Font = new Font("Microsoft JhengHei UI", 11f, FontStyle.Bold), ForeColor = isActive ? ClrGreen : ClrAccent, Size = new Size(200, 30), Location = new Point(420, 20), BackColor = Color.Transparent };
            var lblExpire = new Label { Text = "到期日：" + expire, Font = new Font("Microsoft JhengHei UI", 10f), ForeColor = ClrSub, Size = new Size(200, 24), Location = new Point(420, 52), BackColor = Color.Transparent };
            var lblJoin   = new Label { Text = "加入日：" + member["JoinDate"], Font = new Font("Microsoft JhengHei UI", 10f), ForeColor = ClrSub, Size = new Size(200, 24), Location = new Point(420, 78), BackColor = Color.Transparent };
            pnlInfo.Controls.Add(lblStatus); pnlInfo.Controls.Add(lblExpire); pnlInfo.Controls.Add(lblJoin);
            Controls.Add(pnlInfo);

            // ── Tab 切換 ──────────────────────────────────────────────────────
            var tabs = new TabControl
            {
                Size      = new Size(640, 450),
                Location  = new Point(30, 155),
                Font      = new Font("Microsoft JhengHei UI", 10f),
                DrawMode  = TabDrawMode.OwnerDrawFixed,
                ItemSize  = new Size(160, 38),
                SizeMode  = TabSizeMode.Fixed
            };
            tabs.DrawItem += (s, e) =>
            {
                var g    = e.Graphics;
                bool sel = (e.Index == tabs.SelectedIndex);
                var bgColor  = sel ? Color.FromArgb(255, 107, 107) : Color.FromArgb(35, 35, 65);
                var txtColor = sel ? Color.White : Color.FromArgb(140, 140, 180);
                g.FillRectangle(new System.Drawing.SolidBrush(bgColor), e.Bounds);
                var sf = new System.Drawing.StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(tabs.TabPages[e.Index].Text, new Font("Microsoft JhengHei UI", 10f, sel ? FontStyle.Bold : FontStyle.Regular), new System.Drawing.SolidBrush(txtColor), e.Bounds, sf);
            };
            Controls.Add(tabs);

            // 繳費記錄
            var tabPay = new TabPage("💳 繳費記錄") { BackColor = ClrBg };
            var gridPay = MakeGrid();
            gridPay.Size = new Size(620, 390); gridPay.Location = new Point(5, 10);
            var dtPay = new DataTable();
            dtPay.Columns.Add("方案"); dtPay.Columns.Add("金額"); dtPay.Columns.Add("繳費日期"); dtPay.Columns.Add("到期日");
            foreach (DataRow p in payments.AsEnumerable().Where(r => r["MemberID"].ToString() == _memberId).OrderByDescending(r => r["PayDate"].ToString()).ToList())
            {
                var pl = plans.AsEnumerable().FirstOrDefault(r => r["PlanID"].ToString() == p["PlanID"].ToString());
                dtPay.Rows.Add(pl?["PlanName"] ?? "—", pl?["Price"] ?? "—", p["PayDate"], p["ExpireDate"]);
            }
            gridPay.DataSource = dtPay;
            tabPay.Controls.Add(gridPay);

            // 打卡記錄
            var tabCheck = new TabPage("✅ 打卡記錄") { BackColor = ClrBg };
            var gridCheck = MakeGrid();
            gridCheck.Size = new Size(620, 390); gridCheck.Location = new Point(5, 10);
            var dtCheck = new DataTable();
            dtCheck.Columns.Add("入場時間"); dtCheck.Columns.Add("退場時間"); dtCheck.Columns.Add("在館時間");
            foreach (DataRow c in checkins.AsEnumerable().Where(r => r["MemberID"].ToString() == _memberId).OrderByDescending(r => r["CheckInTime"].ToString()).ToList())
            {
                string inTime  = c["CheckInTime"].ToString();
                string outTime = SafeGet(c, "CheckOutTime");
                string duration = "";
                if (!string.IsNullOrEmpty(outTime))
                {
                    var span = DateTime.Parse(outTime) - DateTime.Parse(inTime);
                    duration = (int)span.TotalHours + " 小時 " + span.Minutes + " 分鐘";
                }
                dtCheck.Rows.Add(inTime, string.IsNullOrEmpty(outTime) ? "尚未退場" : outTime, string.IsNullOrEmpty(outTime) ? "—" : duration);
            }
            gridCheck.DataSource = dtCheck;
            tabCheck.Controls.Add(gridCheck);

            // 課程報名
            var tabClass = new TabPage("📅 報名課程") { BackColor = ClrBg };
            var gridClass = MakeGrid();
            gridClass.Size = new Size(620, 390); gridClass.Location = new Point(5, 10);
            var dtClass = new DataTable();
            dtClass.Columns.Add("課程名稱"); dtClass.Columns.Add("教練"); dtClass.Columns.Add("時間"); dtClass.Columns.Add("報名日期");
            foreach (DataRow en in enrollments.AsEnumerable().Where(r => r["MemberID"].ToString() == _memberId).ToList())
            {
                var cl = classes.AsEnumerable().FirstOrDefault(r => r["ClassID"].ToString() == en["ClassID"].ToString());
                if (cl != null) dtClass.Rows.Add(cl["ClassName"], cl["Coach"], cl["Schedule"], en["EnrollDate"]);
            }
            gridClass.DataSource = dtClass;
            tabClass.Controls.Add(gridClass);

            tabs.TabPages.AddRange(new[] { tabPay, tabCheck, tabClass });
        }

        private DataGridView MakeGrid()
        {
            var g = new DataGridView
            {
                BackgroundColor = Color.FromArgb(22, 22, 44), GridColor = Color.FromArgb(50, 50, 80),
                BorderStyle = BorderStyle.None, RowHeadersVisible = false,
                AllowUserToAddRows = false, AllowUserToDeleteRows = false, ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                Font = new Font("Microsoft JhengHei UI", 9f),
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight = 36
            };
            g.DefaultCellStyle.BackColor          = Color.FromArgb(22, 22, 44);
            g.DefaultCellStyle.ForeColor          = Color.FromArgb(240, 240, 255);
            g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(60, 30, 80);
            g.DefaultCellStyle.SelectionForeColor = Color.White;
            g.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(35, 35, 65);
            g.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(255, 107, 107);
            g.ColumnHeadersDefaultCellStyle.Font      = new Font("Microsoft JhengHei UI", 10f, FontStyle.Bold);
            g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(26, 26, 50);
            return g;
        }

        private static string SafeGet(DataRow row, string col)
        {
            if (!row.Table.Columns.Contains(col)) return "";
            return row[col]?.ToString() ?? "";
        }
    }
}
