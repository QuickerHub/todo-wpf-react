using Microsoft.AspNetCore.Mvc;
using TodoApp.Core.Models;

namespace TodoApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TodoTagsController : ControllerBase
{
    private static readonly List<TodoTag> _tags = new();
    private static int _nextId = 1;

    [HttpGet]
    public ActionResult<IEnumerable<TodoTag>> GetTags()
    {
        return Ok(_tags);
    }

    [HttpGet("{id}")]
    public ActionResult<TodoTag> GetTag(int id)
    {
        var tag = _tags.FirstOrDefault(t => t.Id == id);
        if (tag == null)
            return NotFound();

        return Ok(tag);
    }

    [HttpPost]
    public ActionResult<TodoTag> CreateTag(TodoTag tag)
    {
        tag.Id = _nextId++;
        tag.CreatedAt = DateTime.UtcNow;

        _tags.Add(tag);

        return CreatedAtAction(nameof(GetTag), new { id = tag.Id }, tag);
    }

    [HttpPut("{id}")]
    public ActionResult<TodoTag> UpdateTag(int id, TodoTag updatedTag)
    {
        var tag = _tags.FirstOrDefault(t => t.Id == id);
        if (tag == null)
            return NotFound();

        tag.Name = updatedTag.Name;
        tag.Color = updatedTag.Color;

        return Ok(tag);
    }

    [HttpDelete("{id}")]
    public ActionResult DeleteTag(int id)
    {
        var tag = _tags.FirstOrDefault(t => t.Id == id);
        if (tag == null)
            return NotFound();

        _tags.Remove(tag);
        return NoContent();
    }
}
