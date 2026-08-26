var builder = WebApplication.CreateBuilder(args);

// Render free instances can exhaust inotify watches; disable config
// file-watchers (hot-reload isn't needed in production) so startup
// never creates FileSystemWatcher instances.
builder.Configuration.Sources.Clear();
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);
builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false);
builder.Configuration.AddEnvironmentVariables();

// MVC
builder.Services.AddControllersWithViews();

// Backend API client (token is attached per-request by controllers)
builder.Services.AddHttpClient("Api", client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5240/api/");
    // Cloud APIs on free tiers can take ~60s to wake from sleep.
    client.Timeout = TimeSpan.FromSeconds(120);
});

// Session for auth state (JWT kept server-side, never exposed to the browser)
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseSession();


app.MapControllerRoute(
    name: "dashboard_short",
    pattern: "Dashboard",
    defaults: new { controller = "Dashboard", action = "Index" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
