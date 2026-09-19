# AnyDesk Rehber

WinUI 3 ile geliştirilmiş, AnyDesk bilgisayarlarını gruplar ve favoriler ile yönetmek ve bağlantı başlatmak için hazırlanmış Windows masaüstü rehber uygulaması.

> Bu proje bağımsız bir topluluk projesidir; AnyDesk Software GmbH tarafından geliştirilmemiş veya onaylanmamıştır. AnyDesk adı ve markaları ilgili sahiplerine aittir.

## Özellikler

- Bilgisayar adı, AnyDesk ID/alias, grup ve not bilgisiyle kayıt oluşturma
- Kayıt düzenleme ve onay pencereli silme
- AnyDesk bağlantısını doğrudan başlatma
- Favoriler ve arama
- Kalıcı gruplar; grup boşalsa bile korunur
- Grup yeniden adlandırma ve grup silme
- Açık / koyu tema
- ID kopyalama bildirimi
- Manuel Bilinmiyor / Online / Offline durum bilgisi
- Yinelenen AnyDesk ID kontrolü
- Güvenli JSON kaydı ve otomatik yedek
- Inno Setup ile Setup.exe oluşturma

## Teknolojiler

- C# / .NET 8
- WinUI 3
- Windows App SDK
- Inno Setup 6

## Çalıştırma

1. `AnyDeskRehber.WinUI3.sln` dosyasını Visual Studio'da açın.
2. Platform olarak `x64` seçin.
3. F5 ile çalıştırın.

## Veri depolama

Kullanıcı verileri `%APPDATA%\AnyDeskRehber` altında tutulur ve Git deposuna dahil edilmez.

## Lisans

GNU General Public License v3.0 (GPL-3.0).
