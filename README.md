# Duplicate Detection - Kopya Dosya Tespit Sistemi

Bu C# console uygulaması, belirtilen sürücüdeki tüm dosyaları tarayarak hash kodlarını hesaplar ve kopya dosyaları tespit eder.

## Özellikler

- ✅ Sürücü bazlı dosya tarama
- ✅ SHA256 hash hesaplama
- ✅ Hash kayıtlarını JSON formatında saklama
- ✅ Kopya dosya tespiti
- ✅ Detaylı rapor oluşturma
- ✅ Klasör bazlı analiz
- ✅ Gereksiz alan hesaplama

## Gereksinimler

- .NET Framework 4.8
- Visual Studio 2019 veya üzeri (veya MSBuild)
- NuGet paket yöneticisi

## Kurulum

1. NuGet paketlerini yükleyin:
```bash
nuget restore
```

veya Visual Studio'da Solution Explorer'da projeye sağ tıklayıp "Restore NuGet Packages" seçeneğini kullanın.

## Kullanım

### Derleme
Visual Studio'da `F6` tuşuna basın veya:
```bash
msbuild DuplicateDetect.csproj /p:Configuration=Release
```

### Çalıştırma
```bash
bin\Release\DuplicateDetect.exe
```

Veya sürücü harfini parametre olarak verin:
```bash
bin\Release\DuplicateDetect.exe C
```

## Çıktı Dosyaları

- **file_hashes.json**: Tüm dosyaların hash kodlarını içeren JSON dosyası
- **duplicate_report.txt**: Kopya dosyaların detaylı raporu

## Teknik Detaylar

- .NET Framework 4.8 kullanılmaktadır
- SHA256 hash algoritması kullanılır
- Newtonsoft.Json kütüphanesi kullanılır
- Büyük dosyalar için optimize edilmiştir
- Erişim hatası durumlarında devam eder

## Notlar

- İlk tarama uzun sürebilir (disk boyutuna bağlı)
- Sistem dosyalarına erişim reddedilebilir (normal davranış)
- Hash hesaplama büyük dosyalarda zaman alabilir
- Newtonsoft.Json NuGet paketi otomatik olarak yüklenmelidir

