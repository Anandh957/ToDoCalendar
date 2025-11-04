using Microsoft.AspNetCore.Mvc;
using ToDoCalendar.DAL;
using ToDoCalendar.Models; // Assuming Todo model is here

namespace ToDoCalendar.Controllers;

[Route("[controller]/[action]")]
public class TodoController : Controller
{
    private readonly TodoRepository _repo;
    private readonly ILogger<TodoController> _logger;

    public TodoController(TodoRepository repo, ILogger<TodoController> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public JsonResult GetEvents()
    {
        try
        {
            var data = _repo.GetAll().Select(t => new {
                id = t.TodoId,
                title = t.Title + (t.IsDone ? " ✔" : string.Empty),
                start = t.DueDate.ToString("yyyy-MM-dd"),
                description = t.Description,
                isDone = t.IsDone
            });
            return Json(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all events.");
            return Json(new { success = false, message = "Failed to retrieve events.", error = ex.Message });
        }
    }

    [HttpGet]
    public JsonResult GetByDate(DateTime date)
    {
        try
        {
            var todos = _repo.GetByDate(date);
            return Json(todos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting todos for date {date}.", date.ToShortDateString());
            return Json(new { success = false, message = "Failed to retrieve tasks for the selected date.", error = ex.Message });
        }
    }

   
    [HttpGet]
    public JsonResult GetById(int id)
    {
        if (id <= 0)
        {
            return Json(new { success = false, message = "Invalid task ID provided." });
        }

        try
        {
            var todo = _repo.GetById(id);
            if (todo == null)
            {
                _logger.LogWarning("Todo with ID {id} not found.", id);
                return Json(new { success = false, message = "Task not found." });
            }

            _logger.LogInformation("Retrieved todo with ID: {id}", id);
            return Json(todo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving todo with ID {id}.", id);
            return Json(new { success = false, message = "Failed to retrieve task.", error = ex.Message });
        }
    }

    [HttpPost]
    public IActionResult AddEdit([Bind("TodoId,Title,Description,DueDate,IsDone")] Todo t)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning("Invalid model state in AddEdit: {errors}", string.Join(", ", errors));
            return Json(new { success = false, message = "Invalid data provided.", errors = errors });
        }

        try
        {
            if (t.TodoId == 0) // It's a new task
            {
                var id = _repo.Create(t);
                t.TodoId = id; // Update the Todo object with the new ID from the DB
                _logger.LogInformation("Created new todo with ID: {id}", id);
                return Json(new { success = true, todo = t, message = "Task added successfully." });
            }
            else // It's an existing task
            {
                _repo.Update(t);
                _logger.LogInformation("Updated todo with ID: {id}", t.TodoId);
                return Json(new { success = true, todo = t, message = "Task updated successfully." });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in AddEdit operation for todo ID {id}.", t.TodoId);
            return Json(new { success = false, message = "Failed to save task.", error = ex.Message });
        }
    }

    [HttpPost]
    public IActionResult Delete(int id)
    {
        if (id <= 0)
        {
            return Json(new { success = false, message = "Invalid task ID provided for deletion." });
        }

        try
        {
            _repo.Delete(id);
            _logger.LogInformation("Deleted todo with ID: {id}", id);
            return Json(new { success = true, message = "Task deleted successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting todo with ID {id}.", id);
            return Json(new { success = false, message = "Failed to delete task.", error = ex.Message });
        }
    }
}