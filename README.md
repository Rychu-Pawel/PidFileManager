# PID file manager
Takes care of creating and deleting PID file in ASP.Net Core applications. Mechanics on which the library is based are described in [this article](https://pawelrychlicki.pl/Article/Details/71/create-pid-file-in-aspnet-core-ubuntu-2004--net-5). You can also find there information on how to create a systemd service that creates `/var/run/yourapp/` directory with necessary write permissions for you.

# Usage
1. Install nuget package:
```
dotnet add package Rychusoft.PidFileManager
```
2. In the `Startup.cs` add `services.AddPidFileHostedService(Configuration.GetSection("PidFileOptions"));` to the `ConfigureServices()` method:
```csharp
public class Startup
{
    [...]

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddPidFileHostedService(Configuration.GetSection("PidFileOptions"));

        [...]
    }

    [...]
}
```
3. In the `appsettings.json` add new section - note that the user under which your app is run need to have a privilages to write to this path:
```json
"PidFileOptions": {
  "PidFilePath": "/var/run/yourapp/yourapp.pid"
}
```
4. In the `appsettings.Development.json` add new section that will disable creating PID file when running in development environment (e.g., from Visual Studio):
```json
"PidFileOptions": {
  "IsDisabled": true
}
```

# Configuration

1. You can configure the service as shown above, in the Usage section, by calling `services.AddPidFileHostedService(Configuration.GetSection("PidFileOptions"));`. Possible appsettings properties:
* PidFilePath - `string` - full file path where the PID file will be created
* IsDisabled - `bool` - if set to `true` then service will not create a PID file. Useful for disabling the service in development environment.
2. You can configure the service by providing a lambda expression that yields an instance of the options class:
```csharp
services.AddPidFileHostedService(options =>
{
    options.PidFilePath = "/var/run/yourapp/yourapp.pid";
    options.IsDisabled = false;
});
```