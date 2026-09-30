// ============================================================================
//  Calculatrice .NET MAUI – Activité n°4, Atelier de développement mobile
//  Auteur : PETNOU Honlue Frédéric Armel – ENSPD
//  Fichier : CalculatorEngine.cs – logique de calcul (indépendante de l'UI)
// ============================================================================
using System.Globalization;

namespace Calculatrice;

public enum Operation { None, Add, Subtract, Multiply, Divide }

/// <summary>
/// Moteur de la calculatrice. Il ne connaît rien de l'interface : la page
/// appelle ses méthodes depuis les gestionnaires d'événements puis relit
/// <see cref="DisplayText"/> et <see cref="ExpressionText"/>.
/// Les calculs se font en <c>decimal</c> pour éviter les erreurs d'arrondi
/// du type 0,1 + 0,2 = 0,30000000000000004.
/// </summary>
public sealed class CalculatorEngine
{
    private const int MaxDigits = 15;
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    private string _entry = "0";          // saisie courante (séparateur '.')
    private decimal? _accumulator;        // opérande gauche
    private Operation _pending = Operation.None;
    private bool _overwrite = true;       // le prochain chiffre remplace la saisie
    private bool _hasOperand;             // une 2e opérande a été fournie
    private bool _justEvaluated;          // on vient d'appuyer sur « = »
    private string _expression = "";

    public string? Error { get; private set; }
    public bool HasError => Error is not null;

    /// <summary>Déclenché après chaque calcul : (expression, résultat).</summary>
    public event Action<string, string>? HistoryAdded;

    public string DisplayText => Error ?? ToDisplay(_entry);
    public string ExpressionText => ToDisplay(_expression);

    // ------------------------------------------------------------------ Saisie

    public void InputDigit(char digit)
    {
        PrepareForNewInput();
        if (_overwrite)
        {
            _entry = digit.ToString();
            _overwrite = false;
        }
        else if (_entry == "0") _entry = digit.ToString();
        else if (_entry == "-0") _entry = "-" + digit;
        else if (CountDigits(_entry) < MaxDigits) _entry += digit;
        _hasOperand = true;
    }

    public void InputDecimalSeparator()
    {
        PrepareForNewInput();
        if (_overwrite)
        {
            _entry = "0.";
            _overwrite = false;
        }
        else if (!_entry.Contains('.') && CountDigits(_entry) < MaxDigits)
        {
            _entry += ".";
        }
        _hasOperand = true;
    }

    public void Backspace()
    {
        if (HasError) { ClearAll(); return; }
        if (_overwrite)
        {
            // Sur un résultat : on efface seulement la ligne d'opération.
            if (_justEvaluated) _expression = "";
            return;
        }
        _entry = _entry[..^1];
        if (_entry is "" or "-" or "-0") _entry = "0";
    }

    public void ClearAll()
    {
        _entry = "0";
        _accumulator = null;
        _pending = Operation.None;
        _overwrite = true;
        _hasOperand = false;
        _justEvaluated = false;
        _expression = "";
        Error = null;
    }

    // -------------------------------------------------------------- Opérations

    public void SetOperation(Operation op)
    {
        if (HasError || op == Operation.None) return;

        // Opérateur pressé deux fois de suite : on remplace simplement le précédent.
        if (_pending != Operation.None && !_hasOperand)
        {
            _pending = op;
            _expression = $"{Format(_accumulator!.Value)} {Symbol(op)}";
            return;
        }

        decimal value = Parse(_entry);
        if (_pending != Operation.None && _accumulator.HasValue)
        {
            // Enchaînement : 2 + 3 × → on calcule 2 + 3 d'abord.
            if (!TryCompute(_accumulator.Value, value, _pending, out decimal partial)) return;
            _accumulator = partial;
        }
        else
        {
            _accumulator = value;
        }

        _pending = op;
        _entry = Format(_accumulator.Value);
        _expression = $"{_entry} {Symbol(op)}";
        _overwrite = true;
        _hasOperand = false;
        _justEvaluated = false;
    }

    public void Evaluate()
    {
        if (HasError || _pending == Operation.None || !_accumulator.HasValue) return;

        decimal left = _accumulator.Value;
        decimal right = Parse(_entry);
        string expr = $"{Format(left)} {Symbol(_pending)} {Wrap(Format(right))} =";

        if (!TryCompute(left, right, _pending, out decimal result))
        {
            _expression = expr;
            return;
        }

        string resultText = Format(result);
        HistoryAdded?.Invoke(ToDisplay(expr), ToDisplay(resultText));

        _expression = expr;
        _entry = resultText;
        _accumulator = null;
        _pending = Operation.None;
        _overwrite = true;
        _hasOperand = false;
        _justEvaluated = true;
    }

