using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using OwListy.ViewModels.AuthModels;
using OwListy.Services;
using OwListy.Models;
using OwListy.Data;
using RestSharp;

namespace OwListy.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly OwListyDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly string _baseUrl;

        public AuthController(OwListyDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
            _baseUrl = _env.IsDevelopment() ? Settings.DevUrl : Settings.ProdUrl;
        }

        [HttpGet, Authorize]
        public async Task<ActionResult> GetUser()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue("id"));

                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    return NotFound();

                var response = new
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt,
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Erro ao buscar usuário.", error = ex.Message });
            }
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody] RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            try
            {
                var validEmail = Utils.IsValidEmail(model.Email);
                if (!validEmail)
                    return BadRequest(new { message = "O email não é válido." });

                var existingUser = await _context.Users.FirstOrDefaultAsync(
                    u => u.Email == model.Email
                );
                if (existingUser != null)
                    return BadRequest(new { message = "Um usuário com esse email já existe." });

                string passwordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);

                var user = new User
                {
                    Name = model.Name,
                    Email = model.Email,
                    PasswordHash = passwordHash,
                };

                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();

                var response = new
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt,
                };
                return Created("api/auth/register", response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Erro ao criar usuário.", error = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                if (user == null)
                    return NotFound();

                if (!BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
                    return Unauthorized(new { message = "Senha incorreta." });

                string token = TokenService.GenerateToken(user);

                var response = new { Token = token };
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Erro ao criar usuário.", error = ex.Message });
            }
        }

        [HttpDelete("delete")]
        public async Task<ActionResult> DeleteUser()
        {
            var userId = int.Parse(User.FindFirstValue("id"));
            var requestJwt = Request.Headers["Authorization"][0] ?? "";
            var restClient = new RestClient();
            restClient.AddDefaultHeader("Authorization", requestJwt);

            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                    return NotFound();

                var userGroups = await _context.Groups
                    .Include(g => g.GroupMembers)
                    .ThenInclude(m => m.User)
                    .Where(g => g.GroupMembers.Any(m => m.UserId == user.Id))
                    .Select(
                        g =>
                            new
                            {
                                Id = g.Id,
                                Name = g.Name,
                                Color = (string?)g.Color,
                                CreatorId = g.CreatorId,
                                CreatedAt = g.CreatedAt,
                                UpdatedAt = g.UpdatedAt,
                                Members = g.GroupMembers
                                    .Select(
                                        m =>
                                            new
                                            {
                                                Id = m.Id,
                                                UserId = m.UserId,
                                                Name = m.User.Name
                                            }
                                    )
                                    .ToList()
                            }
                    )
                    .ToListAsync();

                foreach (var group in userGroups)
                {
                    if (group.Members.Count > 1)
                    {
                        int memberId = group.Members.First(m => m.UserId == user.Id).Id;
                        var payload = new { memberId = memberId };

                        var request = new RestRequest(
                            $"{_baseUrl}/api/groups/members/remove",
                            Method.Delete
                        );
                        request.AddJsonBody(payload);

                        var response = await restClient.ExecuteAsync(request);
                        if (!response.IsSuccessful)
                            return BadRequest(
                                new
                                {
                                    message = "Erro ao deletar usuário de membros.",
                                    error = response.Content
                                }
                            );
                    }
                    else if (group.Members.Count == 1)
                    {
                        int groupId = group.Id;
                        var payload = new { groupId = groupId };

                        var request = new RestRequest(
                            $"{_baseUrl}/api/groups/delete",
                            Method.Delete
                        );
                        request.AddJsonBody(payload);

                        var response = await restClient.ExecuteAsync(request);
                        if (!response.IsSuccessful)
                            return BadRequest(
                                new
                                {
                                    message = "Erro ao deletar grupos do usuário.",
                                    error = response.Content
                                }
                            );
                    }
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                return Ok("Usuário deletado");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Erro ao deletar usuário.", error = ex.Message });
            }
        }

        [HttpPost("password/change"), Authorize]
        public async Task<ActionResult<string>> ChangePassword(
            [FromBody] ChangePasswordViewModel model
        )
        {
            var userId = int.Parse(User.FindFirstValue("id"));
            if (!ModelState.IsValid)
                return BadRequest();

            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                    return NotFound();

                if (BCrypt.Net.BCrypt.Verify(model.NewPassword, user.PasswordHash))
                    return BadRequest(new { message = "Você já utiliza essa senha." });

                string newPasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);

                user.PasswordHash = newPasswordHash;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                return Ok("Senha alterada!");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Erro ao alterar senha.", error = ex.Message });
            }
        }

        [HttpPost("password/forgot")]
        public async Task<ActionResult<string>> ForgotPassword(
            [FromBody] ForgotPasswordViewModel model
        )
        {
            if (!ModelState.IsValid)
                return BadRequest();

            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                if (user == null)
                    return NotFound();

                string randomCode = Utils.GenerateRandomCode();
                string validationToken = BCrypt.Net.BCrypt.HashPassword(randomCode);

                user.ValidationToken = validationToken;
                user.ValidationTokenExp = DateTime.Now.AddMinutes(5);

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                string emailTemplate = System.IO.File.ReadAllText(
                    "./Templates/ForgotPasswordEmailTemplate.html"
                );
                await EmailService.SendEmail(
                    user.Email,
                    "Recuperação de Senha",
                    emailTemplate.Replace("{{code}}", randomCode)
                );

                return Ok(
                    "Um email com as informações de recuperação de senha foi enviado para você, por favor verifique sua caixa postal!"
                );
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Erro ao recuperar senha.", error = ex.Message });
            }
        }

        [HttpPost("password/reset")]
        public async Task<ActionResult<string>> ResetPassword(
            [FromBody] ResetPasswordViewModel model
        )
        {
            if (!ModelState.IsValid)
                return BadRequest();

            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                if (user == null)
                    return NotFound(new { message = "Usuário não encontrado." });

                if (
                    user.ValidationTokenExp < DateTime.Now
                    || !BCrypt.Net.BCrypt.Verify(model.ValidationCode, user.ValidationToken)
                )
                    return Unauthorized(new { message = "Código inválido." });

                if (BCrypt.Net.BCrypt.Verify(model.NewPassword, user.PasswordHash))
                    return BadRequest(new { message = "Você já utiliza essa senha." });

                string newPasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);

                user.PasswordHash = newPasswordHash;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                return Ok("Senha redefinida com sucesso!");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Erro ao redefinir senha.", error = ex.Message });
            }
        }
    }
}
