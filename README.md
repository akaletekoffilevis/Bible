# Bible Louis Segond (LSG)

Application **Progressive Web App (PWA)** pour lire la Bible Louis Segond en ligne et hors-ligne. Développée en **Blazor WebAssembly .NET 9** avec **MudBlazor**.

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
- **Image de verset** — génération Canvas PNG téléchargeable
- **Export PDF** — impression du chapitre en PDF
- **Mode nuit** — thème sombre/clair
- **Navigation clavier** — flèches entre chapitres
- **Carte biblique** — 12 lieux avec Leaflet
- **Quiz** — mot manquant, 10 questions par partie
- **Progression** — suivi des chapitres lus
- **PWA** — installation sur téléphone/desktop, fonctionnement hors-ligne
- **SEO** — sitemap, robots.txt, JSON-LD structuré

---

## Stack technique

| Technologie | Usage |
|---|---|
| **Blazor WebAssembly .NET 9** | Framework frontend |
| **MudBlazor 8.x** | UI components |
| **IndexedDB** | Stockage local (marque-pages, notes, progression) |
| **Leaflet** | Carte biblique interactive |
| **Web Speech API** | Synthèse vocale TTS |
| **Web Share API** | Partage natif |
| **Canvas API** | Génération d'images de versets |
| **PWA** | Service worker + manifeste |

---

## Structure du projet

```
BibleApp/
├── Layout/
│   └── MainLayout.razor       # Layout principal + drawer
├── Pages/
│   ├── Index.razor            # Accueil (verset du jour)
│   ├── LivreChapitres.razor   # Sélection de chapitre
│   ├── Lecture.razor          # Lecture chapitre + actions verset
│   ├── Recherche.razor        # Recherche plein texte
│   ├── Favoris.razor          # Marque-pages et notes
│   ├── Progression.razor      # Suivi de lecture
│   ├── Quiz.razor             # Quiz biblique
│   ├── Carte.razor            # Carte des lieux
│   └── Parametres.razor       # Paramètres (police, voix, etc.)
├── Shared/
│   ├── NavMenu.razor          # Navigation sidebar
│   ├── NoteDialog.razor       # Dialog de note
│   └── VersetCard.razor       # Carte verset réutilisable
├── Services/
│   ├── BibleService.cs        # Chargement livres JSON + cache IndexedDB
│   ├── SearchIndexService.cs  # Index recherche inversé
│   ├── IndexedDbService.cs    # CRUD IndexedDB
│   └── ThemeService.cs        # Thème clair/sombre
├── Models/
│   └── Bible.cs               # Modèles de données
├── wwwroot/
│   ├── css/app.css            # Styles personnalisés + responsive
│   ├── js/app.js              # JS interop (TTS, IndexedDB, drawer, etc.)
│   ├── data/books/*.json      # 66 fichiers livres
│   ├── data/index.json        # Index des livres
│   ├── index.html             # Point d'entrée + SEO JSON-LD
│   ├── favicon.svg            # Icône vectorielle personnalisée
│   ├── favicon.png            # Favicon fallback
│   ├── icon-192.png           # Icône PWA
│   ├── icon-512.png           # Icône PWA haute résolution
│   ├── manifest.webmanifest   # Manifeste PWA
│   ├── service-worker.js      # Service worker
│   ├── service-worker.published.js  # Service worker production
│   ├── sitemap.xml            # SEO
│   └── robots.txt             # SEO
├── vercel.json                # Configuration déploiement Vercel
├── LICENSE                    # Licence MIT
└── BibleApp.csproj            # Projet .NET
```

---

## Déploiement

### Prérequis

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

### Développement local

```bash
dotnet run
```

### Build

```bash
dotnet publish -c Release
```

### Déploiement Vercel (automatique)

Le projet est connecté à GitHub + Vercel. Chaque push sur `main` déclenche automatiquement :

1. Installation de .NET 9 SDK
2. `dotnet publish -c Release`
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

**Koffi Levis** — [koffilevis21@gmail.com](mailto:koffilevis21@gmail.com)

---

*"Car la parole de Dieu est vivante et efficace." — Hébreux 4:12*