    public void ToggleSign()
    {
        if (HasError) return;
        if (_entry == "0" || _entry == "0.") return;
        _entry = _entry.StartsWith('-') ? _entry[1..] : "-" + _entry;
        if (_pending != Operation.None) _hasOperand = true;
    }

    /// <summary>
    /// Pourcentage « à la calculatrice » : 200 + 10 % → 200 + 20 ;
    /// 200 × 10 % → 200 × 0,1 ; sans opération en cours → x / 100.
    /// </summary>
    public void Percent()
    {
        if (HasError) return;
        decimal value = Parse(_entry);
        decimal result = (_pending is Operation.Add or Operation.Subtract && _accumulator.HasValue)
            ? _accumulator.Value * value / 100m
            : value / 100m;
        SetUnaryResult(result);
    }

    // ------------------------------------------ Fonctions avancées (bonus)

    public void SquareRoot()
    {
        if (HasError) return;
        decimal x = Parse(_entry);
        if (x < 0) { SetError("Racine d'un nombre négatif"); return; }
        _expression = _pending == Operation.None ? $"√({Format(x)})" : _expression;
        SetUnaryResult((decimal)Math.Sqrt((double)x));
    }

    public void Square()
    {
        if (HasError) return;
        decimal x = Parse(_entry);
        try
        {
            decimal r = checked(x * x);
            _expression = _pending == Operation.None ? $"sqr({Format(x)})" : _expression;
            SetUnaryResult(r);
        }
        catch (OverflowException) { SetError("Dépassement de capacité"); }
    }

    public void Inverse()
    {
        if (HasError) return;
        decimal x = Parse(_entry);
        if (x == 0) { SetError("Division par zéro impossible"); return; }
        _expression = _pending == Operation.None ? $"1/({Format(x)})" : _expression;
        SetUnaryResult(1m / x);
    }

    public void InsertPi()
    {
        PrepareForNewInput();
        SetUnaryResult((decimal)Math.PI);
    }

    /// <summary>Réutilise une valeur (ex. : tap sur une ligne d'historique).</summary>
    public void LoadValue(string displayedValue)
    {
        PrepareForNewInput();
        string raw = displayedValue.Replace(',', '.').Replace(" ", "");
        if (decimal.TryParse(raw, NumberStyles.Float, Inv, out decimal v))
            SetUnaryResult(v);
    }

    // ------------------------------------------------------------- Interne

    private void PrepareForNewInput()
    {
        if (HasError)
        {
            ClearAll();
        }
        else if (_justEvaluated)
        {
            // Après « = », taper un chiffre démarre un nouveau calcul.
            _expression = "";
            _justEvaluated = false;
            _overwrite = true;
        }
    }

    private void SetUnaryResult(decimal value)
    {
        _entry = Format(value);
        _overwrite = true;              // taper un chiffre remplace le résultat
        _hasOperand = _pending != Operation.None;
    }

    private bool TryCompute(decimal a, decimal b, Operation op, out decimal result)
    {
        result = 0;
        try
        {
            switch (op)
            {
                case Operation.Add: result = checked(a + b); break;
                case Operation.Subtract: result = checked(a - b); break;
                case Operation.Multiply: result = checked(a * b); break;
                case Operation.Divide:
                    if (b == 0) { SetError("Division par zéro impossible"); return false; }
                    result = a / b;
                    break;
            }
            return true;
        }
        catch (OverflowException)
        {
            SetError("Dépassement de capacité");
            return false;
        }
    }

    private void SetError(string message)
    {
        Error = message;
        _accumulator = null;
        _pending = Operation.None;
        _entry = "0";
        _overwrite = true;
        _hasOperand = false;
        _justEvaluated = false;
    }

    private static decimal Parse(string s) =>
        decimal.Parse(s, NumberStyles.Float, Inv);

    private static string Format(decimal value)
    {
        value = Math.Round(value, 10);
        string s = value.ToString("0.##########", Inv);
        // Trop long pour l'écran : notation scientifique.
        if (CountDigits(s) > MaxDigits)
            s = ((double)value).ToString("0.#########E+0", Inv);
        return s == "-0" ? "0" : s;
    }

    private static int CountDigits(string s) => s.Count(char.IsDigit);

    private static string Wrap(string s) => s.StartsWith('-') ? $"({s})" : s;

    private static string ToDisplay(string s) => s.Replace('.', ',');

    private static string Symbol(Operation op) => op switch
    {
        Operation.Add => "+",
        Operation.Subtract => "−",
        Operation.Multiply => "×",
        Operation.Divide => "÷",
        _ => ""
    };
}
