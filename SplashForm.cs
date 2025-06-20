using Operating_Timer.Components;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Operating_Timer
{
    public partial class SplashForm : Form
    {
        private Timer splashTimer;
        private RoundedProgressBar loadingBar;
        private Label lblTitle, lblVendor, lblVersion, lblRS;
        private PictureBox logoBox;
        private int progress = 0;

        private Timer fadeTimer;

        public SplashForm()
        {
            InitializeComponent();
            this.AutoScaleMode = AutoScaleMode.Dpi;

            this.Opacity = 0;

            fadeTimer = new Timer();
            fadeTimer.Interval = 20;
            fadeTimer.Tick += FadeTimer_Tick;
            fadeTimer.Start();

            // Form dasar
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(25, 40, 65);
            this.ClientSize = new Size(600, 360);
            this.TopMost = true;
            this.ShowInTaskbar = false;
            this.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, this.Width, this.Height, 30, 30));

            // Logo RS
            logoBox = new PictureBox()
            {
                SizeMode = PictureBoxSizeMode.Zoom,
                Size = new Size(120, 120),
                Location = new Point((this.Width - 120) / 2, 30),
                Anchor = AnchorStyles.Top
            };

            try
            {
                logoBox.Image = LoadEmbeddedImage("Operating_Timer.Resources.Icons.logo_rsud.png");
            }
            catch
            {
                logoBox.BackColor = Color.DarkGray;
                logoBox.BorderStyle = BorderStyle.FixedSingle;

                Label lblFallback = new Label()
                {
                    Text = "LOGO",
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    ForeColor = Color.White,
                    AutoSize = true,
                    Location = new Point((logoBox.Width - 45) / 2, (logoBox.Height - 20) / 2)
                };
                logoBox.Controls.Add(lblFallback);
            }
            this.Controls.Add(logoBox);


            // RS Name
            lblRS = new Label()
            {
                Text = "RSUD Anuntaloko Parigi",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.LightSkyBlue,
                AutoSize = true,
                Anchor = AnchorStyles.Top
            };
            this.Controls.Add(lblRS);

            // App Title
            lblTitle = new Label()
            {
                Text = "Operating Timer",
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Anchor = AnchorStyles.Top
            };
            this.Controls.Add(lblTitle);

            // Vendor
            lblVendor = new Label()
            {
                Text = "PT Teknomed Indo Timur | Elfatech",
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                ForeColor = Color.Gainsboro,
                AutoSize = true,
                Anchor = AnchorStyles.Top
            };
            this.Controls.Add(lblVendor);

            // Version
            lblVersion = new Label()
            {
                Text = "v1.0.0",
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Color.LightGray,
                AutoSize = true,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            this.Controls.Add(lblVersion);

            // Loading Bar
            loadingBar = new RoundedProgressBar()
            {
                Size = new Size(350, 10),
                Location = new Point((this.Width - 350) / 2, this.Height - 45),
                BarColor = Color.MediumSpringGreen,
                BackBarColor = Color.FromArgb(35, 55, 90),
                Value = 0,
                Anchor = AnchorStyles.Bottom
            };
            this.Controls.Add(loadingBar);

            // Timer
            splashTimer = new Timer();
            splashTimer.Interval = 20;
            progress += 5;
            splashTimer.Tick += SplashTimer_Tick;
            splashTimer.Start();

            this.Load += SplashForm_Load;
            this.Resize += (s, e) => UpdateLayoutPositions();

            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        }

        private void FadeTimer_Tick(object sender, EventArgs e)
        {
            if (this.Opacity < 1)
                this.Opacity += 0.05;
            else
                fadeTimer.Stop();
        }

        private void SplashForm_Load(object sender, EventArgs e)
        {
            UpdateLayoutPositions();
        }

        private void UpdateLayoutPositions()
        {
            lblRS.Location = new Point((this.Width - lblRS.Width) / 2, 160);
            lblTitle.Location = new Point((this.Width - lblTitle.Width) / 2, 195);
            lblVendor.Location = new Point((this.Width - lblVendor.Width) / 2, 230);
            loadingBar.Location = new Point((this.Width - loadingBar.Width) / 2, this.Height - 45);
            lblVersion.Location = new Point(this.Width - lblVersion.Width - 20, this.Height - 25);
        }

        private void SplashTimer_Tick(object sender, EventArgs e)
        {
            progress += 2;
            loadingBar.Value = progress;

            if (progress >= 100)
            {
                splashTimer.Stop();
                this.DialogResult = DialogResult.OK;
            }
        }

        private Image LoadEmbeddedImage(string resourceName)
        {
            var asm = typeof(SplashForm).Assembly;
            using (Stream stream = asm.GetManifestResourceStream(resourceName))
            {
                return Image.FromStream(stream);
            }
        }

        [System.Runtime.InteropServices.DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(
            int nLeftRect, int nTopRect, int nRightRect, int nBottomRect,
            int nWidthEllipse, int nHeightEllipse);
    }
}
