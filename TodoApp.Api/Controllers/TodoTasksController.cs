using Microsoft.AspNetCore.Mvc;
using TodoApp.Core.Models;

namespace TodoApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TodoTasksController : ControllerBase
{
    private static readonly List<TodoTask> _tasks = new();
    private static int _nextId = 1;

    [HttpGet]
    public ActionResult<IEnumerable<TodoTask>> GetTasks()
    {
        return Ok(_tasks);
    }

    [HttpGet("{id}")]
    public ActionResult<TodoTask> GetTask(int id)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == id);
        if (task == null)
            return NotFound();

        return Ok(task);
    }

    [HttpPost]
    public ActionResult<TodoTask> CreateTask(TodoTask task)
    {
        task.Id = _nextId++;
        task.CreatedAt = DateTime.UtcNow;
        task.UpdatedAt = DateTime.UtcNow;

        _tasks.Add(task);

        return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
    }

    [HttpPut("{id}")]
    public ActionResult<TodoTask> UpdateTask(int id, TodoTask updatedTask)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == id);
        if (task == null)
            return NotFound();

        task.Title = updatedTask.Title;
        task.Description = updatedTask.Description;
        task.IsCompleted = updatedTask.IsCompleted;
        task.Priority = updatedTask.Priority;
        task.DueDate = updatedTask.DueDate;
        task.UpdatedAt = DateTime.UtcNow;

        return Ok(task);
    }

    [HttpDelete("{id}")]
    public ActionResult DeleteTask(int id)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == id);
        if (task == null)
            return NotFound();

        _tasks.Remove(task);
        return NoContent();
    }
}
