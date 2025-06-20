using System;
using System.Drawing;
using System.Windows.Forms;

namespace Operating_Timer.Components
{
    public class ErrorDialog : Form
    {
        private Timer autoCloseTimer;

        public enum DialogType
        {
            Error,
            Info,
            Success
        }

        public ErrorDialog(string message, string title = "Kesalahan", DialogType type = DialogType.Error)
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(420, 180);
            this.BackColor = Color.FromArgb(25, 40, 65);
            this.TopMost = true;
            this.ShowInTaskbar = false;
            this.Text = title;
            this.Opacity = 0.96;

            this.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, this.Width, this.Height, 15, 15));

            // Ikon
            Label iconLabel = new Label()
            {
                AutoSize = false,
                Size = new Size(40, 40),
                Location = new Point(20, 20),
                Font = new Font("Segoe UI Symbol", 24, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White
            };

            switch (type)
            {
                case DialogType.Error:
                    iconLabel.Text = "❌";
                    iconLabel.ForeColor = Color.Tomato;
                    break;
                case DialogType.Info:
                    iconLabel.Text = "ℹ️";
                    iconLabel.ForeColor = Color.DeepSkyBlue;
                    break;
                case DialogType.Success:
                    iconLabel.Text = "✅";
                    iconLabel.ForeColor = Color.MediumSeaGreen;
                    break;
            }

            this.Controls.Add(iconLabel);

            // Pesan
            Label lbl = new Label()
            {
                Text = message,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10),
                Size = new Size(330, 60),
                Location = new Point(70, 20),
                TextAlign = ContentAlignment.MiddleLeft
            };
            this.Controls.Add(lbl);

            // Tombol OK
            Button btnOK = new Button()
            {
                Text = "OK",
                DialogResult = DialogResult.OK,
                Size = new Size(80, 32),
                Location = new Point((this.Width - 80) / 2, 110),
                BackColor = GetColorByType(type),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnOK.FlatAppearance.BorderSize = 0;
            btnOK.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btnOK.Width, btnOK.Height, 8, 8));
            this.Controls.Add(btnOK);
            this.AcceptButton = btnOK;

            // Auto-close timer (3 detik)
            autoCloseTimer = new Timer();
            autoCloseTimer.Interval = 3000;
            autoCloseTimer.Tick += (s, e) =>
            {
                autoCloseTimer.Stop();
                this.DialogResult = DialogResult.OK;
                this.Close();
            };
            autoCloseTimer.Start();
        }

        private Color GetColorByType(DialogType type)
        {
            switch (type)
            {
                case DialogType.Error: return Color.IndianRed;
                case DialogType.Info: return Color.DodgerBlue;
                case DialogType.Success: return Color.SeaGreen;
                default: return Color.Teal;
            }
        }

        [System.Runtime.InteropServices.DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(
            int nLeftRect, int nTopRect, int nRightRect, int nBottomRect,
            int nWidthEllipse, int nHeightEllipse);

        private void ErrorDialog_Load(object sender, EventArgs e)
        {

        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // ErrorDialog
            // 
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Name = "ErrorDialog";
            this.Load += new System.EventHandler(this.ErrorDialog_Load_1);
            this.ResumeLayout(false);

        }

        private void ErrorDialog_Load_1(object sender, EventArgs e)
        {

        }
    }
}
