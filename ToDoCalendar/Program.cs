//using ToDoCalendar.DAL;

//var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddControllersWithViews();
//builder.Services.AddScoped<TodoRepository>();

//var app = builder.Build();

//if (!app.Environment.IsDevelopment())
//{
//    app.UseExceptionHandler("/Home/Error");
//    app.UseHsts();
//}

//app.UseHttpsRedirection();
//app.UseStaticFiles();
//app.UseRouting();

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Todo}/{action=Index}/{id?}");

//app.MapGet("/", context =>
//{
//    context.Response.Redirect("/Todo/Index");
//    return Task.CompletedTask;
//});


//app.Run();

// Inside Program.cs
using ToDoCalendar.DAL; // Make sure to include this namespace
// ... other usings

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register TodoRepository for dependency injection
builder.Services.AddScoped<TodoRepository>();

// Configure Logging (ASP.NET Core has default logging, but you can customize)
builder.Logging.ClearProviders(); // Clear default providers if you want full control
builder.Logging.AddConsole();
builder.Logging.AddDebug();
// Add other logging providers like Serilog, NLog, etc. if needed

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Todo}/{action=Index}/{id?}"); // Changed default controller to Todo

app.MapGet("/", context =>
{
    context.Response.Redirect("/Todo/Index");
    return Task.CompletedTask;
});
app.Run();