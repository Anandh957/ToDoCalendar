using System.Data;
using Microsoft.Data.SqlClient;
using ToDoCalendar.Models; // Ensure this namespace matches your Todo model location

namespace ToDoCalendar.DAL;

public class TodoRepository
{
    private readonly string _conn;
    private readonly ILogger<TodoRepository> _logger; // Add logging

    public TodoRepository(IConfiguration config, ILogger<TodoRepository> logger)
    {
        _conn = config.GetConnectionString("ToDoDb") ?? throw new InvalidOperationException("Connection string 'ToDoDb' not found.");
        _logger = logger;
    }

    public int Create(Todo t)
    {
        try
        {
            using var con = new SqlConnection(_conn);
            using var cmd = new SqlCommand("sp_CreateTodo", con) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@Title", t.Title);
            cmd.Parameters.AddWithValue("@Description", (object?)t.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@DueDate", t.DueDate.Date); // Store date only
            cmd.Parameters.AddWithValue("@IsDone", t.IsDone);
            con.Open();
            return Convert.ToInt32(cmd.ExecuteScalar());
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "SQL Error creating todo: {Title}", t.Title);
            throw; // Re-throw to be caught by the controller
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating todo: {Title}", t.Title);
            throw;
        }
    }

    public IEnumerable<Todo> GetAll()
    {
        var list = new List<Todo>();
        try
        {
            using var con = new SqlConnection(_conn);
            using var cmd = new SqlCommand("sp_GetAllTodos", con) { CommandType = CommandType.StoredProcedure };
            con.Open();
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(Map(r));
            }
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "SQL Error getting all todos.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all todos.");
            throw;
        }
        return list;
    }

    // NEW: Get single todo by ID
    public Todo? GetById(int id)
    {
        try
        {
            using var con = new SqlConnection(_conn);
            using var cmd = new SqlCommand("sp_GetTodoById", con) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@TodoId", id);
            con.Open();
            using var r = cmd.ExecuteReader();
            if (r.Read())
            {
                return Map(r);
            }
            return null; // Todo not found
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "SQL Error getting todo by ID: {TodoId}", id);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting todo by ID: {TodoId}", id);
            throw;
        }
    }

    public IEnumerable<Todo> GetByDate(DateTime date)
    {
        var list = new List<Todo>();
        try
        {
            using var con = new SqlConnection(_conn);
            using var cmd = new SqlCommand("sp_GetTodosByDate", con) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@Date", date.Date); // Pass date only for comparison
            con.Open();
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(Map(r));
            }
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "SQL Error getting todos by date: {Date}", date.ToShortDateString());
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting todos by date: {Date}", date.ToShortDateString());
            throw;
        }
        return list;
    }

    public void Update(Todo t)
    {
        try
        {
            using var con = new SqlConnection(_conn);
            using var cmd = new SqlCommand("sp_UpdateTodo", con) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@TodoId", t.TodoId);
            cmd.Parameters.AddWithValue("@Title", t.Title);
            cmd.Parameters.AddWithValue("@Description", (object?)t.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@DueDate", t.DueDate.Date); // Store date only
            cmd.Parameters.AddWithValue("@IsDone", t.IsDone);
            con.Open();
            cmd.ExecuteNonQuery();
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "SQL Error updating todo with ID: {TodoId}", t.TodoId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating todo with ID: {TodoId}", t.TodoId);
            throw;
        }
    }

    public void Delete(int id)
    {
        try
        {
            using var con = new SqlConnection(_conn);
            using var cmd = new SqlCommand("sp_DeleteTodo", con) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@TodoId", id);
            con.Open();
            cmd.ExecuteNonQuery();
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "SQL Error deleting todo with ID: {TodoId}", id);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting todo with ID: {TodoId}", id);
            throw;
        }
    }

    private static Todo Map(SqlDataReader r) => new()
    {
        TodoId = Convert.ToInt32(r["TodoId"]),
        Title = r["Title"].ToString()!,
        Description = r["Description"] == DBNull.Value ? null : r["Description"].ToString(),
        DueDate = Convert.ToDateTime(r["DueDate"]),
        IsDone = Convert.ToBoolean(r["IsDone"]),
        CreatedAt = Convert.ToDateTime(r["CreatedAt"]) // Ensure this column exists in your DB
    };
}