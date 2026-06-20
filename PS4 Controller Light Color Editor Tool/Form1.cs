using HidSharp;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Timers;
using System.Windows.Forms;

namespace PS4_Controller_Light_Color_Editor_Tool
{
    public partial class Form1 : Form
    {
        // Hacemos los campos nullable
        HidDevice? ds4Device;
        HidStream? ds4Stream;
        System.Timers.Timer? rainbowTimer;
        int hue = 0;

        public Form1()
        {
            InitializeComponent();
            // Ahora no necesitamos null! porque son nullable
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            panel1.BackColor = Color.Black;
            ConectarDualShock4();
        }

        private void ConectarDualShock4()
        {
            var deviceList = DeviceList.Local;

            foreach (var device in deviceList.GetHidDevices())
            {
                if (device.VendorID == 0x054C) // Sony
                {
                    if (device.TryOpen(out ds4Stream))
                    {
                        ds4Device = device;
                        break;
                    }
                }
            }

            if (ds4Stream == null)
            {
                MessageBox.Show("No PS4 controller detected.");
            }
        }

        // Naming convention corregida: Button1_Click (mayúscula inicial)
        private void Button1_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog
            {
                FullOpen = true
            };

            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                Color color = colorDialog.Color;
                panel1.BackColor = color;

                EnviarColorAlMando(color);
            }
        }

        private void ButtonRainbow_Click(object sender, EventArgs e)
        {
            rainbowTimer?.Stop();
            rainbowTimer?.Dispose();

            rainbowTimer = new System.Timers.Timer(50); // 20 FPS
            rainbowTimer.Elapsed += (s, ev) =>
            {
                hue += 2;
                if (hue > 360) hue = 0;
                Color color = ColorFromHSV(hue, 1, 1);
                panel1.Invoke((MethodInvoker)(() => panel1.BackColor = color));
                EnviarColorAlMando(color);
            };
            rainbowTimer.Start();
        }

        private void EnviarColorAlMando(Color color)
        {
            if (ds4Stream == null) return;

            byte[] report = new byte[32];
            report[0] = 0x05; // Report ID
            report[1] = 0xFF; // Flags
            report[6] = color.R;
            report[7] = color.G;
            report[8] = color.B;

            try
            {
                ds4Stream.Write(report);
            }
            catch { /* ignora errores pequeños */ }
        }

        public Color ColorFromHSV(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);
            value = value * 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            return hi switch
            {
                0 => Color.FromArgb(v, t, p),
                1 => Color.FromArgb(q, v, p),
                2 => Color.FromArgb(p, v, t),
                3 => Color.FromArgb(p, q, v),
                4 => Color.FromArgb(t, p, v),
                _ => Color.FromArgb(v, p, q),
            };
        }

        private void LinkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://lukiblokck.github.io",
                UseShellExecute = true
            });
        }

        private void LinkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://creativecommons.org/licenses/by-nc-sa/4.0/",
                UseShellExecute = true
            });
        }
    }
}
