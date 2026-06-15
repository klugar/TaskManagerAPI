using Asp.Versioning;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using TaskManagerAPI.Data;
using TaskManagerAPI.Middleware;
using TaskManagerAPI.Repositories;
using TaskManagerAPI.Validators;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Add API versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Task Manager API", Version = "v1" });
    c.SwaggerDoc("v2", new() { Title = "Task Manager API", Version = "v2" });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=taskmanager.db"));

builder.Services.AddScoped<ITaskRepository, TaskRepository>();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateTaskValidator>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVue", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader());
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseCors("AllowVue");

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Task Manager API v1");
    c.SwaggerEndpoint("/swagger/v2/swagger.json", "Task Manager API v2");
});

app.UseAuthorization();
app.MapControllers();

app.Run("http://localhost:5000");