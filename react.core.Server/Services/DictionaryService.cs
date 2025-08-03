using duoword.admin.Server.Data;
using duoword.admin.Server.Repositories;
using System.IO.Compression;
using System.Text;
using System.Text.Json;

namespace duoword.admin.Server.Services
{
    public class DictionaryService
    {
        IRepository<WordDictionary> dictionaries;
        public DictionaryService(IRepository<WordDictionary> rep)
        {
            dictionaries = rep;
        }

        public (string, string) GetJson(int id)
        {
            WordDictionary? dict = dictionaries.Include(d => d.Translations).FirstOrDefault(d => d.Id == (int)id);
            if (dict == null)
            {
                return ("", "");
            }

            var fileContent = JsonSerializer.Serialize(dict, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            return ($"d.{dict.LanguageCode}.json", fileContent);
        }
        public string ZipBundle()
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    IQueryable<WordDictionary> allLocales = dictionaries.Include(d => d.Translations);
                    foreach (var file in allLocales)
                    {
                        var entry = archive.CreateEntry($"d.{file.LanguageCode}.json", CompressionLevel.Optimal);
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

        public WordDictionary? Deserialize(IFormFile? file)
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
        public WordDictionary? Deserialize(string fileContent)
        {
            return JsonSerializer.Deserialize<WordDictionary>(fileContent, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }

        public string GetCategory(Curriculum curriculum, string WordId)
        {
            var result = curriculum.Topics
                .SelectMany(topic => topic.Subtopics
                    .SelectMany(subtopic => subtopic.Lessons
                        .Where(lesson => lesson.Words.Any(word => word.WordId == "major.n.01"))
                        .Select(lesson => new { TopicName = topic.Name, SubtopicName = subtopic.Name })))
                .ToList();
            var cat = result.Count == 1 ? result[0] : result.Where(r => r.TopicName != "Fundamentals").FirstOrDefault()!;

            return cat.TopicName + @"\" + cat.SubtopicName;
        }
    }
}
