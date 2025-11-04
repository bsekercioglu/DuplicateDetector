# Duplicate Detection - Kopya Dosya Tespit Sistemi

Bu C# console uygulaması, belirtilen sürücü veya klasördeki tüm dosyaları tarayarak hash kodlarını hesaplar ve kopya dosyaları tespit eder.

**Yazar:** Burak ŞEKERCİOĞLU  
**Web:** www.sekercioglu.eu

## Özellikler

- ✅ **Sürücü bazlı dosya tarama** - Tüm bir sürücüyü tarayabilir
- ✅ **Path bazlı dosya tarama** - Belirli bir klasör yolunu tarayabilir
- ✅ **SHA256 hash hesaplama** - Güvenilir dosya karşılaştırması için
- ✅ **Hash kayıtlarını JSON formatında saklama** - Tarama sonuçlarını kaydetme
- ✅ **Kopya dosya tespiti** - Aynı içeriğe sahip dosyaları bulma
- ✅ **Detaylı rapor oluşturma** - Kopya dosyalar hakkında detaylı bilgi
- ✅ **Gereksiz alan hesaplama** - Kopya dosyaların kapladığı alanı gösterir
- ✅ **İnteraktif kullanıcı arayüzü** - Renkli ASCII art başlık ve menü sistemi
- ✅ **İlerleme göstergesi** - Tarama sırasında anlık durum bilgisi

## Gereksinimler

- .NET Framework 4.8.1 veya üzeri
- Visual Studio 2019 veya üzeri (veya MSBuild)
- NuGet paket yöneticisi

## Kurulum

1. NuGet paketlerini yükleyin:
```bash
dotnet restore
```

veya Visual Studio'da Solution Explorer'da projeye sağ tıklayıp "Restore NuGet Packages" seçeneğini kullanın.

## Kullanım

### Derleme

Visual Studio'da `F6` tuşuna basın veya:
```bash
dotnet build
```

veya Release modunda:
```bash
dotnet build -c Release
```

### Çalıştırma

#### Komut satırından parametre ile:

**Sürücü bazlı tarama:**
```bash
DuplicateDetect.exe C
```

**Path bazlı tarama:**
```bash
DuplicateDetect.exe "C:\Users\Documents"
```

#### İnteraktif mod:

Parametresiz çalıştırdığınızda program size iki seçenek sunar:

1. **Sürücü bazlı tarama** - Tüm bir sürücüyü tarar (örn: C:\)
2. **Klasör/Path bazlı tarama** - Belirli bir klasör yolunu tarar (örn: C:\Users\Documents)

```bash
DuplicateDetect.exe
```

Program çalıştırıldığında:
- Renkli ASCII art başlık gösterilir
- Tarama modu seçimi yapılır
- Seçilen hedefte tarama başlar
- İşlenen dosya sayısı ve ilerleme gösterilir
- Hash kodları JSON dosyasına kaydedilir
- Kopya dosyalar tespit edilir ve rapor oluşturulur

## Çıktı Dosyaları

- **file_hashes.json**: Tüm dosyaların hash kodlarını, boyutlarını ve son değiştirilme tarihlerini içeren JSON dosyası
- **duplicate_report.txt**: Kopya dosyaların detaylı raporu, gereksiz alan bilgisi ve klasör bazlı analiz

## Teknik Detaylar

- **.NET Framework 4.8.1** kullanılmaktadır
- **SHA256** hash algoritması kullanılır (güvenilir dosya karşılaştırması için)
- **Newtonsoft.Json** kütüphanesi (v13.0.3) kullanılır
- Büyük dosyalar için optimize edilmiştir
- Erişim hatası durumlarında devam eder (sistem dosyaları için)
- Console renkleri ile geliştirilmiş kullanıcı arayüzü

## Proje Yapısı

- `Program.cs` - Ana program ve kullanıcı arayüzü
- `FileScanner.cs` - Dosya tarama ve hash hesaplama sınıfı
- `HashStorage.cs` - Hash kayıtlarını JSON formatında saklama
- `DuplicateReporter.cs` - Kopya dosya raporu oluşturma

## Notlar

- İlk tarama uzun sürebilir (disk boyutuna ve dosya sayısına bağlı)
- Sistem dosyalarına erişim reddedilebilir (normal davranış, program devam eder)
- Hash hesaplama büyük dosyalarda zaman alabilir
- 0 byte dosyalar otomatik olarak atlanır
- Tarama sırasında her 100 dosyada bir ilerleme bilgisi gösterilir

## Örnek Kullanım Senaryoları

### Senaryo 1: Tüm C sürücüsünü tarama
```bash
DuplicateDetect.exe C
```

### Senaryo 2: Belirli bir klasörü tarama
```bash
DuplicateDetect.exe "C:\Users\Username\Downloads"
```

### Senaryo 3: İnteraktif mod
```bash
DuplicateDetect.exe
# Ardından seçim yapın: 1 (Sürücü) veya 2 (Klasör)
```

## Lisans

Bu proje Burak ŞEKERCİOĞLU tarafından geliştirilmiştir.  
Daha fazla bilgi için: www.sekercioglu.eu

