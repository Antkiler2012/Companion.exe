using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Security.Principal;
using Godot;

public partial class Win32API : Node
{
	public const uint SW_MINIMIZE = 2;
	public const uint SW_RESTORE = 9;

	[DllImport("user32.dll", SetLastError = true)]
	private static extern bool SetForegroundWindow(IntPtr hWnd);

	[DllImport("user32.dll")]
	private static extern bool ShowWindow(IntPtr hWnd, uint nCmdShow);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern int SystemParametersInfo(int uAction, int uParam, StringBuilder lpvParam, int fuWinIni);

	[DllImport("user32.dll")]
	private static extern IntPtr GetActiveWindow();

	[DllImport("user32.dll")]
	private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

	[DllImport("user32.dll")]
	private static extern short VkKeyScan(char ch);

	private const int KEYEVENTF_KEYUP = 0x0002;

	private void SendKeysWin32(string text, int delayMs = 100)
	{
		foreach (char c in text)
		{
			short vk = VkKeyScan(c);
			byte vkCode = (byte)(vk & 0xff);
			keybd_event(vkCode, 0, 0, UIntPtr.Zero);
			keybd_event(vkCode, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
			Thread.Sleep(delayMs);
		}
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

	public void OpenNotepadAndTypeHello()
	{
		IntPtr gameHandle = GetActiveWindow();
		Process notepad = Process.Start("notepad.exe");
		notepad.WaitForInputIdle();
		Thread.Sleep(300);
		SetForegroundWindow(notepad.MainWindowHandle);
		SendKeysWin32("Hello", 200);
		Thread.Sleep(500);
		ShowWindow(gameHandle, SW_RESTORE);
		SetForegroundWindow(gameHandle);
	}

	public void OpenDownloadingBat()
	{
		IntPtr gameHandle = GetActiveWindow();
		string exeDir = AppDomain.CurrentDomain.BaseDirectory;
		string batPath = Path.Combine(exeDir, "downloading.bat");
		Process batProcess = new Process()
		{
			StartInfo = new ProcessStartInfo()
			{
				FileName = batPath,
				UseShellExecute = true,
				WorkingDirectory = exeDir
			}
			};
		batProcess.Start();
		Thread.Sleep(5000);
		ShowWindow(gameHandle, SW_RESTORE);
		SetForegroundWindow(gameHandle);
		batProcess.CloseMainWindow();
		batProcess.Kill(); 
	}

	public void StealFocus()
	{
		IntPtr hWnd = Process.GetCurrentProcess().MainWindowHandle;
		ShowWindow(hWnd, SW_RESTORE);
		SetForegroundWindow(hWnd);
	}

	public bool MinimizeExternalWindow(string processName)
	{
		IntPtr hWnd = GetWindowHandleByProcessName(processName);
		return ShowWindow(hWnd, SW_MINIMIZE);
	}

	public Godot.Image GetWindowsUserIcon()
	{
		string username = WindowsIdentity.GetCurrent().Name.Split('\\')[1];
		string path = $@"C:\Users\{username}\AppData\Roaming\Microsoft\Windows\AccountPictures";
		string[] files = Directory.GetFiles(path, "*.png");
		if (files.Length == 0)
			return null;

		Godot.Image img = new Godot.Image();
		Error err = img.Load(files[0]);
		if (err != Error.Ok)
			return null;

		return img;
	}
}
