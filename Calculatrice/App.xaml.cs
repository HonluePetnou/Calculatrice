// ============================================================================
//  Calculatrice .NET MAUI – Activité n°4, Atelier de développement mobile
//  Auteur : PETNOU Honlue Frédéric Armel – ENSPD
//  Fichier : App.xaml.cs – point d'entrée de l'application
// ============================================================================
namespace Calculatrice
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}