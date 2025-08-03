using duoword.admin.Server.Data;
using duoword.admin.Server.Repositories;
using duoword.admin.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using System.Text;


namespace duoword.admin.Server.Controllers
{
    [Route("/api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class LocaleController : ControllerBase
    {
        IRepository<Locale> locales;
        IChatClient AI;
        LocaleService localeService;

        public LocaleController(IRepository<Locale> rep, IChatClient chatClient, LocaleService service)
        {
            locales = rep;
            AI = chatClient;
            localeService = service;
        }


        [HttpGet("all")]
        public IActionResult All()
        {
            return Ok(locales.GetAll().ToList());
        }

        [HttpGet("get/{id}")]
        public IActionResult Get(int id)
        {
            Locale? dict = locales.Get(id);
            return dict == null ? NotFound() : Ok(dict);
        }

        [HttpGet("search/{id}")]
        public IActionResult Search(string code)
        {
            var result = locales.Where(d => d.LanguageCode == code);
            return result.Count() > 0 ? Ok(result.ToList()) : NotFound();
        }

        [HttpPost("add")]
        public IActionResult Add([FromForm] string languageCode, IFormFile? file)
        {
            if (string.IsNullOrWhiteSpace(languageCode))
            {
                return BadRequest("Language code is required.");
            }

            Locale? lc = localeService.Deserialize(file);
            lc ??= new Locale { LanguageCode = languageCode };

            locales.Insert(lc);
            locales.SaveChanges();
            return Ok(new LocaleInfo { Id = lc.Id, LanguageCode = lc.LanguageCode, TranslationCount = lc.Entries.Count });
        }


        [HttpDelete("delete/{id}")]
        public IActionResult Delete(int id)
        {
            locales.Delete(id);
            locales.SaveChanges();
            return Ok();
        }

        [HttpGet("infolist")]
        public IActionResult InfoList()
        {
            var result = locales.Select(lc => new LocaleInfo
            {
                Id = lc.Id,
                LanguageCode = lc.LanguageCode,
                TranslationCount = lc.Entries.Count
            }).OrderBy(lc => lc.Id);
            return result.Count() > 0 ? Ok(result.ToList()) : NotFound();
        }

        [HttpGet("download/{id}")]
        public IActionResult Download(int id)
        {
            var (fileName, fileContent) = localeService.GetJson(id);
            return Ok(new { fileName, fileContent = Convert.ToBase64String(Encoding.UTF8.GetBytes(fileContent)) });
        }


        [HttpGet("downloadall")]
        public IActionResult DownloadAll()
        {
            return Ok(new { fileName = "locales.zip", fileContent = localeService.ZipBundle() });
        }

        [HttpGet("export/{id}")]
        public IActionResult Export(int id)
        {
            (string fileName, string fileContent) = localeService.GetArb(id);
            return Ok(new { fileName, fileContent });
        }


        [HttpGet("exportall")]
        public IActionResult ExportAll()
        {
            return Ok(new { fileName = "arb_locales.zip", fileContent = localeService.GetArbBundle() });
        }

        [HttpPost("update/{id}")]
        public IActionResult Update(int id, IFormFile file)
        {
            Locale? lc = locales.Get(id);
            if (lc == null)
                return NotFound();

            Locale? upload = localeService.Deserialize(file);
            if (upload == null)
                return BadRequest("Corrupt File");

            lc.Entries.Clear();
            foreach (var entry in upload.Entries)
            {
                lc.Entries.Add(entry);
            }

            locales.Update(lc);
            locales.SaveChanges();

            return Ok();
        }

        [HttpGet("sync/{id}")]
        public async Task<IActionResult> Sync(int id)
        {
            await localeService.Queue(id);
            return Ok(new { translationCount = -1 });
        }


        [HttpGet("syncall")]
        public async Task<IActionResult> SyncAll()
        {
            Locale? en = locales.Where(l => l.LanguageCode == "en-US").Include(l => l.Entries).FirstOrDefault();
            if (en == null)
            {
                return NotFound();
            }

            var allLocales = locales.Include(l => l.Entries);
            foreach (var locale in allLocales)
            {
                await localeService.Queue(locale.Id);
            }

            return Ok(allLocales.Select(lc => new LocaleInfo(lc)).ToList());
        }
    }

    public class LocaleInfo
    {
        public int Id { get; set; }
        public string LanguageCode { get; set; } = "";
        public int TranslationCount { get; set; }
        public LocaleInfo() { }
        public LocaleInfo(Locale lc)
        {
            Id = lc.Id;
            LanguageCode = lc.LanguageCode;
            TranslationCount = lc.Entries.Count;
        }
    }
}
