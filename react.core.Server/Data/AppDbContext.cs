using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using duoword.admin.Server.Models;


namespace duoword.admin.Server.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Word> Words { get; set; }
        public DbSet<Language> Languages { get; set; }
        public DbSet<GoogleVoice> GoogleVoices { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<Topic> Topics { get; set; }
        public DbSet<Subtopic> Subtopics { get; set; }
        public DbSet<LessonWord> LessonWords { get; set; }
        public DbSet<WordDictionary> WordDictionaries { get; set; }
        public DbSet<WordTranslation> WordTranslations { get; set; }
        public DbSet<Locale> Locales { get; set; }
        //public DbSet<WordCategorization> WordCategorizations { get; set; }

        IDistributedCache distributedCache;

        public AppDbContext(DbContextOptions<AppDbContext> options, IDistributedCache cache) : base(options)
        {
            distributedCache = cache;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Language>().HasData(
                new Language { Code = "es-ES", Name = "Spanish", LocalName = "Español", TotalSpeaker = 460 },
                new Language { Code = "en-US", Name = "English", LocalName = "English", TotalSpeaker = 1132 },
                new Language { Code = "ja-JP", Name = "Japanese", LocalName = "日本語", TotalSpeaker = 126 },
                new Language { Code = "fr-FR", Name = "French", LocalName = "Français", TotalSpeaker = 277, HasArticles = true },
                new Language { Code = "ko-KR", Name = "Korean", LocalName = "한국어", TotalSpeaker = 77 },
                new Language { Code = "de-DE", Name = "German", LocalName = "Deutsch", TotalSpeaker = 120, HasArticles = true },
                new Language { Code = "it-IT", Name = "Italian", LocalName = "Italiano", TotalSpeaker = 65, HasArticles = true },
                new Language { Code = "tr-TR", Name = "Turkish", LocalName = "Türkçe", TotalSpeaker = 82 },
                new Language { Code = "zh-CN", Name = "Mandarin Chinese", LocalName = "普通话", TotalSpeaker = 1121 },
                new Language { Code = "hi-IN", Name = "Hindi", LocalName = "हिन्दी", TotalSpeaker = 602 },
                new Language { Code = "ar", Name = "Arabic", LocalName = "العربية", TotalSpeaker = 319, HasArticles = true },
                new Language { Code = "bn-BD", Name = "Bengali", LocalName = "বাংলা", TotalSpeaker = 228 },
                new Language { Code = "pt-BR", Name = "Portuguese", LocalName = "Português", TotalSpeaker = 221 },
                new Language { Code = "ru-RU", Name = "Russian", LocalName = "Русский", TotalSpeaker = 154 },
                new Language { Code = "ro-RO", Name = "Romanian", LocalName = "Română", TotalSpeaker = 24 },
                new Language { Code = "uk-UA", Name = "Ukrainian", LocalName = "Українська", TotalSpeaker = 45 },
                new Language { Code = "sq-AL", Name = "Albanian", LocalName = "Shqip", TotalSpeaker = 8 },
                new Language { Code = "bs-BA", Name = "Bosnian", LocalName = "Bosanski Jezik", TotalSpeaker = 8 },
                new Language { Code = "pa-IN", Name = "Punjabi", LocalName = "ਪੰਜਾਬੀ", TotalSpeaker = 102 },
                new Language { Code = "id-ID", Name = "Indonesian", LocalName = "Bahasa Indonesia", TotalSpeaker = 199 },
                new Language { Code = "fa-IR", Name = "Persian", LocalName = "فارسی", TotalSpeaker = 78 },
                new Language { Code = "pl-PL", Name = "Polish", LocalName = "Polski", TotalSpeaker = 38 },
                new Language { Code = "hu-HU", Name = "Hungarian", LocalName = "Magyar", TotalSpeaker = 13 },
                new Language { Code = "lv-LV", Name = "Latvian", LocalName = "Latviešu", TotalSpeaker = 1.9 },
                new Language { Code = "mk-MK", Name = "Macedonian", LocalName = "Македонски", TotalSpeaker = 2.0 },
                new Language { Code = "no-NO", Name = "Norwegian", LocalName = "Norsk", TotalSpeaker = 5.4 },
                new Language { Code = "sk-SK", Name = "Slovak", LocalName = "Slovenčina", TotalSpeaker = 5.4 },
                new Language { Code = "sl-SI", Name = "Slovenian", LocalName = "Slovenščina", TotalSpeaker = 2.5 },
                new Language { Code = "sv-SE", Name = "Swedish", LocalName = "Svenska", TotalSpeaker = 10.3 },
                new Language { Code = "bg-BG", Name = "Bulgarian", LocalName = "Български", TotalSpeaker = 9.0 },
                new Language { Code = "hr-HR", Name = "Croatian", LocalName = "Hrvatski", TotalSpeaker = 5.6 },
                new Language { Code = "cs-CZ", Name = "Czech", LocalName = "Čeština", TotalSpeaker = 10.7 },
                new Language { Code = "da-DK", Name = "Danish", LocalName = "Dansk", TotalSpeaker = 5.7 },
                new Language { Code = "nl-NL", Name = "Dutch", LocalName = "Nederlands", TotalSpeaker = 24.1 },
                new Language { Code = "et-EE", Name = "Estonian", LocalName = "Eesti", TotalSpeaker = 1.1 },
                new Language { Code = "fi-FI", Name = "Finnish", LocalName = "Suomi", TotalSpeaker = 5.5 },
                new Language { Code = "el-GR", Name = "Greek", LocalName = "Ελληνικά", TotalSpeaker = 13.2 },
                new Language { Code = "ota", Name = "Ottoman Turkish", LocalName = "Osmanlı Türkçesi", TotalSpeaker = 13.2 }
            );

            modelBuilder.Entity<GoogleVoice>().HasData(
                new GoogleVoice { Id = 1, LanguageCode = "es-ES", Name = "es-ES-Wavenet-D", Alias = "WD", Gender = "Female" },
                new GoogleVoice { Id = 2, LanguageCode = "en-US", Name = "en-US-Wavenet-J", Alias = "WJ", Gender = "Male" },
                new GoogleVoice { Id = 3, LanguageCode = "ja-JP", Name = "ja-JP-Wavenet-D", Alias = "WD", Gender = "Male" },
                new GoogleVoice { Id = 4, LanguageCode = "fr-FR", Name = "fr-FR-Studio-A", Alias = "SA", Gender = "Female" },
                new GoogleVoice { Id = 5, LanguageCode = "ko-KR", Name = "ko-KR-Wavenet-D", Alias = "WD", Gender = "Male" },
                new GoogleVoice { Id = 6, LanguageCode = "de-DE", Name = "de-DE-Studio-C", Alias = "SC", Gender = "Female" },
                new GoogleVoice { Id = 7, LanguageCode = "it-IT", Name = "it-IT-Wavenet-D", Alias = "WD", Gender = "Male" },
                new GoogleVoice { Id = 8, LanguageCode = "tr-TR", Name = "tr-TR-Wavenet-D", Alias = "WD", Gender = "Female" }
            );

            //modelBuilder.Entity<Topic>().HasData(new List<Topic>
            //{
            //    new() { Id = 1, Name = "Business & Travel", Order = 1 },
            //    new() { Id = 2, Name = "Chaos & Order", Order = 2 },
            //    new() { Id = 3, Name = "Farming & Cooking", Order = 3 },
            //    new() { Id = 4, Name = "Fundamentals", Order = 4 },
            //    new() { Id = 5, Name = "Nature", Order = 5 },
            //    new() { Id = 6, Name = "People", Order = 6 },
            //    new() { Id = 7, Name = "Science", Order = 7 }
            //});

            //modelBuilder.Entity<Subtopic>().HasData(new List<Subtopic>
            //{
            //    new() { Id = 1, Name = "Computers", TopicId = 1, Order = 1 },
            //    new() { Id = 2, Name = "Jobs", TopicId = 1, Order = 2 },
            //    new() { Id = 3, Name = "Trade", TopicId = 1, Order = 3 },
            //    new() { Id = 4, Name = "Travel", TopicId = 1, Order = 4 },
            //    new() { Id = 5, Name = "Work", TopicId = 1, Order = 5 },

            //    new() { Id = 6, Name = "Government", TopicId = 2, Order = 1 },
            //    new() { Id = 7, Name = "Law", TopicId = 2, Order = 2 },
            //    new() { Id = 8, Name = "Religion", TopicId = 2, Order = 3 },
            //    new() { Id = 9, Name = "War", TopicId = 2, Order = 4 },

            //    new() { Id = 10, Name = "Cooking", TopicId = 3, Order = 1 },
            //    new() { Id = 11, Name = "Drinks", TopicId = 3, Order = 2 },
            //    new() { Id = 12, Name = "Farming", TopicId = 3, Order = 3 },
            //    new() { Id = 13, Name = "Foods", TopicId = 3, Order = 4 },
            //    new() { Id = 14, Name = "Fruits", TopicId = 3, Order = 5 },
            //    new() { Id = 15, Name = "Kitchen", TopicId = 3, Order = 6 },
            //    new() { Id = 16, Name = "Vegetables", TopicId = 3, Order = 7 },

            //    new() { Id = 17, Name = "Colors & Shapes", TopicId = 4, Order = 1 },
            //    new() { Id = 18, Name = "Nations & Languages", TopicId = 4, Order = 2 },
            //    new() { Id = 19, Name = "Numbers & Ordinals", TopicId = 4, Order = 3 },
            //    new() { Id = 20, Name = "Time", TopicId = 4, Order = 4 },
            //    new() { Id = 21, Name = "Verbs", TopicId = 4, Order = 5 },
            //    new() { Id = 22, Name = "Nouns", TopicId = 4, Order = 6 },
            //    new() { Id = 23, Name = "Adjectives", TopicId = 4, Order = 7 },
            //    new() { Id = 24, Name = "Adverbs", TopicId = 4, Order = 8 },

            //    new() { Id = 25, Name = "Animals", TopicId = 5, Order = 1 },
            //    new() { Id = 26, Name = "Disaster", TopicId = 5, Order = 2 },
            //    new() { Id = 27, Name = "Flowers", TopicId = 5, Order = 3 },
            //    new() { Id = 28, Name = "Plants", TopicId = 5, Order = 4 },
            //    new() { Id = 29, Name = "Trees", TopicId = 5, Order = 5 },
            //    new() { Id = 30, Name = "Weather", TopicId = 5, Order = 6 },

            //    new() { Id = 31, Name = "Body", TopicId = 6, Order = 1 },
            //    new() { Id = 32, Name = "Clothes", TopicId = 6, Order = 2 },
            //    new() { Id = 33, Name = "Education", TopicId = 6, Order = 3 },
            //    new() { Id = 34, Name = "Events & Activities", TopicId = 6, Order = 4 },
            //    new() { Id = 35, Name = "Health", TopicId = 6, Order = 5 },
            //    new() { Id = 36, Name = "Home", TopicId = 6, Order = 6 },
            //    new() { Id = 37, Name = "Sports", TopicId = 6, Order = 7 },

            //    new() { Id = 38, Name = "Biology", TopicId = 7, Order = 1 },
            //    new() { Id = 39, Name = "Chemistry", TopicId = 7, Order = 2 },
            //    new() { Id = 40, Name = "Geography", TopicId = 7, Order = 3 },
            //    new() { Id = 41, Name = "Math", TopicId = 7, Order = 4 },
            //    new() { Id = 42, Name = "Medicine", TopicId = 7, Order = 5 },
            //    new() { Id = 43, Name = "Physics", TopicId = 7, Order = 6 },
            //    new() { Id = 44, Name = "Space", TopicId = 7, Order = 7 },
            //    new() { Id = 45, Name = "Quantities", TopicId = 7, Order = 8 } }
            //);
        }

    }
}
