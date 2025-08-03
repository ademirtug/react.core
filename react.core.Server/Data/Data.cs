using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;
using ProtoBuf;
using duoword.admin.Server.Services;

namespace duoword.admin.Server.Data
{
    public class Word : IComparable<Word>
    {
        [Key]
        public string Id { get; set; } = "";

        [NotMapped]
        public string Name => Id.Length > 0 ? Id.Split(".")[0] : "";
        public string Pos { get; set; } = "";
        public double BaseFrequency { get; set; } = 0;
        public double Frequency { get; set; } = 0;
        public string Definition { get; set; } = "";
        public bool IsWNConstitued { get; set; } = false;
        public bool IsChosen { get; set; } = false;
        public string Examples { get; set; } = "";
        public string Synonyms { get; set; } = "";
        public int CompareTo(Word? r)
        {
            if (r == null)
                return -1;
            if (Id == r.Id)
                return 0;
            if (r.Frequency - Frequency != 0)
                return Math.Sign(r.Frequency - Frequency);
            return r.Id.CompareTo(Id);
        }
    }
    public class GoogleVoice
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; } = "";

        public string Alias { get; set; } = "";

        public string Gender { get; set; } = "";

        public string LanguageCode { get; set; } = "";

        public virtual Language Language { get; set; } = null!;
    }
    public class Language
    {
        [Key]
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string LocalName { get; set; } = "";
        public double TotalSpeaker { get; set; } = 0;
        public bool HasArticles { get; set; } = false;
        public virtual List<GoogleVoice> Voices { get; set; } = new();
    }
    public class Curriculum
    {
        public int Version { get; set; } = 1;
        public DateTime PublishedAt { get; set; } = DateTime.Now;
        public virtual List<Topic> Topics { get; set; } = new();
    }
    public class Topic
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = "";
        public int Order { get; set; } = 0;
        public virtual List<Subtopic> Subtopics { get; set; } = new();
    }
    public class Subtopic : IComparable<Subtopic>
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = "";
        public int Order { get; set; } = 0;
        [JsonIgnore]
        //[ForeignKey("TopicId")]
        public int TopicId { get; set; }

        public virtual List<Lesson> Lessons { get; set; } = new();

        public int CompareTo(Subtopic? other)
        {
            int result = Order.CompareTo(other?.Order);
            if (result == 0)
            {
                result = Name.CompareTo(other?.Name);
            }
            return result;
        }
    }
    public class Lesson
    {
        [Key]
        public int Id { get; set; }

        public int Order { get; set; } = 0;

        public string Rank { get; set; } = "D1";

        [JsonIgnore]
        public int SubtopicId { get; set; }

        public virtual List<LessonWord> Words { get; set; } = new();
    }
    public class LessonWord
    {
        [Key]
        public int Id { get; set; }

        public int WordOrder { get; set; }

        [Required]
        public string WordId { get; set; } = "";

        [JsonIgnore]
        public int LessonId { get; set; }
    }
    public class LessonCatalog
    {
        public virtual List<Topic> Topics { get; set; } = new();
    }
    public class WordDictionary
    {
        [Key]
        public int Id { get; set; } = 38;

        public string LanguageCode { get; set; } = "";

        [JsonIgnore]
        public virtual Language Language { get; set; } = null!;

        public int Version { get; set; } = 1;

        public DateTime PublishedAt { get; set; } = DateTime.Now;

        public virtual List<WordTranslation> Translations { get; set; } = new();
    }
    public class WordTranslation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string WordId { get; set; } = "";

        public string Translation { get; set; } = "";

        public string Ipa { get; set; } = "";

        public string Phonemic { get; set; } = "";

        public string Romanization { get; set; } = "";

        [JsonIgnore]
        public int WordDictionaryId { get; set; }
    }
    public class LevelCatalog
    {
        public SortedDictionary<string, string> WordLevels { get; set; } = new SortedDictionary<string, string>();
        public void Remove(string wid)
        {
            WordLevels.Remove(wid);
        }
        public string? this[string WordId]
        {
            get
            {
                return WordLevels.FirstOrDefault(wlx => wlx.Key == WordId).Value ?? null;
            }
            set
            {
                WordLevels[WordId] = value ?? "";
            }
        }

        public static string Rank(int order)
        {
            if (order < 1250)
            {
                return "A1";
            }
            else if (order < 2500)
            {
                return "A2"; //< 79.11
            }
            else if (order < 3750)
            {
                return "B1"; // <31.06
            }
            else if (order < 5000)
            {
                return "B2";// < 16.11
            }
            else if (order < 6250)
            {
                return "C1"; // < 10.32
            }
            else
            {
                return "C2";//6.99
            }
        }
        public static string Rank(double freq)
        {
            if (freq > 79.11)
            {
                return "A1";
            }
            else if (freq > 31.06)
            {
                return "A2"; //< 79.11
            }
            else if (freq > 16.11)
            {
                return "B1"; // <31.06
            }
            else if (freq > 10.32)
            {
                return "B2";// < 16.11
            }
            else if (freq > 6.99)
            {
                return "C1"; // < 10.32
            }
            else
            {
                return "C2";//6.99
            }
        }
        public static double Rank(string level)
        {
            if (level == "A1")
            {
                return 85;
            }
            else if (level == "A2")
            {
                return 33;
            }
            else if (level == "B1")
            {
                return 18;
            }
            else if (level == "B2")
            {
                return 12;
            }
            else if (level == "C1")
            {
                return 8;
            }
            else if (level == "C2")
            {
                return 4;
            }
            else
            {
                return 0;
            }
        }
        public void Save(string path = "")
        {
            path = path == "" ? Location.dataPath + "levelcatalog.json" : path;

            JsonSerializerOptions options = new() { WriteIndented = true };
            string json = JsonSerializer.Serialize(this, options);

            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? "");
            File.WriteAllText(path, json);
        }

        public static LevelCatalog LoadFromFile(string path = "")
        {
            path = path == "" ? Location.dataPath + "levelcatalog.json" : path;
            if (File.Exists(path))
                return JsonSerializer.Deserialize<LevelCatalog>(File.ReadAllText(path)) ?? new LevelCatalog();
            return new LevelCatalog();
        }
    }

    [ProtoContract]
    public class LocaleEntry
    {
        [Key]
        [ProtoMember(1)] // Protobuf field number
        [JsonIgnore]
        public int Id { get; set; }

        [ProtoMember(2)]
        public string Name { get; set; } = "";

        [ProtoMember(3)]
        public string Text { get; set; } = "";

        [ProtoMember(4)]
        public string Description { get; set; } = "";

        [ProtoMember(5)]
        [JsonIgnore]
        public int LocaleId { get; set; }
    }

    [ProtoContract]
    public class Locale
    {
        [Key]
        [ProtoMember(1)]
        public int Id { get; set; }

        [ProtoMember(2)]
        public string LanguageCode { get; set; } = "";

        [ProtoMember(3)]
        public virtual List<LocaleEntry> Entries { get; set; } = new();
    }
}
