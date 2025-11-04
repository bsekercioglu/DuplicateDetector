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
                
                Dictionary<string, FileHashInfo> fileHashes;
                // Path bir sürücü kök dizini mi kontrol et (örn: C:\)
                if (scanPath.Length == 3 && scanPath.EndsWith(":\\", StringComparison.OrdinalIgnoreCase))
                {
                    fileHashes = fileScanner.ScanDrive(scanPath);
                }
                else
                {
                    fileHashes = fileScanner.ScanPath(scanPath);
                }
                
                Console.WriteLine($"\nToplam {fileHashes.Count} dosya tespit edildi.");

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

            // Kullanıcıdan seçim yapmasını iste
            Console.WriteLine("Tarama modu seçin:");
            Console.WriteLine("1. Sürücü bazlı tarama");
            Console.WriteLine("2. Klasör/Path bazlı tarama");
            Console.Write("\nSeçiminiz (1 veya 2): ");
            string choice = Console.ReadLine()?.Trim();

            if (choice == "1")
            {
                // Sürücü seçimi
                Console.WriteLine("\nKullanılabilir sürücüler:");
                var drives = DriveInfo.GetDrives()
                    .Where(d => d.IsReady && d.DriveType == DriveType.Fixed)
                    .ToList();

                for (int i = 0; i < drives.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {drives[i].Name} ({drives[i].TotalSize / (1024L * 1024 * 1024)} GB)");
                }

                Console.Write("\nSürücü harfini girin (örn: C): ");
                string input = Console.ReadLine();
                if (input != null)
                {
                    input = input.ToUpper().Trim();
                    if (!string.IsNullOrEmpty(input) && input.Length == 1 && char.IsLetter(input[0]))
                    {
                        return $"{input}:\\";
                    }
                }
            }
            else if (choice == "2")
            {
                // Path seçimi
                Console.Write("\nTaranacak klasör yolunu girin (örn: C:\\Users\\Documents): ");
                string inputPath = Console.ReadLine()?.Trim();
                if (!string.IsNullOrEmpty(inputPath))
                {
                    // Path'in sonundaki tırnak işaretlerini temizle
                    inputPath = inputPath.Trim('"');
                    
                    if (Directory.Exists(inputPath))
                    {
                        return inputPath;
                    }
                    else
                    {
                        Console.WriteLine($"Uyarı: '{inputPath}' klasörü bulunamadı, ancak tarama deneniyor...");
                        return inputPath;
                    }
                }
            }
            else
            {
                Console.WriteLine("Geçersiz seçim!");
            }

            return null;
        }

        static List<DuplicateGroup> FindDuplicates(Dictionary<string, FileHashInfo> fileHashes)
        {
            var hashGroups = fileHashes
                .GroupBy(kvp => kvp.Key)
                .Where(g => g.Count() > 1)
                .ToList();

            var duplicates = new List<DuplicateGroup>();

            foreach (var group in hashGroups)
            {
                duplicates.Add(new DuplicateGroup
                {
                    Hash = group.Key,
                    Files = group.Select(g => g.Value).ToList()
                });
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

