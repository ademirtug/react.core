using duoword.admin.Server.Data;
using duoword.admin.Server.Repositories;
using duoword.admin.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using System.IO.Compression;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;


namespace duoword.admin.Server.Controllers
{
    [Route("/api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class DictionaryController : ControllerBase
    {
        IRepository<WordDictionary> dictionaries;
        IRepository<Curriculum> curriculumRepository;
        IRepository<Word> words;
        DictionaryService dictionaryService;
        IChatClient ai;

        public DictionaryController(IRepository<WordDictionary> wordDictionaryRepository, DictionaryService service, IChatClient chatClient, IRepository<Word> wordRepository, IRepository<Curriculum> curRep)
        {
            dictionaries = wordDictionaryRepository;
            dictionaryService = service;
            ai = chatClient;
            words = wordRepository;
            curriculumRepository = curRep;
        }


        [HttpGet("all")]
        public IActionResult All()
        {
            return Ok(dictionaries.GetAll().ToList());
        }

        [HttpGet("get/{id}")]
        public IActionResult Get(int id)
        {
            WordDictionary? dict = dictionaries.Get(id);
            return dict == null ? NotFound() : Ok(dict);
        }

        [HttpGet("search/{id}")]
        public IActionResult Search(string code)
        {
            var result = dictionaries.Where(d => d.LanguageCode == code);
            return result.Count() > 0 ? Ok(result.ToList()) : NotFound();
        }

        [HttpPost("add")]
        public IActionResult Add([FromForm] string languageCode, IFormFile? file)
        {
            if (string.IsNullOrWhiteSpace(languageCode))
            {
                return BadRequest("Language code is required.");
            }
            //new Language { Code = "ota", Name = "Ottoman Turkish", LocalName = "Osmanlı Türkçesi", TotalSpeaker = 13.2 }

            WordDictionary? wd = dictionaryService.Deserialize(file);
            wd ??= new WordDictionary { LanguageCode = languageCode };

            dictionaries.Insert(wd);
            dictionaries.SaveChanges();
            return Ok(new DictionaryInfo(wd));
        }


        [HttpDelete("delete/{id}")]
        public IActionResult Delete(int id)
        {
            dictionaries.Delete(id);
            dictionaries.SaveChanges();
            return Ok();
        }

        [HttpGet("infolist")]
        public IActionResult InfoList()
        {
            var result = dictionaries.Select(dict => new DictionaryInfo
            {
                Id = dict.Id,
                LanguageCode = dict.LanguageCode,
                TranslationCount = dict.Translations.Count
            });
            return result.Count() > 0 ? Ok(result.ToList()) : NoContent();
        }

        [HttpGet("download/{id}")]
        public IActionResult Download(int id)
        {
            WordDictionary? dict = dictionaries.Where(d => d.Id == id).Include(d => d.Translations).FirstOrDefault();
            if (dict == null)
            {
                return NotFound();
            }

            var fileName = $"d.{dict.LanguageCode}.json";

            var fileContent = JsonSerializer.Serialize(dict, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All) // Ensure non-ASCII characters are not escaped
            });

            var base64Json = Convert.ToBase64String(Encoding.UTF8.GetBytes(fileContent));

            return Ok(new { fileName, fileContent = base64Json });
        }

        [HttpGet("downloadall")]
        public async Task<IActionResult> DownloadAll()
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    IQueryable<WordDictionary> allDictionaries = dictionaries.Include(d => d.Translations);
                    foreach (var file in allDictionaries)
                    {
                        var entry = archive.CreateEntry($"d.{file.LanguageCode}.json", CompressionLevel.Optimal);
                        using (var entryStream = entry.Open())
                        using (var writer = new StreamWriter(entryStream, Encoding.UTF8))
                        {
                            await writer.WriteAsync(JsonSerializer.Serialize(file, new JsonSerializerOptions
                            {
                                WriteIndented = true,
                                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                            }));
                        }
                    }
                }

