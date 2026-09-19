using System.Diagnostics;

namespace AnyDeskRehber.WinUI3.Services;

public sealed class AnyDeskService
{
    private static readonly string[] Paths =
    [
        @"C:\Program Files (x86)\AnyDesk\AnyDesk.exe",
        @"C:\Program Files\AnyDesk\AnyDesk.exe"
    ];

    public string? FindExecutable() => Paths.FirstOrDefault(File.Exists);

    public bool Connect(string address, out string error)
    {
        error = "";
        address = address.Trim();
        if (string.IsNullOrWhiteSpace(address))
        {
            error = "AnyDesk ID / Alias boş.";
            return false;
        }

        var exe = FindExecutable();
        if (exe is null)
        {
            error = "AnyDesk.exe bulunamadı. AnyDesk'in kurulu olduğundan emin olun.";
            return false;
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = exe,
                Arguments = $"\"{address.Replace("\"", "\\\"")}\"",
                UseShellExecute = true
            });
            return true;
        }
        catch (Exception ex)
        {
            error = $"AnyDesk başlatılamadı: {ex.Message}";
            return false;
        }
    }
}
