using duoword.admin.Server.Data;
using duoword.admin.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace duoword.admin.Server.Controllers
{
    [Route("/api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class MigrationController : ControllerBase
    {
        AppDbContext dbContext;
        public MigrationController(AppDbContext context, IConfiguration cfg)
        {
            dbContext = context;
        }

        [Route("ws/start")]
        public async Task<IActionResult> Start()
        {
            if (!HttpContext.WebSockets.IsWebSocketRequest)
                return BadRequest();

            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            await MigrateWithWebSocket(webSocket);
            return new EmptyResult();
        }

        private async Task MigrateWithWebSocket(WebSocket webSocket)
        {
            try
            {
                await SendMessage(webSocket, new { message = "Migration started." });

                //Dictionaries
               //await SendMessage(webSocket, new { message = "Importing dictionaries..." });
               // string dictionaryFolderPath = Path.Combine(Location.dataPath, "dictionaries");
               // DictionaryImporter.ImportDictionaries(dbContext, dictionaryFolderPath);
               // await SendMessage(webSocket, new { percent = 20, message = "Dictionaries imported." });

                // Lexicon
                await SendMessage(webSocket, new { message = "Migrating words..." });
                string wordsFilePath = Path.Combine(Location.dataPath, "words.json");
                string levelCatalogFilePath = Path.Combine(Location.dataPath, "levelcatalog.json");
                LexiconMigrationService migrationService = new LexiconMigrationService(dbContext, levelCatalogFilePath);
                migrationService.MigrateWords(wordsFilePath);
                await SendMessage(webSocket, new { percent = 60, message = "Words migrated." });

                // Lesson catalog
                await SendMessage(webSocket, new { message = "Migrating lesson catalog..." });
                string lessonCatalogFilePath = Path.Combine(Location.dataPath, "lessoncatalog.json");
                LessonCatalogMigrationService lessonMigrationService = new LessonCatalogMigrationService(dbContext);
                lessonMigrationService.MigrateFromJson(lessonCatalogFilePath);
                await SendMessage(webSocket, new { percent = 80, message = "Catalogs migrated." });

                //Locale Migration
                await SendMessage(webSocket, new { message = "Migrating Locales..." });
                LocaleMigrationService localeMigrationService = new LocaleMigrationService(dbContext);
                localeMigrationService.MigrateTranslationsFromDirectory(Path.Combine(Location.dataPath, "ui_translations"));
                await SendMessage(webSocket, new { percent = 100, message = "Locales migrated." });

                await SendMessage(webSocket, new { percent = 100, message = "Migration completed." });
            }
            catch (Exception ex)
            {
                await SendMessage(webSocket, new { percent = 0, message = "Migration failed.", error = ex.Message });
            }
            finally
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Migration finished", CancellationToken.None);
            }
        }

        private async Task SendMessage(WebSocket webSocket, object data)
        {
            var json = JsonSerializer.Serialize(data);
            var bytes = Encoding.UTF8.GetBytes(json);
            await webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }



        [Route("ws/fake")]
        public async Task<IActionResult> FakeMigrate()
        {
            if (!HttpContext.WebSockets.IsWebSocketRequest)
                return BadRequest("Not a WebSocket request");

            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            await FakeMigrationProcess(webSocket);
            return new EmptyResult();
        }

        private async Task FakeMigrationProcess(WebSocket webSocket)
        {
            try
            {
                await SendMessage(webSocket, new { percent = 0, message = "Starting migration..." });

                int progress = 0;
                string[] steps = {
                    "Initializing database...",
                    "Importing dictionaries...",
                    "Migrating words...",
                    "Processing lesson catalog...",
                    "Finalizing migration..."
                };

                foreach (var step in steps)
                {
                    progress += 20; // Increase progress by 20% at each step
                    await SendMessage(webSocket, new { percent = progress, message = step });
                    await Task.Delay(1500); // Simulate delay
                }

                await SendMessage(webSocket, new { percent = 100, message = "Migration completed successfully!" });
            }
            catch (Exception ex)
            {
                await SendMessage(webSocket, new { percent = 0, message = "Migration failed!", error = ex.Message });
            }
            finally
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Migration finished", CancellationToken.None);
            }
        }
    }
}