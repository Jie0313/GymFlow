using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;

namespace GymFlow
{
    public partial class frmLogin : Form
    {
        static readonly Color ClrBg     = Color.FromArgb(18, 18, 35);
        static readonly Color ClrInput  = Color.FromArgb(38, 38, 68);
        static readonly Color ClrText   = Color.FromArgb(240, 240, 255);
        static readonly Color ClrAccent = Color.FromArgb(255, 107, 107);
        static readonly Color ClrSub    = Color.FromArgb(140, 140, 180);

        private const string ConfigFile = "GymFlowData/admins.xml";

        public frmLogin() { InitializeComponent(); BuildUI(); }

        private void BuildUI()
        {
            Text            = "GymFlow 管理員登入";
            ClientSize      = new Size(420, 520);
            BackColor       = ClrBg;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox     = false;
            StartPosition   = FormStartPosition.CenterScreen;
            Font            = new Font("Microsoft JhengHei UI", 10f);

            this.Paint += (s, e) =>
            {
                using (var br = new LinearGradientBrush(ClientRectangle, Color.FromArgb(25, 18, 50), Color.FromArgb(18, 18, 35), 135f))
                    e.Graphics.FillRectangle(br, ClientRectangle);
            };

            var lblLogo = new Label { Text = "💪", Font = new Font("Segoe UI Emoji", 38f), ForeColor = ClrAccent, Size = new Size(420, 80), Location = new Point(0, 40), TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.Transparent };
            var lblTitle = new Label { Text = "GymFlow", Font = new Font("Microsoft JhengHei UI", 22f, FontStyle.Bold), ForeColor = ClrAccent, Size = new Size(420, 45), Location = new Point(0, 120), TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.Transparent };
            var lblSub   = new Label { Text = "健身房管理系統", Font = new Font("Microsoft JhengHei UI", 11f), ForeColor = ClrSub, Size = new Size(420, 30), Location = new Point(0, 163), TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.Transparent };
            var sep      = new Panel { Size = new Size(300, 1), Location = new Point(60, 205), BackColor = Color.FromArgb(60, 60, 90) };

            var lblUser = new Label { Text = "帳號", ForeColor = ClrSub, Size = new Size(300, 22), Location = new Point(60, 222), BackColor = Color.Transparent };
            var txtUser = new TextBox { Size = new Size(300, 34), Location = new Point(60, 245), BackColor = ClrInput, ForeColor = ClrText, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Microsoft JhengHei UI", 11f) };

            var lblPass = new Label { Text = "密碼", ForeColor = ClrSub, Size = new Size(300, 22), Location = new Point(60, 290), BackColor = Color.Transparent };
            var txtPass = new TextBox { Size = new Size(300, 34), Location = new Point(60, 313), BackColor = ClrInput, ForeColor = ClrText, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Microsoft JhengHei UI", 11f), PasswordChar = '●' };

            var lblErr = new Label { Text = "", ForeColor = ClrAccent, Size = new Size(300, 24), Location = new Point(60, 353), BackColor = Color.Transparent, Font = new Font("Microsoft JhengHei UI", 9f) };

            var btnLogin = new Button { Text = "登　入", Size = new Size(300, 44), Location = new Point(60, 380), FlatStyle = FlatStyle.Flat, BackColor = ClrAccent, ForeColor = Color.White, Font = new Font("Microsoft JhengHei UI", 12f, FontStyle.Bold), Cursor = Cursors.Hand };
            btnLogin.FlatAppearance.BorderSize = 0;

            var lblHint = new Label { Text = "", Size = new Size(1, 1), Location = new Point(0, 0), BackColor = Color.Transparent };

            Action doLogin = () =>
            {
                var admins = LoadAdmins();
                string user = txtUser.Text.Trim();
                string pass = txtPass.Text;
                if (admins.ContainsKey(user) && admins[user] == pass)
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    lblErr.Text = "❌ 帳號或密碼錯誤，請再試一次";
                    txtPass.Clear();
                    txtPass.Focus();
                }
            };

            btnLogin.Click  += (s, e) => doLogin();
            txtPass.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) doLogin(); };
            txtUser.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) txtPass.Focus(); };

            Controls.AddRange(new Control[] { lblLogo, lblTitle, lblSub, sep, lblUser, txtUser, lblPass, txtPass, lblErr, btnLogin, lblHint });
            ActiveControl = txtUser;
        }

        // ── 管理員資料存取 ────────────────────────────────────────────────────
        public static Dictionary<string, string> LoadAdmins()
        {
            var dict = new Dictionary<string, string>();
            if (!File.Exists(ConfigFile))
            {
                dict["admin"] = "1234";
                SaveAdmins(dict);
                return dict;
            }
            try
            {
                var root = XElement.Load(ConfigFile);
                foreach (var a in root.Elements("Admin"))
                    dict[a.Element("Username")?.Value ?? ""] = a.Element("Password")?.Value ?? "";
            }
            catch { dict["admin"] = "1234"; }
            return dict;
        }

        public static void SaveAdmins(Dictionary<string, string> admins)
        {
            Directory.CreateDirectory("GymFlowData");
            var root = new XElement("Admins");
            foreach (var kv in admins)
                root.Add(new XElement("Admin", new XElement("Username", kv.Key), new XElement("Password", kv.Value)));
            root.Save(ConfigFile);
        }

        public static string GetPassword(string username)
        {
            var admins = LoadAdmins();
            return admins.ContainsKey(username) ? admins[username] : null;
        }
    }
}
