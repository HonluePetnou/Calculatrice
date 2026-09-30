// ============================================================================
//  Calculatrice .NET MAUI – Activité n°4, Atelier de développement mobile
//  Auteur : PETNOU Honlue Frédéric Armel – ENSPD
//  Fichier : MauiProgram.cs – configuration de l'application MAUI
// ============================================================================
using Microsoft.Extensions.Logging;

namespace Calculatrice
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
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
