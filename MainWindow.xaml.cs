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
            if (!(appCbx.SelectedItem is string s)) return;

            var pId = int.Parse(s.Substring(0, s.IndexOf(":", StringComparison.Ordinal)));
            ShowWindow(Process.GetProcessById(pId).MainWindowHandle, 6);
        }

        private void MaxBtn_OnClick(object sender, RoutedEventArgs e)
        {
            if (!(appCbx.SelectedItem is string s)) return;

            var pId = int.Parse(s.Substring(0, s.IndexOf(":", StringComparison.Ordinal)));
            ShowWindow(Process.GetProcessById(pId).MainWindowHandle, 3);
        }


        private void MoveWindowX(int dx)
        {
            if (!(appCbx.SelectedItem is string s)) return;

            var pId = int.Parse(s.Substring(0, s.IndexOf(":", StringComparison.Ordinal)));
            IntPtr hWnd = Process.GetProcessById(pId).MainWindowHandle;

            if (hWnd == IntPtr.Zero) return;

            var windowR = GetWindowRect(hWnd);

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

        /* Move to the corner button hadnlers*/
        private void cornUlBtn_OnClick(object sender, RoutedEventArgs e)
        {
            /* To move window to the upper left corner and take 1/4 of the screen*/
            var item = appCbx.SelectedItem;

            IntPtr hWnd = GetMainWindowHandle(item);
            if (hWnd == IntPtr.Zero)
            {
                return;
            }

            Rect windowR = GetWindowRect(hWnd);
            var info = GetMonitorSize(windowR);

            var cWidth = info.rcWork.Right / 2;
            var cHeight = info.rcWork.Bottom / 2;

            /*Upper left of the screen is always 0,0*/
            MoveWindow(hWnd, 0, 0, cWidth, cHeight, true);
        }

        private static MONITORINFOEX GetMonitorSize(Rect windowR)
        {
            /*1. Get a full screen size*/
            /*
             * Size of the primary monitor: GetSystemMetrics SM_CXSCREEN / SM_CYSCREEN
             *                              GetDeviceCaps can also be used
             * Size of all monitors (combined): GetSystemMetrics SM_CX/YVIRTUALSCREEN
             * Size of work area (screen excluding taskbar and other docked bars) on primary monitor: SystemParametersInfo SPI_GETWORKAREA
             * Size of a specific monitor (work area and "screen"): GetMonitorInfo
             */
            var monitorPoint = new POINTSTRUCT(windowR.Left, windowR.Top);
            int MONITOR_DEFAULT_TO_PRIMARY = 1;
            IntPtr ptr = MonitorFromPoint(monitorPoint, MONITOR_DEFAULT_TO_PRIMARY);
            MONITORINFOEX info = new MONITORINFOEX();
            GetMonitorInfo(ptr, info);
            return info;
        }

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern bool GetMonitorInfo(IntPtr hmonitor, [In, Out] MONITORINFOEX info);

        [DllImport("User32.dll", ExactSpelling = true)]
        public static extern IntPtr MonitorFromPoint(POINTSTRUCT pt, int flags);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 4)]
        public class MONITORINFOEX
        {
            public int cbSize = Marshal.SizeOf(typeof(MONITORINFOEX));
            public Rect rcMonitor = new Rect();
            public Rect rcWork = new Rect();
            public int dwFlags = 0;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public char[] szDevice = new char[32];
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINTSTRUCT
        {
            public int x;
            public int y;

            public POINTSTRUCT(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }

        private void cornUrBtn_OnClick(object sender, RoutedEventArgs e)
        {
            /* To move window to the upper right corner and take 1/4 of the screen*/
            var item = appCbx.SelectedItem;

            IntPtr hWnd = GetMainWindowHandle(item);
            if (hWnd == IntPtr.Zero)
            {
                return;
            }

            Rect windowR = GetWindowRect(hWnd);
            var info = GetMonitorSize(windowR);

            var x = info.rcWork.Right / 2;
            var y = 0;

            var cWidth = info.rcWork.Right / 2;
            var cHeight = info.rcWork.Bottom / 2;

            MoveWindow(hWnd, x, y, cWidth, cHeight, true);
        }

        private void cornDlBtn_OnClick(object sender, RoutedEventArgs e)
        {
            /* To move window to the lower left corner and take 1/4 of the screen*/
            var item = appCbx.SelectedItem;

            IntPtr hWnd = GetMainWindowHandle(item);
            if (hWnd == IntPtr.Zero)
            {
                return;
            }

            Rect windowR = GetWindowRect(hWnd);
            var info = GetMonitorSize(windowR);

            var x = 0;
            var y = info.rcWork.Bottom / 2;

            var cWidth = info.rcWork.Right / 2;
            var cHeight = info.rcWork.Bottom / 2;

            MoveWindow(hWnd, x, y, cWidth, cHeight, true);
        }

        private void cornDrBtn_OnClick(object sender, RoutedEventArgs e)
        {
            /* To move window to the lower right corner and take 1/4 of the screen*/
            var item = appCbx.SelectedItem;

            IntPtr hWnd = GetMainWindowHandle(item);
            if (hWnd == IntPtr.Zero)
            {
                return;
            }

            Rect windowR = GetWindowRect(hWnd);
            var info = GetMonitorSize(windowR);

            var x = info.rcWork.Right / 2;
            var y = info.rcWork.Bottom / 2;

            var cWidth = info.rcWork.Right / 2;
            var cHeight = info.rcWork.Bottom / 2;

            MoveWindow(hWnd, x, y, cWidth, cHeight, true);
        }
    }
}