                var base64Zip = Convert.ToBase64String(memoryStream.ToArray());
                return Ok(new { fileName = "dictionaries.zip", fileContent = base64Zip });
            }
        }

        [HttpPost("update/{id}")]
        public IActionResult Update(int id, IFormFile file)
        {
            WordDictionary? wd = dictionaries.Get(id);
            if (wd == null)
                return NotFound();

            WordDictionary? upload = dictionaryService.Deserialize(file);
            if (upload == null)
                return BadRequest("Corrupt File");

            upload.Id = id;
            dictionaries.Delete(id);
            dictionaries.SaveChanges();
            dictionaries.Insert(upload);
            dictionaries.SaveChanges();

            return Ok(new DictionaryInfo
            {
                Id = upload.Id,
                LanguageCode = upload.LanguageCode,
                TranslationCount = upload.Translations.Count
            });
        }

        [HttpPost("updatex")]
        public IActionResult Update([FromBody] TestData td)
        {


            return BadRequest(new { message = "Failed To Update because KABOOM has happened" });
        }

        //TODO : Implement sync functionality
        //[HttpGet("sync/{id}")]
        //public async Task<IActionResult> Sync(int id)
        //{
        //    List<Word> chosenWords = words.Where(w => w.IsChosen == true).ToList();
        //    Curriculum curriculum = curriculumRepository.GetWithDetails(1)!;

        //    WordDictionary? wd = dictionaries.GetWithDetails(id)!;
        //    var missing = chosenWords.Where(w => !wd.Translations.Any(t => t.WordId == w.Id)).Select(w => new
        //    {
        //        w.Id,
        //        Word = w.Name,
        //        PartOfSpeech = w.Pos,
        //        Category = dictionaryService.GetCategory(curriculum, w.Id),
        //        w.Definition,
        //        ExampleSentences = string.Join("|", w.Examples)
        //    }).ToList();

        //    // Process in batches of 50 words
        //    int batchSize = 20;

        //    while (missing.Any())
        //    {
        //        var limited = missing.Take(batchSize).ToList(); // Take the first 50 words
        //        missing = missing.Skip(batchSize).ToList(); // Remove the words we've already processed

        //        string jsonContent = JsonSerializer.Serialize(limited, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        //        var messageContent = $"You are a professional translator tasked with translating dictionary entries to ota(Ottoman Turkish). Each entry contains a word, its part of speech, category, definition, and example sentences. " +
        //                             "Your task is to provide translations in everyday, commonly used language. Additionally, provide phonetic details for the translations, including IPA, Romanization, and any native phonetic scripts such as Ottoman Arabic script for Ottoman Turkish or similar for other languages. " +
        //                             "If no suitable translation exists, leave the translation field as an empty string and mark the phonetic details as empty as well. Your response should be in JSON format, structured as follows: " +
        //                             "[{\"wordId\": \"Id\", \"translation\": \"translated word\", \"ipa\": \"International Phonetic Alphabet\", \"romanization\": \"Romanized version\", \"phonemic\": \"\"}]. \r\n";

        //        messageContent += jsonContent;

        //        var response = await ai.GetResponseAsync(messageContent);
        //        var translated = JsonSerializer.Deserialize<List<WordTranslation>>(response?.Message?.Text?.Replace("```json", "").Replace("```", "") ?? "", new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        //        // Add the translated words to the list
        //        wd.Translations.AddRange(translated ?? new List<WordTranslation>());
        //        dictionaries.SaveChanges();
        //    }

        //    // Add all translations to the dictionary

        //    dictionaries.SaveChanges();

        //    return Ok(new DictionaryInfo(wd));
        //}

    }

    //        { id: 2, name: "Jane Smith", age: "32", city: "Los Angeles", active: false },
    public class TestData
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public int Age { get; set; }
        public string City { get; set; } = "";
    }

    public class DictionaryInfo
    {
        public int Id { get; set; }
        public string LanguageCode { get; set; } = "";
        public int TranslationCount { get; set; }

        public DictionaryInfo() { }
        public DictionaryInfo(WordDictionary wd)
        {
            Id = wd.Id;
            LanguageCode = wd.LanguageCode;
            TranslationCount = wd.Translations.Count;
        }
    }
}
