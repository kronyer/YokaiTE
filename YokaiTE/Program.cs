using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using TG.Blazor.IndexedDB;
using YokaiTE;
using YokaiTE.Interfaces;
using YokaiTE.Utils;
using YokaiTE.Utils.TextHandlers;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddScoped<IFileExporter, FileExporter>();
builder.Services.AddScoped<IFileImporter, FileImporter>();
builder.Services.AddScoped<ITextFormatter, TextFormatter>();
builder.Services.AddScoped<IFileInfoProvider, FileInfoProvider>();

builder.Services.AddMudServices();

//TODO remove this
builder.RootComponents.Add<App>("#app");


builder.Services.AddIndexedDB(dbStore =>
{
    dbStore.DbName = "YokaiTE";
    dbStore.Version = 1;
    dbStore.Stores.Add(new StoreSchema
    {
        Name = "documents",
        PrimaryKey = new IndexSpec { Name = "id", KeyPath = "id", Auto = true },
        Indexes = new List<IndexSpec>
        {
            new IndexSpec { Name = "title", KeyPath = "title", Auto = false }
        }
    });
});

await builder.Build().RunAsync();