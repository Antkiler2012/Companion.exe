using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Diagnostics;
using Godot;

public partial class Win32API : Godot.Node
{
	public const uint SW_MINIMIZE = 2; 
	[DllImport("user32.dll", SetLastError = true)]
	private static extern bool SetForegroundWindow(IntPtr hWnd);
	
	[DllImport("user32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern int SystemParametersInfo(int uAction, int uParam, StringBuilder lpvParam, int fuWinIni);

	[DllImport("user32.dll")]
	private static extern bool ShowWindow(IntPtr hWnd, uint nCmdShow);
	
	[DllImport("user32.dll")]
	public static extern bool FlashWindowEx(ref FLASHWINFO pwfi);


	[StructLayout(LayoutKind.Sequential)]
	public struct FLASHWINFO
	{
		public uint cbSize;
		public IntPtr hwnd;
		public FlashFlags dwFlags;
		public uint uCount;
		public uint dwTimeout;
	}

	[Flags]
	public enum FlashFlags : uint
	{
		FLASHW_STOP = 0,
		FLASHW_CAPTION = 1,
		FLASHW_TRAY = 2,
		FLASHW_ALL = 3,
		FLASHW_TIMER = 4,
		FLASHW_TIMERNOFG = 12
	}


	private IntPtr GetWindowHandleByProcessName(string processName)
	{
		Process[] processes = Process.GetProcessesByName(processName);
		return processes.Length > 0 ? processes[0].MainWindowHandle : IntPtr.Zero;
	}

	public string GetDesktopWallpaperPath()
	{
		const int SPI_GETDESKWALLPAPER = 0x0073;
		const int MAX_PATH = 260;
		
		StringBuilder wallpaperPath = new StringBuilder(MAX_PATH);
		SystemParametersInfo(SPI_GETDESKWALLPAPER, MAX_PATH, wallpaperPath, 0);
		return wallpaperPath.ToString();
	}

	public void StartFlashing(string processName)
	{
		IntPtr hWnd = GetWindowHandleByProcessName(processName);
		
		FLASHWINFO fInfo = new FLASHWINFO();
		fInfo.cbSize = (uint)Marshal.SizeOf(fInfo);
		fInfo.hwnd = hWnd;
		fInfo.dwFlags = FlashFlags.FLASHW_ALL | FlashFlags.FLASHW_TIMERNOFG; 
		fInfo.uCount = uint.MaxValue;
		fInfo.dwTimeout = 0;

		FlashWindowEx(ref fInfo);
	}
	
	public void StealFocus()
	{
		IntPtr hWnd = Process.GetCurrentProcess().MainWindowHandle;
		SetForegroundWindow(hWnd);
	}
	
	public bool MinimizeExternalWindow(string processName)
	{
		IntPtr hWnd = GetWindowHandleByProcessName(processName); 
		return ShowWindow(hWnd, SW_MINIMIZE); 
	}
}
