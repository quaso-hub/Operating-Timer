using System.Diagnostics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
namespace Operating_Timer.Components
{
    public class TimerBlock : Panel
    {
        private Label lblTitle;
        private Label lblTimer;
        private Label lblStatus;
        private Panel ledIndicator;
        private Button btnStartPause;
        private Button btnReset;
        private Timer timer;
        private Timer blinkTimer;
        private bool ledVisible = true;

        private TimeSpan elapsed;
        private bool isRunning;

        private string saveKey;
        private string savePath = "timer_state.json";
        private Timer autoSaveTimer;
        private static readonly object fileLock = new object(); 

        public TimerBlock(string title)
        {
            this.Size = new Size(900, 560);
            this.BackColor = Color.FromArgb(20, 40, 70);
            this.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, this.Width, this.Height, 20, 20));

            lblTitle = new Label()
            {
                Text = title.ToUpper(),
                Font = new Font("Segoe UI", 45F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true
            };

            lblTimer = new Label()
            {
                Text = "00:00:00",
                Font = new Font("Consolas", 130, FontStyle.Bold),
                ForeColor = Color.LightBlue,
                AutoSize = true
            };

            ledIndicator = new Panel()
            {
                Size = new Size(20, 20),
                BackColor = Color.Red
            };
            ledIndicator.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, 20, 20, 20, 20));

            lblStatus = new Label()
            {
                Text = "STOPPED",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.LightGray,
                AutoSize = true
            };

            btnStartPause = new Button()
            {
                Text = "START",
                Font = new Font("Segoe UI", 26F, FontStyle.Bold),
                Size = new Size(180, 70),
                BackColor = Color.FromArgb(30, 60, 100),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnStartPause.FlatAppearance.BorderSize = 0;
            btnStartPause.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btnStartPause.Width, btnStartPause.Height, 20, 20));
            btnStartPause.Click += ToggleTimer;

            btnReset = new Button()
            {
                Text = "RESET",
                Font = new Font("Segoe UI", 26F, FontStyle.Bold),
                Size = new Size(180, 70),
                BackColor = Color.Maroon,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnReset.FlatAppearance.BorderSize = 0;
            btnReset.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btnReset.Width, btnReset.Height, 20, 20));
            btnReset.Click += (s, e) => ResetTimer();

            this.Controls.Add(lblTitle);
            this.Controls.Add(lblTimer);
            this.Controls.Add(ledIndicator);
            this.Controls.Add(lblStatus);
            this.Controls.Add(btnStartPause);
            this.Controls.Add(btnReset);

            LayoutComponents();
            this.Resize += (s, e) => LayoutComponents();

            saveKey = title.ToLower();
            LoadSavedTime();

            timer = new Timer();
            timer.Interval = 1000;
            timer.Tick += (s, e) =>
            {
                elapsed = elapsed.Add(TimeSpan.FromSeconds(1));
                string format = elapsed.TotalHours >= 100 ? "hhh\\:mm\\:ss" : "hh\\:mm\\:ss";
                lblTimer.Text = elapsed.ToString(format);
                LayoutComponents();
            };

            blinkTimer = new Timer();
            blinkTimer.Interval = 1000;
            blinkTimer.Tick += (s, e) =>
            {
                if (isRunning)
                {
                    ledVisible = !ledVisible;
                    ledIndicator.Visible = ledVisible;
                }
                else
                {
                    ledIndicator.Visible = true;
                }
            };
            blinkTimer.Start();

            autoSaveTimer = new Timer();
            autoSaveTimer.Interval = 60000; 
            autoSaveTimer.Tick += (s, e) =>
            {
                if (isRunning) SaveTime();
            };
            autoSaveTimer.Start();
        }

        private void LayoutComponents()
        {
            int centerX = this.Width / 2;
            lblTitle.Location = new Point(centerX - lblTitle.Width / 2, 20);
            lblTimer.Location = new Point(centerX - lblTimer.Width / 2, 140);

            ledIndicator.Location = new Point(centerX - lblStatus.Width / 2 - 30, lblTimer.Bottom + 30);
            lblStatus.Location = new Point(ledIndicator.Right + 10, lblTimer.Bottom + 26);

            int buttonY = lblStatus.Bottom + 30;
            btnStartPause.Location = new Point(centerX - btnStartPause.Width - 20, buttonY);
            btnReset.Location = new Point(centerX + 20, buttonY);
        }

        private void ToggleTimer(object sender, EventArgs e)
        {
            if (!isRunning)
            {
                isRunning = true;
                btnStartPause.Text = "PAUSE";
                btnStartPause.BackColor = Color.Orange;
                lblStatus.Text = "RUNNING";
                lblStatus.ForeColor = Color.Lime;
                ledIndicator.BackColor = Color.Lime;
                timer.Start();
            }
            else
            {
                isRunning = false;
                btnStartPause.Text = "START";
                btnStartPause.BackColor = Color.FromArgb(30, 60, 100);
                lblStatus.Text = "PAUSED";
                lblStatus.ForeColor = Color.Gold;
                ledIndicator.BackColor = Color.Gold;
                timer.Stop();
            }
        }

        private void ResetTimer()
        {
            timer.Stop();
            isRunning = false;
            elapsed = TimeSpan.Zero;
            lblTimer.Text = "00:00:00";
            btnStartPause.Text = "START";
            btnStartPause.BackColor = Color.FromArgb(30, 60, 100);
            lblStatus.Text = "STOPPED";
            lblStatus.ForeColor = Color.LightGray;
            ledIndicator.BackColor = Color.Red;
            SaveTime();
            LayoutComponents();
        }

        private void SaveTime()
        {
            try
            {
                lock (fileLock)
                {
                    Dictionary<string, int> data = new Dictionary<string, int>();

                    if (File.Exists(savePath))
                    {
                        string json = File.ReadAllText(savePath);
                        if (!string.IsNullOrWhiteSpace(json))
                        {
                            data = JsonConvert.DeserializeObject<Dictionary<string, int>>(json);
                        }
                    }

                    data[saveKey] = (int)elapsed.TotalSeconds;
                    File.WriteAllText(savePath, JsonConvert.SerializeObject(data, Formatting.Indented));
                }
            }
            catch { }
        }

        private void LoadSavedTime()
        {
            try
            {
                if (File.Exists(savePath))
                {
                    string json = File.ReadAllText(savePath);
                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        var data = JsonConvert.DeserializeObject<Dictionary<string, int>>(json);
                        if (data != null && data.ContainsKey(saveKey))
                        {
                            elapsed = TimeSpan.FromSeconds(data[saveKey]);
                            string format = elapsed.TotalHours >= 100 ? "hhh\\:mm\\:ss" : "hh\\:mm\\:ss";
                            lblTimer.Text = elapsed.ToString(format);
                        }
                    }
                }
            }
            catch
            {
                elapsed = TimeSpan.Zero;
                lblTimer.Text = "00:00:00";
            }
        }

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(
            int nLeftRect, int nTopRect, int nRightRect,
            int nBottomRect, int nWidthEllipse, int nHeightEllipse);
    }
}
