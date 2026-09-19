# AnyDesk Rehber — WinUI 3 Refined

Bu sürüm, WinUI 3 prototipinin son UI düzenlemelerini içerir.

## Bu sürümde
- Sağ üstteki AR avatarı kaldırıldı; yalnızca tema düğmesi kaldı.
- Açık/koyu tema gerçek renk paletini değiştirir.
- Favoriler sayfasının başlık ikonu artık yıldızdır.
- Arama alanında `Ara...` sola hizalı ve dikey ortalıdır.
- AnyDesk ID ortalıdır ve kopyalama sonrası minimal toast gösterilir.
- Sidebar ikonları tek bir 42px sütununda hizalıdır.
- Grup listesi sidebar'da dinamik olarak oluşur.
- Yeni bilgisayar ekranında mevcut gruplar ComboBox'tan seçilebilir; `＋ Yeni grup...` ile yeni grup oluşturulabilir.
- Kartlardaki `Hazır` ifadesi kaldırıldı. Durum artık `Bilinmiyor / Online / Offline` olarak tutulur ve düzenleme ekranından değiştirilebilir.
- Eski JSON kayıtlarında durum alanı yoksa otomatik olarak `Bilinmiyor` kabul edilir.

## Durum mantığı
Bu uygulamanın yerel JSON rehberi, herhangi bir uzak AnyDesk adresinin online olup olmadığını kendi başına güvenilir biçimde bilemez. Bu nedenle yeşil `Hazır` gibi yanıltıcı bir durum kullanılmaz. Gerçek otomatik uzak durum için ileride AnyDesk'in lisans/management entegrasyonu eklenebilir.

## Çalıştırma
1. `AnyDeskRehber.WinUI3.sln` dosyasını Visual Studio'da açın.
2. `x64` ve `Debug` seçin.
3. F5 ile çalıştırın.

## Veri
Kayıtlar `%APPDATA%\AnyDeskRehber\address_book.json` altında tutulur.

## Setup.exe

Dağıtım için `Installer\Build-Installer.ps1` scripti self-contained x64 publish üretir ve Inno Setup 6 ile tek bir `AnyDeskRehber_Setup.exe` kurulum dosyasına paketler.

## v1.2 güvenlik ve kullanım iyileştirmeleri
- Kartlara **Sil** butonu eklendi; işlem onay penceresi olmadan gerçekleşmez.
- Aynı AnyDesk ID/Alias'ın ikinci kez eklenmesi engellendi. Sayısal ID'lerde boşluk ve tire farkları yok sayılır.
- JSON kayıtları önce geçici dosyaya yazılır, ardından canlı dosya değiştirilir.
- Her başarılı güncellemede önceki `address_book.json`, `address_book.backup.json` olarak korunur.
- Ana JSON bozulursa açılışta yedek otomatik denenir.
- Inno Setup algılama scriptindeki Program Files (x86) yolu düzeltildi ve kullanıcı-bazlı kurulum yolu eklendi.


## v1.3 changes
- Groups are persisted independently in `%APPDATA%\AnyDeskRehber\groups.json`; deleting the last contact no longer deletes the group.
- Right-click a group to rename or delete it. Deleting a group keeps its computers and makes them ungrouped.
- Existing v1.2 groups are migrated automatically from contact records.
- Light theme sidebar/main-area contrast increased.
- Search placeholder changed to `Bilgisayar ara...`.
