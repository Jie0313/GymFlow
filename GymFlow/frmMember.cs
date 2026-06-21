using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace GymFlow
{
    public partial class frmMember : Form
    {
        static readonly Color ClrBg     = Color.FromArgb(18, 18, 35);
        static readonly Color ClrInput  = Color.FromArgb(38, 38, 68);
        static readonly Color ClrText   = Color.FromArgb(240, 240, 255);
        static readonly Color ClrAccent = Color.FromArgb(255, 107, 107);

        public frmMember() { InitializeComponent(); BuildUI(); }

        private void BuildUI()
        {
            Text = "新增會員"; ClientSize = new Size(400, 340); BackColor = ClrBg;
            FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Microsoft JhengHei UI", 10f);

            string[] labels = { "姓名 *", "電話", "Email" };
            var txts = new TextBox[3];
            for (int i = 0; i < 3; i++)
            {
                Controls.Add(new Label { Text = labels[i], ForeColor = ClrText, Size = new Size(320, 24), Location = new Point(30, 40 + i * 65), BackColor = Color.Transparent });
                txts[i] = new TextBox { Size = new Size(310, 30), Location = new Point(30, 65 + i * 65), BackColor = ClrInput, ForeColor = ClrText, BorderStyle = BorderStyle.FixedSingle };
                Controls.Add(txts[i]);
            }

            var btn = new Button { Text = "💾 儲存", Size = new Size(120, 38), Location = new Point(140, 270), FlatStyle = FlatStyle.Flat, BackColor = ClrAccent, ForeColor = Color.White, Font = new Font("Microsoft JhengHei UI", 10f, FontStyle.Bold), Cursor = Cursors.Hand };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += (s, e) =>
            {
                string name = txts[0].Text.Trim();
                if (string.IsNullOrEmpty(name)) { MessageBox.Show("請輸入姓名！"); return; }
                DatabaseHelper.InsertRow("Members", "MemberID", new Dictionary<string, string>
                {
                    { "Name", name }, { "Phone", txts[1].Text.Trim() },
                    { "Email", txts[2].Text.Trim() }, { "JoinDate", DateTime.Today.ToString("yyyy-MM-dd") }
                });
                DialogResult = DialogResult.OK; Close();
            };
            Controls.Add(btn);
        }
    }
}
