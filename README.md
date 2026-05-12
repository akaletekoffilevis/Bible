# Bible Louis Segond (LSG)

Application **Progressive Web App (PWA)** pour lire la Bible Louis Segond en ligne et hors-ligne. Développée en **Blazor WebAssembly .NET 9** avec **MudBlazor 8.x**.

**Site :** [bibeli.vercel.app](https://bibeli.vercel.app)

---

## Fonctionnalités

- **66 livres** — Ancien et Nouveau Testament chargés en lazy-loading
- **Recherche plein texte** — index inversé, filtre par testament/livre, intersection
- **Audio TTS** — lecture verset-par-verset avec voix française, pause/reprise
- **Marque-pages** — sauvegardés en IndexedDB
- **Notes personnelles** — attacher une note à chaque verset
- **Surlignage** — mettre en évidence des versets
- **Copie / Partage** — Web Share API + copie au clic
- **Image de verset** — génération Canvas PNG avec 12 palettes, dégradé, lien `bibeli.vercel.app` en bas
- **Export PDF** — impression du chapitre en PDF
- **Mode nuit** — thème sombre/clair
- **Navigation clavier** — flèches entre chapitres
- **Fil d'Ariane (breadcrumb)** — navigation contextuelle sur toutes les pages
- **Carte biblique** — 12 lieux avec Leaflet (lazy-load)
- **Quiz** — mot manquant, 10 questions aléatoires par partie
- **Progression** — suivi des chapitres lus avec statistiques AT/NT
- **PWA** — installation sur téléphone/desktop, fonctionnement hors-ligne, cache 66 livres à l'install
- **SEO** — sitemap.xml, robots.txt, JSON-LD structuré, balises Open Graph

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
| **PWA** | Service worker + manifeste + cache offline complet |

---

## Nouveautés (v2.0+)

- **Fil d'Ariane** : `Shared/Breadcrumb.razor` avec navigation contextuelle sur chaque page
- **Image de verset** : lien `bibeli.vercel.app` visible en bas + partage avec URL directe du verset
- **Loading screen** : barre de progression avec pourcentage, étapes progressives (ralentissement vers la fin)
- **Paramètres** : bindings corrigés avec `@bind-Value:after`, réinitialisation complète avec dialogue de confirmation
- **`EffacerDonnees`** : boîte de dialogue `ShowMessageBox`, réinitialisation UI + Snackbar
- **`MainLayout.razor`** : chargement global des préférences (police, taille, interligne, mode nuit) à chaque page
- **`app.css`** : transitions de page, ombres portées, skeleton shimmer, scrollbar subtile, print media query
- **Service worker** : cache `bible-data-v1` séparé, 66 livres mis en cache à l'installation
- **SEO** : sitemap.xml (66 livres), robots.txt, JSON-LD structuré, canonical
- **PWA** : notification d'installation après 30s puis toutes les 5 min
- **Performance** : `PublishTrimmed` + `AggressiveTrimming` (8.9 MB Brotli, 3.6 MB framework)

---

## Structure du projet

```
BibleApp/
├── Layout/
│   └── MainLayout.razor       # Layout principal + drawer + chargement préférences
├── Pages/
│   ├── Index.razor            # Accueil (verset du jour + skeleton)
│   ├── LivreChapitres.razor   # Sélection de chapitre + breadcrumb
│   ├── Lecture.razor          # Lecture chapitre + TTS + actions verset + breadcrumb
│   ├── Recherche.razor        # Recherche plein texte + breadcrumb
│   ├── Favoris.razor          # Marque-pages et notes + breadcrumb
│   ├── Progression.razor      # Suivi de lecture AT/NT + breadcrumb
│   ├── Quiz.razor             # Quiz biblique (Random.Shared) + breadcrumb
│   ├── Carte.razor            # Carte Leaflet lazy-load + breadcrumb
│   └── Parametres.razor       # Paramètres (police, voix, traduction, données) + breadcrumb
├── Shared/
│   ├── NavMenu.razor          # Navigation sidebar 2 colonnes + MudChip
│   ├── NoteDialog.razor       # Dialog de note
│   ├── VersetCard.razor       # Carte verset réutilisable
│   └── Breadcrumb.razor       # Fil d'Ariane contextuel
├── Services/
│   ├── BibleService.cs        # Chargement livres JSON + cache IndexedDB
│   ├── SearchIndexService.cs  # Index recherche inversé (Singleton)
│   ├── IndexedDbService.cs    # CRUD IndexedDB
│   └── ThemeService.cs        # Thème clair/sombre
├── Models/
│   ├── Bible.cs               # Modèles de données (Livre, Chapitre, Verset)
│   └── BreadcrumbNode.cs      # Modèle pour le fil d'Ariane
├── wwwroot/
│   ├── css/app.css            # Styles personnalisés + responsive + transitions
│   ├── js/app.js              # JS interop (TTS, IndexedDB, image, drawer, etc.)
│   ├── data/books/*.json      # 66 fichiers livres (LSG)
│   ├── data/index.json        # Index des livres
│   ├── index.html             # Point d'entrée + loading screen + SEO JSON-LD + CSP
│   ├── favicon.svg            # Icône vectorielle personnalisée
│   ├── icon-192.png           # Icône PWA
│   ├── icon-512.png           # Icône PWA haute résolution
│   ├── manifest.webmanifest   # Manifeste PWA
│   ├── service-worker.js      # Service worker
│   ├── service-worker.published.js  # Service worker production (cache offline)
│   ├── sitemap.xml            # SEO (66 livres)
│   └── robots.txt             # SEO
├── vercel.json                # Configuration déploiement Vercel + en-têtes sécurité
├── LICENSE                    # Licence MIT
└── BibleApp.csproj            # Projet .NET 9 (PublishTrimmed)
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
    { "source": "/(.*)", "destination": "/index.html" }
  ]
}
```

> Vercel n'inclut pas .NET SDK nativement. La commande de build l'installe avant de compiler.

---

## Licence

Distribué sous licence **MIT**. Voir le fichier [LICENSE](LICENSE).

---

## Crédits

**AKALETE Koffi Levis** — [koffilevis21@gmail.com](mailto:koffilevis21@gmail.com) — [WhatsApp +227 91 53 52 20](https://wa.me/22791535220)

---

*"Car la parole de Dieu est vivante et efficace." — Hébreux 4:12*
