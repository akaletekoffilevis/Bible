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
builder.Services.AddScoped<SearchIndexService>(sp =>
    new SearchIndexService(sp.GetRequiredService<BibleService>(), sp.GetRequiredService<HttpClient>()));
builder.Services.AddScoped<IndexedDbService>();
builder.Services.AddSingleton<ThemeService>();
builder.Services.AddScoped<ReadingPlanService>();
builder.Services.AddScoped<MemorisationService>();
builder.Services.AddMudServices();

await builder.Build().RunAsync();
