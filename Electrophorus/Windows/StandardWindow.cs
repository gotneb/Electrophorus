﻿using System;
using System.Windows.Forms;

namespace Electrophorus
{
    public partial class StandardWindow : Form
    {
        // Move a janela
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        //  =======================

        private bool _isMaximized;
        //private readonly Image _imgContrained = Image.FromFile(@"..\..\..\..\Imagens\Contrained.png");
        //private readonly Image _imgExpanded = Image.FromFile(@"..\..\..\..\Imagens\Expanded.png");

        public StandardWindow()
        {
            InitializeComponent();
           _isMaximized = false;
        }

        public StandardWindow(bool maximized) : this()
        {
            if (maximized)
            {
                _isMaximized = maximized;
                //btnMaximize.BackgroundImage = _imgContrained;
                WindowState = FormWindowState.Maximized;
            }
        }

        // Move a janela
        private void panTop_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e) => Application.Exit();

        private void btnMinimeze_Click(object sender, EventArgs e) => WindowState = FormWindowState.Minimized;

        private void btnMaximize_Click(object sender, EventArgs e)
        {
            if (!_isMaximized) {
                WindowState = FormWindowState.Maximized;
                //btnMaximize.BackgroundImage = _imgContrained;
            }
            else { 
                WindowState = FormWindowState.Normal;
                //btnMaximize.BackgroundImage = _imgExpanded;
            }
            _isMaximized = !_isMaximized;
        }
    }
}
