using JLPT;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddNewtonsoftJson(); 
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
ConfigurationManager configuration = builder.Configuration;
string sqlConnectionString = configuration.GetConnectionString("jlpt");
builder.Services.AddDbContext<JLPTDbContext>(options => options.UseSqlServer(sqlConnectionString));

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
