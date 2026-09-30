// ============================================================================
//  Calculatrice .NET MAUI – Activité n°4, Atelier de développement mobile
//  Auteur : PETNOU Honlue Frédéric Armel – ENSPD
//  Fichier : MainPage.xaml.cs – code-behind : événements + adaptation à l'écran
// ============================================================================
namespace Calculatrice;

public partial class MainPage : ContentPage
{
    private readonly CalculatorEngine _calc = new();
    private bool? _isLandscape;

    public MainPage()
    {
        InitializeComponent();

        _calc.HistoryAdded += AddHistoryItem;
        SizeChanged += OnPageSizeChanged;
        DisplayCard.SizeChanged += (_, _) => FitResultFont();

        // Tap sur le résultat : copie dans le presse-papiers.
        var copyTap = new TapGestureRecognizer();
        copyTap.Tapped += OnResultTapped;
        DisplayCard.GestureRecognizers.Add(copyTap);

        Refresh();
    }

    // ------------------------------------------------ Gestionnaires d'événements

    private void OnDigitClicked(object? sender, EventArgs e)
    {
        if (sender is Button b) _calc.InputDigit(b.Text[0]);
        Refresh();
    }

    private void OnDecimalClicked(object? sender, EventArgs e)
    {
        _calc.InputDecimalSeparator();
        Refresh();
    }

    private void OnOperatorClicked(object? sender, EventArgs e)
    {
        if (sender is not Button b) return;
        _calc.SetOperation(b.Text switch
        {
            "+" => Operation.Add,
            "−" => Operation.Subtract,
            "×" => Operation.Multiply,
            "÷" => Operation.Divide,
            _ => Operation.None
        });
        Refresh();
    }

    private void OnEqualsClicked(object? sender, EventArgs e)
    {
        _calc.Evaluate();
        Refresh();
    }

    private void OnClearClicked(object? sender, EventArgs e)
    {
        _calc.ClearAll();
        Refresh();
    }

    private void OnBackspaceClicked(object? sender, EventArgs e)
    {
        _calc.Backspace();
        Refresh();
    }

    private void OnSignClicked(object? sender, EventArgs e)
    {
        _calc.ToggleSign();
        Refresh();
    }

    private void OnPercentClicked(object? sender, EventArgs e)
    {
        _calc.Percent();
        Refresh();
    }

    private void OnAdvancedClicked(object? sender, EventArgs e)
    {
        if (sender is not Button b) return;
        switch (b.CommandParameter as string)
        {
            case "sqrt": _calc.SquareRoot(); break;
            case "square": _calc.Square(); break;
            case "inverse": _calc.Inverse(); break;
            case "pi": _calc.InsertPi(); break;
        }
        Refresh();
    }

    private void OnClearHistoryClicked(object? sender, EventArgs e)
    {
        HistoryList.Children.Clear();
        HistoryList.Children.Add(EmptyHistoryLabel);
    }

    private async void OnResultTapped(object? sender, TappedEventArgs e)
    {
        if (_calc.HasError) return;
        await Clipboard.Default.SetTextAsync(_calc.DisplayText);
        await ShowToastAsync("Résultat copié");
    }

    // ------------------------------------------------------------ Affichage

    private void Refresh()
    {
        ExpressionLabel.Text = string.IsNullOrEmpty(_calc.ExpressionText) ? " " : _calc.ExpressionText;
        ResultLabel.Text = _calc.DisplayText;
        if (_calc.HasError)
            ResultLabel.TextColor = Colors.IndianRed;
        else
            ResultLabel.ClearValue(Label.TextColorProperty); // couleur du thème clair/sombre
        FitResultFont();
    }

    /// <summary>
    /// Réduit la police du résultat quand le nombre est long, pour qu'il
    /// tienne toujours sur une ligne, même sur un petit écran.
    /// </summary>
    private void FitResultFont()
    {
        double available = DisplayCard.Width > 0 ? DisplayCard.Width - 32 : 300;
        int length = Math.Max(1, ResultLabel.Text?.Length ?? 1);
        double maxSize = _isLandscape == true ? 40 : 52;
        ResultLabel.FontSize = Math.Clamp(available / (length * 0.62), 18, maxSize);
    }

    private void AddHistoryItem(string expression, string result)
    {
        HistoryList.Children.Remove(EmptyHistoryLabel);

        var item = new VerticalStackLayout { Spacing = 0 };
        item.Children.Add(new Label
        {
            Text = expression,
            FontSize = 13,
            HorizontalTextAlignment = TextAlignment.End,
            LineBreakMode = LineBreakMode.HeadTruncation,
            TextColor = Colors.Gray
        });
        item.Children.Add(new Label
        {
            Text = result,
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            HorizontalTextAlignment = TextAlignment.End,
            LineBreakMode = LineBreakMode.HeadTruncation
        });

        // Tap sur une ligne d'historique : on réutilise son résultat.
        var tap = new TapGestureRecognizer();
        tap.Tapped += async (_, _) =>
        {
            _calc.LoadValue(result);
            Refresh();
            await ShowToastAsync("Valeur réutilisée");
        };
        item.GestureRecognizers.Add(tap);

        HistoryList.Children.Insert(0, item);
    }

    private async Task ShowToastAsync(string message)
    {
        ToastLabel.Text = message;
        Toast.CancelAnimations();
        await Toast.FadeTo(1, 150);
        await Task.Delay(900);
        await Toast.FadeTo(0, 250);
    }

    // ------------------------------------------ Changement d'orientation

    private void OnPageSizeChanged(object? sender, EventArgs e)
    {
        if (Width <= 0 || Height <= 0) return;
        bool landscape = Width > Height;
        if (landscape == _isLandscape) return;
        _isLandscape = landscape;

        RootGrid.RowDefinitions.Clear();
        RootGrid.ColumnDefinitions.Clear();

        if (landscape)
        {
            // Paysage : écran + historique à gauche, clavier à droite.
            RootGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
            RootGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            RootGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1.4, GridUnitType.Star)));
            Grid.SetRow(InfoPanel, 0); Grid.SetColumn(InfoPanel, 0);
            Grid.SetRow(KeysPanel, 0); Grid.SetColumn(KeysPanel, 1);
            Keypad.RowSpacing = Keypad.ColumnSpacing = 6;
        }
        else
        {
            // Portrait : écran + historique en haut, clavier en bas.
            RootGrid.RowDefinitions.Add(new RowDefinition(new GridLength(2, GridUnitType.Star)));
            RootGrid.RowDefinitions.Add(new RowDefinition(new GridLength(3, GridUnitType.Star)));
            RootGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            Grid.SetRow(InfoPanel, 0); Grid.SetColumn(InfoPanel, 0);
            Grid.SetRow(KeysPanel, 1); Grid.SetColumn(KeysPanel, 0);
            Keypad.RowSpacing = Keypad.ColumnSpacing = 8;
        }

        Grid.SetRowSpan(ToastLayer, RootGrid.RowDefinitions.Count);
        Grid.SetColumnSpan(ToastLayer, RootGrid.ColumnDefinitions.Count);
        FitResultFont();
    }
}
