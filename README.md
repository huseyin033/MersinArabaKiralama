# Mersin Araba Kiralama

Mersin Araba Kiralama, kullanıcıların online olarak araç kiralayabileceği bir web uygulamasıdır.

## Özellikler

- Kullanıcı girişi ve kayıt işlemleri
- Araç listeleme ve filtreleme
- Rezervasyon yönetimi
- Kullanıcı paneli
- Yönetici paneli

## Kurulum

### Ön Gereksinimler
- [.NET 6.0 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) veya üzeri
- [SQL Server](https://www.microsoft.com/tr-tr/sql-server/sql-server-downloads) (2016 veya üzeri)
- [Visual Studio 2022](https://visualstudio.microsoft.com/tr/vs/) (IDE olarak önerilir) veya Visual Studio Code

### Adım 1: Projeyi Hazırlama
1. Projeyi bilgisayarınıza indirin veya kopyalayın
2. Komut istemini (CMD veya PowerShell) açın ve proje dizinine gidin:
   ```
   cd "c:\proje\yolu\MersinArabaKiralama"
   ```

### Adım 2: Paketleri Yükleme
1. Gerekli NuGet paketlerini yükleyin:
   ```
   dotnet restore
   ```
   > **Not:** Bu işlem, projenin bağımlılıklarını indirecektir.

### Adım 3: Veritabanı Kurulumu
1. `appsettings.json` dosyasında bağlantı dizesini kontrol edin:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MersinArabaKiralama;Trusted_Connection=True;MultipleActiveResultSets=true"
   }
   ```
2. Veritabanını oluşturmak için Entity Framework Core migrations'ları çalıştırın:
   ```
   dotnet ef database update
   ```
   > **Not:** Bu komut, veritabanını otomatik olarak oluşturacak ve gerekli tabloları kuracaktır.

### Adım 4: Uygulamayı Çalıştırma
1. Geliştirme sunucusunu başlatın:
   ```
   dotnet run
   ```
2. Tarayıcınızı açın ve şu adrese gidin:
   ```
   https://localhost:5001
   ```
   veya
   ```
   http://localhost:5000
   ```

### Geliştirme Modu İçin İpuçları
- Hata ayıklama modunda çalıştırmak için Visual Studio'da F5 tuşuna basın
- Veritabanı değişiklikleri için yeni bir migration oluşturmak isterseniz:
  ```
  dotnet ef migrations add "MigrationAdi"
  ```

## Kullanılan Teknolojiler

- ASP.NET Core 6.0
- Entity Framework Core
- SQL Server
- Bootstrap 5
- jQuery

## Lisans

Bu proje [MIT Lisansı](LICENSE) altında lisanslanmıştır. © 2025 Hüseyin Aziz
