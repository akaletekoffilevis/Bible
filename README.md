# Bible Louis Segond (LSG)

Application **Progressive Web App (PWA)** pour lire la Bible Louis Segond en ligne et hors-ligne. Développée en **Blazor WebAssembly .NET 9** avec **MudBlazor 8.x**.

**Site :** [bibeli.vercel.app](https://bibeli.vercel.app)

---

## Fonctionnalités

- **66 livres** — Ancien et Nouveau Testament chargés en lazy-loading
- **Recherche plein texte** — index inversé, filtre par testament/livre, intersection
- **Audio TTS** — lecture verset-par-verset avec voix française, pause/reprise, `prime()` silencieux pour débloquer mobile
- **Marque-pages** — sauvegardés en IndexedDB
- **Notes personnelles** — attacher une note à chaque verset
- **Copie / Partage** — Web Share API avec image PNG + copie au clic
- **Image de verset** — génération Canvas PNG, 12 palettes, dégradé, lien `bibeli.vercel.app` + URL directe du verset
- **Image Verset du Jour** — partage avec titre "✨ Verset Du Jour ✨", police aléatoire (Georgia, Palatino, Garamond...)
- **Export PDF** — impression du chapitre en PDF
- **Mode nuit** — thème sombre/clair
- **Navigation clavier** — flèches ← → entre chapitres
- **Fil d'Ariane (breadcrumb)** — navigation contextuelle sur toutes les pages
- **Carte biblique** — 12 lieux avec Leaflet (lazy-load via unpkg)
- **Quiz** — mot manquant, 10 questions aléatoires par partie, bouton quitter
- **Progression** — suivi des chapitres lus avec statistiques AT/NT
- **PWA** — installation sur téléphone/desktop, cache 66 livres à l'install, notification répétée
- **SEO** — sitemap.xml (66 livres), robots.txt optimisé, JSON-LD structuré, balises Open Graph, Google Search Console
- **Analytics** — Google Analytics (G-FWDV79PT44)
- **Contact & Support** — signalement de bug via WhatsApp/email, suggestions
- **Personnalisation** — taille police (14-28px), famille (Serif/Sans-serif/Georgia), interlignage (1.5-2.8)
- **Traduction** — Google Translate intégré (11 langues, chargé uniquement si activé)

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
| **Google Analytics** | Mesure d'audience |
| **Vercel** | Hébergement + déploiement automatique |

---

## Nouveautés

### v2.1 (mai 2026)
- **Partage image Verset du Jour** — titre "✨ Verset Du Jour ✨" avec police aléatoire (6 polices système), canvas 600×520px
- **ErrorBoundary global** — capture les erreurs de rendu Blazor avec fallback UI + bouton Réessayer
- **Gestion erreur JS** — `window.onerror` global avec fallback dans le loader
- **Écran blanc mobile** — loader 45s avec message d'erreur + bouton + contact WhatsApp au lieu de page blanche
- **Mémoire réduite** — `InvariantGlobalization=true` + `PreserveCollationData=false` (-2 MB WASM)
- **IndexedDB résilient** — `_supported` flag, try/catch, ne rejette plus en mode privé
- **CSS override lecture** — `!important` sur `.verset-texte`/`.verset-card-body` pour que les réglages police/taille/interligne s'appliquent malgré MudBlazor
- **Recherche réparée** — `@bind-Value:after="Rechercher"` manquant bloquait toute recherche
- **IndexedDbService** — `_initialized` flag évite d'ouvrir une connexion à chaque opération
- **Recherche UX** — état "Indexation en cours..." avec skeleton pendant le chargement de l'index
- **ErrorBoundary Recover** — navigation `forceLoad: false` (pas de reload complet inutile)

### v2.0
- Fil d'Ariane sur 8 pages, image verset avec branding, loading screen %, Paramètres robustes
- Support & Contact, panels expansibles, consent banner avec Snackbar
- Drawer PC/Mobile, sidebar MudButton, UI chapitres/versets améliorée
- Google Analytics, Search Console, sitemap/robots optimisés, vercel.json propre
- app.css complet, service worker cache séparé, PublishTrimmed + AggressiveTrimming

---

## Structure du projet

```
BibleApp/
├── Layout/
│   └── MainLayout.razor       # Layout principal + drawer (Persistent/Temporary) + consentement + préférences
├── Pages/
│   ├── Index.razor            # Accueil (verset du jour + skeleton + partage image)
│   ├── LivreChapitres.razor   # Sélection de chapitre (grille cards + breadcrumb)
│   ├── Lecture.razor          # Lecture chapitre (cartes versets + TTS + actions + breadcrumb)
│   ├── Recherche.razor        # Recherche plein texte + indexation + breadcrumb
│   ├── Favoris.razor          # Marque-pages et notes + breadcrumb
│   ├── Progression.razor      # Suivi de lecture AT/NT + breadcrumb
│   ├── Quiz.razor             # Quiz biblique (Random.Shared) + breadcrumb
│   ├── Carte.razor            # Carte Leaflet lazy-load + breadcrumb
│   └── Parametres.razor       # Paramètres + À propos + Confidentialité + CGU + Support + breadcrumb
├── Shared/
│   ├── NavMenu.razor          # Navigation sidebar 2 colonnes (MudButton nav + MudNavLink livres)
│   ├── NoteDialog.razor       # Dialog de note
│   ├── VersetCard.razor       # Carte verset réutilisable
│   └── Breadcrumb.razor       # Fil d'Ariane contextuel réutilisable
├── Services/
│   ├── BibleService.cs        # Chargement livres JSON + cache IndexedDB
│   ├── SearchIndexService.cs  # Index recherche inversé (Singleton)
│   ├── IndexedDbService.cs    # CRUD IndexedDB (flag _initialized)
│   └── ThemeService.cs        # Thème clair/sombre
├── Models/
│   ├── Bible.cs               # Modèles de données (Livre, Chapitre, Verset)
│   └── BreadcrumbNode.cs      # Modèle pour le fil d'Ariane
├── wwwroot/
│   ├── css/app.css            # Styles personnalisés + responsive + transitions + print + error-boundary
│   ├── js/app.js              # JS interop (TTS, IndexedDB résilient, image canvas, drawer, Leaflet, PWA install)
│   ├── data/books/*.json      # 66 fichiers livres (LSG)
│   ├── data/index.json        # Index des livres
│   ├── index.html             # Point d'entrée + loading screen % + SEO JSON-LD + CSP + GA + Google Console
│   ├── favicon.svg            # Icône vectorielle personnalisée
│   ├── icon-192.png           # Icône PWA
│   ├── icon-512.png           # Icône PWA haute résolution
│   ├── manifest.webmanifest   # Manifeste PWA
│   ├── service-worker.js      # Service worker
│   ├── service-worker.published.js  # Service worker production (cache offline 66 livres)
│   ├── sitemap.xml            # SEO (66 livres ch.1)
│   └── robots.txt             # SEO optimisé (Googlebot, Bingbot, Crawl-delay)
├── vercel.json                # Build .NET 9 + rewrites statics + en-têtes sécurité
├── LICENSE                    # Licence MIT
└── BibleApp.csproj            # Projet .NET 9 (PublishTrimmed + AggressiveTrimming + InvariantGlobalization)
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

### vercel.json (extrait)

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
