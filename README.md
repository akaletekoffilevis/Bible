# Bible Louis Segond (LSG)

Application **Progressive Web App (PWA)** pour lire la Bible Louis Segond en ligne et hors-ligne. Développée en **Blazor WebAssembly .NET 9** avec **MudBlazor 8.x**.

**Site :** [bibeli.vercel.app](https://bibeli.vercel.app)

---

## Fonctionnalités

- **66 livres** — Ancien et Nouveau Testament chargés en lazy-loading
- **Recherche plein texte** — index inversé, filtres testament/livre, modes ET/OU, recherche exacte `"entre guillemets"`
- **Audio TTS** — lecture verset-par-verset avec voix française, pause/reprise, prime silencieux pour débloquer mobile
- **Marque-pages** — sauvegardés en IndexedDB
- **Notes personnelles** — attacher une note à chaque verset
- **Surlignage 5 couleurs** — jaune, vert, bleu, rose, orange, sauvegardé en IndexedDB
- **Partage texte brut** — Web Share API ou presse-papier
- **Image de verset** — génération Canvas PNG, 12 palettes aléatoires, 600×400px optimisé WhatsApp
- **Image Verset du Jour** — bouton dédié Image + bouton Partager texte séparés
- **Export PDF** — impression du chapitre en PDF
- **Navigation par référence** — taper "Genèse 1:1", "Jn 3:16", "Ps 23" → accès direct
- **Plan de lecture 1 an** — 365 jours, AT + NT + Psaumes, progression, streak jours consécutifs
- **Mode nuit** — thème sombre/clair fiable (contournement bug MudBlazor 8.15)
- **Navigation clavier** — flèches ← → entre chapitres
- **Fil d'Ariane (breadcrumb)** — navigation contextuelle sur toutes les pages
- ~~Carte biblique~~ — retirée du menu de navigation
- **Quiz** — mot manquant, 10 questions aléatoires par partie
- **Progression** — suivi des chapitres lus avec statistiques AT/NT
- **PWA** — installation sur téléphone/desktop, cache offline 66 livres, notification Snackbar, détection automatique
- **SEO** — sitemap.xml (66 livres), robots.txt optimisé, JSON-LD structuré, balises Open Graph, Google Search Console
- **Analytics** — Google Analytics (G-FWDV79PT44)
- **Contact & Support** — signalement de bug via WhatsApp/email, suggestions
- **Personnalisation** — taille police (14-28px), famille (Serif/Sans-serif/Georgia), interlignage (1.5-2.8)
- **Traduction** — Google Translate intégré (11 langues, chargé uniquement si activé)
- **Version dynamique** — affichée depuis `version.json` dans les Paramètres

---

## Stack technique

| Technologie | Usage |
|---|---|
| **Blazor WebAssembly .NET 9** | Framework frontend |
| **MudBlazor 8.x** | UI components |
| **IndexedDB** | Stockage local (marque-pages, notes, progression) |
| **Leaflet** | Carte biblique interactive (lazy-load via unpkg) |
| **Web Speech API** | Synthèse vocale TTS |
| **Web Share API** | Partage natif avec image PNG |
| **Canvas API** | Génération d'images de versets (12 palettes, gradient, URL branding) |
| **PWA** | Service worker + manifeste + cache offline progressif par livre |
| **Google Analytics** | Mesure d'audience |
| **Vercel** | Hébergement + déploiement automatique |

---

## Nouveautés

### v2.4 (mai 2026)
- **Thème nuit/corrigé** — `@key` sur `MudThemeProvider` + `bibleUtils.applyTheme()` JS pour contourner un bug MudBlazor 8.15 qui ne ré-appliquait pas la classe `mud-theme-dark`
- **Page Recherche fonctionnelle** — indexation automatique des 66 livres, debounce 300ms, recherche plein texte instantanée avec résultats cliquables
- **Actions verset en overlay** — popup (🔖📝🖍️📋🖼️) et ☰ passés en `position:absolute` : ne poussent plus le texte. Fond `#ffffff` opaque, boutons agrandis (`1.2em`)
- **DI corrigé** — `SearchIndexService` passe en `AddScoped` (était Singleton mais dépendait de `BibleService` scoped → bloquait tout le rendu)
- **Slugifier sécurisé** — `Normalize(FormD)` + `CharUnicodeInfo` protégés par try/catch pour `InvariantGlobalization=true`
- **Logs ajoutés** — `catch { }` → `catch (Exception ex) avec Console.Error.WriteLine` dans `BibleService`
- **Navigation carte retirée** — lien `/carte` supprimé du menu de navigation

### v2.3 (mai 2026)
- **Partage Verset du Jour** — deux boutons séparés : Image (téléchargement PNG) et Partager (texte brut)
- **Stabilité** — tous les `onclick` avec JS interop sont protégés par try/catch
- **JsRuntime déplacé** — `LivreChapitres` ne fait plus d'appels JS dans `OnInitializedAsync`
- **Paramètres fiabilisés** — chargement localStorage déplacé en `OnAfterRenderAsync`
- **Build projet à la racine** — suppression du dossier `BibleApp/` en double, `vercel.json` sans `rootDirectory`
- **Background fix** — `background-attachment: fixed` empêche le fond de bouger au scroll
- **Cache offline progressif** — 67 fichiers/43 Mo mis en cache un par un (lots de 5), un échec ne bloque pas le reste
- **Mémoire index recherche** — `VerseRef` interne sans texte dupliqué : ~150MB → ~15MB

### v2.2 (mai 2026)
- Surlignage versets 5 couleurs, navigation par référence, recherche avancée ET/OU/exacte
- Plan de lecture 365 jours, thème nuit fiable, image verset normalisée
- Build 0 err 0 warn

### v2.1 (mai 2026)
- Partage image Verset du Jour, ErrorBoundary global, gestion erreur JS
- Écran blanc mobile, mémoire réduite, IndexedDB résilient, CSS override lecture
- Recherche réparée, IndexedDbService optimisé, ErrorBoundary Recover

---

## Structure du projet

```
├── Layout/
│   └── MainLayout.razor       # Layout principal + drawer + consentement + préférences
├── Pages/
│   ├── Index.razor            # Accueil (verset du jour + image + partage texte)
│   ├── LivreChapitres.razor   # Sélection de chapitre (grille cards + reprise lecture)
│   ├── Lecture.razor          # Lecture chapitre (versets + TTS + actions + surlignage)
│   ├── Recherche.razor        # Recherche plein texte + modes ET/OU + exacte + filtres
│   ├── Reference.razor        # Navigation rapide par référence biblique
│   ├── PlanLecture.razor      # Plan de lecture 1 an + progression + streak
│   ├── Favoris.razor          # Marque-pages et notes
│   ├── Progression.razor      # Suivi de lecture AT/NT
│   ├── Quiz.razor             # Quiz biblique
│   ├── Carte.razor            # Carte Leaflet lazy-load
│   └── Parametres.razor       # Paramètres + À propos + version dynamique
├── Shared/
│   ├── NavMenu.razor          # Navigation sidebar
│   ├── NoteDialog.razor       # Dialog de note
│   ├── VersetCard.razor       # Carte verset réutilisable
│   └── Breadcrumb.razor       # Fil d'Ariane contextuel
├── Services/
│   ├── BibleService.cs        # Chargement livres JSON + cache IndexedDB
│   ├── SearchIndexService.cs  # Index recherche inversé (Scoped, mémoire optimisée)
│   ├── IndexedDbService.cs    # CRUD IndexedDB + highlights + planProgress
│   ├── ReadingPlanService.cs  # Plan de lecture 365 jours
│   └── ThemeService.cs        # Thème clair/sombre
├── Models/
│   ├── Bible.cs               # Modèles de données (Livre, Chapitre, Verset)
│   ├── Highlight.cs           # Modèle surlignage (5 couleurs)
│   ├── PlanJour.cs            # Modèle progression plan lecture
│   └── BreadcrumbNode.cs      # Modèle pour le fil d'Ariane
├── wwwroot/
│   ├── css/app.css            # Styles personnalisés + responsive + transitions
│   ├── js/app.js              # JS interop (TTS, IndexedDB, image canvas, PWA install, partage)
│   ├── data/books/*.json      # 66 fichiers livres (LSG)
│   ├── data/index.json        # Index des livres
│   ├── index.html             # Point d'entrée + loading screen + SEO + CSP
│   ├── version.json           # Version dynamique
│   ├── favicon.svg / .png     # Icônes
│   ├── icon-192.png / 512.png # Icônes PWA
│   ├── manifest.webmanifest   # Manifeste PWA
│   ├── service-worker.js      # Service worker développement
│   ├── service-worker.published.js  # Service worker production (cache progressif)
│   ├── sitemap.xml            # SEO (66 livres ch.1)
│   └── robots.txt             # SEO optimisé
├── vercel.json                # Build .NET 9 + rewrites + en-têtes sécurité
├── App.razor                  # Root component + ErrorBoundary
├── Program.cs                 # Point d'entrée + DI
├── _Imports.razor             # Usings globaux
├── BibleApp.csproj            # Projet .NET 9
├── LICENSE                    # Licence MIT
└── .gitignore
```

---

## Déploiement

### Prérequis

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

### Développement local

```bash
dotnet run
```

Dev server : `http://localhost:5106`

### Build

```bash
dotnet publish -c Release
```

### Déploiement Vercel (automatique)

Le projet est connecté à GitHub + Vercel. Chaque push sur `main` déclenche automatiquement :

1. Installation de .NET 9 SDK via `dotnet-install.sh`
2. `dotnet publish -c Release --nologo`
3. Déploiement sur **bibeli.vercel.app**

---

## Configuration

### vercel.json

```json
{
  "buildCommand": "curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 9.0 --install-dir /opt/dotnet && /opt/dotnet/dotnet publish -c Release --nologo",
  "outputDirectory": "bin/Release/net9.0/publish/wwwroot",
  "rewrites": [
    { "source": "/sitemap.xml", "destination": "/sitemap.xml" },
    { "source": "/robots.txt", "destination": "/robots.txt" },
    { "source": "/data/(.*)", "destination": "/data/$1" },
    { "source": "/(.*)\\.(json|js|css|wasm|dll|png|svg|ico|webmanifest)", "destination": "/$1.$2" },
    { "source": "/(.*)", "destination": "/index.html" }
  ],
  "headers": [
    {
      "source": "/(.*)\\.wasm",
      "headers": [{ "key": "Content-Type", "value": "application/wasm" }]
    },
    {
      "source": "/(.*)\\.dll",
      "headers": [{ "key": "Content-Type", "value": "application/octet-stream" }]
    },
    {
      "source": "/data/(.*)\\.json",
      "headers": [{ "key": "Content-Type", "value": "application/json; charset=utf-8" }]
    },
    {
      "source": "/(.*)",
      "headers": [
        { "key": "X-Content-Type-Options", "value": "nosniff" },
        { "key": "X-Frame-Options", "value": "DENY" },
        { "key": "Referrer-Policy", "value": "strict-origin-when-cross-origin" },
        { "key": "Permissions-Policy", "value": "camera=(), microphone=(), geolocation=()" }
      ]
    }
  ]
}
```

> Vercel n'inclut pas .NET SDK nativement. La commande de build l'installe avant de compiler.

---

## License

Distribué sous licence **MIT**. Voir le fichier [LICENSE](LICENSE).

---

## Crédits

**AKALETE Koffi Levis** — [koffilevis21@gmail.com](mailto:koffilevis21@gmail.com) — [WhatsApp +227 91 53 52 20](https://wa.me/22791535220)

Pour signaler un bug ou suggérer une amélioration :
- [WhatsApp](https://wa.me/22791535220?text=Bonjour%20Koffi%2C%20je%20signale%20un%20probl%C3%A8me%20sur%20Bibeli%20%3A)
- [Email](mailto:koffilevis21@gmail.com?subject=Bibeli%20-%20Signalement%20de%20bug)

---

*"Car la parole de Dieu est vivante et efficace." — Hébreux 4:12*
