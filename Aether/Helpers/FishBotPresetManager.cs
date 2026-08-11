using Aether.Models;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aether.Helpers
{
    /// <summary>
    /// FishBot ön kayıt (preset) dosyalarını AppData\.mmdfishbot klasöründe yöneten yardımcı sınıf.
    /// Kaydetme, yükleme, listeleme ve silme işlemlerini kapsar.
    /// </summary>
    public static class FishBotPresetManager
    {
        // AppData\Roaming\.mmdfishbot
        private static readonly string FolderPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            ".mmdfishbot");

        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        // -----------------------------------------------------------------
        // Klasör
        // -----------------------------------------------------------------

        /// <summary>
        /// .mmdfishbot klasörünü oluşturur. Zaten varsa hiçbir şey yapmaz.
        /// </summary>
        public static void EnsureFolder()
        {
            if (!Directory.Exists(FolderPath))
                Directory.CreateDirectory(FolderPath);
        }

        // -----------------------------------------------------------------
        // Kaydetme
        // -----------------------------------------------------------------

        /// <summary>
        /// Verilen FishBotSettings nesnesini belirtilen isimle JSON olarak kaydeder.
        /// Aynı isimde dosya varsa üzerine yazar.
        /// </summary>
        /// <returns>Kaydedilen dosyanın tam yolu.</returns>
        public static string SavePreset(string presetName, FishBotSettings settings)
        {
            EnsureFolder();
            string filePath = GetFilePath(presetName);
            string json = JsonSerializer.Serialize(settings, SerializerOptions);
            File.WriteAllText(filePath, json);
            return filePath;
        }

        // -----------------------------------------------------------------
        // Yükleme
        // -----------------------------------------------------------------

        /// <summary>
        /// Belirtilen isimle kayıtlı JSON dosyasını okur ve FishBotSettings nesnesine dönüştürür.
        /// Dosya bulunamazsa veya ayrıştırılamazsa null döner.
        /// </summary>
        public static FishBotSettings? LoadPreset(string presetName)
        {
            string filePath = GetFilePath(presetName);
            if (!File.Exists(filePath)) return null;

            try
            {
                string json = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<FishBotSettings>(json, SerializerOptions);
            }
            catch
            {
                return null;
            }
        }

        // -----------------------------------------------------------------
        // Listeleme
        // -----------------------------------------------------------------

        /// <summary>
        /// .mmdfishbot klasöründeki tüm preset isimlerini (dosya adı uzantısız) döner.
        /// Klasör yoksa boş dizi döner.
        /// </summary>
        public static string[] GetPresetNames()
        {
            if (!Directory.Exists(FolderPath))
                return Array.Empty<string>();

            var files = Directory.GetFiles(FolderPath, "*.json");
            var names = new string[files.Length];
            for (int i = 0; i < files.Length; i++)
                names[i] = Path.GetFileNameWithoutExtension(files[i]);

            return names;
        }

        // -----------------------------------------------------------------
        // Silme
        // -----------------------------------------------------------------

        /// <summary>
        /// Belirtilen isimle kayıtlı JSON dosyasını siler.
        /// Dosya yoksa işlem yapılmaz.
        /// </summary>
        public static void DeletePreset(string presetName)
        {
            string filePath = GetFilePath(presetName);
            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        /// <summary>
        /// Belirtilen preset isminin dosya sisteminde mevcut olup olmadığını döner.
        /// </summary>
        public static bool PresetExists(string presetName)
            => File.Exists(GetFilePath(presetName));

        // -----------------------------------------------------------------
        // Yardımcı
        // -----------------------------------------------------------------

        private static string GetFilePath(string presetName)
            => Path.Combine(FolderPath, $"{presetName}.json");
    }
}
