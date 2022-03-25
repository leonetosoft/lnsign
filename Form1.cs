using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace lnsign
{

    public partial class Form1 : Form
    {
        private Timer tmr;

        public Form1()
        {
            InitializeComponent();

            tmr = new Timer();
            tmr.Tick += delegate {
                this.Hide();
                notifyIcon1.ShowBalloonTip(500);
                tmr.Stop();
            };
            tmr.Interval = (int)TimeSpan.FromSeconds(10).TotalMilliseconds;
            tmr.Start();
            var version = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);
            this.lb_build_date.Text = "Version: " + version.ProductVersion;
        }

        private void reiniciarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Restart();
            Environment.Exit(0);
        }

        private void fecharToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void sobreToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.Show();
        }


        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        /*private SecureString GetSecurePin(string PinCode)
        {
            SecureString pwd = new SecureString();
            foreach (var c in PinCode.ToCharArray()) pwd.AppendChar(c);
            return pwd;
        }*/

    }
}
