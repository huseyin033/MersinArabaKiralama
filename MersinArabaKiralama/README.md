# Mersin Araba Kiralama Sistemi

ASP.NET Core 9.0 ile geliştirilmiş araç kiralama uygulaması.

## 🚀 Hızlı Başlangıç

### Ön Gereksinimler

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Node.js](https://nodejs.org/) (Frontend için)
- [SQL Server 2022](https://www.microsoft.com/tr-tr/sql-server/sql-server-downloads) (veya Docker üzerinde çalıştırılabilir)

### Yerel Geliştirme Ortamı

1. **Veritabanı Kurulumu**

   ```bash
   # SQL Server'ı Docker ile başlat
   docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Passw0rd" \
      -p 1433:1433 --name mssql -d mcr.microsoft.com/mssql/server:2022-latest
   ```

2. **Veritabanı Oluşturma**

   ```sql
   -- SSMS veya Azure Data Studio ile bağlanıp çalıştırın
   CREATE DATABASE MersinArabaKiralama;
   GO
   ```

3. **Uygulamayı Çalıştırma**

   ```bash
   # Bağımlılıkları yükle
   dotnet restore
   
   # Veritabanı migration'larını çalıştır
   dotnet ef database update
   
   # Uygulamayı başlat
   dotnet run
   ```

   Veya Docker Compose ile tüm servisleri başlatın:
   ```bash
   docker-compose up -d
   ```

### API Endpoint'leri

- **Swagger UI**: https://localhost:5001/swagger
- **API Base URL**: https://localhost:5001/api/v1

### Veritabanı Bağlantı Bilgileri

| Ayar | Değer |
|------|-------|
| Sunucu | localhost,1433 |
| Veritabanı | MersinArabaKiralama |
| Kullanıcı | sa |
| Şifre | YourStrong@Passw0rd |
| Bağlantı Dizesi | `Server=localhost,1433;Database=MersinArabaKiralama;User=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;` |

### Ortam Değişkenleri

`.env` dosyası oluşturup aşağıdaki değişkenleri ayarlayın:

```env
ASPNETCORE_ENVIRONMENT=Development
ConnectionStrings__DefaultConnection=Server=localhost,1433;Database=MersinArabaKiralama;User=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;
Jwt__Key=YourSecureKey-ChangeThisInProduction123!
Jwt__Issuer=MersinArabaKiralama
Jwt__Audience=MersinArabaKiralamaUsers
Jwt__ExpireDays=7
```

## 🐳 Docker ile Çalıştırma

### Geliştirme Modu

```bash
# Tüm servisleri başlat
docker-compose up -d

# Sadece veritabanını başlat
docker-compose up -d db

# Migration'ları çalıştır
docker-compose exec mersin-arabakiralama dotnet ef database update
```

### Üretim Modu

```bash
# Üretim build'i oluştur
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d --build
```

## 🔒 Güvenlik

- Tüm API endpoint'leri JWT ile korunmaktadır
- Varsayılan kullanıcı bilgilerini değiştirmeyi unutmayın
- Üretimde SSL/TLS kullanın

## 📝 Lisans

Bu proje [MIT lisansı](LICENSE) altında lisanslanmıştır.

## 📞 İletişim

Proje ekibi - [ornek@email.com](mailto:ornek@email.com)
