using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace GymFlow
{
    public partial class frmPayment : Form
    {
        static readonly Color ClrBg     = Color.FromArgb(18, 18, 35);
        static readonly Color ClrInput  = Color.FromArgb(38, 38, 68);
        static readonly Color ClrText   = Color.FromArgb(240, 240, 255);
        static readonly Color ClrAccent = Color.FromArgb(255, 209, 92);

        private ComboBox cmbMember, cmbPlan;
        private Label    lblPrice, lblExpire;
        private DataTable dtPlans;

        public frmPayment() { InitializeComponent(); BuildUI(); }

        private void BuildUI()
        {
            Text = "新增繳費"; ClientSize = new Size(400, 320); BackColor = ClrBg;
            FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Microsoft JhengHei UI", 10f);

            Controls.Add(new Label { Text = "選擇會員 *", ForeColor = Color.FromArgb(140,140,180), Size = new Size(320, 24), Location = new Point(30, 30), BackColor = Color.Transparent });
            cmbMember = new ComboBox { Size = new Size(310, 30), Location = new Point(30, 55), BackColor = ClrInput, ForeColor = ClrText, DropDownStyle = ComboBoxStyle.DropDownList };
            Controls.Add(cmbMember);

            Controls.Add(new Label { Text = "選擇方案 *", ForeColor = Color.FromArgb(140,140,180), Size = new Size(320, 24), Location = new Point(30, 100), BackColor = Color.Transparent });
            cmbPlan = new ComboBox { Size = new Size(310, 30), Location = new Point(30, 125), BackColor = ClrInput, ForeColor = ClrText, DropDownStyle = ComboBoxStyle.DropDownList };
            Controls.Add(cmbPlan);

            lblPrice  = new Label { Text = "", ForeColor = ClrAccent, Size = new Size(310, 30), Location = new Point(30, 168), BackColor = Color.Transparent, Font = new Font("Microsoft JhengHei UI", 11f, FontStyle.Bold) };
            lblExpire = new Label { Text = "", ForeColor = Color.FromArgb(78, 205, 196), Size = new Size(310, 40), Location = new Point(30, 198), BackColor = Color.Transparent };
            Controls.Add(lblPrice);
            Controls.Add(lblExpire);

            var dtMembers = DatabaseHelper.LoadTable("Members");
            cmbMember.DataSource = dtMembers; cmbMember.DisplayMember = "Name"; cmbMember.ValueMember = "MemberID";

            dtPlans = DatabaseHelper.LoadTable("Plans");
            cmbPlan.DataSource = dtPlans; cmbPlan.DisplayMember = "PlanName"; cmbPlan.ValueMember = "PlanID";

            cmbPlan.SelectedIndexChanged   += (s, e) => UpdateInfo();
            cmbMember.SelectedIndexChanged += (s, e) => UpdateInfo();
            UpdateInfo();

            var btn = new Button { Text = "💳 確認繳費", Size = new Size(150, 38), Location = new Point(125, 260), FlatStyle = FlatStyle.Flat, BackColor = ClrAccent, ForeColor = Color.FromArgb(18, 18, 35), Font = new Font("Microsoft JhengHei UI", 10f, FontStyle.Bold), Cursor = Cursors.Hand };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += (s, e) =>
            {
                if (cmbMember.SelectedValue == null || cmbPlan.SelectedIndex < 0) return;
                string memberId = cmbMember.SelectedValue.ToString();
                string planId   = cmbPlan.SelectedValue.ToString();
                int    dur      = Convert.ToInt32(dtPlans.Rows[cmbPlan.SelectedIndex]["DurationDays"]);
                string payDate  = DateTime.Today.ToString("yyyy-MM-dd");

                // 計算到期日（從現有到期日延伸或從今天開始）
                DateTime baseDate   = DateTime.Today;
                var existPayments   = DatabaseHelper.LoadTable("Payments");
                string latestExpire = existPayments.AsEnumerable()
                    .Where(p => p["MemberID"].ToString() == memberId)
                    .OrderByDescending(p => p["ExpireDate"].ToString())
                    .Select(p => p["ExpireDate"].ToString()).FirstOrDefault();
                if (!string.IsNullOrEmpty(latestExpire) &&
                    DateTime.TryParse(latestExpire, out DateTime expDt) && expDt > DateTime.Today)
                    baseDate = expDt;

                string expireDate = baseDate.AddDays(dur).ToString("yyyy-MM-dd");

                // 找到此會員現有的繳費記錄
                var allPayments = DatabaseHelper.LoadTable("Payments");
                DataRow existing = allPayments.AsEnumerable()
                    .FirstOrDefault(p => p["MemberID"].ToString() == memberId);

                if (existing != null)
                {
                    // 直接更新，保留原本 ID
                    existing["PlanID"]     = planId;
                    existing["PayDate"]    = payDate;
                    existing["ExpireDate"] = expireDate;
                    DatabaseHelper.SaveTable("Payments", allPayments);
                }
                else
                {
                    // 沒有舊記錄才新增
                    DatabaseHelper.InsertRow("Payments", "PaymentID", new Dictionary<string, string>
                    {
                        { "MemberID", memberId }, { "PlanID", planId },
                        { "PayDate",  payDate  }, { "ExpireDate", expireDate }
                    });
                }

                MessageBox.Show($"繳費完成！\n到期日：{expireDate}", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            };
            Controls.Add(btn);
        }

        private void UpdateInfo()
        {
            if (cmbPlan.SelectedIndex < 0 || dtPlans == null) return;
            var row = dtPlans.Rows[cmbPlan.SelectedIndex];
            int dur = Convert.ToInt32(row["DurationDays"]);
            lblPrice.Text = $"金額：NT$ {row["Price"]}";

            DateTime baseDate = DateTime.Today;
            if (cmbMember.SelectedValue != null)
            {
                string memberId     = cmbMember.SelectedValue.ToString();
                var payments        = DatabaseHelper.LoadTable("Payments");
                string latestExpire = payments.AsEnumerable()
                    .Where(p => p["MemberID"].ToString() == memberId)
                    .OrderByDescending(p => p["ExpireDate"].ToString())
                    .Select(p => p["ExpireDate"].ToString()).FirstOrDefault();
                if (!string.IsNullOrEmpty(latestExpire) &&
                    DateTime.TryParse(latestExpire, out DateTime expDt) && expDt > DateTime.Today)
                {
                    baseDate       = expDt;
                    lblExpire.Text = $"到期日：{baseDate.AddDays(dur):yyyy-MM-dd}\n（從現有到期日延伸）";
                    return;
                }
            }
            lblExpire.Text = $"到期日：{baseDate.AddDays(dur):yyyy-MM-dd}";
        }
    }
}
