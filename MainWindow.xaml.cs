using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;

namespace window_mover
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
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

        private static IntPtr GetMainWindowHandle(object item)
        {
            if (item == null)
            {
                return IntPtr.Zero;
            }

            var selected = item.ToString();
            var pId = int.Parse(selected.Substring(0, selected.IndexOf(":", StringComparison.Ordinal)));
            IntPtr hWnd = Process.GetProcessById(pId).MainWindowHandle;
            return hWnd;
        }

        private void MinBtn_OnClick(object sender, RoutedEventArgs e)
        {
            var item = appCbx.SelectedItem;
            IntPtr hWnd = GetMainWindowHandle(item);
            if (hWnd == IntPtr.Zero)
            {
                return;
            }

            ShowWindow(hWnd, 6);
        }

        private void MaxBtn_OnClick(object sender, RoutedEventArgs e)
        {
            var item = appCbx.SelectedItem;
            IntPtr hWnd = GetMainWindowHandle(item);
            if (hWnd == IntPtr.Zero)
            {
                return;
            }

            ShowWindow(hWnd, 3);
        }


        private void MoveWindowX(int dx)
        {
            var item = appCbx.SelectedItem;

            IntPtr hWnd = GetMainWindowHandle(item);
            if (hWnd == IntPtr.Zero)
            {
                return;
            }

            Rect windowR = GetWindowRect(hWnd);

            var x = windowR.Left + dx;
            var y = windowR.Top;

            var cWidth = windowR.Right - windowR.Left;
            var cHeight = windowR.Bottom - windowR.Top;

            MoveWindow(hWnd, x, y, cWidth, cHeight, true);
        }

        private void MoveWindowY(int dy)
        {
            var item = appCbx.SelectedItem;

            IntPtr hWnd = GetMainWindowHandle(item);
            if (hWnd == IntPtr.Zero)
            {
                return;
            }

            Rect windowR = GetWindowRect(hWnd);
            
            var x = windowR.Left;
            var y = windowR.Top + dy;

            var cWidth = windowR.Right - windowR.Left;
            var cHeight = windowR.Bottom - windowR.Top;
            
            MoveWindow(hWnd, x, y, cWidth, cHeight, true);
        }

        private void UpBtn_OnClick(object sender, RoutedEventArgs e)
        {
            MoveWindowY(-10);
        }

        private void DownBtn_OnClick(object sender, RoutedEventArgs e)
        {
            MoveWindowY(10);
        }


        private void LeftBtn_OnClick(object sender, RoutedEventArgs e)
        {
            MoveWindowX(-10);
        }

        private void RightBtn_OnClick(object sender, RoutedEventArgs e)
        {
            MoveWindowX(10);
        }

        private void AppCbx_OnDropDownClosed(object o, EventArgs e)
        {
            var item = appCbx.SelectedItem;

            IntPtr hWnd = GetMainWindowHandle(item);
            if (hWnd == IntPtr.Zero)
            {
                return;
            }

            Rect wsize = GetWindowRect(hWnd);
            /*
             * Topleft (2202, 139) , BotRight(2982,618)
             */
            var rect = $"TopLeft:({wsize.Left}, {wsize.Top}) , BotRight:({wsize.Right},{wsize.Bottom})";

            Rect csize = GetClientRect(hWnd);
            /*
             * Client: 0, 0, 765, 422
             */
            var cRect = $"Height: {csize.Bottom}, Width: {csize.Right}";
            infoTbx.Text = $"{rect}\n{cRect}";
        }


        private Rect GetWindowRect(IntPtr hWnd)
        {
            if (!GetWindowRect(hWnd, out Rect rct))
            {
                MessageBox.Show("ERROR");
            }

            return rct;
        }

        private Rect GetClientRect(IntPtr hWnd)
        {
            if (!GetClientRect(hWnd, out Rect rct))
            {
                MessageBox.Show("ERROR");
            }

            return rct;
        }
    }
}