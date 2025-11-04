using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DuplicateDetect
{
    public class DuplicateReporter
    {
        public void GenerateReport(List<DuplicateGroup> duplicates, string reportPath, string basePath)
        {
            var report = new StringBuilder();
            
            report.AppendLine("=".PadRight(80, '='));
            report.AppendLine("KOPYA DOSYA TESPİT RAPORU");
            report.AppendLine("=".PadRight(80, '='));
            report.AppendLine($"Rapor Oluşturulma Tarihi: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine($"Toplam Kopya Grup Sayısı: {duplicates.Count}");
            report.AppendLine($"Taranan Sürücü: {basePath}");
            report.AppendLine();

            long totalWastedSpace = 0;
            int totalDuplicateFiles = 0;

            for (int i = 0; i < duplicates.Count; i++)
            {
                var group = duplicates[i];
                int fileCount = group.Files.Count;
                long fileSize = group.Files[0].Size;
                long wastedSpace = fileSize * (fileCount - 1); // İlk dosya hariç diğerleri gereksiz

                totalDuplicateFiles += fileCount;
                totalWastedSpace += wastedSpace;

                report.AppendLine($"Grup {i + 1}:");
                report.AppendLine($"  Hash: {group.Hash}");
                report.AppendLine($"  Dosya Sayısı: {fileCount}");
                report.AppendLine($"  Dosya Boyutu: {FormatSize(fileSize)}");
                report.AppendLine($"  Gereksiz Alan: {FormatSize(wastedSpace)}");
                report.AppendLine($"  Dosyalar:");
                
                foreach (var file in group.Files.OrderBy(f => f.Path))
                {
                    report.AppendLine($"    - {file.Path}");
                    report.AppendLine($"      Boyut: {FormatSize(file.Size)} | Değiştirilme: {file.LastModified:yyyy-MM-dd HH:mm:ss}");
                }
                
                report.AppendLine();
            }

            report.AppendLine("=".PadRight(80, '='));
            report.AppendLine("ÖZET");
            report.AppendLine("=".PadRight(80, '='));
            report.AppendLine($"Toplam Kopya Grup Sayısı: {duplicates.Count}");
            report.AppendLine($"Toplam Kopya Dosya Sayısı: {totalDuplicateFiles}");
            report.AppendLine($"Kazanılabilir Alan: {FormatSize(totalWastedSpace)}");
            report.AppendLine();

            // Klasör bazlı analiz
            var folderGroups = AnalyzeByFolder(duplicates, basePath);
            if (folderGroups.Any())
            {
                report.AppendLine("=".PadRight(80, '='));
                report.AppendLine("KLASÖR BAZLI ANALİZ");
                report.AppendLine("=".PadRight(80, '='));
                
                foreach (var folderGroup in folderGroups.OrderByDescending(f => f.Count))
                {
                    report.AppendLine($"Klasör: {folderGroup.FolderPath}");
                    report.AppendLine($"  Kopya Dosya Sayısı: {folderGroup.Count}");
                    report.AppendLine();
                }
            }

            File.WriteAllText(reportPath, report.ToString(), Encoding.UTF8);
        }

        private List<FolderAnalysis> AnalyzeByFolder(List<DuplicateGroup> duplicates, string basePath)
        {
            var folderStats = new Dictionary<string, int>();

            foreach (var group in duplicates)
            {
                foreach (var file in group.Files)
                {
                    string folder = Path.GetDirectoryName(file.Path) ?? "";
                    if (folder.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
                    {
                        if (!folderStats.ContainsKey(folder))
                        {
                            folderStats[folder] = 0;
                        }
                        folderStats[folder]++;
                    }
                }
            }

            return folderStats.Select(kvp => new FolderAnalysis
            {
                FolderPath = kvp.Key,
                Count = kvp.Value
            }).ToList();
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

    public class FolderAnalysis
    {
        public string FolderPath { get; set; }
        public int Count { get; set; }

        public FolderAnalysis()
        {
            FolderPath = string.Empty;
        }
    }
}

