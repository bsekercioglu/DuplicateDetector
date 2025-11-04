using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace DuplicateDetect
{
    public class FileScanner
    {
        private readonly SHA256 _sha256 = SHA256.Create();
        private int _processedFiles = 0;
        private long _totalSize = 0;

        public Dictionary<string, FileHashInfo> ScanDrive(string drivePath)
        {
            if (!Directory.Exists(drivePath))
            {
                throw new DirectoryNotFoundException($"Sürücü bulunamadı: {drivePath}");
            }

            var fileHashes = new Dictionary<string, FileHashInfo>();
            _processedFiles = 0;
            _totalSize = 0;

            Console.WriteLine($"Tarama başlatılıyor: {drivePath}");

            try
            {
                ScanDirectory(drivePath, fileHashes);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nTarama sırasında hata: {ex.Message}");
            }

            Console.WriteLine($"\nTarama tamamlandı. İşlenen dosya sayısı: {_processedFiles}");
            Console.WriteLine($"Toplam boyut: {FormatSize(_totalSize)}");

            return fileHashes;
        }

        public Dictionary<string, FileHashInfo> ScanPath(string path)
        {
            if (!Directory.Exists(path))
            {
                throw new DirectoryNotFoundException($"Klasör bulunamadı: {path}");
            }

            var fileHashes = new Dictionary<string, FileHashInfo>();
            _processedFiles = 0;
            _totalSize = 0;

            Console.WriteLine($"Tarama başlatılıyor: {path}");

            try
            {
                ScanDirectory(path, fileHashes);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nTarama sırasında hata: {ex.Message}");
            }

            Console.WriteLine($"\nTarama tamamlandı. İşlenen dosya sayısı: {_processedFiles}");
            Console.WriteLine($"Toplam boyut: {FormatSize(_totalSize)}");

            return fileHashes;
        }

        private void ScanDirectory(string directory, Dictionary<string, FileHashInfo> fileHashes)
        {
            try
            {
                // Dosyaları tara
                var files = Directory.GetFiles(directory);
                foreach (var file in files)
                {
                    try
                    {
                        ProcessFile(file, fileHashes);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Erişim reddedildi, devam et
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"\nDosya işlenirken hata: {file} - {ex.Message}");
                    }
                }

                // Alt klasörleri tara
                var directories = Directory.GetDirectories(directory);
                foreach (var dir in directories)
                {
                    try
                    {
                        ScanDirectory(dir, fileHashes);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Erişim reddedildi, devam et
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"\nKlasör taranırken hata: {dir} - {ex.Message}");
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Erişim reddedildi, devam et
            }
        }

        private void ProcessFile(string filePath, Dictionary<string, FileHashInfo> fileHashes)
        {
            var fileInfo = new System.IO.FileInfo(filePath);
            
            // Çok küçük dosyaları atla (0 byte)
            if (fileInfo.Length == 0)
            {
                return;
            }

            string hash = CalculateHash(filePath);
            
            var info = new FileHashInfo
            {
                Path = filePath,
                Size = fileInfo.Length,
                LastModified = fileInfo.LastWriteTime
            };

            fileHashes[hash] = info;
            _processedFiles++;
            _totalSize += fileInfo.Length;

            // İlerleme göster (her 100 dosyada bir)
            if (_processedFiles % 100 == 0)
            {
                Console.Write($"\rİşlenen dosya: {_processedFiles} | Toplam boyut: {FormatSize(_totalSize)}");
            }
        }

        private string CalculateHash(string filePath)
        {
            try
            {
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 8192))
                {
                    byte[] hashBytes = _sha256.ComputeHash(stream);
                    return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Hash hesaplanamadı: {filePath} - {ex.Message}", ex);
            }
        }

        private string FormatSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}

