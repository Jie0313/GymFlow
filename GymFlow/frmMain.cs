using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace GymFlow
{
    public partial class frmMain : Form
    {
        static readonly Color ClrBg       = Color.FromArgb(18,  18,  35);
        static readonly Color ClrSidebar  = Color.FromArgb(25,  25,  50);
        static readonly Color ClrCard1    = Color.FromArgb(255, 107, 107);
        static readonly Color ClrCard2    = Color.FromArgb(78,  205, 196);
        static readonly Color ClrCard3    = Color.FromArgb(255, 209,  92);
        static readonly Color ClrCard4    = Color.FromArgb(162,  89, 255);
        static readonly Color ClrAccent   = Color.FromArgb(255, 107, 107);
        static readonly Color ClrTextMain = Color.FromArgb(240, 240, 255);
        static readonly Color ClrTextSub  = Color.FromArgb(140, 140, 180);
        static readonly Color ClrNavHover = Color.FromArgb(40,  40,  70);

        private Panel        pnlSidebar, pnlContent;
        private Button       _activeNav;
        private DataGridView _checkinGrid;

        public frmMain()
        {
            InitializeComponent();
            BuildUI();
            ShowDashboard();
        }

        private void BuildUI()
        {
            Text            = "GymFlow 健身房管理系統";
            ClientSize      = new Size(1100, 680);
            BackColor       = ClrBg;
            DoubleBuffered  = true;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox     = false;
            StartPosition   = FormStartPosition.CenterScreen;
            Font            = new Font("Microsoft JhengHei UI", 9f);

            pnlSidebar = new Panel { Size = new Size(200, 680), Location = new Point(0, 0), BackColor = ClrSidebar };
            Controls.Add(pnlSidebar);

            pnlSidebar.Controls.Add(new Label
            {
                Text = "💪 GymFlow", Font = new Font("Microsoft JhengHei UI", 15f, FontStyle.Bold),
                ForeColor = ClrAccent, Size = new Size(200, 60), Location = new Point(0, 20),
                TextAlign = ContentAlignment.MiddleCenter
            });
            pnlSidebar.Controls.Add(new Panel { Size = new Size(160, 1), Location = new Point(20, 85), BackColor = Color.FromArgb(60, 60, 90) });

            var btnDash       = MakeNavBtn("🏠  總覽",      100);
            var btnMembers    = MakeNavBtn("👥  會員管理",   150);
            var btnClasses    = MakeNavBtn("📅  課程管理",   200);
            var btnAttendance = MakeNavBtn("📋  出席記錄",   250);
            var btnCheckin    = MakeNavBtn("✅  入場打卡",   300);
            var btnPayments   = MakeNavBtn("💳  繳費管理",   350);
            var btnStats      = MakeNavBtn("📊  統計圖表",   400);
            var btnExport     = MakeNavBtn("📋  匯出 CSV",   450);
            var btnPlans      = MakeNavBtn("💰  方案管理",   500);

            btnDash.Click       += (s, e) => { SetNav(btnDash);       ShowDashboard(); };
            btnMembers.Click    += (s, e) => { SetNav(btnMembers);    ShowMembers(); };
            btnClasses.Click    += (s, e) => { SetNav(btnClasses);    ShowClasses(); };
            btnAttendance.Click += (s, e) => { SetNav(btnAttendance); ShowAttendance(); };
            btnCheckin.Click    += (s, e) => { SetNav(btnCheckin);    ShowCheckin(); };
            btnPayments.Click   += (s, e) => { SetNav(btnPayments);   ShowPayments(); };
            btnStats.Click      += (s, e) => { SetNav(btnStats);      ShowStats(); };
            btnExport.Click     += (s, e) => { SetNav(btnExport);     ShowExport(); };
            btnPlans.Click      += (s, e) => { SetNav(btnPlans);      ShowPlans(); };

            // 更改密碼按鈕（底部）
            var btnChangePass = new Button
            {
                Text      = "🔑  管理員管理",
                Size      = new Size(200, 44),
                Location  = new Point(0, 618),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.FromArgb(140, 140, 180),
                BackColor = Color.FromArgb(25, 25, 50),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(20, 0, 0, 0),
                Font      = new Font("Microsoft JhengHei UI", 10f),
                Cursor    = Cursors.Hand
            };
            btnChangePass.FlatAppearance.BorderSize = 0;
            btnChangePass.FlatAppearance.MouseOverBackColor = Color.FromArgb(40, 40, 70);
            btnChangePass.Click += (s, e) => ShowAdminManager();
            pnlSidebar.Controls.Add(btnChangePass);

            pnlContent = new Panel { Size = new Size(900, 680), Location = new Point(200, 0), BackColor = ClrBg };
            Controls.Add(pnlContent);
            SetNav(btnDash);
        }

        private Button MakeNavBtn(string text, int y)
        {
            var btn = new Button
            {
                Text = text, Size = new Size(200, 44), Location = new Point(0, y),
                FlatStyle = FlatStyle.Flat, ForeColor = ClrTextSub, BackColor = ClrSidebar,
                TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(20, 0, 0, 0),
                Font = new Font("Microsoft JhengHei UI", 10f), Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = ClrNavHover;
            pnlSidebar.Controls.Add(btn);
            return btn;
        }

        private void SetNav(Button btn)
        {
            if (_activeNav != null) { _activeNav.ForeColor = ClrTextSub; _activeNav.BackColor = ClrSidebar; }
            _activeNav = btn;
            btn.ForeColor = ClrTextMain;
            btn.BackColor = ClrNavHover;
        }

        // ══════════════════════════════════════════════════════════════════════
        // 總覽
        // ══════════════════════════════════════════════════════════════════════
        private void ShowDashboard()
        {
            pnlContent.Controls.Clear();
            pnlContent.Controls.Add(MakeHeader("🏠 今日總覽"));

            var members  = DatabaseHelper.LoadTable("Members");
            var payments = DatabaseHelper.LoadTable("Payments");
            var checkins = DatabaseHelper.LoadTable("CheckIns");

            string today = DateTime.Today.ToString("yyyy-MM-dd");
            string week  = DateTime.Today.AddDays(7).ToString("yyyy-MM-dd");

            int totalMembers    = members.Rows.Count;
            int activeMembers   = payments.AsEnumerable().Where(r => string.Compare(r["ExpireDate"].ToString(), today) >= 0)
                                          .Select(r => r["MemberID"].ToString()).Distinct().Count();
            int todayCheckins   = checkins.AsEnumerable().Count(r => r["CheckInTime"].ToString().StartsWith(today));
            int expiringMembers = payments.AsEnumerable()
                                          .Where(r => string.Compare(r["ExpireDate"].ToString(), today) >= 0 &&
                                                      string.Compare(r["ExpireDate"].ToString(), week)  <= 0)
                                          .Select(r => r["MemberID"].ToString()).Distinct().Count();
            int inGymNow        = checkins.AsEnumerable().Count(r =>
                                      r["CheckInTime"].ToString().StartsWith(today) &&
                                      string.IsNullOrEmpty(SafeGet(r, "CheckOutTime")));

            // 5張卡，每張寬160，間距10，起點20
            int cx = 20;
            pnlContent.Controls.Add(MakeStat("總會員數", totalMembers.ToString(),    ClrCard1, cx)); cx += 170;
            pnlContent.Controls.Add(MakeStat("有效會員", activeMembers.ToString(),   ClrCard2, cx)); cx += 170;
            pnlContent.Controls.Add(MakeStat("今日打卡", todayCheckins.ToString(),   ClrCard3, cx)); cx += 170;
            pnlContent.Controls.Add(MakeStat("目前在館", inGymNow.ToString(),        Color.FromArgb(0, 200, 120), cx)); cx += 170;
            pnlContent.Controls.Add(MakeStat("即將到期", expiringMembers.ToString(), ClrCard4, cx));

            pnlContent.Controls.Add(MakeSectionLabel("⚠️  7 天內即將到期的會員", ClrCard3, 230));

            // 到期提醒表
            var dtExpiring = new DataTable();
            dtExpiring.Columns.Add("會員ID"); dtExpiring.Columns.Add("姓名");
            dtExpiring.Columns.Add("電話");   dtExpiring.Columns.Add("到期日"); dtExpiring.Columns.Add("剩餘天數");

            foreach (DataRow p in payments.Rows)
            {
                string exp = p["ExpireDate"].ToString();
                if (string.Compare(exp, today) >= 0 && string.Compare(exp, week) <= 0)
                {
                    var m = members.AsEnumerable().FirstOrDefault(r => r["MemberID"].ToString() == p["MemberID"].ToString());
                    if (m != null)
                    {
                        int days = (DateTime.Parse(exp) - DateTime.Today).Days;
                        dtExpiring.Rows.Add(m["MemberID"], m["Name"], m["Phone"], exp, days);
                    }
                }
            }
            var grid1 = MakeGrid(); grid1.Location = new Point(30, 265); grid1.Size = new Size(840, 180); grid1.DataSource = dtExpiring;
            pnlContent.Controls.Add(grid1);

            pnlContent.Controls.Add(MakeSectionLabel("📋  今日打卡記錄", ClrCard2, 460));

            var dtCheckin = new DataTable();
            dtCheckin.Columns.Add("姓名"); dtCheckin.Columns.Add("打卡時間");
            foreach (DataRow c in checkins.Rows)
            {
                if (!c["CheckInTime"].ToString().StartsWith(today)) continue;
                var m = members.AsEnumerable().FirstOrDefault(r => r["MemberID"].ToString() == c["MemberID"].ToString());
                if (m != null) dtCheckin.Rows.Add(m["Name"], c["CheckInTime"]);
            }
            var grid2 = MakeGrid(); grid2.Location = new Point(30, 495); grid2.Size = new Size(840, 160);
            grid2.DataSource = dtCheckin;
            pnlContent.Controls.Add(grid2);
        }

        private Panel MakeStat(string label, string value, Color color, int x)
        {
            var p = new Panel { Size = new Size(160, 130), Location = new Point(x, 75), BackColor = Color.FromArgb(30, 30, 55) };
            p.Paint += (s, e) =>
            {
                var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var br = new LinearGradientBrush(p.ClientRectangle, Color.FromArgb(40, color.R, color.G, color.B), Color.FromArgb(10, color.R, color.G, color.B), 135f))
                    g.FillRectangle(br, p.ClientRectangle);
                using (var pen = new Pen(Color.FromArgb(80, color.R, color.G, color.B), 1.5f))
                    g.DrawRectangle(pen, 0, 0, p.Width - 1, p.Height - 1);
                using (var br2 = new SolidBrush(color))
                    g.FillRectangle(br2, 0, 0, p.Width, 5);
            };
            p.Controls.Add(new Label { Text = value, Font = new Font("Microsoft JhengHei UI", 26f, FontStyle.Bold), ForeColor = color, Size = new Size(160, 55), Location = new Point(0, 28), TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.Transparent });
            p.Controls.Add(new Label { Text = label, Font = new Font("Microsoft JhengHei UI", 9f), ForeColor = ClrTextSub, Size = new Size(160, 28), Location = new Point(0, 90), TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.Transparent });
            return p;
        }

        // ══════════════════════════════════════════════════════════════════════
        // 會員管理
        // ══════════════════════════════════════════════════════════════════════
        private void ShowMembers()
        {
            pnlContent.Controls.Clear();
            pnlContent.Controls.Add(MakeHeader("👥 會員管理"));

            var txtSearch = new TextBox { Size = new Size(260, 30), Location = new Point(30, 75), BackColor = Color.FromArgb(38, 38, 68), ForeColor = ClrTextSub, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Microsoft JhengHei UI", 10f), Text = "搜尋姓名 / 電話…" };
            txtSearch.GotFocus  += (s, e) => { if (txtSearch.Text == "搜尋姓名 / 電話…") { txtSearch.Text = ""; txtSearch.ForeColor = ClrTextMain; } };
            txtSearch.LostFocus += (s, e) => { if (txtSearch.Text == "") { txtSearch.Text = "搜尋姓名 / 電話…"; txtSearch.ForeColor = ClrTextSub; } };

            var btnSearch = MakeBtn("🔍 搜尋",      ClrCard2,                       new Point(310, 70));
            var btnAdd    = MakeBtn("➕ 新增",      ClrCard1,                       new Point(455, 70));
            var btnEdit   = MakeBtn("✏️ 編輯",      Color.FromArgb(162, 89, 255),   new Point(600, 70));
            var btnDel    = MakeBtn("🗑 刪除",      Color.FromArgb(180, 60, 60),    new Point(745, 70));
            var btnDetail = MakeBtn("🔍 詳細資料",  Color.FromArgb(78, 205, 196),   new Point(600, 115));

            var grid = MakeGrid(); grid.Location = new Point(30, 165); grid.Size = new Size(840, 475);

            Action load = () =>
            {
                string kw      = (txtSearch.Text == "搜尋姓名 / 電話…") ? "" : txtSearch.Text.Trim().ToLower();
                var members    = DatabaseHelper.LoadTable("Members");
                var payments   = DatabaseHelper.LoadTable("Payments");
                var dt         = new DataTable();
                dt.Columns.Add("ID"); dt.Columns.Add("姓名"); dt.Columns.Add("電話");
                dt.Columns.Add("Email"); dt.Columns.Add("加入日期"); dt.Columns.Add("到期日");

                foreach (DataRow m in members.Rows)
                {
                    if (!string.IsNullOrEmpty(kw) && !m["Name"].ToString().ToLower().Contains(kw) && !m["Phone"].ToString().Contains(kw)) continue;
                    string mid    = m["MemberID"].ToString();
                    string expire = payments.AsEnumerable().Where(p => p["MemberID"].ToString() == mid)
                                            .OrderByDescending(p => p["ExpireDate"].ToString())
                                            .Select(p => p["ExpireDate"].ToString()).FirstOrDefault() ?? "—";
                    dt.Rows.Add(mid, m["Name"], m["Phone"], m["Email"], m["JoinDate"], expire);
                }
                grid.DataSource = dt;
            };
            load();

            btnSearch.Click   += (s, e) => load();
            txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) load(); };
            btnAdd.Click      += (s, e) => { if (new frmMember().ShowDialog() == DialogResult.OK) load(); };
            btnEdit.Click     += (s, e) =>
            {
                if (grid.SelectedRows.Count == 0) { MessageBox.Show("請先選取一位會員！"); return; }
                string id = grid.SelectedRows[0].Cells["ID"].Value?.ToString();
                if (new frmEditMember(id).ShowDialog() == DialogResult.OK) load();
            };
            btnDetail.Click   += (s, e) =>
            {
                if (grid.SelectedRows.Count == 0) { MessageBox.Show("請先選取一位會員！"); return; }
                string id = grid.SelectedRows[0].Cells["ID"].Value?.ToString();
                new frmMemberDetail(id).ShowDialog();
            };
            btnDel.Click      += (s, e) =>
            {
                if (grid.SelectedRows.Count == 0) { MessageBox.Show("請先選取一位會員！"); return; }
                string id = grid.SelectedRows[0].Cells["ID"].Value?.ToString();
                if (MessageBox.Show("確定刪除此會員？", "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    DatabaseHelper.DeleteRow("CheckIns",   "MemberID", id);
                    DatabaseHelper.DeleteRow("Enrollments","MemberID", id);
                    DatabaseHelper.DeleteRow("Payments",   "MemberID", id);
                    DatabaseHelper.DeleteRow("Members",    "MemberID", id);
                    load();
                }
            };

            pnlContent.Controls.Add(txtSearch); pnlContent.Controls.Add(btnSearch);
            pnlContent.Controls.Add(btnAdd);    pnlContent.Controls.Add(btnEdit);
            pnlContent.Controls.Add(btnDetail); pnlContent.Controls.Add(btnDel);
            pnlContent.Controls.Add(grid);
        }

        // ══════════════════════════════════════════════════════════════════════
        // 課程管理
        // ══════════════════════════════════════════════════════════════════════
        private void ShowClasses()
        {
            pnlContent.Controls.Clear();
            pnlContent.Controls.Add(MakeHeader("📅 課程管理"));

            var btnAdd    = MakeBtn("➕ 新增課程", ClrCard1, new Point(30, 70));
            var btnDel    = MakeBtn("🗑 刪除課程", Color.FromArgb(180, 60, 60), new Point(170, 70));
            var btnEnroll = MakeBtn("📝 報名課程", ClrCard3, new Point(310, 70));
            var btnUnenroll = MakeBtn("❌ 取消報名", Color.FromArgb(120, 80, 180), new Point(450, 70));

            var grid = MakeGrid(); grid.Location = new Point(30, 120); grid.Size = new Size(840, 210);

            Action load = () =>
            {
                var classes     = DatabaseHelper.LoadTable("Classes");
                var enrollments = DatabaseHelper.LoadTable("Enrollments");
                var dt = new DataTable();
                dt.Columns.Add("ID"); dt.Columns.Add("課程名稱"); dt.Columns.Add("教練");
                dt.Columns.Add("時間"); dt.Columns.Add("人數上限"); dt.Columns.Add("已報名");
                foreach (DataRow c in classes.Rows)
                {
                    string cid   = c["ClassID"].ToString();
                    int enrolled = enrollments.AsEnumerable().Count(e => e["ClassID"].ToString() == cid);
                    dt.Rows.Add(cid, c["ClassName"], c["Coach"], c["Schedule"], c["MaxCapacity"], enrolled);
                }
                grid.DataSource = dt;
            };
            load();

            // 報名名單 grid（需要先宣告讓 SelectionChanged 可以用）
            var gridEnroll = MakeGrid(); gridEnroll.Location = new Point(30, 390); gridEnroll.Size = new Size(840, 260);

            Action loadEnroll = () =>
            {
                if (grid.SelectedRows.Count == 0) return;
                string classId  = grid.SelectedRows[0].Cells["ID"].Value?.ToString();
                var members     = DatabaseHelper.LoadTable("Members");
                var enrollments = DatabaseHelper.LoadTable("Enrollments");
                var dt = new DataTable();
                dt.Columns.Add("報名ID"); dt.Columns.Add("姓名"); dt.Columns.Add("電話"); dt.Columns.Add("報名日期");
                foreach (DataRow en in enrollments.Rows)
                {
                    if (en["ClassID"].ToString() != classId) continue;
                    var m = members.AsEnumerable().FirstOrDefault(r => r["MemberID"].ToString() == en["MemberID"].ToString());
                    if (m != null) dt.Rows.Add(en["EnrollID"], m["Name"], m["Phone"], en["EnrollDate"]);
                }
                gridEnroll.DataSource = dt;
            };

            btnAdd.Click += (s, e) => { if (new frmClass().ShowDialog() == DialogResult.OK) load(); };

            btnDel.Click += (s, e) =>
            {
                if (grid.SelectedRows.Count == 0) { MessageBox.Show("請先選取一個課程！"); return; }
                string id = grid.SelectedRows[0].Cells["ID"].Value?.ToString();
                if (MessageBox.Show("確定刪除此課程？", "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    DatabaseHelper.DeleteRow("Enrollments", "ClassID", id);
                    DatabaseHelper.DeleteRow("Classes",     "ClassID", id);
                    load();
                }
            };

            btnEnroll.Click += (s, e) =>
            {
                if (grid.SelectedRows.Count == 0) { MessageBox.Show("請先選取一個課程！"); return; }
                string classId   = grid.SelectedRows[0].Cells["ID"].Value?.ToString();
                string className = grid.SelectedRows[0].Cells["課程名稱"].Value?.ToString();
                int maxCap       = Convert.ToInt32(grid.SelectedRows[0].Cells["人數上限"].Value);
                int enrolled     = Convert.ToInt32(grid.SelectedRows[0].Cells["已報名"].Value);

                if (enrolled >= maxCap) { MessageBox.Show("此課程已額滿！"); return; }

                // 選擇會員的對話框
                var members     = DatabaseHelper.LoadTable("Members");
                var enrollments = DatabaseHelper.LoadTable("Enrollments");

                var dlg = new Form
                {
                    Text = "報名課程 - " + className, Size = new Size(400, 480),
                    BackColor = Color.FromArgb(18, 18, 35), FormBorderStyle = FormBorderStyle.FixedDialog,
                    MaximizeBox = false, StartPosition = FormStartPosition.CenterParent
                };

                dlg.Controls.Add(new Label { Text = "搜尋姓名：", ForeColor = Color.FromArgb(240, 240, 255), Size = new Size(320, 24), Location = new Point(20, 20), BackColor = Color.Transparent, Font = new Font("Microsoft JhengHei UI", 10f) });

                var txtSearch = new TextBox
                {
                    Size = new Size(330, 32), Location = new Point(20, 48),
                    BackColor = Color.FromArgb(38, 38, 68), ForeColor = Color.FromArgb(240, 240, 255),
                    BorderStyle = BorderStyle.FixedSingle, Font = new Font("Microsoft JhengHei UI", 11f)
                };

                dlg.Controls.Add(new Label { Text = "選擇會員：", ForeColor = Color.FromArgb(140, 140, 180), Size = new Size(320, 24), Location = new Point(20, 90), BackColor = Color.Transparent, Font = new Font("Microsoft JhengHei UI", 10f) });

                var lstMembers = new ListBox
                {
                    Size = new Size(350, 270), Location = new Point(20, 115),
                    BackColor = Color.FromArgb(38, 38, 68), ForeColor = Color.FromArgb(240, 240, 255),
                    Font = new Font("Microsoft JhengHei UI", 11f), BorderStyle = BorderStyle.FixedSingle
                };

                // 取得未報名會員清單
                var availableMembers = members.AsEnumerable().Where(m =>
                    !enrollments.AsEnumerable().Any(en =>
                        en["MemberID"].ToString() == m["MemberID"].ToString() &&
                        en["ClassID"].ToString()  == classId)).ToList();

                if (availableMembers.Count == 0) { MessageBox.Show("所有會員都已報名此課程！"); return; }

                Action refreshList = () =>
                {
                    string kw = txtSearch.Text.Trim();
                    lstMembers.Items.Clear();
                    foreach (var m in availableMembers)
                        if (string.IsNullOrEmpty(kw) || m["Name"].ToString().Contains(kw))
                            lstMembers.Items.Add(m["MemberID"] + " - " + m["Name"]);
                };
                refreshList();

                txtSearch.TextChanged += (ts, te) => refreshList();

                var btnConfirm = new Button
                {
                    Text = "✅ 確認報名", Size = new Size(160, 40), Location = new Point(100, 405),
                    FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(255, 209, 92),
                    ForeColor = Color.FromArgb(18, 18, 35), Font = new Font("Microsoft JhengHei UI", 10f, FontStyle.Bold), Cursor = Cursors.Hand
                };
                btnConfirm.FlatAppearance.BorderSize = 0;
                btnConfirm.Click += (bs, be) =>
                {
                    if (lstMembers.SelectedItem == null) { MessageBox.Show("請選擇會員！"); return; }
                    string memberId = lstMembers.SelectedItem.ToString().Split('-')[0].Trim();

                    // 驗證會籍是否有效
                    string today      = DateTime.Today.ToString("yyyy-MM-dd");
                    var paymentCheck  = DatabaseHelper.LoadTable("Payments");
                    string expire     = paymentCheck.AsEnumerable()
                        .Where(p => p["MemberID"].ToString() == memberId)
                        .OrderByDescending(p => p["ExpireDate"].ToString())
                        .Select(p => p["ExpireDate"].ToString()).FirstOrDefault();
                    if (string.IsNullOrEmpty(expire) || string.Compare(expire, today) < 0)
                    {
                        string mName = lstMembers.SelectedItem.ToString().Split('-')[1].Trim();
                        MessageBox.Show(mName + " 的會籍已過期或尚未繳費！\n請先完成繳費後再報名課程。", "無法報名", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    DatabaseHelper.InsertRow("Enrollments", "EnrollID", new System.Collections.Generic.Dictionary<string, string>
                    {
                        { "MemberID",   memberId },
                        { "ClassID",    classId },
                        { "EnrollDate", DateTime.Today.ToString("yyyy-MM-dd") }
                    });
                    MessageBox.Show("報名成功！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    dlg.DialogResult = DialogResult.OK;
                    dlg.Close();
                };

                dlg.Controls.Add(txtSearch);
                dlg.Controls.Add(lstMembers);
                dlg.Controls.Add(btnConfirm);
                if (dlg.ShowDialog() == DialogResult.OK) { load(); loadEnroll(); }
            };

            btnUnenroll.Click += (s, e) =>
            {
                if (grid.SelectedRows.Count == 0) { MessageBox.Show("請先選取一個課程！"); return; }
                string classId2 = grid.SelectedRows[0].Cells["ID"].Value?.ToString();
                string className2 = grid.SelectedRows[0].Cells["課程名稱"].Value?.ToString();

                var enrollments2 = DatabaseHelper.LoadTable("Enrollments");
                var members2     = DatabaseHelper.LoadTable("Members");

                var dlg2 = new Form
                {
                    Text = "取消報名 - " + className2, Size = new Size(400, 480),
                    BackColor = Color.FromArgb(18, 18, 35), FormBorderStyle = FormBorderStyle.FixedDialog,
                    MaximizeBox = false, StartPosition = FormStartPosition.CenterParent
                };

                dlg2.Controls.Add(new Label { Text = "搜尋姓名：", ForeColor = Color.FromArgb(240, 240, 255), Size = new Size(320, 24), Location = new Point(20, 20), BackColor = Color.Transparent, Font = new Font("Microsoft JhengHei UI", 10f) });

                var txtSearch2 = new TextBox
                {
                    Size = new Size(330, 32), Location = new Point(20, 48),
                    BackColor = Color.FromArgb(38, 38, 68), ForeColor = Color.FromArgb(240, 240, 255),
                    BorderStyle = BorderStyle.FixedSingle, Font = new Font("Microsoft JhengHei UI", 11f)
                };

                dlg2.Controls.Add(new Label { Text = "選擇要取消報名的會員：", ForeColor = Color.FromArgb(140, 140, 180), Size = new Size(320, 24), Location = new Point(20, 90), BackColor = Color.Transparent, Font = new Font("Microsoft JhengHei UI", 10f) });

                var lstEnrolled = new ListBox
                {
                    Size = new Size(350, 270), Location = new Point(20, 115),
                    BackColor = Color.FromArgb(38, 38, 68), ForeColor = Color.FromArgb(240, 240, 255),
                    Font = new Font("Microsoft JhengHei UI", 11f), BorderStyle = BorderStyle.FixedSingle
                };

                // 取得已報名的會員
                var enrolledList = enrollments2.AsEnumerable()
                    .Where(en => en["ClassID"].ToString() == classId2)
                    .Select(en => new {
                        EnrollID = en["EnrollID"].ToString(),
                        Member   = members2.AsEnumerable().FirstOrDefault(m => m["MemberID"].ToString() == en["MemberID"].ToString())
                    })
                    .Where(x => x.Member != null).ToList();

                if (enrolledList.Count == 0) { MessageBox.Show("此課程目前沒有報名的會員！"); return; }

                Action refreshList2 = () =>
                {
                    string kw = txtSearch2.Text.Trim();
                    lstEnrolled.Items.Clear();
                    foreach (var en in enrolledList)
                        if (string.IsNullOrEmpty(kw) || en.Member["Name"].ToString().Contains(kw))
                            lstEnrolled.Items.Add(en.EnrollID + " - " + en.Member["Name"]);
                };
                refreshList2();
                txtSearch2.TextChanged += (ts, te) => refreshList2();

                var btnConfirm2 = new Button
                {
                    Text = "❌ 確認取消報名", Size = new Size(180, 40), Location = new Point(85, 405),
                    FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(180, 60, 60),
                    ForeColor = Color.White, Font = new Font("Microsoft JhengHei UI", 10f, FontStyle.Bold), Cursor = Cursors.Hand
                };
                btnConfirm2.FlatAppearance.BorderSize = 0;
                btnConfirm2.Click += (bs, be) =>
                {
                    if (lstEnrolled.SelectedItem == null) { MessageBox.Show("請選擇會員！"); return; }
                    string enrollId = lstEnrolled.SelectedItem.ToString().Split('-')[0].Trim();
                    string name     = lstEnrolled.SelectedItem.ToString().Split('-')[1].Trim();
                    if (MessageBox.Show("確定取消 " + name + " 的報名？", "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                    {
                        DatabaseHelper.DeleteRow("Enrollments", "EnrollID", enrollId);
                        dlg2.DialogResult = DialogResult.OK;
                        dlg2.Close();
                    }
                };

                dlg2.Controls.Add(txtSearch2);
                dlg2.Controls.Add(lstEnrolled);
                dlg2.Controls.Add(btnConfirm2);
                if (dlg2.ShowDialog() == DialogResult.OK) { load(); loadEnroll(); }
            };

            grid.SelectionChanged += (s, e) => loadEnroll();

            pnlContent.Controls.Add(MakeSectionLabel("📋 選取課程的報名名單", ClrCard2, 355));
            pnlContent.Controls.Add(btnAdd); pnlContent.Controls.Add(btnDel);
            pnlContent.Controls.Add(btnEnroll); pnlContent.Controls.Add(btnUnenroll);
            pnlContent.Controls.Add(grid); pnlContent.Controls.Add(gridEnroll);
        }

        // ══════════════════════════════════════════════════════════════════════
        // 入場 / 退場打卡
        // ══════════════════════════════════════════════════════════════════════
        private void ShowCheckin()
        {
            pnlContent.Controls.Clear();
            pnlContent.Controls.Add(MakeHeader("✅ 入場 / 退場打卡"));

            // ── 入場卡片 ──
            var pnlIn = new Panel { Size = new Size(400, 210), Location = new Point(30, 80), BackColor = Color.FromArgb(28, 28, 52) };
            pnlIn.Paint += (s, e) => { using (var pen = new Pen(Color.FromArgb(78, 205, 196), 1.5f)) e.Graphics.DrawRectangle(pen, 0, 0, pnlIn.Width - 1, pnlIn.Height - 1); };
            pnlIn.Controls.Add(new Label { Text = "🚪 入場打卡", Font = new Font("Microsoft JhengHei UI", 12f, FontStyle.Bold), ForeColor = ClrCard2, Size = new Size(360, 35), Location = new Point(20, 15), BackColor = Color.Transparent });

            var txtIn     = new TextBox { Size = new Size(190, 32), Location = new Point(20, 60), BackColor = Color.FromArgb(38, 38, 68), ForeColor = ClrTextMain, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Microsoft JhengHei UI", 12f) };
            var btnIn     = MakeBtn("✅ 入場", ClrCard2, new Point(220, 58)); btnIn.Size = new Size(90, 34);
            var lblIn     = new Label { Text = "", Font = new Font("Microsoft JhengHei UI", 10f, FontStyle.Bold), ForeColor = ClrCard2, Size = new Size(360, 80), Location = new Point(20, 105), BackColor = Color.Transparent };
            pnlIn.Controls.Add(txtIn); pnlIn.Controls.Add(btnIn); pnlIn.Controls.Add(lblIn);

            // ── 退場卡片 ──
            var pnlOut = new Panel { Size = new Size(400, 210), Location = new Point(450, 80), BackColor = Color.FromArgb(28, 28, 52) };
            pnlOut.Paint += (s, e) => { using (var pen = new Pen(Color.FromArgb(255, 107, 107), 1.5f)) e.Graphics.DrawRectangle(pen, 0, 0, pnlOut.Width - 1, pnlOut.Height - 1); };
            pnlOut.Controls.Add(new Label { Text = "🚶 退場打卡", Font = new Font("Microsoft JhengHei UI", 12f, FontStyle.Bold), ForeColor = ClrCard1, Size = new Size(360, 35), Location = new Point(20, 15), BackColor = Color.Transparent });

            var txtOut    = new TextBox { Size = new Size(190, 32), Location = new Point(20, 60), BackColor = Color.FromArgb(38, 38, 68), ForeColor = ClrTextMain, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Microsoft JhengHei UI", 12f) };
            var btnOut    = MakeBtn("🚶 退場", ClrCard1, new Point(220, 58)); btnOut.Size = new Size(90, 34);
            var lblOut    = new Label { Text = "", Font = new Font("Microsoft JhengHei UI", 10f, FontStyle.Bold), ForeColor = ClrCard1, Size = new Size(360, 80), Location = new Point(20, 105), BackColor = Color.Transparent };
            pnlOut.Controls.Add(txtOut); pnlOut.Controls.Add(btnOut); pnlOut.Controls.Add(lblOut);

            // ── 找會員共用方法 ──
            Func<string, DataTable, (DataRow row, string error)> findMember = (input, members) =>
            {
                if (string.IsNullOrEmpty(input)) return (null, "❌ 請輸入姓名");
                var exact = members.AsEnumerable().FirstOrDefault(r => r["Name"].ToString() == input);
                if (exact != null) return (exact, null);
                var matches = members.AsEnumerable().Where(r => r["Name"].ToString().Contains(input)).ToList();
                if (matches.Count > 1) return (null, "找到多位會員：" + string.Join("、", matches.Select(r => r["Name"].ToString())) + "\n請輸入完整姓名");
                if (matches.Count == 1) return (matches[0], null);
                return (null, "❌ 找不到此會員");
            };

            // ── 入場邏輯 ──
            Action doCheckin = () =>
            {
                var members  = DatabaseHelper.LoadTable("Members");
                var payments = DatabaseHelper.LoadTable("Payments");
                var (member, err) = findMember(txtIn.Text.Trim(), members);
                if (member == null) { lblIn.Text = err; lblIn.ForeColor = ClrCard1; return; }
                string idStr  = member["MemberID"].ToString();
                string today  = DateTime.Today.ToString("yyyy-MM-dd");
                string expire = payments.AsEnumerable().Where(p => p["MemberID"].ToString() == idStr)
                                        .OrderByDescending(p => p["ExpireDate"].ToString())
                                        .Select(p => p["ExpireDate"].ToString()).FirstOrDefault();
                if (string.IsNullOrEmpty(expire) || string.Compare(expire, today) < 0)
                { lblIn.Text = "⚠️  " + member["Name"] + " 的會籍已過期！\n請先續費後再入場。"; lblIn.ForeColor = ClrCard3; return; }

                // 檢查是否已在館內（今日有入場但沒退場）
                var checkins = DatabaseHelper.LoadTable("CheckIns");
                bool alreadyIn = checkins.AsEnumerable().Any(r =>
                    r["MemberID"].ToString() == idStr &&
                    r["CheckInTime"].ToString().StartsWith(today) &&
                    string.IsNullOrEmpty(SafeGet(r, "CheckOutTime")));
                if (alreadyIn)
                {
                    lblIn.Text = "⚠️  " + member["Name"] + " 已在館內！\n請先退場再重新入場。";
                    lblIn.ForeColor = ClrCard3;
                    return;
                }

                DatabaseHelper.InsertRow("CheckIns", "CheckInID", new System.Collections.Generic.Dictionary<string, string>
                {
                    { "MemberID", idStr }, { "CheckInTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }, { "CheckOutTime", "" }
                });
                lblIn.Text = "✅ " + member["Name"] + " 入場成功！\n會籍到期：" + expire;
                lblIn.ForeColor = ClrCard2;
                txtIn.Clear();
                LoadCheckinHistory();
            };

            // ── 退場邏輯 ──
            Action doCheckout = () =>
            {
                var members  = DatabaseHelper.LoadTable("Members");
                var (member, err) = findMember(txtOut.Text.Trim(), members);
                if (member == null) { lblOut.Text = err; lblOut.ForeColor = ClrCard1; return; }
                string idStr = member["MemberID"].ToString();
                string today = DateTime.Today.ToString("yyyy-MM-dd");

                var checkins = DatabaseHelper.LoadTable("CheckIns");
                var record   = checkins.AsEnumerable().FirstOrDefault(r =>
                    r["MemberID"].ToString() == idStr &&
                    r["CheckInTime"].ToString().StartsWith(today) &&
                    string.IsNullOrEmpty(SafeGet(r, "CheckOutTime")));

                if (record == null)
                {
                    // 檢查是否已退場過
                    bool alreadyOut = checkins.AsEnumerable().Any(r =>
                        r["MemberID"].ToString() == idStr &&
                        r["CheckInTime"].ToString().StartsWith(today) &&
                        !string.IsNullOrEmpty(SafeGet(r, "CheckOutTime")));
                    lblOut.Text = alreadyOut
                        ? "⚠️  " + member["Name"] + " 今日已退場！"
                        : "⚠️  " + member["Name"] + " 今日尚未入場！";
                    lblOut.ForeColor = ClrCard3;
                    return;
                }

                string checkoutTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                record["CheckOutTime"] = checkoutTime;
                DatabaseHelper.SaveTable("CheckIns", checkins);

                TimeSpan duration = DateTime.Now - DateTime.Parse(record["CheckInTime"].ToString());
                lblOut.Text = "🚶 " + member["Name"] + " 退場成功！\n在館時間：" + (int)duration.TotalHours + " 小時 " + duration.Minutes + " 分鐘";
                lblOut.ForeColor = ClrCard1;
                txtOut.Clear();
                LoadCheckinHistory();
            };

            btnIn.Click    += (s, e) => doCheckin();
            txtIn.KeyDown  += (s, e) => { if (e.KeyCode == Keys.Enter) doCheckin(); };
            btnOut.Click   += (s, e) => doCheckout();
            txtOut.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) doCheckout(); };

            pnlContent.Controls.Add(pnlIn);
            pnlContent.Controls.Add(pnlOut);
            pnlContent.Controls.Add(MakeSectionLabel("📋 今日打卡記錄", ClrCard2, 305));

            _checkinGrid = MakeGrid(); _checkinGrid.Location = new Point(30, 340); _checkinGrid.Size = new Size(840, 310);
            pnlContent.Controls.Add(_checkinGrid);
            LoadCheckinHistory();
        }

        private void LoadCheckinHistory()
        {
            if (_checkinGrid == null) return;
            string today = DateTime.Today.ToString("yyyy-MM-dd");
            var members  = DatabaseHelper.LoadTable("Members");
            var checkins = DatabaseHelper.LoadTable("CheckIns");
            var dt = new DataTable();
            dt.Columns.Add("姓名"); dt.Columns.Add("入場時間"); dt.Columns.Add("退場時間"); dt.Columns.Add("狀態");
            foreach (DataRow c in checkins.AsEnumerable().Where(r => r["CheckInTime"].ToString().StartsWith(today)).OrderByDescending(r => r["CheckInTime"].ToString()).ToList())
            {
                var m      = members.AsEnumerable().FirstOrDefault(r => r["MemberID"].ToString() == c["MemberID"].ToString());
                string out_time = SafeGet(c, "CheckOutTime");
                string status   = string.IsNullOrEmpty(out_time) ? "🟢 在館中" : "⚪ 已離館";
                if (m != null) dt.Rows.Add(m["Name"], c["CheckInTime"], string.IsNullOrEmpty(out_time) ? "—" : out_time, status);
            }
            _checkinGrid.DataSource = dt;
        }

        // ══════════════════════════════════════════════════════════════════════
        // 課程出席記錄
        // ══════════════════════════════════════════════════════════════════════
        private void ShowAttendance()
        {
            pnlContent.Controls.Clear();
            pnlContent.Controls.Add(MakeHeader("📋 課程出席記錄"));

            // 選擇課程
            pnlContent.Controls.Add(new Label { Text = "選擇課程：", ForeColor = Color.FromArgb(140, 140, 180), Size = new Size(100, 30), Location = new Point(30, 75), BackColor = Color.Transparent, Font = new Font("Microsoft JhengHei UI", 10f) });

            var cmbClass = new ComboBox
            {
                Size = new Size(250, 32), Location = new Point(135, 73),
                BackColor = Color.FromArgb(38, 38, 68), ForeColor = Color.FromArgb(240, 240, 255),
                DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Microsoft JhengHei UI", 10f)
            };

            // 選擇日期
            pnlContent.Controls.Add(new Label { Text = "上課日期：", ForeColor = Color.FromArgb(140, 140, 180), Size = new Size(100, 30), Location = new Point(400, 75), BackColor = Color.Transparent, Font = new Font("Microsoft JhengHei UI", 10f) });

            var dtpDate = new DateTimePicker
            {
                Size = new Size(180, 32), Location = new Point(505, 73),
                Format = DateTimePickerFormat.Short, Font = new Font("Microsoft JhengHei UI", 10f),
                Value = DateTime.Today
            };
            dtpDate.CalendarForeColor  = Color.FromArgb(240, 240, 255);
            dtpDate.CalendarMonthBackground = Color.FromArgb(38, 38, 68);

            var btnRecord = MakeBtn("📝 記錄出席", Color.FromArgb(78, 205, 196), new Point(700, 70));

            // 載入課程清單
            var classes = DatabaseHelper.LoadTable("Classes");
            cmbClass.Items.Add("-- 請選擇課程 --");
            foreach (DataRow c in classes.Rows)
                cmbClass.Items.Add(c["ClassID"] + " | " + c["ClassName"] + " (" + c["Schedule"] + ")");
            cmbClass.SelectedIndex = 0;

            // 出席記錄表
            pnlContent.Controls.Add(MakeSectionLabel("📋 出席名單", Color.FromArgb(255, 209, 92), 120));

            var grid = MakeGrid();
            grid.Location = new Point(30, 155);
            grid.Size     = new Size(840, 480);
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            Action loadAttendance = () =>
            {
                if (cmbClass.SelectedIndex <= 0) { grid.DataSource = null; return; }
                string classId   = cmbClass.SelectedItem.ToString().Split('|')[0].Trim();
                string dateStr   = dtpDate.Value.ToString("yyyy-MM-dd");
                var attendance   = DatabaseHelper.LoadTable("Attendance");
                var members      = DatabaseHelper.LoadTable("Members");
                var enrollments  = DatabaseHelper.LoadTable("Enrollments");

                var dt = new DataTable();
                dt.Columns.Add("出席ID"); dt.Columns.Add("姓名"); dt.Columns.Add("電話"); dt.Columns.Add("出席時間"); dt.Columns.Add("備註");

                foreach (DataRow a in attendance.AsEnumerable()
                    .Where(r => r["ClassID"].ToString() == classId && r["Date"].ToString() == dateStr)
                    .OrderBy(r => r["AttendTime"].ToString()).ToList())
                {
                    var m = members.AsEnumerable().FirstOrDefault(r => r["MemberID"].ToString() == a["MemberID"].ToString());
                    if (m != null) dt.Rows.Add(a["AttendID"], m["Name"], m["Phone"], a["AttendTime"], a["Note"]);
                }
                grid.DataSource = dt;
            };

            btnRecord.Click += (s, e) =>
            {
                if (cmbClass.SelectedIndex <= 0) { MessageBox.Show("請先選擇課程！"); return; }
                string classId   = cmbClass.SelectedItem.ToString().Split('|')[0].Trim();
                string className = cmbClass.SelectedItem.ToString().Split('|')[1].Trim();
                string dateStr   = dtpDate.Value.ToString("yyyy-MM-dd");

                // 取得已報名此課程的會員
                var enrollments = DatabaseHelper.LoadTable("Enrollments");
                var members     = DatabaseHelper.LoadTable("Members");
                var attendance  = DatabaseHelper.LoadTable("Attendance");

                var enrolled = enrollments.AsEnumerable()
                    .Where(r => r["ClassID"].ToString() == classId).ToList();

                if (enrolled.Count == 0) { MessageBox.Show("此課程目前沒有報名的會員！"); return; }

                // 出席記錄視窗
                var dlg = new Form
                {
                    Text = "記錄出席 - " + className + " (" + dateStr + ")",
                    Size = new Size(520, 640), BackColor = Color.FromArgb(18, 18, 35),
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    MaximizeBox = false, StartPosition = FormStartPosition.CenterParent,
                    Font = new Font("Microsoft JhengHei UI", 10f)
                };

                dlg.Controls.Add(new Label { Text = "搜尋姓名：", ForeColor = Color.FromArgb(140, 140, 180), Size = new Size(460, 24), Location = new Point(25, 15), BackColor = Color.Transparent });
                var txtSearch = new TextBox { Size = new Size(460, 32), Location = new Point(25, 40), BackColor = Color.FromArgb(38, 38, 68), ForeColor = Color.FromArgb(240, 240, 255), BorderStyle = BorderStyle.FixedSingle, Font = new Font("Microsoft JhengHei UI", 11f) };
                dlg.Controls.Add(txtSearch);

                dlg.Controls.Add(new Label { Text = "勾選今日出席的會員：", ForeColor = Color.FromArgb(140, 140, 180), Size = new Size(460, 24), Location = new Point(25, 82), BackColor = Color.Transparent });

                var clbMembers = new CheckedListBox
                {
                    Size = new Size(460, 360), Location = new Point(25, 108),
                    BackColor = Color.FromArgb(38, 38, 68), ForeColor = Color.FromArgb(240, 240, 255),
                    Font = new Font("Microsoft JhengHei UI", 11f), BorderStyle = BorderStyle.FixedSingle,
                    CheckOnClick = true
                };

                // 已出席的會員ID
                var alreadyAttended = attendance.AsEnumerable()
                    .Where(r => r["ClassID"].ToString() == classId && r["Date"].ToString() == dateStr)
                    .Select(r => r["MemberID"].ToString()).ToList();

                var memberList = new System.Collections.Generic.List<(string id, string name, string phone)>();
                foreach (var en in enrolled)
                {
                    var m = members.AsEnumerable().FirstOrDefault(r => r["MemberID"].ToString() == en["MemberID"].ToString());
                    if (m != null) memberList.Add((m["MemberID"].ToString(), m["Name"].ToString(), m["Phone"].ToString()));
                }

                Action refreshClb = () =>
                {
                    string kw = txtSearch.Text.Trim();
                    clbMembers.Items.Clear();
                    foreach (var mb in memberList)
                    {
                        if (!string.IsNullOrEmpty(kw) && !mb.name.Contains(kw)) continue;
                        int idx = clbMembers.Items.Add(mb.id + " - " + mb.name);
                        if (alreadyAttended.Contains(mb.id))
                            clbMembers.SetItemChecked(idx, true);
                    }
                };
                refreshClb();
                txtSearch.TextChanged += (ts, te) => refreshClb();

                dlg.Controls.Add(new Label { Text = "備註：", ForeColor = Color.FromArgb(140, 140, 180), Size = new Size(100, 24), Location = new Point(25, 480), BackColor = Color.Transparent });
                var txtNote = new TextBox { Size = new Size(460, 32), Location = new Point(25, 506), BackColor = Color.FromArgb(38, 38, 68), ForeColor = Color.FromArgb(240, 240, 255), BorderStyle = BorderStyle.FixedSingle, Font = new Font("Microsoft JhengHei UI", 10f) };
                dlg.Controls.Add(txtNote);

                var btnSave = new Button { Text = "💾 儲存出席", Size = new Size(160, 40), Location = new Point(170, 552), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(78, 205, 196), ForeColor = Color.White, Font = new Font("Microsoft JhengHei UI", 10f, FontStyle.Bold), Cursor = Cursors.Hand };
                btnSave.FlatAppearance.BorderSize = 0;
                btnSave.Click += (bs, be) =>
                {
                    // 先刪除今日此課程的舊記錄
                    var attDt = DatabaseHelper.LoadTable("Attendance");
                    for (int i = attDt.Rows.Count - 1; i >= 0; i--)
                        if (attDt.Rows[i]["ClassID"].ToString() == classId && attDt.Rows[i]["Date"].ToString() == dateStr)
                            attDt.Rows.RemoveAt(i);
                    DatabaseHelper.SaveTable("Attendance", attDt);

                    // 新增勾選的出席記錄
                    foreach (int idx in clbMembers.CheckedIndices)
                    {
                        string memberId = clbMembers.Items[idx].ToString().Split('-')[0].Trim();
                        DatabaseHelper.InsertRow("Attendance", "AttendID", new System.Collections.Generic.Dictionary<string, string>
                        {
                            { "ClassID",    classId },
                            { "MemberID",   memberId },
                            { "Date",       dateStr },
                            { "AttendTime", DateTime.Now.ToString("HH:mm") },
                            { "Note",       txtNote.Text.Trim() }
                        });
                    }
                    MessageBox.Show("出席記錄已儲存！共 " + clbMembers.CheckedItems.Count + " 人出席。", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    dlg.DialogResult = DialogResult.OK;
                    dlg.Close();
                };

                dlg.Controls.Add(clbMembers);
                dlg.Controls.Add(btnSave);
                if (dlg.ShowDialog() == DialogResult.OK) loadAttendance();
            };

            cmbClass.SelectedIndexChanged += (s, e) => loadAttendance();
            dtpDate.ValueChanged          += (s, e) => loadAttendance();

            pnlContent.Controls.Add(cmbClass);
            pnlContent.Controls.Add(dtpDate);
            pnlContent.Controls.Add(btnRecord);
            pnlContent.Controls.Add(grid);
        }

        // ══════════════════════════════════════════════════════════════════════
        // 繳費管理
        // ══════════════════════════════════════════════════════════════════════
        private void ShowPayments()
        {
            pnlContent.Controls.Clear();
            pnlContent.Controls.Add(MakeHeader("💳 繳費管理"));

            var btnAdd = MakeBtn("➕ 新增繳費", ClrCard1, new Point(30, 70));
            var grid   = MakeGrid(); grid.Location = new Point(30, 165); grid.Size = new Size(840, 475);

            Action load = () =>
            {
                var payments = DatabaseHelper.LoadTable("Payments");
                var members  = DatabaseHelper.LoadTable("Members");
                var plans    = DatabaseHelper.LoadTable("Plans");
                var dt = new DataTable();
                dt.Columns.Add("ID"); dt.Columns.Add("會員姓名"); dt.Columns.Add("方案");
                dt.Columns.Add("金額"); dt.Columns.Add("繳費日期"); dt.Columns.Add("到期日");
                foreach (DataRow p in payments.AsEnumerable().OrderByDescending(r => r["PaymentID"].ToString()).ToList())
                {
                    var m  = members.AsEnumerable().FirstOrDefault(r => r["MemberID"].ToString() == p["MemberID"].ToString());
                    var pl = plans.AsEnumerable().FirstOrDefault(r => r["PlanID"].ToString()    == p["PlanID"].ToString());
                    dt.Rows.Add(p["PaymentID"], m?["Name"] ?? "—", pl?["PlanName"] ?? "—", pl?["Price"] ?? "—", p["PayDate"], p["ExpireDate"]);
                }
                grid.DataSource = dt;
            };
            load();

            btnAdd.Click += (s, e) => { if (new frmPayment().ShowDialog() == DialogResult.OK) load(); };
            pnlContent.Controls.Add(btnAdd);
            pnlContent.Controls.Add(grid);
        }

        // ── Helpers ───────────────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════
        // 統計圖表
        // ══════════════════════════════════════════════════════════════════════
        private void ShowStats()
        {
            pnlContent.Controls.Clear();
            pnlContent.Controls.Add(MakeHeader("📊 統計圖表"));

            var payments = DatabaseHelper.LoadTable("Payments");
            var members  = DatabaseHelper.LoadTable("Members");
            var checkins = DatabaseHelper.LoadTable("CheckIns");

            // ── 每月收入長條圖 ────────────────────────────────────────────────
            pnlContent.Controls.Add(MakeSectionLabel("💰 近6個月收入統計", Color.FromArgb(255, 209, 92), 75));

            var pnlBar = new Panel { Size = new Size(840, 200), Location = new Point(30, 108), BackColor = Color.FromArgb(25, 25, 50) };
            pnlBar.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                // 計算近6個月收入
                var monthData = new System.Collections.Generic.Dictionary<string, double>();
                for (int i = 5; i >= 0; i--)
                {
                    var d   = DateTime.Today.AddMonths(-i);
                    string key = d.ToString("yyyy-MM");
                    monthData[key] = 0;
                }
                var plans = DatabaseHelper.LoadTable("Plans");
                foreach (DataRow p in payments.Rows)
                {
                    string payMonth = p["PayDate"].ToString().Substring(0, 7);
                    if (monthData.ContainsKey(payMonth))
                    {
                        var plan = plans.AsEnumerable().FirstOrDefault(r => r["PlanID"].ToString() == p["PlanID"].ToString());
                        if (plan != null && double.TryParse(plan["Price"].ToString(), out double price))
                            monthData[payMonth] += price;
                    }
                }

                double maxVal = 1;
                foreach (var v in monthData.Values) if (v > maxVal) maxVal = v;

                int barW  = 100, gap = 40, startX = 60, chartH = 150, baseY = 175;
                int idx   = 0;
                Color barColor = Color.FromArgb(255, 209, 92);

                foreach (var kv in monthData)
                {
                    int barH  = (int)(kv.Value / maxVal * chartH);
                    int x     = startX + idx * (barW + gap);
                    int y     = baseY - barH;

                    if (barH > 0)
                    {
                        using (var br = new System.Drawing.Drawing2D.LinearGradientBrush(
                            new Rectangle(x, y, barW, barH),
                            Color.FromArgb(255, 220, 100), Color.FromArgb(200, 150, 30),
                            System.Drawing.Drawing2D.LinearGradientMode.Vertical))
                            g.FillRectangle(br, x, y, barW, barH);
                    }
                    else
                    {
                        using (var br = new SolidBrush(Color.FromArgb(60, 60, 90)))
                            g.FillRectangle(br, x, baseY - 2, barW, 2);
                    }

                    // 金額標籤
                    using (var fnt = new Font("Microsoft JhengHei UI", 8f))
                    using (var brW = new SolidBrush(Color.FromArgb(240, 240, 255)))
                    {
                        string valStr = kv.Value > 0 ? "NT$" + kv.Value.ToString("0") : "0";
                        g.DrawString(valStr, fnt, brW, x + barW / 2 - 20, y - 18);
                        g.DrawString(kv.Key.Substring(5) + "月", fnt, brW, x + barW / 2 - 15, baseY + 5);
                    }
                    idx++;
                }
                // 基線
                using (var pen = new Pen(Color.FromArgb(80, 80, 120), 1))
                    g.DrawLine(pen, 30, baseY, pnlBar.Width - 30, baseY);
            };
            pnlContent.Controls.Add(pnlBar);

            // ── 每月新會員折線圖 ──────────────────────────────────────────────
            pnlContent.Controls.Add(MakeSectionLabel("👥 近6個月新增會員", Color.FromArgb(78, 205, 196), 325));

            var pnlLine = new Panel { Size = new Size(840, 200), Location = new Point(30, 358), BackColor = Color.FromArgb(25, 25, 50) };
            pnlLine.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                var monthData = new System.Collections.Generic.Dictionary<string, int>();
                for (int i = 5; i >= 0; i--)
                {
                    var d = DateTime.Today.AddMonths(-i);
                    monthData[d.ToString("yyyy-MM")] = 0;
                }
                foreach (DataRow m in members.Rows)
                {
                    string joinMonth = m["JoinDate"].ToString().Length >= 7 ? m["JoinDate"].ToString().Substring(0, 7) : "";
                    if (monthData.ContainsKey(joinMonth)) monthData[joinMonth]++;
                }

                int maxVal = 1;
                foreach (var v in monthData.Values) if (v > maxVal) maxVal = v;

                int gap = 140, startX = 80, chartH = 140, baseY = 170;
                var points = new System.Collections.Generic.List<Point>();
                int idx2 = 0;

                foreach (var kv in monthData)
                {
                    int x = startX + idx2 * gap;
                    int y = baseY - (int)((double)kv.Value / maxVal * chartH);
                    points.Add(new Point(x, y));

                    using (var brC = new SolidBrush(Color.FromArgb(78, 205, 196)))
                        g.FillEllipse(brC, x - 6, y - 6, 12, 12);

                    using (var fnt = new Font("Microsoft JhengHei UI", 8f))
                    using (var brW = new SolidBrush(Color.FromArgb(240, 240, 255)))
                    {
                        g.DrawString(kv.Value.ToString() + " 人", fnt, brW, x - 15, y - 22);
                        g.DrawString(kv.Key.Substring(5) + "月", fnt, brW, x - 15, baseY + 5);
                    }
                    idx2++;
                }
                if (points.Count > 1)
                    using (var pen = new Pen(Color.FromArgb(78, 205, 196), 2.5f))
                        g.DrawLines(pen, points.ToArray());

                using (var pen = new Pen(Color.FromArgb(80, 80, 120), 1))
                    g.DrawLine(pen, 30, baseY, pnlLine.Width - 30, baseY);
            };
            pnlContent.Controls.Add(pnlLine);
        }

        // ══════════════════════════════════════════════════════════════════════
        // 匯出 CSV
        // ══════════════════════════════════════════════════════════════════════
        private void ShowExport()
        {
            pnlContent.Controls.Clear();
            pnlContent.Controls.Add(MakeHeader("📋 匯出 CSV"));

            Color clrCard  = Color.FromArgb(28, 28, 52);
            Color clrText  = Color.FromArgb(240, 240, 255);
            Color clrSub   = Color.FromArgb(140, 140, 180);

            // 匯出選項卡片
            var exports = new[]
            {
                ("👥 會員資料",    "匯出所有會員的姓名、電話、Email、加入日期、到期日",  Color.FromArgb(255, 107, 107), "Members"),
                ("💳 繳費記錄",    "匯出所有繳費記錄，包含會員姓名、方案、金額、日期",   Color.FromArgb(78, 205, 196),  "Payments"),
                ("📅 課程報名",    "匯出所有課程的報名名單",                             Color.FromArgb(255, 209, 92),  "Enrollments"),
                ("✅ 打卡記錄",    "匯出所有入場/退場打卡記錄",                          Color.FromArgb(162, 89, 255),  "CheckIns"),
            };

            for (int i = 0; i < exports.Length; i++)
            {
                var (title, desc, color, tableKey) = exports[i];
                int row = i / 2, col = i % 2;
                int px = 30 + col * 430, py = 100 + row * 180;

                var pnl = new Panel { Size = new Size(400, 150), Location = new Point(px, py), BackColor = clrCard };
                int ci = i;
                pnl.Paint += (s, e) =>
                {
                    using (var pen = new Pen(Color.FromArgb(60, 60, 100), 1.5f))
                        e.Graphics.DrawRectangle(pen, 0, 0, pnl.Width - 1, pnl.Height - 1);
                    using (var br = new SolidBrush(exports[ci].Item3))
                        e.Graphics.FillRectangle(br, 0, 0, pnl.Width, 4);
                };

                pnl.Controls.Add(new Label { Text = title, Font = new Font("Microsoft JhengHei UI", 13f, FontStyle.Bold), ForeColor = color, Size = new Size(370, 35), Location = new Point(15, 15), BackColor = Color.Transparent });
                pnl.Controls.Add(new Label { Text = desc, Font = new Font("Microsoft JhengHei UI", 9f), ForeColor = clrSub, Size = new Size(370, 40), Location = new Point(15, 50), BackColor = Color.Transparent });

                var btnExport = new Button { Text = "⬇️ 匯出", Size = new Size(110, 36), Location = new Point(15, 100), FlatStyle = FlatStyle.Flat, BackColor = color, ForeColor = Color.White, Font = new Font("Microsoft JhengHei UI", 10f, FontStyle.Bold), Cursor = Cursors.Hand };
                btnExport.FlatAppearance.BorderSize = 0;

                string capturedKey = tableKey;
                string capturedTitle = title;
                btnExport.Click += (s, e) => ExportCSV(capturedKey, capturedTitle);
                pnl.Controls.Add(btnExport);
                pnlContent.Controls.Add(pnl);
            }
        }

        private void ExportCSV(string tableKey, string title)
        {
            var sfd = new SaveFileDialog
            {
                Title      = "匯出 " + title,
                FileName   = tableKey + "_" + DateTime.Today.ToString("yyyyMMdd") + ".csv",
                Filter     = "CSV 檔案 (*.csv)|*.csv",
                DefaultExt = "csv"
            };
            if (sfd.ShowDialog() != DialogResult.OK) return;

            var members  = DatabaseHelper.LoadTable("Members");
            var payments = DatabaseHelper.LoadTable("Payments");
            var plans    = DatabaseHelper.LoadTable("Plans");
            var checkins = DatabaseHelper.LoadTable("CheckIns");
            var enroll   = DatabaseHelper.LoadTable("Enrollments");
            var classes  = DatabaseHelper.LoadTable("Classes");

            var lines = new System.Collections.Generic.List<string>();

            if (tableKey == "Members")
            {
                lines.Add("會員ID,姓名,電話,Email,加入日期,到期日");
                foreach (DataRow m in members.Rows)
                {
                    string expire = payments.AsEnumerable()
                        .Where(p => p["MemberID"].ToString() == m["MemberID"].ToString())
                        .OrderByDescending(p => p["ExpireDate"].ToString())
                        .Select(p => p["ExpireDate"].ToString()).FirstOrDefault() ?? "尚未繳費";
                    lines.Add($"{m["MemberID"]},{m["Name"]},{m["Phone"]},{m["Email"]},{m["JoinDate"]},{expire}");
                }
            }
            else if (tableKey == "Payments")
            {
                lines.Add("繳費ID,會員姓名,方案,金額,繳費日期,到期日");
                foreach (DataRow p in payments.Rows)
                {
                    var m  = members.AsEnumerable().FirstOrDefault(r => r["MemberID"].ToString() == p["MemberID"].ToString());
                    var pl = plans.AsEnumerable().FirstOrDefault(r => r["PlanID"].ToString() == p["PlanID"].ToString());
                    lines.Add($"{p["PaymentID"]},{m?["Name"]},{pl?["PlanName"]},{pl?["Price"]},{p["PayDate"]},{p["ExpireDate"]}");
                }
            }
            else if (tableKey == "Enrollments")
            {
                lines.Add("報名ID,會員姓名,課程名稱,教練,時間,報名日期");
                foreach (DataRow en in enroll.Rows)
                {
                    var m  = members.AsEnumerable().FirstOrDefault(r => r["MemberID"].ToString() == en["MemberID"].ToString());
                    var cl = classes.AsEnumerable().FirstOrDefault(r => r["ClassID"].ToString()  == en["ClassID"].ToString());
                    lines.Add($"{en["EnrollID"]},{m?["Name"]},{cl?["ClassName"]},{cl?["Coach"]},{cl?["Schedule"]},{en["EnrollDate"]}");
                }
            }
            else if (tableKey == "CheckIns")
            {
                lines.Add("打卡ID,會員姓名,入場時間,退場時間");
                foreach (DataRow c in checkins.Rows)
                {
                    var m = members.AsEnumerable().FirstOrDefault(r => r["MemberID"].ToString() == c["MemberID"].ToString());
                    string outTime = SafeGet(c, "CheckOutTime");
                    lines.Add($"{c["CheckInID"]},{m?["Name"]},{c["CheckInTime"]},{(string.IsNullOrEmpty(outTime) ? "尚未退場" : outTime)}");
                }
            }

            System.IO.File.WriteAllLines(sfd.FileName, lines, System.Text.Encoding.UTF8);
            MessageBox.Show("✅ 匯出成功！\n檔案已儲存至：\n" + sfd.FileName, "匯出完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ══════════════════════════════════════════════════════════════════════
        // 方案管理
        // ══════════════════════════════════════════════════════════════════════
        private void ShowPlans()
        {
            pnlContent.Controls.Clear();
            pnlContent.Controls.Add(MakeHeader("💰 方案管理"));

            var btnAdd  = MakeBtn("➕ 新增方案", ClrCard1, new Point(30, 70));
            var btnEdit = MakeBtn("✏️ 編輯",     Color.FromArgb(162, 89, 255), new Point(170, 70));
            var btnDel  = MakeBtn("🗑 刪除",     Color.FromArgb(180, 60, 60),  new Point(310, 70));

            var grid = MakeGrid();
            grid.Location = new Point(30, 125);
            grid.Size     = new Size(840, 520);
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            Action load = () =>
            {
                var plans = DatabaseHelper.LoadTable("Plans");
                var dt    = new DataTable();
                dt.Columns.Add("ID"); dt.Columns.Add("方案名稱"); dt.Columns.Add("價格 (NT$)"); dt.Columns.Add("天數");
                foreach (DataRow p in plans.Rows)
                    dt.Rows.Add(p["PlanID"], p["PlanName"], p["Price"], p["DurationDays"]);
                grid.DataSource = dt;
            };
            load();

            // 新增方案
            btnAdd.Click += (s, e) =>
            {
                var dlg = MakePlanDialog("新增方案", "", "", "");
                if (dlg.ShowDialog() == DialogResult.OK) load();
            };

            // 編輯方案
            btnEdit.Click += (s, e) =>
            {
                if (grid.SelectedRows.Count == 0) { MessageBox.Show("請先選取一個方案！"); return; }
                string id    = grid.SelectedRows[0].Cells["ID"].Value?.ToString();
                string name  = grid.SelectedRows[0].Cells["方案名稱"].Value?.ToString();
                string price = grid.SelectedRows[0].Cells["價格 (NT$)"].Value?.ToString();
                string days  = grid.SelectedRows[0].Cells["天數"].Value?.ToString();
                var dlg = MakePlanDialog("編輯方案", name, price, days, id);
                if (dlg.ShowDialog() == DialogResult.OK) load();
            };

            // 刪除方案
            btnDel.Click += (s, e) =>
            {
                if (grid.SelectedRows.Count == 0) { MessageBox.Show("請先選取一個方案！"); return; }
                string id   = grid.SelectedRows[0].Cells["ID"].Value?.ToString();
                string name = grid.SelectedRows[0].Cells["方案名稱"].Value?.ToString();

                // 檢查是否有繳費記錄使用此方案
                var payments = DatabaseHelper.LoadTable("Payments");
                bool inUse   = payments.AsEnumerable().Any(p => p["PlanID"].ToString() == id);
                if (inUse) { MessageBox.Show("此方案已有繳費記錄，無法刪除！"); return; }

                if (MessageBox.Show("確定刪除方案「" + name + "」？", "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    DatabaseHelper.DeleteRow("Plans", "PlanID", id);
                    load();
                }
            };

            pnlContent.Controls.Add(btnAdd);
            pnlContent.Controls.Add(btnEdit);
            pnlContent.Controls.Add(btnDel);
            pnlContent.Controls.Add(grid);
        }

        private Form MakePlanDialog(string title, string name, string price, string days, string editId = null)
        {
            Color clrBg    = Color.FromArgb(18, 18, 35);
            Color clrInput = Color.FromArgb(38, 38, 68);
            Color clrText  = Color.FromArgb(240, 240, 255);
            Color clrSub   = Color.FromArgb(140, 140, 180);

            var dlg = new Form
            {
                Text = title, Size = new Size(400, 350),
                BackColor = clrBg, FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false, StartPosition = FormStartPosition.CenterParent,
                Font = new Font("Microsoft JhengHei UI", 10f)
            };

            string[] labels = { "方案名稱 *", "價格 (NT$) *", "天數 *" };
            string[] vals   = { name, price, days };
            var txts = new TextBox[3];

            for (int i = 0; i < 3; i++)
            {
                dlg.Controls.Add(new Label { Text = labels[i], ForeColor = clrSub, Size = new Size(330, 24), Location = new Point(35, 25 + i * 70), BackColor = Color.Transparent });
                txts[i] = new TextBox { Size = new Size(330, 34), Location = new Point(35, 50 + i * 70), BackColor = clrInput, ForeColor = clrText, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Microsoft JhengHei UI", 11f), Text = vals[i] };
                dlg.Controls.Add(txts[i]);
            }

            var lblErr = new Label { Text = "", ForeColor = Color.FromArgb(255, 107, 107), Size = new Size(330, 22), Location = new Point(35, 238), BackColor = Color.Transparent, Font = new Font("Microsoft JhengHei UI", 9f) };
            dlg.Controls.Add(lblErr);

            var btnSave = new Button { Text = "💾 儲存", Size = new Size(140, 40), Location = new Point(130, 265), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(255, 107, 107), ForeColor = Color.White, Font = new Font("Microsoft JhengHei UI", 11f, FontStyle.Bold), Cursor = Cursors.Hand };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += (s, e) =>
            {
                string pName = txts[0].Text.Trim();
                if (string.IsNullOrEmpty(pName)) { lblErr.Text = "❌ 方案名稱不能為空！"; return; }
                if (!double.TryParse(txts[1].Text.Trim(), out double pPrice) || pPrice < 0) { lblErr.Text = "❌ 請輸入有效的價格！"; return; }
                if (!int.TryParse(txts[2].Text.Trim(), out int pDays) || pDays <= 0) { lblErr.Text = "❌ 請輸入有效的天數！"; return; }

                if (editId == null)
                {
                    // 新增
                    DatabaseHelper.InsertRow("Plans", "PlanID", new System.Collections.Generic.Dictionary<string, string>
                    {
                        { "PlanName", pName }, { "Price", pPrice.ToString() }, { "DurationDays", pDays.ToString() }
                    });
                }
                else
                {
                    // 編輯
                    var dt = DatabaseHelper.LoadTable("Plans");
                    foreach (DataRow r in dt.Rows)
                    {
                        if (r["PlanID"].ToString() != editId) continue;
                        r["PlanName"]     = pName;
                        r["Price"]        = pPrice.ToString();
                        r["DurationDays"] = pDays.ToString();
                        break;
                    }
                    DatabaseHelper.SaveTable("Plans", dt);
                }
                dlg.DialogResult = DialogResult.OK;
                dlg.Close();
            };
            dlg.Controls.Add(btnSave);
            return dlg;
        }

        // ══════════════════════════════════════════════════════════════════════
        // 管理員管理
        // ══════════════════════════════════════════════════════════════════════
        private void ShowAdminManager()
        {
            Color clrBg    = Color.FromArgb(18, 18, 35);
            Color clrInput = Color.FromArgb(38, 38, 68);
            Color clrText  = Color.FromArgb(240, 240, 255);
            Color clrSub   = Color.FromArgb(140, 140, 180);
            Color clrRed   = Color.FromArgb(255, 107, 107);

            var dlg = new Form
            {
                Text = "🔑 管理員管理", Size = new Size(500, 560),
                BackColor = clrBg, FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false, StartPosition = FormStartPosition.CenterParent,
                Font = new Font("Microsoft JhengHei UI", 10f)
            };

            // 管理員清單
            dlg.Controls.Add(new Label { Text = "目前管理員", ForeColor = clrSub, Size = new Size(420, 24), Location = new Point(30, 20), BackColor = Color.Transparent, Font = new Font("Microsoft JhengHei UI", 11f, FontStyle.Bold) });

            var lstAdmins = new ListBox
            {
                Size = new Size(420, 150), Location = new Point(30, 48),
                BackColor = clrInput, ForeColor = clrText,
                Font = new Font("Microsoft JhengHei UI", 11f), BorderStyle = BorderStyle.FixedSingle
            };

            Action refreshList = () =>
            {
                lstAdmins.Items.Clear();
                foreach (var kv in frmLogin.LoadAdmins())
                    lstAdmins.Items.Add(kv.Key);
            };
            refreshList();

            // 刪除按鈕
            var btnDel = new Button { Text = "🗑 刪除選取", Size = new Size(140, 34), Location = new Point(30, 208), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(180, 60, 60), ForeColor = Color.White, Font = new Font("Microsoft JhengHei UI", 9f, FontStyle.Bold), Cursor = Cursors.Hand };
            btnDel.FlatAppearance.BorderSize = 0;
            btnDel.Click += (s, e) =>
            {
                if (lstAdmins.SelectedItem == null) return;
                string user   = lstAdmins.SelectedItem.ToString();
                var admins    = frmLogin.LoadAdmins();
                if (admins.Count <= 1) { MessageBox.Show("至少需要保留一個管理員！"); return; }
                if (MessageBox.Show("確定刪除管理員 " + user + "？", "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    admins.Remove(user);
                    frmLogin.SaveAdmins(admins);
                    refreshList();
                }
            };

            var sep1 = new Panel { Size = new Size(420, 1), Location = new Point(30, 255), BackColor = Color.FromArgb(60, 60, 90) };

            // 新增管理員
            dlg.Controls.Add(new Label { Text = "新增管理員", ForeColor = clrSub, Size = new Size(420, 24), Location = new Point(30, 270), BackColor = Color.Transparent, Font = new Font("Microsoft JhengHei UI", 11f, FontStyle.Bold) });

            dlg.Controls.Add(new Label { Text = "帳號", ForeColor = clrSub, Size = new Size(180, 22), Location = new Point(30, 302), BackColor = Color.Transparent });
            var txtNewUser = new TextBox { Size = new Size(180, 32), Location = new Point(30, 325), BackColor = clrInput, ForeColor = clrText, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Microsoft JhengHei UI", 11f) };

            dlg.Controls.Add(new Label { Text = "密碼", ForeColor = clrSub, Size = new Size(180, 22), Location = new Point(240, 302), BackColor = Color.Transparent });
            var txtNewPass = new TextBox { Size = new Size(180, 32), Location = new Point(240, 325), BackColor = clrInput, ForeColor = clrText, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Microsoft JhengHei UI", 11f), PasswordChar = '●' };

            var lblAddErr = new Label { Text = "", ForeColor = clrRed, Size = new Size(420, 22), Location = new Point(30, 365), BackColor = Color.Transparent, Font = new Font("Microsoft JhengHei UI", 9f) };

            var btnAdd = new Button { Text = "➕ 新增管理員", Size = new Size(180, 38), Location = new Point(30, 388), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(78, 205, 196), ForeColor = Color.White, Font = new Font("Microsoft JhengHei UI", 10f, FontStyle.Bold), Cursor = Cursors.Hand };
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Click += (s, e) =>
            {
                string user = txtNewUser.Text.Trim();
                string pass = txtNewPass.Text;
                if (string.IsNullOrEmpty(user)) { lblAddErr.Text = "❌ 帳號不能為空！"; return; }
                if (string.IsNullOrEmpty(pass)) { lblAddErr.Text = "❌ 密碼不能為空！"; return; }
                var admins = frmLogin.LoadAdmins();
                if (admins.ContainsKey(user)) { lblAddErr.Text = "❌ 此帳號已存在！"; return; }
                admins[user] = pass;
                frmLogin.SaveAdmins(admins);
                txtNewUser.Clear(); txtNewPass.Clear();
                lblAddErr.Text = "✅ 管理員 " + user + " 新增成功！";
                lblAddErr.ForeColor = Color.FromArgb(78, 205, 196);
                refreshList();
            };

            var sep2 = new Panel { Size = new Size(420, 1), Location = new Point(30, 440), BackColor = Color.FromArgb(60, 60, 90) };

            // 更改密碼
            dlg.Controls.Add(new Label { Text = "更改密碼（選取管理員後輸入新密碼）", ForeColor = clrSub, Size = new Size(420, 24), Location = new Point(30, 455), BackColor = Color.Transparent, Font = new Font("Microsoft JhengHei UI", 10f, FontStyle.Bold) });
            var txtChgPass = new TextBox { Size = new Size(260, 32), Location = new Point(30, 480), BackColor = clrInput, ForeColor = clrText, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Microsoft JhengHei UI", 11f), PasswordChar = '●' };
            var btnChg = new Button { Text = "💾 更新密碼", Size = new Size(130, 32), Location = new Point(300, 480), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(162, 89, 255), ForeColor = Color.White, Font = new Font("Microsoft JhengHei UI", 9f, FontStyle.Bold), Cursor = Cursors.Hand };
            btnChg.FlatAppearance.BorderSize = 0;
            btnChg.Click += (s, e) =>
            {
                if (lstAdmins.SelectedItem == null) { MessageBox.Show("請先選取管理員！"); return; }
                if (string.IsNullOrEmpty(txtChgPass.Text)) { MessageBox.Show("新密碼不能為空！"); return; }
                string user  = lstAdmins.SelectedItem.ToString();
                var admins   = frmLogin.LoadAdmins();
                admins[user] = txtChgPass.Text;
                frmLogin.SaveAdmins(admins);
                txtChgPass.Clear();
                MessageBox.Show(user + " 的密碼已更新！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            dlg.Controls.AddRange(new Control[] { lstAdmins, btnDel, sep1, txtNewUser, txtNewPass, lblAddErr, btnAdd, sep2, txtChgPass, btnChg });
            dlg.ShowDialog();
        }

        private static string SafeGet(DataRow row, string col)
        {
            if (!row.Table.Columns.Contains(col)) return "";
            return row[col]?.ToString() ?? "";
        }

        private Label MakeHeader(string text) => new Label { Text = text, Font = new Font("Microsoft JhengHei UI", 16f, FontStyle.Bold), ForeColor = ClrTextMain, Size = new Size(860, 50), Location = new Point(30, 15), BackColor = Color.Transparent };
        private Label MakeSectionLabel(string text, Color color, int y) => new Label { Text = text, Font = new Font("Microsoft JhengHei UI", 11f, FontStyle.Bold), ForeColor = color, Size = new Size(860, 30), Location = new Point(30, y), BackColor = Color.Transparent };

        private Button MakeBtn(string text, Color color, Point loc)
        {
            var btn = new Button { Text = text, Size = new Size(130, 38), Location = loc, FlatStyle = FlatStyle.Flat, BackColor = color, ForeColor = Color.White, Font = new Font("Microsoft JhengHei UI", 10f, FontStyle.Bold), Cursor = Cursors.Hand };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
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
            g.DefaultCellStyle.ForeColor          = ClrTextMain;
            g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(60, 30, 80);
            g.DefaultCellStyle.SelectionForeColor = Color.White;
            g.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(35, 35, 65);
            g.ColumnHeadersDefaultCellStyle.ForeColor = ClrAccent;
            g.ColumnHeadersDefaultCellStyle.Font      = new Font("Microsoft JhengHei UI", 11f, FontStyle.Bold);
            g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(26, 26, 50);
            return g;
        }
    }
}
