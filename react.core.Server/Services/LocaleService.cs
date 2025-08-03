using duoword.admin.Server.Data;
using duoword.admin.Server.Repositories;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using System.IO.Compression;
using System.Text;
using System.Text.Json;

namespace duoword.admin.Server.Services
{
    public class LocaleService
    {
        IRepository<Locale> locales;
        RabbitMQService queue;
        public LocaleService(RabbitMQService mqService, IRepository<Locale> rep)
        {
            locales = rep;
            queue = mqService;
        }

        public (string, string) GetArb(int id)
        {
            var locale = locales
                .Where(l => l.Id == id)
                .Include(l => l.Entries)
                .FirstOrDefault();

            return locale == null ? ("", "") : ($"app_{locale.LanguageCode}.arb", ConvertToArb(locale));
        }

        public string ConvertToArb(Locale locale)
        {
            var arbEntries = new Dictionary<string, object>
            {
                ["@@locale"] = locale.LanguageCode.Split("-")[0]
            };

            foreach (var entry in locale.Entries)
            {
                arbEntries[entry.Name] = entry.Text;
                arbEntries[$"@{entry.Name}"] = new { };
            }

            return JsonSerializer.Serialize(arbEntries, new JsonSerializerOptions { WriteIndented = true });
        }

        public string GetArbBundle()
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    IQueryable<Locale> allLocales = locales.Include(l => l.Entries);

                    foreach (var locale in allLocales)
                    {

                        var entry = archive.CreateEntry($"app_{locale.LanguageCode.Split("-")[0]}.arb", CompressionLevel.Optimal);
                        using (var entryStream = entry.Open())
                        using (var writer = new StreamWriter(entryStream, Encoding.UTF8))
                        {
                            writer.Write(ConvertToArb(locale));
                        }
                    }
                }
                return Convert.ToBase64String(memoryStream.ToArray());
            }
        }

        public (string, string) GetJson(int id)
        {
            Locale? dict = locales.Where(l => l.Id == id)
                .Include(l => l.Entries)
                .FirstOrDefault();
            if (dict == null)
            {
                return ("", "");
            }

            var fileContent = JsonSerializer.Serialize(dict, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            return ($"l.{dict.LanguageCode}.json", fileContent);
        }
        public string ZipBundle()
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    IQueryable<Locale> allLocales = locales.Include(l => l.Entries);
                    foreach (var file in allLocales)
                    {
                        var entry = archive.CreateEntry($"l.{file.LanguageCode}.json", CompressionLevel.Optimal);
                        using (var entryStream = entry.Open())
                        using (var writer = new StreamWriter(entryStream, Encoding.UTF8))
                        {
                            writer.Write(JsonSerializer.Serialize(file, new JsonSerializerOptions
                            {
                                WriteIndented = true,
                                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                            }));
                        }
                    }
                }
                return Convert.ToBase64String(memoryStream.ToArray());
            }
        }

        public Locale? Deserialize(IFormFile? file)
        {
            if (file == null)
            {
                return null;
            }

            string fileContent = "";
            using (var stream = new StreamReader(file.OpenReadStream()))
            {
                fileContent = stream.ReadToEnd();
            }
            return Deserialize(fileContent);
        }
        public Locale? Deserialize(string fileContent)
        {
            return JsonSerializer.Deserialize<Locale>(fileContent, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }

        public async Task Queue(int id)
        {
            var channel = await queue.GetChannelAsync();
            var messageBody = Encoding.UTF8.GetBytes(id.ToString());

            await channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: "locale-translation",
                body: messageBody
            );
        }
    }
}
