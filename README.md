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

## Nouveautés (v2.0+)

- **Fil d'Ariane** : `Shared/Breadcrumb.razor` avec `BreadcrumbNode`, navigation contextuelle sur 8 pages
- **Image de verset** : lien `bibeli.vercel.app` visible en bas + partage avec URL directe du verset
- **Loading screen** : barre de progression avec pourcentage, 16 étapes progressives, délai ralenti vers la fin
- **Paramètres** : bindings `@bind-Value:after`, `EffacerDonnees` avec `ShowMessageBox`, réinitialisation complète UI
- **Support & Contact** : panneau dédié avec WhatsApp pré-rempli pour bug, email pour suggestions
- **Panels expansibles** : À propos, Confidentialité, CGU, Support ouverts par défaut (`Expanded="true"`)
- **Consent banner** : confirmation Snackbar au clic sur OK
- **Drawer PC** : `!important` restreint à `mud-drawer--open`, fermeture uniquement sur mobile après navigation
- **Sidebar navigation** : `MudButton` avec `Href` au lieu de `MudChip` (non fonctionnel pour la navigation)
- **UI Chapitres** : bannière gradient, grille responsive en cards, badge "Lu" sur dernier chapitre
- **UI Versets** : cartes individuelles, numéro en cercle, actions au hover, toolbar arrondie, boutons nav remplis
- **Google Analytics** : `G-FWDV79PT44` avec CSP mis à jour (googletagmanager.com, google-analytics.com)
- **Google Search Console** : meta tag de vérification
- **Sitemap/robots** : rewrite Vercel corrigé pour servir `sitemap.xml` et `robots.txt` correctement
- **`vercel.json`** : règles rewrite explicites pour fichiers statiques, `Content-Type: application/json` pour data
- **`app.css`** : `chapitre-grid`, `verset-card`, `nav-chip`, transitions, print, mobile-first
- **Service worker** : cache `bible-data-v1` séparé, 66 livres mis en cache à l'installation
- **Performance** : `PublishTrimmed` + `AggressiveTrimming` (8.9 MB Brotli, 3.6 MB framework)

---

## Structure du projet

```
BibleApp/
├── Layout/
│   └── MainLayout.razor       # Layout principal + drawer (Persistent/Temporary) + consentement + préférences
├── Pages/
│   ├── Index.razor            # Accueil (verset du jour + skeleton)
│   ├── LivreChapitres.razor   # Sélection de chapitre (grille cards + breadcrumb)
│   ├── Lecture.razor          # Lecture chapitre (cartes versets + TTS + actions + breadcrumb)
│   ├── Recherche.razor        # Recherche plein texte + breadcrumb
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
│   ├── IndexedDbService.cs    # CRUD IndexedDB
│   └── ThemeService.cs        # Thème clair/sombre
├── Models/
│   ├── Bible.cs               # Modèles de données (Livre, Chapitre, Verset)
│   └── BreadcrumbNode.cs      # Modèle pour le fil d'Ariane
├── wwwroot/
│   ├── css/app.css            # Styles personnalisés + responsive + transitions + print
│   ├── js/app.js              # JS interop (TTS, IndexedDB, image canvas, drawer, Leaflet, PWA install)
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
└── BibleApp.csproj            # Projet .NET 9 (PublishTrimmed + AggressiveTrimming)
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

## Licence

Distribué sous licence **MIT**. Voir le fichier [LICENSE](LICENSE).

---

## Crédits

**AKALETE Koffi Levis** — [koffilevis21@gmail.com](mailto:koffilevis21@gmail.com) — [WhatsApp +227 91 53 52 20](https://wa.me/22791535220)

Pour signaler un bug ou suggérer une amélioration :
- [WhatsApp](https://wa.me/22791535220?text=Bonjour%20Koffi%2C%20je%20signale%20un%20probl%C3%A8me%20sur%20Bibeli%20%3A)
- [Email](mailto:koffilevis21@gmail.com?subject=Bibeli%20-%20Signalement%20de%20bug)

---

*"Car la parole de Dieu est vivante et efficace." — Hébreux 4:12*
