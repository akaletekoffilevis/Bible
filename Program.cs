using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BibleApp;
using BibleApp.Services;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<BibleService>();
builder.Services.AddScoped<SearchIndexService>();
builder.Services.AddScoped<IndexedDbService>();
builder.Services.AddSingleton<ThemeService>();
builder.Services.AddMudServices();

await builder.Build().RunAsync();
