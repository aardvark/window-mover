using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
    }
}