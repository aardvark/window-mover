using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace window_mover
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public ComboBox AppCbx
        {
            get => appCbx;
            set => appCbx = value;
        }

        public MainWindow()
        {
            InitializeComponent();
            ProcessesWithWindows();
        }

        private void ProcessesWithWindows()
        {
            foreach (var process in Process.GetProcesses())
            {
                if (process.MainWindowHandle != IntPtr.Zero)
                {
                    appCbx.Items.Add($"{process.Id}:{process.ProcessName}, {process.MainWindowTitle}");
                }
            }
        }


        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(IntPtr hWnd, int x, int y, int nWidth, int nHeight, bool bRepaint);


        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool GetWindowRect(IntPtr hWnd, out Rect lpRect);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool GetClientRect(IntPtr hWnd, out Rect lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            public int Left; // x position of upper-left corner
            public int Top; // y position of upper-left corner
            public int Right; // x position of lower-right corner
            public int Bottom; // y position of lower-right corner
        }

        private void MinBtn_OnClick(object sender, RoutedEventArgs e)
        {
            var selected = appCbx.SelectedItem.ToString();
            var pId = int.Parse(selected.Substring(0, selected.IndexOf(":", StringComparison.Ordinal)));
            IntPtr hWnd = Process.GetProcessById(pId).MainWindowHandle;
            ShowWindow(hWnd, 6);
        }

        private void MaxBtn_OnClick(object sender, RoutedEventArgs e)
        {
            var selected = appCbx.SelectedItem.ToString();
            var pId = int.Parse(selected.Substring(0, selected.IndexOf(":", StringComparison.Ordinal)));
            IntPtr hWnd = Process.GetProcessById(pId).MainWindowHandle;
            ShowWindow(hWnd, 3);
        }

        private void UpBtn_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void LeftBtn_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void RightBtn_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void DownBtn_OnClick(object sender, RoutedEventArgs e)
        {
            var selected = appCbx.SelectedItem.ToString();
            var pId = int.Parse(selected.Substring(0, selected.IndexOf(":", StringComparison.Ordinal)));
            IntPtr hWnd = Process.GetProcessById(pId).MainWindowHandle;
            Rect rect = getWSize(hWnd);
            Rect csize = getCSize(hWnd);
            MoveWindow(hWnd, rect.Left, rect.Top, csize.Right, csize.Bottom, true);
        }

        private void AppCbx_OnDropDownClosed(object o, EventArgs e)
        {
            var selected = appCbx.SelectedItem.ToString();
            var pId = int.Parse(selected.Substring(0, selected.IndexOf(":", StringComparison.Ordinal)));
            IntPtr hWnd = Process.GetProcessById(pId).MainWindowHandle;
            Rect wsize = getWSize(hWnd);
            /*
             * Topleft (2202, 139) , BotRight(2982,618)
             */
            var _rect = $"TopLeft:({wsize.Left}, {wsize.Top}) , BotRight:({wsize.Right},{wsize.Bottom})";
            
            Rect csize = getCSize(hWnd);
            /*
             * Client: 0, 0, 765, 422
             */
            var cRect = $"Height: {csize.Bottom}, Width: {csize.Right}";
            infoTbx.Text = $"{_rect}\n{cRect}";
        }


        private Rect getWSize(IntPtr hWnd)
        {
            if (!GetWindowRect(hWnd, out Rect rct))
            {
                MessageBox.Show("ERROR");
            }

            return rct;
        }

        private Rect getCSize(IntPtr hWnd)
        {
            if (!GetClientRect(hWnd, out Rect rct))
            {
                MessageBox.Show("ERROR");
            }

            return rct;
        }
    }
}