using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace GymFlow
{
    public partial class frmEditMember : Form
    {
        static readonly Color ClrBg     = Color.FromArgb(18, 18, 35);
        static readonly Color ClrInput  = Color.FromArgb(38, 38, 68);
        static readonly Color ClrText   = Color.FromArgb(240, 240, 255);
        static readonly Color ClrSub    = Color.FromArgb(140, 140, 180);
        static readonly Color ClrAccent = Color.FromArgb(255, 107, 107);

        private string _memberId;

        public frmEditMember(string memberId) { _memberId = memberId; InitializeComponent(); BuildUI(); }

        private void BuildUI()
        {
            Text = "✏️ 編輯會員資料"; ClientSize = new Size(420, 380);
            BackColor = ClrBg; FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false; StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Microsoft JhengHei UI", 10f);

            // 載入現有資料
            var members = DatabaseHelper.LoadTable("Members");
            var member  = members.AsEnumerable().FirstOrDefault(r => r["MemberID"].ToString() == _memberId);
            if (member == null) { MessageBox.Show("找不到此會員！"); Close(); return; }

            string[] labels = { "姓名 *", "電話", "Email", "加入日期" };
            string[] values = { member["Name"].ToString(), member["Phone"].ToString(), member["Email"].ToString(), member["JoinDate"].ToString() };
            var txts = new TextBox[4];

            for (int i = 0; i < 4; i++)
            {
                Controls.Add(new Label { Text = labels[i], ForeColor = ClrSub, Size = new Size(340, 24), Location = new Point(40, 30 + i * 70), BackColor = Color.Transparent });
                txts[i] = new TextBox { Size = new Size(340, 34), Location = new Point(40, 55 + i * 70), BackColor = ClrInput, ForeColor = ClrText, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Microsoft JhengHei UI", 11f), Text = values[i] };
                Controls.Add(txts[i]);
            }

            var lblErr = new Label { Text = "", ForeColor = ClrAccent, Size = new Size(340, 22), Location = new Point(40, 308), BackColor = Color.Transparent, Font = new Font("Microsoft JhengHei UI", 9f) };
            Controls.Add(lblErr);

            var btnSave = new Button { Text = "💾 儲存", Size = new Size(150, 40), Location = new Point(135, 328), FlatStyle = FlatStyle.Flat, BackColor = ClrAccent, ForeColor = Color.White, Font = new Font("Microsoft JhengHei UI", 11f, FontStyle.Bold), Cursor = Cursors.Hand };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += (s, e) =>
            {
                string name = txts[0].Text.Trim();
                if (string.IsNullOrEmpty(name)) { lblErr.Text = "❌ 姓名不能為空！"; return; }

                var dt = DatabaseHelper.LoadTable("Members");
                foreach (DataRow r in dt.Rows)
                {
                    if (r["MemberID"].ToString() != _memberId) continue;
                    r["Name"]     = txts[0].Text.Trim();
                    r["Phone"]    = txts[1].Text.Trim();
                    r["Email"]    = txts[2].Text.Trim();
                    r["JoinDate"] = txts[3].Text.Trim();
                    break;
                }
                DatabaseHelper.SaveTable("Members", dt);
                MessageBox.Show("會員資料已更新！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            };
            Controls.Add(btnSave);
        }
    }
}
