using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using OwListy.ViewModels.ListsViewModels;
using OwListy.Models;
using OwListy.Data;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OwListy.Controllers
{
    [ApiController]
    [Route("api/lists"), Authorize]
    public class ListsController : ControllerBase
    {
        private readonly OwListyDbContext _context;

        public ListsController(OwListyDbContext context)
        {
            _context = context;
        }

        [HttpGet("{listId}")]
        public async Task<ActionResult> GetList(int listId)
        {
            try
            {
                var list = await _context.Lists
                    .Include(g => g.ListItems)
                    .Select(
                        l =>
                            new
                            {
                                Id = l.Id,
                                Title = l.Title,
                                Color = l.Color,
                                GroupId = l.GroupId,
                                CreatedAt = l.CreatedAt,
                                UpdatedAt = l.UpdatedAt,
                                LastItems = l.ListItems
                                    .OrderByDescending(i => i.CreatedAt)
                                    .Take(3)
                                    .Select(
                                        i =>
                                            new
                                            {
                                                Id = i.Id,
                                                Content = i.Content,
                                                Completed = i.Completed,
                                            }
                                    )
                                    .ToList()
                            }
                    )
                    .FirstOrDefaultAsync(i => i.Id == listId);

                if (list == null)
                    return NotFound();

                var response = list;
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Erro ao buscar lista.", error = ex.Message });
            }
        }

        [HttpGet("{groupId}/lists")]
        public async Task<ActionResult> GetGroupLists(int groupId)
        {
            try
            {
                var lists = await _context.Lists
                    .Include(g => g.ListItems)
                    .Where(l => l.GroupId == groupId)
                    .Select(
                        l =>
                            new
                            {
                                Id = l.Id,
                                Title = l.Title,
                                Color = l.Color,
                                GroupId = l.GroupId,
                                CreatedAt = l.CreatedAt,
                                UpdatedAt = l.UpdatedAt,
                                LastItems = l.ListItems
                                    .OrderByDescending(i => i.CreatedAt)
                                    .Take(3)
                                    .Select(
                                        i =>
                                            new
                                            {
                                                Id = i.Id,
                                                Content = i.Content,
                                                Completed = i.Completed,
                                            }
                                    )
                                    .ToList()
                            }
                    )
                    .ToListAsync();

                return Ok(lists);
            }
            catch (Exception ex)
            {
                return BadRequest(
                    new { message = "Erro ao buscar listas do grupo.", error = ex.Message }
                );
            }
        }

        [HttpPost("create")]
        public async Task<ActionResult> CreateList([FromBody] CreateListViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            try
            {
                var group = await _context.Groups.FirstOrDefaultAsync(g => g.Id == model.GroupId);
                if (group == null)
                    return NotFound(new { message = "Grupo não encontrado." });

                var list = new List
                {
                    Title = model.Title,
                    Color = model.Color,
                    GroupId = model.GroupId
                };

                await _context.Lists.AddAsync(list);
                await _context.SaveChangesAsync();

                var response = new
                {
                    Id = list.Id,
                    Title = list.Title,
                    Color = list.Color,
                    CreatedAt = list.CreatedAt,
                    UpdatedAt = list.UpdatedAt,
                };
                return Created("api/lists/create", response);
            }
            catch (Exception ex)
            {
                return BadRequest(
                    new { message = "Erro ao criar nova lista.", error = ex.Message }
                );
            }
        }

        [HttpPut("update")]
        public async Task<ActionResult> UpdateList([FromBody] UpdateListViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            try
            {
                var list = await _context.Lists.FirstOrDefaultAsync(l => l.Id == model.Id);
                if (list == null)
                    return NotFound(new { message = "Lista não encontrada." });

                list.Title = model.Title;
                list.Color = model.Color;

                _context.Lists.Update(list);
                await _context.SaveChangesAsync();

                var response = new
                {
                    Id = list.Id,
                    Title = list.Title,
                    Color = list.Color,
                    CreatedAt = list.CreatedAt,
                    UpdatedAt = list.UpdatedAt,
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Erro ao atualizar lista.", error = ex.Message });
            }
        }

        [HttpDelete("delete")]
        public async Task<ActionResult> DeleteList([FromBody] DeleteListViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            try
            {
                var list = await _context.Lists
                    .Include(l => l.ListItems)
                    .FirstOrDefaultAsync(l => l.Id == model.ListId);
                if (list == null)
                    return NotFound(new { message = "Lista não encontrada." });

                var listItems = list.ListItems.ToList();
                _context.ListItems.RemoveRange(listItems);

                _context.Lists.Remove(list);
                await _context.SaveChangesAsync();

                return Ok("Lista deletada");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Erro ao deletar lista.", error = ex.Message });
            }
        }

        [HttpGet("items/{listId}")]
        public async Task<ActionResult> GetListItems(int listId)
        {
            try
            {
                var list = await _context.Lists.FirstOrDefaultAsync(l => l.Id == listId);
                if (list == null)
                    return NotFound(new { message = "Lista não encontrada." });

                var listItems = await _context.ListItems
                    .OrderByDescending(i => i.CreatedAt)
                    .Where(i => i.ListId == listId)
                    .Select(
                        i =>
                            new
                            {
                                Id = i.Id,
                                Content = i.Content,
                                Completed = i.Completed,
                                CreatedAt = i.CreatedAt,
                                UpdatedAt = i.UpdatedAt,
                            }
                    )
                    .ToListAsync();

                return Ok(listItems);
            }
            catch (Exception ex)
            {
                return BadRequest(
                    new { message = "Erro ao buscar itens da lista.", error = ex.Message }
                );
            }
        }

        [HttpPost("items/create")]
        public async Task<ActionResult> CreateListItem([FromBody] CreateListItemViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            try
            {
                var list = await _context.Lists.FirstOrDefaultAsync(l => l.Id == model.ListId);
                if (list == null)
                    return NotFound(new { message = "Lista não encontrada." });

                var listItem = new ListItem
                {
                    ListId = model.ListId,
                    Content = model.Content,
                    Completed = model.Completed
                };

                await _context.ListItems.AddAsync(listItem);
                await _context.SaveChangesAsync();

                var response = new
                {
                    Id = listItem.Id,
                    Content = listItem.Content,
                    Completed = listItem.Completed,
                    CreatedAt = listItem.CreatedAt,
                    UpdatedAt = listItem.UpdatedAt,
                };

                return Created($"api/lists/items/create", response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Erro ao criar novo item.", error = ex.Message });
            }
        }

        [HttpPut("items/update")]
        public async Task<ActionResult> UpdateListItem([FromBody] UpdateListItemViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            try
            {
                var listItem = await _context.ListItems.FirstOrDefaultAsync(i => i.Id == model.Id);
                if (listItem == null)
                    return NotFound(new { message = "Item não encontrado." });

                listItem.Content = model.Content;
                listItem.Completed = model.Completed;
                listItem.UpdatedAt = DateTime.Now;

                _context.ListItems.Update(listItem);
                await _context.SaveChangesAsync();

                var response = new
                {
                    Id = listItem.Id,
                    Content = listItem.Content,
                    Completed = listItem.Completed,
                    CreatedAt = listItem.CreatedAt,
                    UpdatedAt = listItem.UpdatedAt,
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Erro ao atualizar item.", error = ex.Message });
            }
        }

        [HttpDelete("items/delete")]
        public async Task<ActionResult> DeleteListItem([FromBody] DeleteListItemViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            try
            {
                var listItem = await _context.ListItems.FirstOrDefaultAsync(
                    i => i.Id == model.ListItemId
                );
                if (listItem == null)
                    return NotFound(new { message = "Item não encontrado." });

                _context.ListItems.Remove(listItem);
                await _context.SaveChangesAsync();

                return Ok("Item deletado");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Erro ao deletar item.", error = ex.Message });
            }
        }
    }
}
