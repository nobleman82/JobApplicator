using JobApplicator2.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JobApplicator2
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });
            builder.Services.AddScoped(sp => new HttpClient());
            builder.Services.AddMauiBlazorWebView();
            builder.Services.AddDbContextFactory<AppDbContext>(opt =>
            opt.UseSqlite("Data Source=jobapplicator.db"));
            builder.Services.AddScoped<ProfileService>();
            builder.Services.AddScoped<DatabaseService>();
            builder.Services.AddScoped<AiService>();
           

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
