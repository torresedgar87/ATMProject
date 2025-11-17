using ATMAPI;
using ATMAPI.Database;
using ATMAPI.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddConsole());

var defaultConnection = builder.Configuration.GetConnectionString("InMemoryDb");
var connection = new SqliteConnection(defaultConnection);
connection.Open();

builder.Services.AddDbContext<APIDbContext>(options => 
    options.UseSqlite(defaultConnection)
);

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();

builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<ITransactionService, TransactionService>();
builder.Services.AddTransient<IAccountService, AccountService>();
builder.Services.AddTransient<ITransactionProcessingService, TransactionProcessingService>();

var clientUrl = builder.Configuration.GetConnectionString("ClientUrl");

builder.Services.AddCors(options =>
{
    options.AddPolicy("ClientApp", policy =>
    {
        policy.WithOrigins(
                clientUrl
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.AddControllers(options =>
{
    options.Filters.Add<DatabaseExceptionFilter>();
    options.Filters.Add<CustomExceptionFilter>();
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<APIDbContext>();
    db.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
   
}

app.UseHttpsRedirection();

app.UseCors("ClientApp");

app.MapControllers();

app.Run();