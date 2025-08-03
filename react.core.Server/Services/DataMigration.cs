using duoword.admin.Server.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace duoword.admin.Server.Services
{
    public class Location
    {
#if DEBUG
        public static string lexTreeFile = "../../../filedata/lexTree.json";
        public static string lexiconFile = "../../../filedata/glexicon.json";
        public static string dataPath = "./filedata/";
        public static string audioPath = "../../../filedata/audio/";
        public static string request = "../../../filedata/requests/";
        public static string root = "../../../";
#else
        public static string lexTreeFile = "./filedata/lexTree.json";
        public static string lexiconFile = "./filedata/glexicon.json";
        public static string dataPath = "./filedata/";
        public static string audioPath = "./filedata/audio/";
        public static string root = "./";
        public static string request = "./filedata/requests/";
#endif
    }

    public class OldWordDictionary
    {
        public Language Language { get; set; } = new Language();
        public int Version { get; set; } = 1;
        public DateTime PublishedAt { get; set; } = DateTime.Now;

        public Dictionary<string, OldWordTranslation> Translations { get; set; } = new();
    }
    public class OldWordTranslation
    {
        public string Id { get; set; } = "";
        public string Translation { get; set; } = "";
        public string Ipa { get; set; } = "";
        public string Phonemic { get; set; } = "";
        public string Romanization { get; set; } = "";
    }


    public static class DictionaryImporter
    {

        public static void ImportDictionaries(AppDbContext dbContext, string dictionaryPath)
        {
            if (!Directory.Exists(dictionaryPath))
            {
                Console.WriteLine("Dictionary folder does not exist: " + dictionaryPath);
                return;
            }

            foreach (var file in Directory.GetFiles(dictionaryPath, "d.*.json"))
            {
                WordDictionary? oldDictionary = null;

                try
                {
                    oldDictionary = JsonSerializer.Deserialize<WordDictionary>(
                        File.ReadAllText(file),
                        new JsonSerializerOptions
                        {
                            WriteIndented = true,
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        });
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error reading or deserializing file {file}: {e.Message}");
                    continue;
                }

                if (oldDictionary == null) continue;


                // Extract language code from filename
                var filename = Path.GetFileName(file); // e.g. d.en-US.json
                var langCode = Path.GetFileNameWithoutExtension(filename).Replace("d.", ""); // e.g. en-US

                var language = dbContext.Languages.FirstOrDefault(l => l.Code == langCode);
                if (language == null)
                {
                    language = new Language
                    {
                        Code = langCode,
                        Name = langCode,
                        LocalName = langCode,
                        TotalSpeaker = 0
                    };
                    dbContext.Languages.Add(language);
                    dbContext.SaveChanges();
                }

                oldDictionary.Language = language;

                // Convert to the new structure
                var newDictionary = new WordDictionary
                {
                    LanguageCode = oldDictionary.Language.Code,
                    Language = language,
                    Version = oldDictionary.Version,
                    PublishedAt = oldDictionary.PublishedAt,
                    Translations = oldDictionary.Translations.Select(kvp => new WordTranslation
                    {
                        WordId = kvp.WordId,
                        Translation = kvp.Translation,
                        Ipa = kvp.Ipa,
                        Phonemic = kvp.Phonemic,
                        Romanization = kvp.Romanization
                    }).ToList()
                };


                dbContext.WordDictionaries.Add(newDictionary);
                dbContext.SaveChanges();

                Console.WriteLine($"Imported dictionary for {langCode}");
            }

        }

        //public static void ImportDictionaries(AppDbContext dbContext, string dictionaryPath)
        //{
        //    if (!Directory.Exists(dictionaryPath))
        //    {
        //        Console.WriteLine("Dictionary folder does not exist: " + dictionaryPath);
        //        return;
        //    }

        //    foreach (var file in Directory.GetFiles(dictionaryPath, "d.*.json"))
        //    {
        //        WordDictionary? oldDictionary = null; 
        //        // Load the old JSON structure
        //        try
        //        {
        //            oldDictionary = JsonSerializer.Deserialize<WordDictionary>(File.ReadAllText(file), new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        //        }catch(Exception e)
        //        {
        //            int a = 0; 
        //        }

        //        if (oldDictionary == null) continue;

        //        // Find the language
        //        var language = dbContext.Languages.FirstOrDefault(l => l.Code == oldDictionary.Language.Code);
        //        if (language == null)
        //        {
        //            // Create new language entry if missing
        //            language = new Language
        //            {
        //                Code = oldDictionary.Language.Code,
        //                Name = oldDictionary.Language.Name,
        //                LocalName = oldDictionary.Language.LocalName,
        //                TotalSpeaker = oldDictionary.Language.TotalSpeaker
        //            };
        //            dbContext.Languages.Add(language);
        //            dbContext.SaveChanges();
        //        }

        //        // Convert to the new structure
        //        var newDictionary = new WordDictionary
        //        {
        //            LanguageCode = oldDictionary.Language.Code,
        //            Language = language,
        //            Version = oldDictionary.Version,
        //            PublishedAt = oldDictionary.PublishedAt,
        //            Translations = oldDictionary.Translations.Select(kvp => new WordTranslation
        //            {
        //                WordId = kvp.Key,
        //                Translation = kvp.Value.Translation,
        //                Ipa = kvp.Value.Ipa,
        //                Phonemic = kvp.Value.Phonemic,
        //                Romanization = kvp.Value.Romanization
        //            }).ToList()
        //        };

        //        // Save to the database
        //        dbContext.WordDictionaries.Add(newDictionary);
        //        dbContext.SaveChanges();

        //        Console.WriteLine($"Imported dictionary for {oldDictionary.Language.Code}");

        //    }
        //}
    }
    public class WordJson
    {
        public string Id { get; set; } = "";
        public string Pos { get; set; } = "";
        public double Frequency { get; set; }
        public double AdjustedFrequency { get; set; } = 0;
        public string Definition { get; set; } = "";
        public bool IsWNConstitued { get; set; } = false;
        public List<string> Examples { get; set; } = new List<string>();
        public List<string> Synonyms { get; set; } = new List<string>();
    }
    public class LexiconJson
    {
        public List<WordJson> Words { get; set; } = new List<WordJson>();
    }
    public class LexiconMigrationService
    {
        private readonly AppDbContext _dbContext;
        private readonly Dictionary<string, double> _adjustedFrequencies;

        public LexiconMigrationService(AppDbContext dbContext, string levelCatalogPath)
        {
            _dbContext = dbContext;
            _adjustedFrequencies = LoadAdjustedFrequencies(levelCatalogPath);
        }

        /// <summary>
        /// Loads adjusted frequencies from levelcatalog.json
        /// </summary>
        private Dictionary<string, double> LoadAdjustedFrequencies(string levelCatalogPath)
        {
            if (!File.Exists(levelCatalogPath))
            {
                Console.WriteLine("Level catalog file not found. Using default frequencies.");
                return new Dictionary<string, double>();
            }

            try
            {
                string jsonContent = File.ReadAllText(levelCatalogPath);
                var levelCatalog = JsonSerializer.Deserialize<LevelCatalog>(jsonContent, new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

                return levelCatalog?.WordLevels
                    .ToDictionary(kvp => kvp.Key, kvp => LevelCatalog.Rank(kvp.Value) + 1.0)
                    ?? new Dictionary<string, double>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load level catalog: {ex.Message}");
                return new Dictionary<string, double>();
            }
        }

        /// <summary>
        /// Migrates words from words.json to the database
        /// </summary>
        public void MigrateWords(string wordsJsonPath)
        {
            if (!File.Exists(wordsJsonPath))
            {
                Console.WriteLine($"Words file not found: {wordsJsonPath}");
                return;
            }

            string jsonContent = File.ReadAllText(wordsJsonPath);
            var lexiconJson = JsonSerializer.Deserialize<LexiconJson>(jsonContent);

            if (lexiconJson == null || lexiconJson.Words.Count == 0)
            {
                Console.WriteLine("No words found in the JSON file.");
                return;
            }
            List<string> chosenWords = JsonSerializer.Deserialize<List<string>>(File.ReadAllText(Location.dataPath + "chosen.json"), new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase }) ?? [];

            List<Word> wordsToInsert = new List<Word>();

            foreach (var wordJson in lexiconJson.Words)
            {
                // You can add any checks or operations you want here
                Console.WriteLine($"Processing Word: {wordJson.Id}");

                double adjustedFreq = _adjustedFrequencies.ContainsKey(wordJson.Id)
                    ? _adjustedFrequencies[wordJson.Id]
                    : wordJson.Frequency;

                Word word = new Word
                {
                    Id = wordJson.Id,
                    Pos = wordJson.Pos,
                    BaseFrequency = wordJson.Frequency,
                    Definition = wordJson.Definition,
                    IsWNConstitued = wordJson.IsWNConstitued,
                    Examples = string.Join("; ", wordJson.Examples),
                    Synonyms = string.Join(", ", wordJson.Synonyms),
                    Frequency = adjustedFreq,
                    IsChosen = chosenWords.Contains(wordJson.Id)

                };

                wordsToInsert.Add(word);
            }

            _dbContext.Words.AddRange(wordsToInsert);
            _dbContext.SaveChanges();

            Console.WriteLine("Lexicon migration completed successfully.");
        }
    }
    public class LessonCatalogJson
    {
        public int Version { get; set; }
        public DateTime PublishedAt { get; set; }
        public List<TopicJson> Topics { get; set; } = new();
    }
    public class TopicJson
    {
        public int Order { get; set; }
        public string Name { get; set; } = "";
        public List<SubtopicJson> Subtopics { get; set; } = new();
    }
    public class SubtopicJson
    {
        public int Order { get; set; }
        public string Name { get; set; } = "";
        public List<LessonJson> Lessons { get; set; } = new();
    }
    public class LessonJson
    {
        public int Order { get; set; }
        public string Rank { get; set; } = "D1";
        public Dictionary<int, string> Words { get; set; } = new(); // Key = Order, Value = WordId
    }

    public class LessonCatalogMigrationService
    {
        private readonly AppDbContext _dbContext;

        public LessonCatalogMigrationService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void MigrateFromJson(string jsonFilePath)
        {
            if (!File.Exists(jsonFilePath))
            {
                Console.WriteLine($"File not found: {jsonFilePath}");
                return;
            }


            string jsonContent = File.ReadAllText(jsonFilePath);
            var lessonCatalog = JsonSerializer.Deserialize<LessonCatalogJson>(jsonContent, new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            if (lessonCatalog?.Topics == null || lessonCatalog.Topics.Count == 0)
            {
                Console.WriteLine("No lesson topics found in the JSON file.");
                return;
            }

            List<Topic> newTopics = new();

            foreach (var topicJson in lessonCatalog.Topics)
            {
                var topic = new Topic
                {
                    Name = topicJson.Name,
                    Order = topicJson.Order,
                    Subtopics = new()
                };

                foreach (var subtopicJson in topicJson.Subtopics)
                {
                    var subtopic = new Subtopic
                    {
                        Name = subtopicJson.Name,
                        Order = subtopicJson.Order,
                        Lessons = new()
                    };

                    foreach (var lessonJson in subtopicJson.Lessons)
                    {
                        var lesson = new Lesson
                        {
                            Order = lessonJson.Order,
                            Rank = lessonJson.Rank,
                            Words = lessonJson.Words.Select(w => new LessonWord
                            {
                                WordOrder = w.Key,
                                WordId = w.Value
                            }).ToList()
                        };

                        subtopic.Lessons.Add(lesson);
                    }

                    topic.Subtopics.Add(subtopic);
                }

                newTopics.Add(topic);
            }

            _dbContext.Topics.AddRange(newTopics);
            _dbContext.SaveChanges();

            Console.WriteLine("Lesson catalog migration completed successfully.");

        }
    }
    public class UITranslationEntry
    {
        public string Name { get; set; }
        public string Text { get; set; }
        public string Description { get; set; }

        public UITranslationEntry(string name, string text, string description)
        {
            Name = name;
            Text = text;
            Description = description;
        }
    }
    public class UITranslationsFile
    {
        string locale = "";

        [JsonPropertyName("entries")]
        public List<UITranslationEntry> Translations { get; set; } = new();

        public static UITranslationsFile LoadFromFile(string code)
        {
            string path = code == string.Empty ? Location.dataPath + "ui_translations/en-US" : Location.dataPath + "ui_translations/" + code + ".json";
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                var f = JsonSerializer.Deserialize<UITranslationsFile>(json, options);
                f.locale = code == string.Empty ? "en-US" : code;
                return f;
            }
            else
            {
                UITranslationsFile f = new UITranslationsFile();
                f.locale = code == string.Empty ? "en-US" : code;
                return f;
            }
        }
    }
    public class LocaleMigrationService
    {
        private readonly AppDbContext _dbContext;

        public LocaleMigrationService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Migrates all translation files from a directory to the database.
        /// </summary>
        public void MigrateTranslationsFromDirectory(string translationsDirectory)
        {
            if (!Directory.Exists(translationsDirectory))
            {
                Console.WriteLine($"Translations directory not found: {translationsDirectory}");
                return;
            }

            var translationFiles = Directory.GetFiles(translationsDirectory, "*.json");

            if (translationFiles.Length == 0)
            {
                Console.WriteLine("No translation files found in the directory.");
                return;
            }

            foreach (var filePath in translationFiles)
            {
                string localeCode = Path.GetFileNameWithoutExtension(filePath).Replace("l.", "");
                MigrateTranslations(filePath, localeCode);
            }
        }

        /// <summary>
        /// Migrates a single translation file to the database.
        /// </summary>
        private void MigrateTranslations(string translationsJsonPath, string localeCode)
        {
            if (!File.Exists(translationsJsonPath))
            {
                Console.WriteLine($"Translation file not found: {translationsJsonPath}");
                return;
            }

            try
            {
                string jsonContent = File.ReadAllText(translationsJsonPath);
                var translationFile = JsonSerializer.Deserialize<UITranslationsFile>(jsonContent, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

                if (translationFile == null || translationFile.Translations.Count == 0)
                {
                    Console.WriteLine("No translations found in the JSON file.");
                    return;
                }

                // Check if the locale already exists
                var locale = _dbContext.Locales.Include(l => l.Entries)
                    .FirstOrDefault(l => l.LanguageCode == localeCode);

                if (locale == null)
                {
                    locale = new Locale { LanguageCode = localeCode, Entries = new List<LocaleEntry>() };
                    _dbContext.Locales.Add(locale);
                }

                foreach (var entry in translationFile.Translations)
                {
                    if (!locale.Entries.Any(e => e.Name == entry.Name))
                    {
                        locale.Entries.Add(new LocaleEntry
                        {
                            Name = entry.Name,
                            Text = entry.Text,
                            Description = entry.Description
                        });
                    }
                }

                _dbContext.SaveChanges();

                Console.WriteLine($"Translation migration for {localeCode} completed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Migration failed for {localeCode}: {ex.Message}");
            }
        }
    }


    public class LexTree
    {
        public int Version { get; set; } = 1;

        public DateTime PublishedAt { get; set; } = DateTime.Now;

        public List<OldWordCategorization> Words { get; set; } = new List<OldWordCategorization>();

        public static LexTree LoadFromFile(string path = "")
        {
            path = path == "" ? Location.dataPath + "lexTree.json" : path;

            return JsonSerializer.Deserialize<LexTree>(File.ReadAllText(path)) ?? new LexTree();
        }
    }

    public class OldWordCategorization
    {
        public string WordId { get; set; } = "";

        public string Topic { get; set; } = "";

        public string Subtopic { get; set; } = "";
    }

    public class LexTreeMigrationService
    {
        private AppDbContext _dbContext;

        public LexTreeMigrationService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Migrates all translation files from a directory to the database.
        /// </summary>
        public void MigrateLextree()
        {
            //LexTree lexTree = LexTree.LoadFromFile();
            //var subtopicsDict = _dbContext.Subtopics.ToDictionary(s => s.Name, s => s.Id);

            //// Prepare list for bulk insertion
            //List<WordCategorization> wordCategorizations = lexTree.Words
            //    .Select(w => new WordCategorization
            //    {
            //        WordId = w.WordId,
            //        SubtopicId = subtopicsDict.TryGetValue(w.Subtopic, out int subtopicId) ? subtopicId : 0
            //    })
            //    .ToList();

            //// Check if there are any invalid TopicId or SubtopicId
            //var missingMappings = wordCategorizations
            //    .Where(wc => wc.SubtopicId == 0)
            //    .ToList();

            //if (missingMappings.Any())
            //{
            //    Console.WriteLine($"Warning: {missingMappings.Count} words have missing topic or subtopic mappings.");
            //    foreach (var missing in missingMappings)
            //    {
            //        Console.WriteLine($"WordId: {missing.WordId}, SubtopicId: {missing.SubtopicId}");
            //    }
            //    throw new Exception("bang"); // Exit or handle accordingly
            //}

            //// Bulk insert only if all mappings are valid
            //_dbContext.WordCategorizations.AddRange(wordCategorizations);
            //_dbContext.SaveChanges();
        }
    }




}
