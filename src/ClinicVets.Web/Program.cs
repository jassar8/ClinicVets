using ClinicVets.Application.Interfaces;
using ClinicVets.Application.Services;
using ClinicVets.Infrastructure.Repositories;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

try
{
    var builder = WebApplication.CreateBuilder(args);
    var configuredUrl = builder.Configuration["DesktopMode:AppUrl"] ?? "http://127.0.0.1:5050";
    var appUrl = ResolveDesktopUrl(configuredUrl);
    builder.WebHost.UseUrls(appUrl);

    // Add services to the container.
    builder.Services.AddControllersWithViews();
    builder.Services.AddSingleton<IEmployeeRepository, InMemoryEmployeeRepository>();
    builder.Services.AddScoped<EmployeeAuthenticationService>();
    builder.Services.AddScoped<EmployeeRegistrationService>();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
    }

    if (app.Environment.IsDevelopment())
    {
        app.UseHttpsRedirection();
    }
    app.UseRouting();

    app.UseAuthorization();

    app.MapStaticAssets();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
        .WithStaticAssets();

    if (builder.Configuration.GetValue<bool>("DesktopMode:AutoOpenBrowser"))
    {
        app.Lifetime.ApplicationStarted.Register(() =>
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = appUrl,
                    UseShellExecute = true
                });
            }
            catch
            {
                // The app still runs normally even if browser auto-open fails.
            }
        });
    }

    app.Run();
}
catch (Exception ex)
{
    ShowStartupError(ex);
}

return;

static string ResolveDesktopUrl(string configuredUrl)
{
    if (!Uri.TryCreate(configuredUrl, UriKind.Absolute, out var preferredUri))
    {
        return "http://127.0.0.1:5050";
    }

    if (preferredUri.Host is not ("127.0.0.1" or "localhost"))
    {
        return "http://127.0.0.1:5050";
    }

    if (!IsPortBusy(preferredUri.Port))
    {
        return configuredUrl;
    }

    var fallbackPort = GetFreeTcpPort();
    return $"http://127.0.0.1:{fallbackPort}";
}

static bool IsPortBusy(int port)
{
    try
    {
        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Bind(new IPEndPoint(IPAddress.Loopback, port));
        return false;
    }
    catch (SocketException)
    {
        return true;
    }
}

static int GetFreeTcpPort()
{
    var listener = new TcpListener(IPAddress.Loopback, 0);
    listener.Start();
    var port = ((IPEndPoint)listener.LocalEndpoint).Port;
    listener.Stop();
    return port;
}

static void ShowStartupError(Exception ex)
{
    var message =
        "ClinicVets could not start.\n\n" +
        "Details:\n" + ex.Message + "\n\n" +
        "Please ensure the publish folder stays complete and no security software blocks the app.";

    MessageBoxW(IntPtr.Zero, message, "ClinicVets Startup Error", 0x00000010);
}

[DllImport("user32.dll", CharSet = CharSet.Unicode)]
static extern int MessageBoxW(IntPtr hWnd, string text, string caption, uint type);
