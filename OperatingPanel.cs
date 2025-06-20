using Operating_Timer.Components;
using Operating_Timer.Properties;
using System;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Operating_Timer
{
    public partial class OperatingTimer : Form
    {
        [System.Runtime.InteropServices.DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
            private static extern IntPtr CreateRoundRectRgn(
            int nLeftRect, int nTopRect, int nRightRect,
            int nBottomRect, int nWidthEllipse, int nHeightEllipse
        );

        ClockManager clock;

        private Label lblTime;
        private Label lblDate;
        private Label lblTemp;
        private Label lblHumidity;
        private Label lblVersion;
        private PictureBox logoRS;

        private SensorReader _sensor;

        int rightMargin = 60;


        public OperatingTimer()
        {
            InitializeComponent();
            this.Opacity = 0;
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.Shown += OperatingTimer_Shown;
            this.FormClosed += MainForm_FormClosed;
            this.Icon = new Icon("logo_operating_timer.ico");
            this.AutoScaleMode = AutoScaleMode.Dpi;
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _sensor?.Dispose();
            System.Diagnostics.Debug.WriteLine("[MainForm] SensorReader disposed on close.");
        }


        private void OperatingTimer_Load(object sender, EventArgs e)
        {
            // Clock initialization moved to InitClock
        }

        private async void OperatingTimer_Shown(object sender, EventArgs e)
        {
            await Task.Delay(10);

            InitWindow();
            InitHeader();
            InitClock();
            InitTempHumidity();
            InitTimers();
            InitializeLayout();

            await Task.Delay(100);

            var fadeTimer = new Timer();
            fadeTimer.Interval = 15;
            fadeTimer.Tick += (s2, e2) =>
            {
                if (this.Opacity < 1)
                {
                    this.Opacity += 0.1;
                }
                else
                {
                    this.Opacity = 1;
                    fadeTimer.Stop();
                }
            };
            fadeTimer.Start();
        }

        private void InitWindow()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(11, 30, 61);
        }

        private void InitHeader()
        {

            Label lblTitleTop = new Label()
            {
                Text = "OPERATING TIMER",
                Font = new Font("Segoe UI", 32F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter
            };
            lblTitleTop.Location = new Point((this.Width - lblTitleTop.PreferredWidth) / 2, 20);
            this.Controls.Add(lblTitleTop);

            Label lblTitleSub = new Label()
            {
                Text = "RSUD Anuntaloko",
                Font = new Font("Segoe UI", 20F, FontStyle.Regular),
                ForeColor = Color.Gainsboro,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter
            };
            lblTitleSub.Location = new Point(
                (this.Width - lblTitleSub.PreferredWidth) / 2,
                lblTitleTop.Bottom - 5 
            );
            this.Controls.Add(lblTitleSub);

            lblTime = new Label()
            {
                Font = new Font("Consolas", 50F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true
            };
            lblTime.Text = DateTime.Now.ToString("HH:mm:ss");
            lblTime.Location = new Point(this.Width - lblTime.PreferredWidth - 50, 60);

            lblDate = new Label()
            {
                Font = new Font("Segoe UI", 28F), 
                ForeColor = Color.LightGray,
                AutoSize = true
            };
            lblDate.Text = DateTime.Now.ToString("dd MMM yyyy");
            lblDate.Location = new Point(this.Width - lblDate.PreferredWidth - rightMargin, 130);

            this.Controls.Add(lblTime);
            this.Controls.Add(lblDate);
        }

        private void InitClock()
        {
            if (clock == null)
            {
                clock = new ClockManager();
                clock.OnTimeUpdate += (time) =>
                {
                    lblTime.Text = time;
                    int rMargin = 40;
                    lblTime.Location = new Point(this.Width - lblTime.PreferredWidth - rMargin, 30);
                };

                clock.OnDateUpdate += (date) =>
                {
                    lblDate.Text = date;
                    int rMargin = 40;
                    lblDate.Location = new Point(this.Width - lblDate.PreferredWidth - rMargin, 110);
                };
            }

            clock.Start();
        }

        private void InitTempHumidity()
        {
            int panelWidth = 520;
            int panelHeight = 160;
            int spacing = 60;
            int paddingX = 100;

            int totalWidth = panelWidth * 2 + spacing;
            int startX = paddingX + ((this.Width - 2 * paddingX - totalWidth) / 2);
            int startY = 160;

            // Panel TEMP 
            Panel panelTemp = new Panel()
            {
                Location = new Point(startX, startY),
                Size = new Size(panelWidth, panelHeight),
                BackColor = Color.FromArgb(20, 40, 70),
                BorderStyle = BorderStyle.None
            };
            panelTemp.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, panelWidth, panelHeight, 20, 20));

            PictureBox picTemp = new PictureBox()
            {
                Size = new Size(64, 64),
                Location = new Point(30, 48),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Image = LoadEmbeddedImage("Operating_Timer.Resources.Icons.thermo.png")
            };

            lblTemp = new Label()
            {
                Text = "TEMP: -- °C",
                Font = new Font("Segoe UI", 32F, FontStyle.Bold),
                ForeColor = Color.LightBlue,
                Location = new Point(100, 53),
                AutoSize = true
            };

            panelTemp.Controls.Add(picTemp);
            panelTemp.Controls.Add(lblTemp);
            this.Controls.Add(panelTemp);

            // Panel HUMIDITY 
            Panel panelHum = new Panel()
            {
                Location = new Point(startX + panelWidth + spacing, startY),
                Size = new Size(panelWidth, panelHeight),
                BackColor = Color.FromArgb(20, 40, 70),
                BorderStyle = BorderStyle.None
            };
            panelHum.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, panelWidth, panelHeight, 20, 20));

            PictureBox picHum = new PictureBox()
            {
                Size = new Size(64, 64),
                Location = new Point(30, 48),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Image = LoadEmbeddedImage("Operating_Timer.Resources.Icons.humd.png")
            };

            lblHumidity = new Label()
         
            {
                Text = "HUMIDITY: -- %",
                Font = new Font("Segoe UI", 32F, FontStyle.Bold),
                ForeColor = Color.LightGreen,
                Location = new Point(100, 53),
                AutoSize = true
            };

            panelHum.Controls.Add(picHum);
            panelHum.Controls.Add(lblHumidity);
            this.Controls.Add(panelHum);

            // Sensor Integration
            _sensor = new SensorReader();
            _sensor.OnSensorUpdate += (suhu, rh) =>
            {
                if (this.InvokeRequired)
                    this.BeginInvoke(new Action(() => UpdateSensorUI(suhu, rh)));
                else
                    UpdateSensorUI(suhu, rh);
            };
        }

        private void UpdateSensorUI(string suhuStr, string rhStr)
        {
            System.Diagnostics.Debug.WriteLine($"[UI] Displaying Temp={suhuStr}, Humidity={rhStr}");

            if (float.TryParse(suhuStr, out float suhu))
            {
                lblTemp.Text = $"TEMP: {suhuStr} °C";

                if (suhu < 18)
                    lblTemp.ForeColor = Color.SkyBlue;
                else if (suhu < 22)
                    lblTemp.ForeColor = Color.LightSkyBlue;
                else if (suhu < 26)
                    lblTemp.ForeColor = Color.MediumSeaGreen;
                else if (suhu < 30)
                    lblTemp.ForeColor = Color.Goldenrod;
                else
                    lblTemp.ForeColor = Color.IndianRed;
            }

            if (float.TryParse(rhStr, out float rh))
            {
                lblHumidity.Text = $"HUMIDITY: {rhStr} %";

                if (rh < 30)
                    lblHumidity.ForeColor = Color.SkyBlue;
                else if (rh < 50)
                    lblHumidity.ForeColor = Color.LightGreen;
                else if (rh < 60)
                    lblHumidity.ForeColor = Color.Goldenrod;
                else
                    lblHumidity.ForeColor = Color.IndianRed;
            }

            lblTemp.Invalidate();
            lblHumidity.Invalidate();
        }

        private void InitTimers()
        {
            int blockWidth = 900;
            int blockHeight = 560;
            int spacing = 60;

            int totalWidth = (blockWidth * 2) + spacing;
            int startX = (this.Width - totalWidth) / 2;
            int startY = 420;

            TimerBlock operasi = new TimerBlock("OPERASI")
            {
                Location = new Point(startX, startY),
                Size = new Size(blockWidth, blockHeight)
            };

            TimerBlock anestesi = new TimerBlock("ANESTESI")
            {
                Location = new Point(startX + blockWidth + spacing, startY),
                Size = new Size(blockWidth, blockHeight)
            };

            this.Controls.Add(operasi);
            this.Controls.Add(anestesi);
        }

        private void InitializeLayout()
        {
            // Logo RSUD (kiri atas)
            logoRS = new PictureBox
            {
                Image = LoadEmbeddedImage("Operating_Timer.Resources.Icons.logo_rsud.png"),
                SizeMode = PictureBoxSizeMode.Zoom,
                Size = new Size(100, 100),
                Location = new Point(30, 30)
            };
            this.Controls.Add(logoRS);

            // Logo Vendor (kiri bawah)
            PictureBox logoVendor = new PictureBox
            {
                Image = LoadEmbeddedImage("Operating_Timer.Resources.Icons.logo_elf.png"),
                SizeMode = PictureBoxSizeMode.Zoom,
                Size = new Size(75, 75),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                Location = new Point(40, this.ClientSize.Height - 90)
            };
            this.Controls.Add(logoVendor);

            // Label Versi (kanan bawah)
            lblVersion = new Label
            {
                Text = $"v1.0.0  © {DateTime.Now.Year} RSUD Anuntaloko",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.LightSlateGray,
                AutoSize = true,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                Location = new Point(this.ClientSize.Width - 280, this.ClientSize.Height - 35)
            };
            this.Controls.Add(lblVersion);

            // Tombol Minimize
            Button btnMinimize = new Button()
            {
                Size = new Size(32, 32),
                Location = new Point(this.ClientSize.Width - 90, 10),
                Text = "–",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                BackColor = Color.FromArgb(60, 80, 110),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Region = System.Drawing.Region.FromHrgn(
                    CreateRoundRectRgn(0, 0, 32, 32, 10, 10))
            };
            btnMinimize.FlatAppearance.BorderSize = 0;
            btnMinimize.Click += (s, e) => this.WindowState = FormWindowState.Minimized;
            this.Controls.Add(btnMinimize);

            // Tombol Close
            Button btnClose = new Button()
            {
                Size = new Size(32, 32),
                Location = new Point(this.ClientSize.Width - 45, 10),
                Text = "✕",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                BackColor = Color.FromArgb(180, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Region = System.Drawing.Region.FromHrgn(
                    CreateRoundRectRgn(0, 0, 32, 32, 10, 10))
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += BtnClose_Click;
            this.Controls.Add(btnClose);
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            var result = ShowConfirmDialog("Yakin ingin menutup aplikasi?");
            if (result == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private DialogResult ShowConfirmDialog(string message)
        {
            using (var overlay = new Components.OverlayForm(this))
            using (var dialog = new Form())
            {
                overlay.Show();

                dialog.Size = new Size(360, 160);
                dialog.FormBorderStyle = FormBorderStyle.None;
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.BackColor = Color.FromArgb(38, 55, 85);
                dialog.ForeColor = Color.White;
                dialog.TopMost = true;
                dialog.ShowInTaskbar = false;
                dialog.KeyPreview = true;

                dialog.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, dialog.Width, dialog.Height, 15, 15));

                dialog.KeyDown += (s, e) =>
                {
                    if (e.KeyCode == Keys.F4 && e.Modifiers == Keys.Alt)
                        e.Handled = true;
                };

                Label lbl = new Label()
                {
                    Text = message,
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Top,
                    Height = 70,
                    Font = new Font("Segoe UI", 11, FontStyle.Regular),
                    Padding = new Padding(10),
                    ForeColor = Color.FromArgb(230, 240, 255)
                };
                dialog.Controls.Add(lbl);

                Button btnYes = new Button()
                {
                    Text = "Yes",
                    DialogResult = DialogResult.Yes,
                    BackColor = Color.FromArgb(55, 180, 120),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    Size = new Size(100, 36),
                    FlatStyle = FlatStyle.Flat,
                    Location = new Point(55, 100)
                };
                btnYes.FlatAppearance.BorderSize = 0;
                btnYes.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btnYes.Width, btnYes.Height, 8, 8));

                Button btnNo = new Button()
                {
                    Text = "No",
                    DialogResult = DialogResult.No,
                    BackColor = Color.FromArgb(180, 60, 60),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    Size = new Size(100, 36),
                    FlatStyle = FlatStyle.Flat,
                    Location = new Point(200, 100)
                };
                btnNo.FlatAppearance.BorderSize = 0;
                btnNo.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btnNo.Width, btnNo.Height, 8, 8));

                dialog.Controls.Add(btnYes);
                dialog.Controls.Add(btnNo);

                return dialog.ShowDialog();
            }
        }

        private Image LoadEmbeddedImage(string resourceName)
        {
            var asm = typeof(TimerBlock).Assembly;
            using (Stream stream = asm.GetManifestResourceStream(resourceName))
            {
                return Image.FromStream(stream);
            }
        }

    }
}
