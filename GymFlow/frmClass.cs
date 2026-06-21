using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace GymFlow
{
    public partial class frmClass : Form
    {
        static readonly Color ClrBg     = Color.FromArgb(18, 18, 35);
        static readonly Color ClrInput  = Color.FromArgb(38, 38, 68);
        static readonly Color ClrText   = Color.FromArgb(240, 240, 255);
        static readonly Color ClrAccent = Color.FromArgb(78, 205, 196);

        public frmClass() { InitializeComponent(); BuildUI(); }

        private void BuildUI()
        {
            Text = "新增課程"; ClientSize = new Size(400, 400); BackColor = ClrBg;
            FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Microsoft JhengHei UI", 10f);

            string[] labels = { "課程名稱 *", "教練", "時間 (如: 週一 09:00)", "人數上限 *" };
            var txts = new TextBox[4];
            for (int i = 0; i < 4; i++)
            {
                Controls.Add(new Label { Text = labels[i], ForeColor = ClrText, Size = new Size(320, 24), Location = new Point(30, 30 + i * 65), BackColor = Color.Transparent });
                txts[i] = new TextBox { Size = new Size(310, 30), Location = new Point(30, 55 + i * 65), BackColor = ClrInput, ForeColor = ClrText, BorderStyle = BorderStyle.FixedSingle };
                Controls.Add(txts[i]);
            }
            txts[3].Text = "20";

            var btn = new Button { Text = "💾 儲存", Size = new Size(120, 38), Location = new Point(140, 340), FlatStyle = FlatStyle.Flat, BackColor = ClrAccent, ForeColor = Color.White, Font = new Font("Microsoft JhengHei UI", 10f, FontStyle.Bold), Cursor = Cursors.Hand };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += (s, e) =>
            {
                string name = txts[0].Text.Trim();
                if (string.IsNullOrEmpty(name)) { MessageBox.Show("請輸入課程名稱！"); return; }
                if (!int.TryParse(txts[3].Text.Trim(), out int cap) || cap <= 0) { MessageBox.Show("請輸入有效的人數上限！"); return; }
                DatabaseHelper.InsertRow("Classes", "ClassID", new Dictionary<string, string>
                {
                    { "ClassName", name }, { "Coach", txts[1].Text.Trim() },
                    { "Schedule", txts[2].Text.Trim() }, { "MaxCapacity", cap.ToString() }
                });
                DialogResult = DialogResult.OK; Close();
            };
            Controls.Add(btn);
        }
    }
}
