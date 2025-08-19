using BaselineTypeDiscovery;
using DinkToPdf;
using DinkToPdf.Contracts;
using DocBook.Data;
using DocBook.Repositories;
using DocBook.Helpers;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
string wkHtmlToPdfPath = Path.Combine(Directory.GetCurrentDirectory(), "wkhtmltopdf", "libwkhtmltox.dll");
DocBook.Helpers.CustomAssemblyLoadContext context = new DocBook.Helpers.CustomAssemblyLoadContext();
context.LoadUnmanagedLibrary(wkHtmlToPdfPath);



// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<AdoHelper>();
builder.Services.AddScoped<PatientRepository>();
builder.Services.AddScoped<DoctorRepository>();
builder.Services.AddScoped<AppointmentRepository>();
builder.Services.AddScoped<PrescriptionRepository>();

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
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
