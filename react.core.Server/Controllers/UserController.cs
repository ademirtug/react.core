using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using duoword.admin.Server.Models;
using duoword.admin.Server.Repositories;

namespace duoword.admin.Server.Controllers
{
    [Route("api/v1/auth/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")] // Require Admin privileges
    public class UserController : ControllerBase
    {
        private readonly IRepository<User> _userRepository;

        public UserController(IRepository<User> userRepository)
        {
            _userRepository = userRepository;
        }

        // POST: api/User
        [HttpPost]
        public IActionResult AddUser([FromBody] User user)
        {
            if (string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(user.PasswordHash))
                return BadRequest("Username and PasswordHash are required.");

            _userRepository.Insert(user);
            _userRepository.SaveChanges();

            return Ok(user);
        }

        // DELETE: api/User/5
        [HttpDelete("{id}")]
        public IActionResult DeleteUser(int id)
        {
            var existingUser = _userRepository.Get(id);
            if (existingUser == null)
                return NotFound($"User with ID {id} not found.");

            _userRepository.Delete(id);
            _userRepository.SaveChanges();

            return NoContent();
        }
    }
}
