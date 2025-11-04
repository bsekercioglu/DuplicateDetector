using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace DuplicateDetect
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            
            // ASCII Art Başlık
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(@"

██████╗░██╗░░░██╗██████╗░██╗░░░░░██╗░█████╗░░█████╗░████████╗███████╗
██╔══██╗██║░░░██║██╔══██╗██║░░░░░██║██╔══██╗██╔══██╗╚══██╔══╝██╔════╝
██║░░██║██║░░░██║██████╔╝██║░░░░░██║██║░░╚═╝███████║░░░██║░░░█████╗░░
██║░░██║██║░░░██║██╔═══╝░██║░░░░░██║██║░░██╗██╔══██║░░░██║░░░██╔══╝░░
██████╔╝╚██████╔╝██║░░░░░███████╗██║╚█████╔╝██║░░██║░░░██║░░░███████╗
╚═════╝░░╚═════╝░╚═╝░░░░░╚══════╝╚═╝░╚════╝░╚═╝░░╚═╝░░░╚═╝░░░╚══════╝

██████╗░███████╗██████╗░███████╗░█████╗░████████╗░█████╗░██████╗░
██╔══██╗██╔════╝██╔══██╗██╔════╝██╔══██╗╚══██╔══╝██╔══██╗██╔══██╗
██║░░██║█████╗░░██║░░██║█████╗░░██║░░╚═╝░░░██║░░░██║░░██║██████╔╝
██║░░██║██╔══╝░░██║░░██║██╔══╝░░██║░░██╗░░░██║░░░██║░░██║██╔══██╗
██████╔╝███████╗██████╔╝███████╗╚█████╔╝░░░██║░░░╚█████╔╝██║░░██║
╚═════╝░╚══════╝╚═════╝░╚══════╝░╚════╝░░░░╚═╝░░░░╚════╝░╚═╝░░╚═╝
");
            Console.ResetColor();
            
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("                     Kopya Dosya Tespit Sistemi\n");
            Console.ResetColor();
            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("                    Yazar: Burak ŞEKERCİOĞLU");
            Console.WriteLine("                    Web: www.sekercioglu.eu\n");
            Console.ResetColor();
            
            Console.WriteLine(new string('=', 80));
            Console.WriteLine();

            string scanPath = GetScanPath(args);
            if (string.IsNullOrEmpty(scanPath))
            {
                Console.WriteLine("Geçersiz tarama yolu! Program sonlandırılıyor.");
                return;
            }

            string hashFile = "file_hashes.json";
            string reportFile = "duplicate_report.txt";

            Console.WriteLine($"Seçilen tarama yolu: {scanPath}");
            Console.WriteLine($"Hash kayıt dosyası: {hashFile}");
            Console.WriteLine($"Rapor dosyası: {reportFile}\n");

            try
            {
                // Dosya tarama ve hash hesaplama
                Console.WriteLine("Dosyalar taranıyor ve hash kodları hesaplanıyor...");
                var fileScanner = new FileScanner();
                
                Dictionary<string, List<FileHashInfo>> fileHashes;
                // Path bir sürücü kök dizini mi kontrol et (örn: C:\)
                if (scanPath.Length == 3 && scanPath.EndsWith(":\\", StringComparison.OrdinalIgnoreCase))
                {
                    fileHashes = fileScanner.ScanDrive(scanPath);
                }
                else
                {
                    fileHashes = fileScanner.ScanPath(scanPath);
                }
                
                int totalFiles = fileHashes.Values.Sum(list => list.Count);
                Console.WriteLine($"\nToplam {totalFiles} dosya tespit edildi.");

                // Hash kayıtlarını dosyaya kaydet
                Console.WriteLine("\nHash kodları kayıt dosyasına kaydediliyor...");
                var hashStorage = new HashStorage();
                hashStorage.SaveHashes(fileHashes, hashFile);
                Console.WriteLine($"Hash kayıtları '{hashFile}' dosyasına kaydedildi.");

                // Kopya dosyaları tespit et
                Console.WriteLine("\nKopya dosyalar tespit ediliyor...");
                var duplicates = FindDuplicates(fileHashes);
                
                if (duplicates.Any())
                {
                    Console.WriteLine($"{duplicates.Count} grup kopya dosya bulundu.");

                    // Rapor oluştur
                    Console.WriteLine("\nRapor oluşturuluyor...");
                    var reporter = new DuplicateReporter();
                    reporter.GenerateReport(duplicates, reportFile, scanPath);
                    Console.WriteLine($"Rapor '{reportFile}' dosyasına kaydedildi.");
                }
                else
                {
                    Console.WriteLine("\nKopya dosya bulunamadı!");
                }

                Console.WriteLine("\nİşlem tamamlandı!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nHata oluştu: {ex.Message}");
                Console.WriteLine($"Detay: {ex.StackTrace}");
            }

            Console.WriteLine("\nÇıkmak için bir tuşa basın...");
            Console.ReadKey();
        }

        static string GetScanPath(string[] args)
        {
            // Komut satırından argüman varsa kullan
            if (args.Length > 0 && !string.IsNullOrEmpty(args[0]))
            {
                string path = args[0].Trim();
                // Eğer sadece harf girildiyse sürücü olarak işle (örn: C -> C:\)
                if (path.Length == 1 && char.IsLetter(path[0]))
                {
                    return $"{path.ToUpper()}:\\";
                }
                // Eğer geçerli bir path ise kullan
                if (Directory.Exists(path) || File.Exists(path))
                {
                    return path;
                }
                // Path yoksa ama geçerli format ise (örn: C:\Users) kullan
                try
                {
                    var dirInfo = new DirectoryInfo(path);
                    return path;
                }
                catch
                {
                    // Geçersiz path
                }
            }

            // Ana menü döngüsü
            while (true)
            {
                Console.WriteLine("\n" + new string('=', 80));
                Console.WriteLine("Tarama modu seçin:");
                Console.WriteLine("1. Sürücü bazlı tarama");
                Console.WriteLine("2. Klasör/Path bazlı tarama");
                Console.WriteLine("0. Programdan çıkış");
                Console.Write("\nSeçiminiz (0, 1 veya 2): ");
                string choice = Console.ReadLine()?.Trim();

                if (choice == "0")
                {
                    Console.WriteLine("\nProgramdan çıkılıyor...");
                    return null;
                }
                else if (choice == "1")
                {
                    string result = SelectDrive();
                    if (result != null)
                    {
                        return result;
                    }
                    // result null ise ana menüye dönecek (döngü devam edecek)
                }
                else if (choice == "2")
                {
                    string result = SelectPath();
                    if (result != null)
                    {
                        return result;
                    }
                    // result null ise ana menüye dönecek (döngü devam edecek)
                }
                else
                {
                    Console.WriteLine("\nGeçersiz seçim! Lütfen 0, 1 veya 2 girin.");
                }
            }
        }

        static string SelectDrive()
        {
            Console.WriteLine("\n" + new string('-', 80));
            Console.WriteLine("Kullanılabilir sürücüler:");
            var drives = DriveInfo.GetDrives()
                .Where(d => d.IsReady && d.DriveType == DriveType.Fixed)
                .ToList();

            for (int i = 0; i < drives.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {drives[i].Name} ({drives[i].TotalSize / (1024L * 1024 * 1024)} GB)");
            }

            Console.WriteLine("0. Ana menüye dön");
            Console.Write("\nSürücü harfini girin (örn: C) veya 0 ile geri dönün: ");
            string input = Console.ReadLine();
            
            if (input != null)
            {
                input = input.ToUpper().Trim();
                
                if (input == "0")
                {
                    Console.WriteLine("\nAna menüye dönülüyor...");
                    return null;
                }
                
                if (!string.IsNullOrEmpty(input) && input.Length == 1 && char.IsLetter(input[0]))
                {
                    // Sürücünün var olup olmadığını kontrol et
                    string drivePath = $"{input}:\\";
                    if (Directory.Exists(drivePath))
                    {
                        return drivePath;
                    }
                    else
                    {
                        Console.WriteLine($"\nUyarı: '{drivePath}' sürücüsü bulunamadı veya erişilemiyor.");
                        Console.WriteLine("Ana menüye dönmek için Enter'a basın...");
                        Console.ReadLine();
                        return null;
                    }
                }
                else
                {
                    Console.WriteLine("\nGeçersiz giriş! Lütfen bir harf (örn: C) veya 0 girin.");
                    Console.WriteLine("Ana menüye dönmek için Enter'a basın...");
                    Console.ReadLine();
                    return null;
                }
            }

            return null;
        }

        static string SelectPath()
        {
            Console.WriteLine("\n" + new string('-', 80));
            Console.WriteLine("Taranacak klasör yolunu girin:");
            Console.WriteLine("(0 ile ana menüye dönebilirsiniz)");
            Console.Write("\nKlasör yolu (örn: C:\\Users\\Documents): ");
            string inputPath = Console.ReadLine()?.Trim();
            
            if (string.IsNullOrEmpty(inputPath))
            {
                Console.WriteLine("\nGeçersiz giriş! Ana menüye dönülüyor...");
                return null;
            }

            // Vazgeçme kontrolü
            if (inputPath == "0")
            {
                Console.WriteLine("\nAna menüye dönülüyor...");
                return null;
            }

            // Path'in sonundaki tırnak işaretlerini temizle
            inputPath = inputPath.Trim('"');
            
            if (Directory.Exists(inputPath))
            {
                return inputPath;
            }
            else
            {
                Console.WriteLine($"\nUyarı: '{inputPath}' klasörü bulunamadı.");
                Console.Write("Yine de taramaya devam etmek istiyor musunuz? (E/H): ");
                string confirm = Console.ReadLine()?.Trim().ToUpper();
                
                if (confirm == "E" || confirm == "EVET" || confirm == "Y")
                {
                    return inputPath;
                }
                else
                {
                    Console.WriteLine("\nAna menüye dönülüyor...");
                    return null;
                }
            }
        }

        static List<DuplicateGroup> FindDuplicates(Dictionary<string, List<FileHashInfo>> fileHashes)
        {
            var duplicates = new List<DuplicateGroup>();

            foreach (var kvp in fileHashes)
            {
                // Eğer aynı hash'e sahip birden fazla dosya varsa, kopya olarak işaretle
                if (kvp.Value.Count > 1)
                {
                    duplicates.Add(new DuplicateGroup
                    {
                        Hash = kvp.Key,
                        Files = kvp.Value
                    });
                }
            }

            return duplicates;
        }
    }

    public class FileHashInfo
    {
        public string Path { get; set; }
        public long Size { get; set; }
        public DateTime LastModified { get; set; }

        public FileHashInfo()
        {
            Path = string.Empty;
        }
    }

    public class DuplicateGroup
    {
        public string Hash { get; set; }
        public List<FileHashInfo> Files { get; set; }

        public DuplicateGroup()
        {
            Hash = string.Empty;
            Files = new List<FileHashInfo>();
        }
    }
}

