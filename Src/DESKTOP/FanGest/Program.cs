using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;
using LibreHardwareMonitor.Hardware;

namespace FanGest
{
    public class FanChannel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string NickName { get; set; }
        public string IconSymbol { get; set; }
        public IControl Control { get; set; }
        public ISensor ControlSensor { get; set; }
        public ISensor FanSensor { get; set; }

        public bool IsManual { get; set; }
        public float BiosBaselinePercent { get; set; }
        public float TargetPercent { get; set; }
        public float CurrentPercent { get; set; }
        public float CurrentRpm { get; set; }

        // UI Controls
        public Panel CardPanel { get; set; }
        public Label LblName { get; set; }
        public Label LblRpm { get; set; }
        public Label LblPercent { get; set; }
        public Label LblModeTag { get; set; }
        public TrackBar SliderPercent { get; set; }
        public CheckBox SwManualMode { get; set; }
        public Button Btn100 { get; set; }
        public Button BtnBios { get; set; }
    }

    public class MainForm : Form
    {
        // Palette matching Dark theme
        private readonly Color ColBgDark = Color.FromArgb(13, 17, 23);
        private readonly Color ColCardBg = Color.FromArgb(22, 27, 34);
        private readonly Color ColCardHeader = Color.FromArgb(33, 38, 45);
        private readonly Color ColBorder = Color.FromArgb(48, 54, 61);
        private readonly Color ColAccentCyan = Color.FromArgb(0, 210, 255);
        private readonly Color ColNvidiaGreen = Color.FromArgb(118, 185, 0);
        private readonly Color ColTextPrimary = Color.FromArgb(240, 246, 252);
        private readonly Color ColTextSecondary = Color.FromArgb(139, 148, 158);
        private readonly Color ColRed = Color.FromArgb(255, 71, 87);
        private readonly Color ColYellow = Color.FromArgb(234, 179, 8);

        private Computer computer;
        private List<FanChannel> channels = new List<FanChannel>();
        private System.Windows.Forms.Timer pollTimer;
        private System.Windows.Forms.Timer rampTimer;
        private NotifyIcon trayIcon;
        private ContextMenuStrip trayMenu;

        private Label lblMonitoringStatus;
        private FlowLayoutPanel flpCards;
        private bool isUpdatingUi = false;
        private bool isExitRequested = false;

        [STAThread]
        public static void Main()
        {
            if (!IsRunningAsAdmin())
            {
                try
                {
                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = Application.ExecutablePath,
                        UseShellExecute = true,
                        Verb = "runas"
                    };
                    Process.Start(psi);
                    return;
                }
                catch
                {
                    MessageBox.Show("FanGest requiere ejecutarse como Administrador para acceder a los controladores de la placa base (chip Nuvoton).", "Permisos Requeridos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        private static bool IsRunningAsAdmin()
        {
            try
            {
                using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
                {
                    WindowsPrincipal principal = new WindowsPrincipal(identity);
                    return principal.IsInRole(WindowsBuiltInRole.Administrator);
                }
            }
            catch
            {
                return false;
            }
        }

        public MainForm()
        {
            this.Text = "FanGest v1.0 • Control Turbo de Ventiladores (Zero-Latency)";
            this.Size = new Size(930, 430);
            this.MinimumSize = new Size(880, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = ColBgDark;
            this.ForeColor = ColTextPrimary;
            this.Font = new Font("Segoe UI", 9f, FontStyle.Regular);
            
            string icoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fan.ico");
            if (File.Exists(icoPath))
            {
                try { this.Icon = new Icon(icoPath); } catch { }
            }
            else
            {
                this.Icon = SystemIcons.Application;
            }

            InitTrayIcon();
            SetupUiLayout();
            InitHardware();

            // Setup smooth ramp timer (150ms step for gradual % acceleration like FanControl)
            rampTimer = new System.Windows.Forms.Timer { Interval = 150 };
            rampTimer.Tick += (s, e) => ProcessRampStep();
            rampTimer.Start();

            // Setup live sensor update timer (1000ms only when window is active)
            pollTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            pollTimer.Tick += (s, e) => PollHardwareSensors();

            this.Activated += (s, e) => StartPolling();
            this.Resize += (s, e) =>
            {
                if (this.WindowState == FormWindowState.Minimized)
                {
                    StopPolling();
                    this.Hide();
                    ShowTrayBalloon("FanGest en Segundo Plano", "Sondeo de hardware DETENIDO (0,00 ms de latencia DPC). Los ventiladores retienen su velocidad.");
                }
            };

            // Closing window via X: Minimizes to System Tray (Zero Latency background mode)
            this.FormClosing += (s, e) =>
            {
                if (!isExitRequested)
                {
                    e.Cancel = true;
                    StopPolling();
                    this.Hide();
                    ShowTrayBalloon("FanGest en Segundo Plano", "Minimizado al área de notificación. Sondeo detenido al 100% para máxima fluidez en juegos.");
                }
                else
                {
                    StopPolling();
                    if (rampTimer != null) rampTimer.Stop();
                    ReleaseAllToBios();
                    if (computer != null)
                    {
                        try { computer.Close(); } catch { }
                    }
                }
            };

            // Auto-trigger Turbo on startup with smooth gradual ramp
            TriggerAllTurboSmooth();
            StartPolling();
        }

        private void StartPolling()
        {
            if (this.WindowState != FormWindowState.Minimized && this.Visible)
            {
                if (lblMonitoringStatus != null)
                {
                    lblMonitoringStatus.Text = "🟢 MONITORIZANDO EN VIVO (Ventana Visible)";
                    lblMonitoringStatus.ForeColor = ColNvidiaGreen;
                }
                pollTimer.Start();
                PollHardwareSensors();
            }
        }

        private void StopPolling()
        {
            pollTimer.Stop();
            if (lblMonitoringStatus != null)
            {
                lblMonitoringStatus.Text = "⚪ REPOSO 0 LATENCIA (Sondeo Detenido en Bandeja)";
                lblMonitoringStatus.ForeColor = ColTextSecondary;
            }
        }

        private void InitTrayIcon()
        {
            trayMenu = new ContextMenuStrip();
            trayMenu.BackColor = ColCardBg;
            trayMenu.ForeColor = ColTextPrimary;
            trayMenu.RenderMode = ToolStripRenderMode.System;

            var itemOpen = new ToolStripMenuItem("🖥️ Mostrar Panel de Ventiladores", null, (s, e) => RestoreWindow());
            itemOpen.Font = new Font("Segoe UI", 9f, FontStyle.Bold);

            var itemTurbo = new ToolStripMenuItem("🌪️ Modo 100% Turbo (Todos)", null, (s, e) => TriggerAllTurboSmooth());
            itemTurbo.ForeColor = ColAccentCyan;

            var itemBios = new ToolStripMenuItem("🍃 Modo BIOS (Control Automático)", null, (s, e) => TriggerAllBiosSmooth());
            itemBios.ForeColor = ColNvidiaGreen;

            var itemSeparator = new ToolStripSeparator();
            var itemExit = new ToolStripMenuItem("❌ Salir y Restaurar BIOS", null, (s, e) =>
            {
                isExitRequested = true;
                this.Close();
            });
            itemExit.ForeColor = ColRed;

            trayMenu.Items.Add(itemOpen);
            trayMenu.Items.Add(new ToolStripSeparator());
            trayMenu.Items.Add(itemTurbo);
            trayMenu.Items.Add(itemBios);
            trayMenu.Items.Add(itemSeparator);
            trayMenu.Items.Add(itemExit);

            trayIcon = new NotifyIcon
            {
                Icon = this.Icon != null ? this.Icon : SystemIcons.Application,
                ContextMenuStrip = trayMenu,
                Text = "FanGest • Control de Ventiladores Turbo",
                Visible = true
            };

            trayIcon.DoubleClick += (s, e) => RestoreWindow();
        }

        private void RestoreWindow()
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.BringToFront();
            StartPolling();
        }

        private void ShowTrayBalloon(string title, string text)
        {
            try
            {
                trayIcon.ShowBalloonTip(3000, title, text, ToolTipIcon.Info);
            }
            catch { }
        }

        private void SetupUiLayout()
        {
            // 1. Top Header Panel
            Panel pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 90,
                BackColor = ColCardBg,
                Padding = new Padding(20, 10, 20, 10)
            };
            pnlHeader.Paint += (s, e) =>
            {
                using (Pen p = new Pen(ColBorder, 1))
                {
                    e.Graphics.DrawLine(p, 0, pnlHeader.Height - 1, pnlHeader.Width, pnlHeader.Height - 1);
                }
            };

            Label lblTitle = new Label
            {
                Text = "🌀 FanGest • Ventiladores Turbo (Trasero, Lateral, Delantero)",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = ColAccentCyan,
                Dock = DockStyle.Left,
                AutoSize = true,
                Padding = new Padding(0, 4, 0, 0)
            };

            lblMonitoringStatus = new Label
            {
                Text = "🟢 MONITORIZANDO EN VIVO",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = ColNvidiaGreen,
                Dock = DockStyle.Right,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleRight,
                Padding = new Padding(0, 6, 0, 0)
            };

            // Global Actions Row inside Header
            Panel pnlQuickActions = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 36,
                Padding = new Padding(0, 4, 0, 0)
            };

            Button btnAllTurbo = new Button
            {
                Text = "🌪️ TODOS AL 100% (TURBO)",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.Black,
                BackColor = ColAccentCyan,
                FlatStyle = FlatStyle.Flat,
                Width = 220,
                Dock = DockStyle.Left,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 10, 0)
            };
            btnAllTurbo.FlatAppearance.BorderSize = 0;
            btnAllTurbo.Click += (s, e) => TriggerAllTurboSmooth();

            Button btnAllBios = new Button
            {
                Text = "🍃 MODO BIOS (AUTOMÁTICO)",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.Black,
                BackColor = ColNvidiaGreen,
                FlatStyle = FlatStyle.Flat,
                Width = 240,
                Dock = DockStyle.Left,
                Cursor = Cursors.Hand,
                Margin = new Padding(10, 0, 0, 0)
            };
            btnAllBios.FlatAppearance.BorderSize = 0;
            btnAllBios.Click += (s, e) => TriggerAllBiosSmooth();

            Button btnMinimizeToTray = new Button
            {
                Text = "🔽 Systray (0 Latencia)",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = ColTextPrimary,
                BackColor = Color.FromArgb(32, 38, 46),
                FlatStyle = FlatStyle.Flat,
                Width = 180,
                Dock = DockStyle.Right,
                Cursor = Cursors.Hand
            };
            btnMinimizeToTray.FlatAppearance.BorderColor = ColBorder;
            btnMinimizeToTray.Click += (s, e) =>
            {
                StopPolling();
                this.Hide();
                ShowTrayBalloon("FanGest en Segundo Plano", "Turbo activo al 100% y sondeo de sensores detenido. Latencia DPC ultra-baja garantizada.");
            };

            Button btnExitRestore = new Button
            {
                Text = "❌ Salir y Restaurar BIOS",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(160, 30, 45),
                FlatStyle = FlatStyle.Flat,
                Width = 185,
                Dock = DockStyle.Right,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 8, 0)
            };
            btnExitRestore.FlatAppearance.BorderSize = 0;
            btnExitRestore.Click += (s, e) =>
            {
                isExitRequested = true;
                this.Close();
            };

            pnlQuickActions.Controls.Add(btnAllBios);
            pnlQuickActions.Controls.Add(new Panel { Dock = DockStyle.Left, Width = 10 });
            pnlQuickActions.Controls.Add(btnAllTurbo);
            pnlQuickActions.Controls.Add(btnExitRestore);
            pnlQuickActions.Controls.Add(new Panel { Dock = DockStyle.Right, Width = 8 });
            pnlQuickActions.Controls.Add(btnMinimizeToTray);

            pnlHeader.Controls.Add(lblMonitoringStatus);
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(pnlQuickActions);

            // 2. Center FlowLayout for Fan Cards
            flpCards = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = ColBgDark,
                Padding = new Padding(15),
                FlowDirection = FlowDirection.LeftToRight
            };

            // 3. Bottom Information Bar
            Panel pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 36,
                BackColor = ColCardBg,
                Padding = new Padding(15, 8, 15, 8)
            };
            Label lblInfo = new Label
            {
                Text = "💡 Modo Cero Latencia: Minimiza a Systray para mantener Turbo sin latencia. Al cerrar la ventana (X), se restaura la BIOS automáticamente.",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = ColTextSecondary,
                Dock = DockStyle.Fill
            };
            pnlBottom.Controls.Add(lblInfo);

            this.Controls.Add(flpCards);
            this.Controls.Add(pnlBottom);
            this.Controls.Add(pnlHeader);
        }

        private void InitHardware()
        {
            try
            {
                computer = new Computer
                {
                    IsCpuEnabled = false,
                    IsMotherboardEnabled = true,
                    IsGpuEnabled = false,
                    IsControllerEnabled = false,
                    IsMemoryEnabled = false,
                    IsStorageEnabled = false,
                    IsNetworkEnabled = false
                };
                computer.Open();

                // Defined Channel Mapping for the 3 Turbo Fans
                var channelDefinitions = new[]
                {
                    new { SearchCtrl = "control/2", SearchFan = "fan/2", Nick = "Trasero", Icon = "💨", DefaultBios = 95f },
                    new { SearchCtrl = "control/3", SearchFan = "fan/3", Nick = "Lateral", Icon = "💨", DefaultBios = 70f },
                    new { SearchCtrl = "control/5", SearchFan = "fan/5", Nick = "Delantero", Icon = "💨", DefaultBios = 70f }
                };

                List<ISensor> allSensors = new List<ISensor>();
                foreach (IHardware hw in computer.Hardware)
                {
                    try { hw.Update(); } catch { }
                    allSensors.AddRange(hw.Sensors);
                    foreach (IHardware sub in hw.SubHardware)
                    {
                        try { sub.Update(); } catch { }
                        allSensors.AddRange(sub.Sensors);
                    }
                }

                foreach (var def in channelDefinitions)
                {
                    ISensor ctrlSensor = allSensors.Find(s => s.Identifier.ToString().Contains(def.SearchCtrl) && s.SensorType == SensorType.Control);
                    ISensor fanSensor = allSensors.Find(s => s.Identifier.ToString().Contains(def.SearchFan) && s.SensorType == SensorType.Fan);

                    if (ctrlSensor != null || fanSensor != null)
                    {
                        float initPercent = (ctrlSensor != null && ctrlSensor.Value.HasValue) ? ctrlSensor.Value.Value : def.DefaultBios;
                        float initRpm = (fanSensor != null && fanSensor.Value.HasValue) ? fanSensor.Value.Value : 0f;

                        FanChannel ch = new FanChannel
                        {
                            Id = ctrlSensor != null ? ctrlSensor.Identifier.ToString() : def.SearchCtrl,
                            Name = ctrlSensor != null ? ctrlSensor.Name : def.Nick,
                            NickName = def.Nick,
                            IconSymbol = def.Icon,
                            Control = ctrlSensor != null ? ctrlSensor.Control : null,
                            ControlSensor = ctrlSensor,
                            FanSensor = fanSensor,
                            IsManual = true,
                            BiosBaselinePercent = def.DefaultBios,
                            TargetPercent = 100f,
                            CurrentPercent = initPercent,
                            CurrentRpm = initRpm
                        };

                        CreateFanCard(ch);
                        channels.Add(ch);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error inicializando hardware de ventiladores: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void CreateFanCard(FanChannel ch)
        {
            Panel card = new Panel
            {
                Width = 275,
                Height = 230,
                BackColor = ColCardBg,
                Margin = new Padding(8),
                Padding = new Padding(12)
            };
            card.Paint += (s, e) =>
            {
                using (Pen p = new Pen(ch.IsManual ? ColAccentCyan : ColBorder, ch.IsManual ? 2 : 1))
                {
                    e.Graphics.DrawRectangle(p, 0, 0, card.Width - 1, card.Height - 1);
                }
            };

            // Card Header: Icon + NickName + Mode Status
            Panel pnlCardHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 32
            };

            Label lblName = new Label
            {
                Text = string.Format("{0} {1}", ch.IconSymbol, ch.NickName),
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = ColTextPrimary,
                Dock = DockStyle.Left,
                AutoSize = true
            };

            CheckBox swManual = new CheckBox
            {
                Text = "Turbo",
                Checked = true,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = ColAccentCyan,
                Dock = DockStyle.Right,
                AutoSize = true,
                Cursor = Cursors.Hand
            };

            pnlCardHeader.Controls.Add(swManual);
            pnlCardHeader.Controls.Add(lblName);

            // Metrics Display (RPM + %)
            Panel pnlMetrics = new Panel
            {
                Dock = DockStyle.Top,
                Height = 52,
                Padding = new Padding(0, 4, 0, 4)
            };

            Label lblRpm = new Label
            {
                Text = ch.CurrentRpm > 0 ? string.Format("{0:N0} RPM", ch.CurrentRpm) : "--- RPM",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = ColAccentCyan,
                Dock = DockStyle.Left,
                AutoSize = true
            };

            Label lblPercent = new Label
            {
                Text = string.Format("{0:N0} %", ch.CurrentPercent),
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = ColNvidiaGreen,
                Dock = DockStyle.Right,
                AutoSize = true
            };

            pnlMetrics.Controls.Add(lblPercent);
            pnlMetrics.Controls.Add(lblRpm);

            // Mode Tag
            Label lblModeTag = new Label
            {
                Text = "⚡ Modo Turbo (Acelerando)",
                Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                ForeColor = ColAccentCyan,
                Dock = DockStyle.Top,
                Height = 18
            };

            // TrackBar Slider (0% - 100%)
            TrackBar slider = new TrackBar
            {
                Dock = DockStyle.Top,
                Minimum = 0,
                Maximum = 100,
                Value = (int)ch.CurrentPercent,
                TickFrequency = 10,
                Height = 32,
                Enabled = true,
                Cursor = Cursors.Hand
            };

            // Quick Buttons (100% / BIOS)
            Panel pnlBtns = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 32
            };

            Button btn100 = new Button
            {
                Text = "100% Turbo",
                Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                ForeColor = Color.Black,
                BackColor = ColAccentCyan,
                FlatStyle = FlatStyle.Flat,
                Width = 120,
                Dock = DockStyle.Left,
                Cursor = Cursors.Hand
            };
            btn100.FlatAppearance.BorderSize = 0;

            Button btnBios = new Button
            {
                Text = "🍃 BIOS Auto",
                Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                ForeColor = ColTextPrimary,
                BackColor = Color.FromArgb(32, 40, 48),
                FlatStyle = FlatStyle.Flat,
                Width = 120,
                Dock = DockStyle.Right,
                Cursor = Cursors.Hand
            };
            btnBios.FlatAppearance.BorderColor = ColBorder;

            pnlBtns.Controls.Add(btnBios);
            pnlBtns.Controls.Add(btn100);

            // Event Bindings
            swManual.CheckedChanged += (s, e) =>
            {
                if (isUpdatingUi) return;
                ch.IsManual = swManual.Checked;
                slider.Enabled = ch.IsManual;
                swManual.ForeColor = ch.IsManual ? ColAccentCyan : ColTextSecondary;

                if (ch.IsManual)
                {
                    ch.TargetPercent = 100f;
                    lblModeTag.Text = "⚡ Modo Turbo 100%";
                    lblModeTag.ForeColor = ColAccentCyan;
                }
                else
                {
                    ch.TargetPercent = ch.BiosBaselinePercent;
                    lblModeTag.Text = "🍃 Control Automático (BIOS)";
                    lblModeTag.ForeColor = ColTextSecondary;
                }
                card.Invalidate();
            };

            slider.Scroll += (s, e) =>
            {
                ch.IsManual = true;
                swManual.Checked = true;
                ch.TargetPercent = slider.Value;
                lblModeTag.Text = string.Format("⚡ Ajuste Manual {0}%", slider.Value);
                lblModeTag.ForeColor = ColAccentCyan;
            };

            btn100.Click += (s, e) =>
            {
                isUpdatingUi = true;
                ch.IsManual = true;
                swManual.Checked = true;
                swManual.ForeColor = ColAccentCyan;
                slider.Enabled = true;
                ch.TargetPercent = 100f;
                lblModeTag.Text = "⚡ Modo Turbo 100%";
                lblModeTag.ForeColor = ColAccentCyan;
                isUpdatingUi = false;
                card.Invalidate();
            };

            btnBios.Click += (s, e) =>
            {
                isUpdatingUi = true;
                ch.IsManual = false;
                swManual.Checked = false;
                swManual.ForeColor = ColTextSecondary;
                slider.Enabled = false;
                ch.TargetPercent = ch.BiosBaselinePercent;
                lblModeTag.Text = "🍃 Control Automático (BIOS)";
                lblModeTag.ForeColor = ColTextSecondary;
                isUpdatingUi = false;
                card.Invalidate();
            };

            ch.CardPanel = card;
            ch.LblName = lblName;
            ch.LblRpm = lblRpm;
            ch.LblPercent = lblPercent;
            ch.LblModeTag = lblModeTag;
            ch.SliderPercent = slider;
            ch.SwManualMode = swManual;
            ch.Btn100 = btn100;
            ch.BtnBios = btnBios;

            card.Controls.Add(pnlBtns);
            card.Controls.Add(slider);
            card.Controls.Add(lblModeTag);
            card.Controls.Add(pnlMetrics);
            card.Controls.Add(pnlCardHeader);

            flpCards.Controls.Add(card);
        }

        // Smooth gradual ramp step processing (Step-Up / Step-Down like FanControl)
        private void ProcessRampStep()
        {
            foreach (FanChannel ch in channels)
            {
                if (Math.Abs(ch.CurrentPercent - ch.TargetPercent) > 0.5f)
                {
                    // Ramp slope: 4% every 150ms (~25% per second smooth climb)
                    float step = 4.0f;
                    if (ch.CurrentPercent < ch.TargetPercent)
                    {
                        ch.CurrentPercent = Math.Min(ch.TargetPercent, ch.CurrentPercent + step);
                    }
                    else
                    {
                        ch.CurrentPercent = Math.Max(ch.TargetPercent, ch.CurrentPercent - step);
                    }

                    try
                    {
                        if (ch.Control != null)
                        {
                            ch.Control.SetSoftware(ch.CurrentPercent);
                        }
                    }
                    catch { }

                    if (ch.LblPercent != null)
                    {
                        ch.LblPercent.Text = string.Format("{0:N0} %", ch.CurrentPercent);
                    }
                    if (ch.SliderPercent != null && !ch.SliderPercent.Focused)
                    {
                        ch.SliderPercent.Value = (int)ch.CurrentPercent;
                    }
                }
                else
                {
                    // Target reached
                    if (!ch.IsManual)
                    {
                        try
                        {
                            if (ch.Control != null)
                            {
                                ch.Control.SetDefault();
                            }
                        }
                        catch { }
                    }
                }
            }
        }

        private void TriggerAllTurboSmooth()
        {
            isUpdatingUi = true;
            foreach (FanChannel ch in channels)
            {
                ch.IsManual = true;
                ch.TargetPercent = 100f;
                if (ch.SwManualMode != null)
                {
                    ch.SwManualMode.Checked = true;
                    ch.SwManualMode.ForeColor = ColAccentCyan;
                }
                if (ch.SliderPercent != null) ch.SliderPercent.Enabled = true;
                if (ch.LblModeTag != null)
                {
                    ch.LblModeTag.Text = "⚡ Modo Turbo 100%";
                    ch.LblModeTag.ForeColor = ColAccentCyan;
                }
                if (ch.CardPanel != null) ch.CardPanel.Invalidate();
            }
            isUpdatingUi = false;
        }

        private void TriggerAllBiosSmooth()
        {
            isUpdatingUi = true;
            foreach (FanChannel ch in channels)
            {
                ch.IsManual = false;
                ch.TargetPercent = ch.BiosBaselinePercent;
                if (ch.SwManualMode != null)
                {
                    ch.SwManualMode.Checked = false;
                    ch.SwManualMode.ForeColor = ColTextSecondary;
                }
                if (ch.SliderPercent != null) ch.SliderPercent.Enabled = false;
                if (ch.LblModeTag != null)
                {
                    ch.LblModeTag.Text = "🍃 Control Automático (BIOS)";
                    ch.LblModeTag.ForeColor = ColTextSecondary;
                }
                if (ch.CardPanel != null) ch.CardPanel.Invalidate();
            }
            isUpdatingUi = false;
        }

        private void ReleaseAllToBios()
        {
            foreach (FanChannel ch in channels)
            {
                try
                {
                    if (ch.Control != null)
                    {
                        // Set back to BIOS baseline percent and release default
                        ch.Control.SetSoftware(ch.BiosBaselinePercent);
                        ch.Control.SetDefault();
                    }
                }
                catch { }
            }
        }

        private void PollHardwareSensors()
        {
            if (computer == null || this.WindowState == FormWindowState.Minimized || !this.Visible) return;

            try
            {
                foreach (IHardware hw in computer.Hardware)
                {
                    try { hw.Update(); } catch { }
                    foreach (IHardware sub in hw.SubHardware)
                    {
                        try { sub.Update(); } catch { }
                    }
                }

                foreach (FanChannel ch in channels)
                {
                    if (ch.FanSensor != null && ch.FanSensor.Value.HasValue)
                    {
                        ch.CurrentRpm = ch.FanSensor.Value.Value;
                        if (ch.LblRpm != null)
                            ch.LblRpm.Text = string.Format("{0:N0} RPM", ch.CurrentRpm);
                    }
                }
            }
            catch { }
        }
    }
}
