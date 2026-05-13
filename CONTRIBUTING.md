# Guide de contribution

Merci de vouloir contribuer à **Bibeli** ! Ce guide détaille les règles, conventions et processus pour contribuer efficacement.

---

## Table des matières

1. [Code de conduite](#code-de-conduite)
2. [Stack technique](#stack-technique)
3. [Structure du projet](#structure-du-projet)
4. [Configuration locale](#configuration-locale)
5. [Workflow de développement](#workflow-de-développement)
6. [Conventions de code](#conventions-de-code)
7. [Conventions de commit](#conventions-de-commit)
8. [Tests](#tests)
9. [Signaler un bug](#signaler-un-bug)
10. [Proposer une fonctionnalité](#proposer-une-fonctionnalité)
11. [Soumettre une Pull Request](#soumettre-une-pull-request)

---

## Code de conduite

### Nos engagements

Dans l'intérêt de favoriser un environnement ouvert et accueillant, nous nous engageons à faire de la participation à notre projet et à notre communauté une expérience sans harcèlement pour tous, indépendamment de l'âge, de la taille corporelle, du handicap, de l'origine ethnique, des caractéristiques sexuelles, de l'identité et de l'expression de genre, du niveau d'expérience, de l'éducation, du statut socio-économique, de la nationalité, de l'apparence personnelle, de la race, de la religion ou de l'identité et de l'orientation sexuelles.

### Nos standards

**Comportements attendus :**
- Faire preuve d'empathie et de bienveillance
- Respecter les opinions et expériences différentes
- Accepter les critiques constructives avec grâce
- Assumer la responsabilité de ses erreurs et en tirer des leçons
- Se concentrer sur ce qui est le mieux pour la communauté

**Comportements inacceptables :**
- Langage ou images à caractère sexuel
- Trollage, commentaires insultants ou désobligeants, attaques personnelles
- Harcèlement public ou privé
- Publication d'informations privées sans autorisation explicite
- Toute autre conduite qui pourrait raisonnablement être considérée comme inappropriée

### Signalement

Les violations peuvent être signalées à **koffilevis21@gmail.com**. Toutes les plaintes seront examinées et traitées de manière appropriée.

---

## Stack technique

| Technologie | Version | Usage |
|---|---|---|
| Blazor WebAssembly | .NET 9 | Framework frontend SPA |
| MudBlazor | 8.x `8.*` | UI components Material Design |
| System.Text.Json | Intégré .NET | Parsing des fichiers JSON bibliques |
| IndexedDB | Via JS interop | Stockage local persistant |
| Leaflet | 1.9.4 (via unpkg) | Carte géographique biblique |
| Web Speech API | Standard navigateur | Synthèse vocale TTS |
| Web Share API | Standard navigateur | Partage natif avec images PNG |
| Canvas API | Standard navigateur | Génération d'images de versets |
| PWA | Standard navigateur | Installation offline |
| Google Analytics | G-FWDV79PT44 | Mesure d'audience |
| Vercel | — | Hébergement et déploiement continu |

---

## Structure du projet

```
BibleApp/
├── Layout/
│   └── MainLayout.razor           # Layout principal (AppBar, Drawer, thème, consentement)
├── Pages/
│   ├── Index.razor                # Accueil avec verset du jour
│   ├── LivreChapitres.razor       # Sélection chapitre par livre
│   ├── Lecture.razor              # Lecture chapitre (versets, TTS, surlignage, navigation)
│   ├── Recherche.razor            # Recherche plein texte (ET/OU/exacte)
│   ├── Reference.razor            # Navigation rapide par référence biblique
│   ├── PlanLecture.razor          # Plan de lecture 365 jours
│   ├── Favoris.razor              # Marque-pages et notes
│   ├── Progression.razor          # Statistiques AT/NT
│   ├── Quiz.razor                 # Quiz biblique
│   ├── Carte.razor                # Carte Leaflet (lazy-load)
│   └── Parametres.razor           # Paramètres, À propos, confidentialité
├── Shared/
│   ├── NavMenu.razor              # Navigation sidebar (2 colonnes, abréviations)
│   ├── NoteDialog.razor           # Dialog ajout/édition de note
│   ├── VersetCard.razor           # Carte verset réutilisable
│   └── Breadcrumb.razor           # Fil d'Ariane contextuel
├── Services/
│   ├── BibleService.cs            # Chargement livres JSON, cache, résolution référence
│   ├── SearchIndexService.cs      # Index inversé, recherche ET/OU/exacte
│   ├── IndexedDbService.cs        # CRUD IndexedDB (bookmarks, notes, highlights, progress, plan)
│   ├── ReadingPlanService.cs      # Génération plan lecture 365 jours
│   └── ThemeService.cs            # Gestion thème clair/sombre
├── Models/
│   ├── Bible.cs                   # Modèles Livre, Chapitre, Verset + abréviations
│   ├── Highlight.cs               # Modèle surlignage (5 couleurs)
│   ├── PlanJour.cs                # Progression plan de lecture
│   └── BreadcrumbNode.cs          # Nœud de fil d'Ariane
├── wwwroot/
│   ├── css/app.css                # Styles personnalisés (dark mode, responsive, print)
│   ├── js/app.js                  # JS interop (TTS, IndexedDB, canvas, Leaflet, PWA)
│   ├── data/books/*.json          # 66 fichiers JSON individuels (1 par livre)
│   ├── data/index.json            # Index des 66 livres
│   ├── index.html                 # Point d'entrée, SEO, CSP, Analytics
│   ├── service-worker.js          # Service worker développement
│   ├── service-worker.published.js# Service worker production (cache offline complet)
│   ├── manifest.webmanifest       # Manifeste PWA
│   ├── sitemap.xml                # SEO (66 livres)
│   ├── robots.txt                 # Instructions moteurs de recherche
│   ├── favicon.svg                # Icône vectorielle
│   ├── icon-192.png               # Icône PWA 192px
│   └── icon-512.png               # Icône PWA 512px
├── vercel.json                    # Configuration déploiement Vercel
├── BibleApp.csproj                # Fichier projet .NET 9
├── README.md                      # Documentation principale
├── CONTRIBUTING.md                # Ce fichier — guide de contribution
└── LICENSE                        # Licence MIT
```

---

## Configuration locale

### Prérequis

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Git](https://git-scm.com/)
- Navigateur moderne (Chrome, Firefox, Edge, Safari)

### Installation

```bash
# Cloner le dépôt
git clone https://github.com/akaletekoffilevis/Bible.git
cd Bible/BibleApp

# Restaurer les dépendances
dotnet restore

# Lancer en développement
dotnet run
```

L'application sera accessible sur `http://localhost:5106`.

### Build de production

```bash
dotnet publish -c Release --nologo
```

Le dossier de sortie est `bin/Release/net9.0/publish/wwwroot`.

---

## Workflow de développement

### 1. Créer une branche

```bash
git checkout -b feature/ma-fonctionnalite
```

Conventions de nommage :
- `feature/ma-fonctionnalite` — nouvelle fonctionnalité
- `fix/mon-correction` — correction de bug
- `refactor/mon-refactoring` — refactoring
- `docs/mise-a-jour` — documentation
- `perf/optimisation` — performance

### 2. Développer

- Suivre les [conventions de code](#conventions-de-code)
- Tester manuellement dans le navigateur
- Vérifier qu'aucune régression n'est introduite

### 3. Valider avant commit

```bash
# Vérifier que le build passe
dotnet build --nologo

# Vérifier les warnings (0 attendu)
dotnet build --nologo --verbosity quiet 2>&1 | grep -i warning
```

### 4. Committer

Suivre les [conventions de commit](#conventions-de-commit).

### 5. Pusher et ouvrir une PR

```bash
git push origin feature/ma-fonctionnalite
```

Puis ouvrir une Pull Request sur GitHub.

---

## Conventions de code

### Général

- **Pas de commentaires** dans le code sauf exception justifiée
- Utiliser les noms de variables et méthodes explicites en anglais
- Suivre les conventions du code existant
- **0 warning** à la compilation — obligatoire

### C# (Blazor)

- Accolades sur la même ligne (style K&R)
- Utiliser `var` quand le type est évident
- Préférer les expressions lambda aux méthodes anonymes
- Marquer les méthodes JSInvokable avec `[JSInvokable]`
- Injecter les dépendances avec `@inject` dans les pages/ composants
- Utiliser `InjectAttribute` dans les `@code` blocks
- Toujours implémenter `IDisposable` si des abonnements événementiels sont faits

```csharp
// Bon
private async Task ChargerDonneesAsync()
{
    var resultat = await Service.GetAsync();
    if (resultat == null) return;
    _donnees = resultat;
}

// Éviter les redondances
private async Task LoadDataAsync()
{
    List<Data> data = await Service.GetDataAsync();
    this.data = data; // pas clair : this.data vs data
}
```

### Razor

- Utiliser les composants MudBlazor (`MudButton`, `MudPaper`, etc.)
- Balisage auto-fermant pour les composants sans children
- Attributs déclarés dans l'ordre : paramètres obligatoires → optionnels → events
- Utiliser `@* ... *@` pour les commentaires Razor (pas HTML `<!-- -->`)
- Limiter la logique dans le balisage — déléguer au `@code`

```razor
@* Bon *@
<MudButton Variant="Variant.Filled" Color="Color.Primary" OnClick="@Handler">Cliquez</MudButton>

@* Éviter *@
<MudButton OnClick="@(() => { var x = 1; FaireQuelqueChose(x); })">Cliquez</MudButton>
```

### JavaScript (interop)

- Toutes les fonctions exposées dans `window.bible*` (namespace)
- Préfixer les fonctions privées avec `_`
- Utiliser `try/catch` pour toutes les opérations IndexedDB
- Commenter les blocs principaux (mais pas chaque ligne)
- Privilégier ES5 (`var`, `function`) pour compatibilité navigateur large

```javascript
// Bon
window.bibleMonModule = {
    _valeurPrivee: 0,

    faireQuelqueChose: function(param) {
        try {
            // Traitement
        } catch(e) {
            console.warn('Erreur:', e);
        }
    }
};
```

### CSS

- Utiliser les **variables CSS** (`var(--mud-palette-*)`) pour la compatibilité dark mode
- Éviter les valeurs hardcodées de couleurs/ombres en mode clair uniquement
- Préfixer les classes personnalisées (pas de conflit avec MudBlazor)
- Les variables dark mode dans `.mud-theme-dark { ... }`
- Animer les transitions du thème avec `transition: ... 0.3s ease`

```css
/* Bon — compatible dark mode */
.ma-classe {
    background: var(--mud-palette-surface, #fff);
    color: var(--mud-palette-text-primary, #111);
}

/* À éviter — couleur fixe */
.ma-classe {
    background: #fff;
    color: #111;
}
```

---

## Conventions de commit

On suit le format [Conventional Commits](https://www.conventionalcommits.org/) :

```
<type>(<scope>): <description>

[optional body]
```

### Types

| Type | Usage |
|---|---|
| `feat` | Nouvelle fonctionnalité |
| `fix` | Correction de bug |
| `refactor` | Refactoring (ni feat ni fix) |
| `perf` | Amélioration de performance |
| `docs` | Documentation |
| `style` | Formatage, espacement (pas de changement logique) |
| `test` | Ajout ou modification de tests |
| `chore` | Tâches techniques (build, CI, dépendances) |

### Scope

- `ui` — Interface utilisateur (pages, composants)
- `service` — Services (BibleService, SearchService, etc.)
- `js` — JavaScript interop
- `pwa` — Service worker, manifeste, offline
- `data` — Données bibliques JSON
- `seo` — SEO, Analytics, meta
- `config` — Configuration (Vercel, CSP, .csproj)
- `docs` — Documentation

### Exemples

```
feat(ui): ajouter le surlignage de versets avec 5 couleurs

- Pick de couleur inline (jaune, vert, bleu, rose, orange)
- Sauvegarde dans IndexedDB
- CSS dark mode compatible

fix(js): corriger la hauteur canvas pour WhatsApp

- Canvas réduit de 520px à 400px
- Fond aléatoire pour chaque partage

docs(readme): mettre à jour la stack technique

refactor(service): extraire la logique de plan de lecture
```

---

## Tests

### Tests de build

```bash
dotnet build --nologo
```

Le build doit passer avec **0 erreur et 0 warning**.

### Tests fonctionnels (manuel)

1. Naviguer sur les pages modifiées
2. Vérifier en mode clair **et** sombre
3. Tester sur mobile (responsive)
4. Vérifier la console JavaScript (F12) : **0 erreur**
5. Tester offline (déconnecter le réseau)
6. Vérifier les animations et transitions

### Points de vérification par fonctionnalité

**Nouvelle page :**
- [ ] Breadcrumb présent et fonctionnel
- [ ] Titre de page correct dans l'AppBar
- [ ] Bouton retour fonctionnel
- [ ] Responsive mobile/desktop
- [ ] Mode nuit compatible

**Surlignage :**
- [ ] Pick de couleur s'affiche au clic
- [ ] Sauvegarde persistante après refresh
- [ ] Retrait possible (deux clics)
- [ ] Couleurs visibles en mode nuit

**Recherche :**
- [ ] Mode ET intersecte correctement
- [ ] Mode OU unionne correctement
- [ ] Phrase exacte `"entre guillemets"` fonctionne
- [ ] Filtres testament/livre appliqués

---

## Signaler un bug

Avant de signaler un bug :
1. Vérifie que le bug n'a pas déjà été signalé dans les [Issues GitHub](https://github.com/akaletekoffilevis/Bible/issues)
2. Vérifie sur la [version live](https://bibeli.vercel.app) si le bug est reproductible

Pour signaler, utilise le modèle suivant :

```markdown
**Description :**
[Description claire et concise du bug]

**Étapes pour reproduire :**
1. Aller sur [...]
2. Cliquer sur [...]
3. Voir l'erreur [...]

**Comportement attendu :**
[Ce qui devrait se produire]

**Comportement actuel :**
[Ce qui se produit actuellement]

**Captures d'écran :**
[Si applicable]

**Environnement :**
- Navigateur : [Chrome/Firefox/Safari/Edge]
- Version : [ex: Chrome 120]
- Appareil : [PC/Mobile/Tablette]
- OS : [Windows/macOS/iOS/Android]

**Contexte supplémentaire :**
[Tout autre contexte pertinent]
```

**Contact direct :**
- WhatsApp : [+227 91 53 52 20](https://wa.me/22791535220?text=Bonjour%20Koffi%2C%20je%20signale%20un%20probl%C3%A8me%20sur%20Bibeli%20%3A)
- Email : [koffilevis21@gmail.com](mailto:koffilevis21@gmail.com?subject=Bibeli%20-%20Signalement%20de%20bug)

---

## Proposer une fonctionnalité

1. Vérifie les [Issues GitHub](https://github.com/akaletekoffilevis/Bible/issues) existantes
2. Vérifie les [Next Steps](README.md#next-steps) dans le README
3. Ouvre une issue avec le label `enhancement`
4. Décris clairement le besoin et le cas d'usage

**Par contact direct :**
- WhatsApp : [+227 91 53 52 20](https://wa.me/22791535220?text=Bonjour%20Koffi%2C%20voici%20ma%20suggestion%20pour%20Bibeli%20%3A)
- Email : [koffilevis21@gmail.com](mailto:koffilevis21@gmail.com?subject=Bibeli%20-%20Suggestion)

---

## Soumettre une Pull Request

### Prérequis

- [ ] Le build passe avec 0 erreur et 0 warning
- [ ] Les conventions de code sont respectées
- [ ] Les commits suivent le format conventionnel
- [ ] La branche est à jour avec `main`

### Étapes

1. **Mettre à jour sa branche**
   ```bash
   git checkout main
   git pull origin main
   git checkout feature/ma-fonctionnalite
   git rebase main
   ```

2. **Vérifier le build une dernière fois**
   ```bash
   dotnet build --nologo
   ```

3. **Ouvrir la Pull Request** sur GitHub
   - Titre clair : `feat(scope): description`
   - Description détaillée des changements
   - Référencer l'issue si applicable (`Closes #123`)

4. **Review**
   - Un mainteneur examinera la PR
   - Des modifications peuvent être demandées
   - Une fois approuvée, la PR sera mergée

### Checklist de PR

- [ ] Build : 0 erreur, 0 warning
- [ ] Console JS : 0 erreur
- [ ] Testé en mode clair et sombre
- [ ] Testé en responsive mobile
- [ ] Testé offline (PWA)
- [ ] Pas de régression sur les fonctionnalités existantes
- [ ] Documentation mise à jour si nécessaire

---

## Questions ?

- **Email :** [koffilevis21@gmail.com](mailto:koffilevis21@gmail.com)
- **WhatsApp :** [+227 91 53 52 20](https://wa.me/22791535220)
- **GitHub :** [akaletekoffilevis/Bible](https://github.com/akaletekoffilevis/Bible)
- **Site :** [bibeli.vercel.app](https://bibeli.vercel.app)

---

*Merci de contribuer à Bibeli ! « Que la parole de Christ habite parmi vous abondamment. » — Colossiens 3:16*
