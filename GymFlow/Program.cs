using System;
using System.Windows.Forms;

namespace GymFlow
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            DatabaseHelper.InitializeDatabase();

            // 先顯示登入畫面
            var login = new frmLogin();
            if (login.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                Application.Run(new frmMain());
        }
    }
}
