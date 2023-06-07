using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using OwListy.ViewModels.GroupsModels;
using OwListy.Models;
using OwListy.Data;

namespace OwListy.Controllers
{
    [Route("api/groups"), Authorize]
    [ApiController]
    public class GroupsController : ControllerBase
    {
        private readonly OwListyDbContext _context;

        public GroupsController(OwListyDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult> GetUserGroups()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue("id"));

                var groups = await _context.Groups
                    .Include(g => g.GroupMembers)
                    .ThenInclude(m => m.User)
                    .Where(g => g.GroupMembers.Any(m => m.UserId == userId))
                    .Select(
                        g =>
                            new
                            {
                                Id = g.Id,
                                Name = g.Name,
                                Color = g.Color,
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

                var response = groups;
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Erro ao buscar grupos.", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetGroup([FromRoute] int id)
        {
            try
            {
                var group = await _context.Groups
                    .Include(g => g.GroupMembers)
                    .ThenInclude(m => m.User)
                    .Select(
                        g =>
                            new
                            {
                                Id = g.Id,
                                Name = g.Name,
                                Color = g.Color,
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
                    .FirstOrDefaultAsync(i => i.Id == id);
                if (group == null)
                    return NotFound();

                var response = group;
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Erro ao buscar grupo.", error = ex.Message });
            }
        }

        [HttpPost("create")]
        public async Task<ActionResult> CreateGroup([FromBody] CreateGroupViewModel model)
        {
            var userId = int.Parse(User.FindFirstValue("id"));
            if (!ModelState.IsValid)
                return BadRequest();

            try
            {
                var group = new Group
                {
                    Name = model.Name,
                    Color = model.Color,
                    CreatorId = userId,
                };
                _context.Groups.Add(group);
                await _context.SaveChangesAsync();

                var groupMember = new GroupMember { GroupId = group.Id, UserId = group.CreatorId };
                _context.GroupMembers.Add(groupMember);
                await _context.SaveChangesAsync();

                var response = new
                {
                    Id = group.Id,
                    Name = group.Name,
                    Color = group.Color,
                    CreatorId = group.CreatorId,
                    CreatedAt = group.CreatedAt,
                    UpdatedAt = group.UpdatedAt,
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Erro ao criar grupo.", error = ex.Message });
            }
        }

        [HttpPut("update")]
        public async Task<ActionResult> UpdateGroup([FromBody] UpdateGroupViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            try
            {
                var group = await _context.Groups.FirstOrDefaultAsync(i => i.Id == model.GroupId);
                if (group == null)
                    return NotFound();

                group.Name = model.Name;
                group.Color = model.Color;

                _context.Groups.Update(group);
                await _context.SaveChangesAsync();

                var response = new
                {
                    Id = group.Id,
                    Name = group.Name,
                    Color = group.Color,
                    CreatorId = group.CreatorId,
                    CreatedAt = group.CreatedAt,
                    UpdatedAt = group.UpdatedAt,
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Erro ao atualizar grupo.", error = ex.Message });
            }
        }

        [HttpDelete("delete")]
        public async Task<ActionResult> DeleteGroup([FromBody] DeleteGroupViewModel model)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue("id"));

                var group = await _context.Groups
                    .Include(g => g.GroupMembers)
                    .Include(l => l.Lists)
                    .ThenInclude(i => i.ListItems)
                    .FirstOrDefaultAsync(i => i.Id == model.GroupId);

                if (group == null)
                    return NotFound();
                if (group.CreatorId != userId)
                    return Unauthorized("Somente o criador pode deletar o grupo");

                var lists = group.Lists.ToList();
                foreach (var list in lists)
                {
                    var listItems = list.ListItems.ToList();
                    _context.ListItems.RemoveRange(listItems);
                }

                _context.Lists.RemoveRange(lists);

                var groupMembers = group.GroupMembers.ToList();
                _context.GroupMembers.RemoveRange(groupMembers);

                _context.Groups.Remove(group);
                await _context.SaveChangesAsync();
                return Ok("Grupo deletado");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Erro ao deletar grupo.", error = ex.Message });
            }
        }

        [HttpPost("members/add")]
        public async Task<ActionResult> AddGroupMember([FromBody] AddMemberViewModel model)
        {
            try
            {
                var group = await _context.Groups.FirstOrDefaultAsync(i => i.Id == model.GroupId);
                if (group == null)
                    return NotFound(new { message = "Grupo não encontrado." });

                var user = await _context.Users.FirstOrDefaultAsync(
                    i => i.Email == model.UserEmail
                );
                if (user == null)
                    return NotFound(new { message = "Usuário não encontrado." });

                var existingMember = await _context.GroupMembers.FirstOrDefaultAsync(
                    i => i.GroupId == model.GroupId && i.UserId == user.Id
                );
                if (existingMember != null)
                    return BadRequest(new { message = "Usuário já é membro do grupo." });

                var newMember = new GroupMember { GroupId = model.GroupId, UserId = user.Id };

                await _context.GroupMembers.AddAsync(newMember);
                await _context.SaveChangesAsync();

                return Ok("Membro adicionado com sucesso!");
            }
            catch (Exception ex)
            {
                return BadRequest(
                    new { message = "Erro ao adicionar membro de grupo.", error = ex.Message }
                );
            }
        }

        [HttpDelete("members/remove")]
        public async Task<ActionResult> RemoveGroupMember([FromBody] RemoveMemberViewModel model)
        {
            try
            {
                var member = await _context.GroupMembers
                    .Include(m => m.Group)
                    .FirstOrDefaultAsync(m => m.Id == model.MemberId);

                if (member == null)
                    return NotFound(new { message = "Membro não encontrado no grupo." });

                var group = member.Group;

                if (group.CreatorId == member.UserId)
                {
                    var newCreator = await _context.GroupMembers
                        .Where(m => m.GroupId == group.Id && m.UserId != member.UserId)
                        .OrderBy(m => m.Id)
                        .FirstOrDefaultAsync();

                    if (newCreator != null)
                    {
                        group.CreatorId = newCreator.UserId;
                    }
                    else
                    {
                        return Unauthorized(
                            new
                            {
                                message = "Não é possível sair de um grupo somente com você. Ao invés disso delete esse grupo."
                            }
                        );
                    }
                }

                _context.GroupMembers.Remove(member);
                await _context.SaveChangesAsync();

                return Ok("Membro removido com sucesso!");
            }
            catch (Exception ex)
            {
                return BadRequest(
                    new { message = "Erro ao remover membro do grupo.", error = ex.Message }
                );
            }
        }
    }
}
