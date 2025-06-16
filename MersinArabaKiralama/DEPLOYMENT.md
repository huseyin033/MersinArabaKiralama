# Mersin Araba Kiralama Dağıtım Talimatları

## Gereksinimler
- Docker ve Docker Compose
- En az 4GB RAM
- 2 CPU çekirdeği

## Yerel Geliştirme Ortamı

### 1. Geliştirme Ortamı Kurulumu
```bash
# Projeyi klonlayın
git clone [repo-url]
cd MersinArabaKiralama

# Geliştirme sertifikasını güvenli olarak işaretleyin
dotnet dev-certs https --trust
```

### 2. Docker ile Çalıştırma
```bash
# Docker konteynerlerini başlat
docker-compose up -d

# Veritabanı migration'larını çalıştır
docker-compose exec mersin-arabakiralama dotnet ef database update
```

## Üretim Dağıtımı

### 1. Docker Image'ını Çekin
```bash
docker pull ghcr.io/kullaniciadiniz/mersin-arabakiralama:latest
```

### 2. docker-compose.prod.yml ile Başlatın
```bash
docker-compose -f docker-compose.prod.yml up -d
```

## Ortam Değişkenleri

| Değişken | Açıklama | Varsayılan |
|----------|----------|------------|
| `ASPNETCORE_ENVIRONMENT` | Çalışma ortamı | `Production` |
| `ConnectionStrings__DefaultConnection` | Veritabanı bağlantı dizesi | |
| `Jwt__Key` | JWT imzalama anahtarı | |

## Yedekleme

### Veritabanı Yedeği Alma
```bash
docker exec -t mssql /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrong@Passw0rd -Q "BACKUP DATABASE [MersinArabaKiralama] TO DISK = N'/var/opt/mssql/backup/MersinArabaKiralama.bak'"
```

### Log Dosyaları
Log dosyaları `/app/Logs` dizininde bulunur ve Docker volume'üne bağlıdır.
