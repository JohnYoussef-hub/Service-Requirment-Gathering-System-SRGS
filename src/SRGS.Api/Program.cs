using Microsoft.EntityFrameworkCore;
using SRGS.Infrastructure;
using SRGS.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Applies pending EF Core migrations automatically on startup — fine for a local dev
    // loop while you're standing this up. Don't ship this to a real environment: an
    // automatic migration running against a shared/prod database on every deploy is how
    // you get a bad migration applied at 2am with nobody watching. Switch to running
    // `dotnet ef database update` as an explicit deploy step before you leave dev.
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    // TODO: add request types and modules types into db
    // TODO: migrate
    // TODO: build the controller

}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();