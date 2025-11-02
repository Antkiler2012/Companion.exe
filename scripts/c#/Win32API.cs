using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Security.Principal;
using System.Collections.Generic;
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
	
public void OpenFileExplorer()
{
	Process explorer = Process.Start("explorer.exe");
	explorer.WaitForInputIdle();
	Thread.Sleep(300);
	SetForegroundWindow(explorer.MainWindowHandle);
	ShowWindow(explorer.MainWindowHandle, SW_RESTORE);
}

	
	public void OpenAdminBat()
	{
		IntPtr gameHandle = GetActiveWindow();
		string exeDir = AppDomain.CurrentDomain.BaseDirectory;
		string batPath = Path.Combine(exeDir, "admin.bat");
		Process adminProcess = new Process()
		{
			StartInfo = new ProcessStartInfo()
			{
				FileName = batPath,
				UseShellExecute = true,
				WorkingDirectory = exeDir
			}
		};
		adminProcess.Start();
		Thread.Sleep(2000);
		ShowWindow(gameHandle, SW_RESTORE);
		SetForegroundWindow(gameHandle);
		adminProcess.Kill();
		adminProcess.Dispose();
	}

public void OpenDownloadingBat()
{
	Node audio = GetNode("../AudioStreamPlayer");
	AudioStreamPlayer player = audio as AudioStreamPlayer;
	player.Stream = (AudioStream)GD.Load("res://SFX/error.mp3");
	IntPtr gameHandle = GetActiveWindow();
	string exeDir = AppDomain.CurrentDomain.BaseDirectory;
	string downloadingPath = Path.Combine(exeDir, "downloading.bat");
	Process downloadingProcess = new Process()
	{
		StartInfo = new ProcessStartInfo()
		{
			FileName = downloadingPath,
			UseShellExecute = true,
			WorkingDirectory = exeDir
		}
	};
	downloadingProcess.Start();
	Thread.Sleep(5000);
	ShowWindow(gameHandle, SW_RESTORE);
	SetForegroundWindow(gameHandle);
	downloadingProcess.CloseMainWindow();
	downloadingProcess.Kill();
	string errorPath = Path.Combine(exeDir, "error.bat");
	List<Process> errorProcesses = new List<Process>();
	for (int i = 0; i < 10; i++)
	{
		Process errorProcess = new Process()
		{
			StartInfo = new ProcessStartInfo()
			{
				FileName = errorPath,
				UseShellExecute = true,
				WorkingDirectory = exeDir
			}
		}; 
		errorProcess.Start();
		player.Play();
		errorProcesses.Add(errorProcess);
		Thread.Sleep(100);
	}
	Thread.Sleep(1000);
	foreach (var proc in errorProcesses)
	{
		proc.CloseMainWindow();
		proc.Kill();
	}
	Node start = GetNode("../VBoxContainer");
	start.CallDeferred("crash");
}

public static void tts(string text)
{
	string escaped = text.Replace("'", "''");
	string psCommand =
		"Add-Type -AssemblyName System.Speech; " +
		$"(New-Object System.Speech.Synthesis.SpeechSynthesizer).Speak('{escaped}')";
	Process.Start(new ProcessStartInfo("powershell", $"-Command \"{psCommand}\"")
	{
		UseShellExecute = false,
		CreateNoWindow = true
	});
}
	public void exit()
	{
		 GetTree().Quit();
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
		Godot.Image img = new Godot.Image();
		img.Load(files[0]);
		return img;
	}
}
