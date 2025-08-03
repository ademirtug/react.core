using duoword.admin.Server.Services;
using Leximo.Service.Backup;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace duoword.admin.Server.Controllers
{
    [ApiController]
    [Route("api/v1/")]
    public class BackupDriveController : ControllerBase
    {
        private readonly BackupService _backupService;

        public BackupDriveController(BackupService backupService)
        {
            _backupService = backupService;
        }

        [HttpGet("backup")]
        public async Task<IActionResult> Backup()
        {
            try
            {
                var failedBackups = await _backupService.PerformBackupAsync();
                if (failedBackups.Count == 0)
                {
                    return Ok("Backup triggered successfully.");
                }
                else
                {
                    return BadRequest(new { message = "Backup failed for the following databases: " + string.Join(", ", failedBackups) });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}

