# Calculatrice .NET MAUI

**Auteur :** PETNOU Honlue Frédéric Armel – ENSPD
**Activité n°4 – Atelier de développement mobile**

Calculatrice mobile développée avec .NET MAUI (Single Project), interface en XAML,
logique dans le code-behind et dans une classe `CalculatorEngine` indépendante de l'UI.

## Fonctionnalités

- Addition, soustraction, multiplication, division, avec enchaînement (`2 + 3 × 4 =` → 20)
- Nombres décimaux (calcul en `decimal` : `0,1 + 0,2 = 0,3`)
- `C` remise à zéro totale, `⌫` effacement du dernier caractère
- `±` changement de signe, `%` pourcentage (`200 + 10 %` → `200 + 20`)
- Division par zéro gérée sans plantage (message d'erreur, la saisie suivante repart de zéro)
- Opération en cours affichée au-dessus du résultat
- Dépassement de capacité intercepté, notation scientifique pour les très grands nombres
- Police du résultat ajustée automatiquement pour ne jamais déborder
- Disposition portrait / paysage adaptée automatiquement, thème clair / sombre

**Bonus :** `√x`, `x²`, `1/x`, `π`, historique défilant (tap = réutiliser la valeur),
tap sur l'écran = copier le résultat.

## Layouts utilisés (7)

| Layout | Rôle |
|---|---|
| Grid | Structure de la page et pavé numérique 5 × 4 en `*` |
| VerticalStackLayout | Opération en cours empilée au-dessus du résultat, liste d'historique |
| HorizontalStackLayout | En-tête de l'historique (titre + bouton Effacer) |
| ScrollView | Historique défilant |
| FlexLayout | Fonctions avancées qui passent à la ligne sur écran étroit |
| Border | Carte d'affichage et notification |
| AbsoluteLayout | Calque de notification superposé |

## Lancer le projet

```bash
dotnet build -t:Run -f net9.0-android
```
