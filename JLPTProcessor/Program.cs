using Microsoft.EntityFrameworkCore;
//using Microsoft.AspNetCore.Mvc;
using JLPT;
using JLPT.Services;

var builder = WebApplication.CreateBuilder(args);

ConfigurationManager configuration = builder.Configuration;

// Entity Framework
builder.Services.AddDbContext<JlptDbContext>(options
    => options.UseSqlServer(configuration.GetConnectionString("JlptConnectionString")!));
// Add services to the container.
builder.Services.AddRazorPages().AddControllersAsServices();
builder.Services.AddControllers();

//Add services
builder.Services.AddScoped<IUserDataInterface, UserDataService>();
builder.Services.AddScoped<IEmailService, EmailService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
