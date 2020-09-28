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

            Rect rect = getWSize(hWnd);
            Rect csize = getCSize(hWnd);

            var x = rect.Left + dx;
            var y = rect.Top;
            var nWidth = csize.Right;
            var nHeight = csize.Bottom;

            MoveWindowCall(hWnd, x, y, nWidth, nHeight);
        }

        private void MoveWindowY(int dy)
        {
            var item = appCbx.SelectedItem;

            IntPtr hWnd = GetMainWindowHandle(item);
            if (hWnd == IntPtr.Zero)
            {
                return;
            }

            Rect rect = getWSize(hWnd);
            Rect csize = getCSize(hWnd);

            var x = rect.Left;
            var y = rect.Top + dy;
            var nWidth = csize.Right;
            var nHeight = csize.Bottom;

            MoveWindowCall(hWnd, x, y, nWidth, nHeight);
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

        /*
         * Need to add next two to the new window size or it will shrink for some reason
         * probably something to do with GetClientRect return. It totally return not what
         * i am expecting
         */
        private readonly int move_window_magic_x = 16;
        private readonly int move_window_magic_y = 59;

        private bool MoveWindowCall(IntPtr hWnd, int x, int y, int nWidth, int nHeight)
        {
            return MoveWindow(hWnd, x, y, nWidth + move_window_magic_x, nHeight + move_window_magic_y, true);
        }

        private void AppCbx_OnDropDownClosed(object o, EventArgs e)
        {
            var item = appCbx.SelectedItem;

            IntPtr hWnd = GetMainWindowHandle(item);
            if (hWnd == IntPtr.Zero)
            {
                return;
            }

            Rect wsize = getWSize(hWnd);
            /*
             * Topleft (2202, 139) , BotRight(2982,618)
             */
            var rect = $"TopLeft:({wsize.Left}, {wsize.Top}) , BotRight:({wsize.Right},{wsize.Bottom})";

            Rect csize = getCSize(hWnd);
            /*
             * Client: 0, 0, 765, 422
             */
            var cRect = $"Height: {csize.Bottom}, Width: {csize.Right}";
            infoTbx.Text = $"{rect}\n{cRect}";
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