# AnyDesk Rehber - Setup.exe oluşturma

Bu klasör, onaylanan WinUI 3 tasarımının dağıtılabilir Windows kurulum paketini üretmek için hazırlanmıştır.

## Gereksinimler

- Windows 10/11
- Visual Studio 2022 + .NET masaüstü/WinUI 3 geliştirme bileşenleri
- Inno Setup 6

## Oluşturma

PowerShell'i proje kökünde açın ve:

```powershell
Set-ExecutionPolicy -Scope Process Bypass
.\Installer\Build-Installer.ps1
```

Script önce `Release / x64 / self-contained` publish yapar, ardından Inno Setup ile:

`Installer\Output\AnyDeskRehber_Setup.exe`

dosyasını üretir.

Kurulum:
- Program Files altına uygulamayı kurar.
- Başlat menüsüne kısayol ekler.
- İsteğe bağlı olarak masaüstü kısayolu oluşturur.
- Kurulum sonrası uygulamayı başlatabilir.
- Kullanıcı adres defterini `%APPDATA%\AnyDeskRehber` altında tuttuğu için kaldırma işlemi bu veriyi silmez.
