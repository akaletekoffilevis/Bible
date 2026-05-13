# Projet Site Bible — Todo List

## Données JSON

| Fichier | Taille | Description |
| `BibleLSFull.json` | ~5.5 Mo | **Fichier source complet** — format Dict, 66 livres, 1189 chapitres |
| `BibleLSFull2.json` | ~5.5 Mo | Copie de secours |
| `BibleLSFull3.json` | ~5.5 Mo | Copie de secours |
| `BibleLS.json` | ~11 Mo | Même données en format Array |
| `data/books/*.json` | ~var | **JSON splitté** — 1 fichier par livre (généré par script) |

**Structure :**
```
ContenuBible[]
  → Titre: "Ancien Testament" / "Nouveau Testament"
  → Livres[]
    → NomLivre, Abreviation
    → ContenuChapitre[]
      → NumeroChapitre
      → ContenuVersets[]
        → NumeroVerset, Verset
```

**Version :** Louis Segond (français) — 39 livres AT + 27 livres NT

**Stratégie données :** Le `BibleLSFull.json` est splitté en 66 fichiers JSON individuels (1 par livre). Chaque livre est chargé à la demande (lazy loading) via un service dédié, ce qui évite de charger les 5.5 Mo en mémoire au démarrage.

---

## Stack Technique — Blazor WebAssembly (.NET 9)

| Composant | Choix |
|-----------|-------|
| **Frontend** | Blazor WebAssembly (PWA natif) |
| **UI** | MudBlazor (composants Material Design) |
| **Stockage local** | **IndexedDB** via JS interop (remplace localStorage) |
| **Données** | JSON splitté par livre (66 fichiers) + lazy loading |
| **Recherche** | **Index inversé** en mémoire (généré au lancement depuis les livres chargés) |
| **JSON** | System.Text.Json (intégré) |
| **PWA** | Service worker intégré + **cache intelligent** (cache-first pour les livres, network-first pour le search index) |
| **Déploiement** | GitHub Pages, Netlify, Azure Static Web Apps, Cloudflare Pages |

---

## Phase 1 — Fondation (MVP Blazor WASM)

- [ ] **Créer le projet** `dotnet new blazorwasm -o BibleApp --pwa`
- [ ] **Ajouter MudBlazor** (NuGet + configuration `Program.cs` + `_Imports.razor`)
- [ ] **Créer le modèle C#** `Bible.cs` (classes `Bible`, `Testament`, `Livre`, `Chapitre`, `Verset`)
- [ ] **Script de splitting JSON** — outil qui découpe `BibleLSFull.json` en 66 fichiers `data/books/{slug}.json`
- [ ] **Générer les slugs** — identifiant unique par livre (ex: `genese`, `exode`, `psaumes`, `matthieu`)
- [ ] **Service `BibleService`** — chargement lazy des livres : `GetLivreAsync(slug)` → fetch + parse + cache mémoire
- [ ] **Index des livres** — fichier `data/index.json` contenant la liste de tous les livres (sans les versets) pour affichage immédiat
- [ ] **Page d'accueil** — liste des livres (Ancien / Nouveau Testament) chargée depuis `index.json`
- [ ] **Page Livre** — sélection du chapitre
- [ ] **Page Lecture** — affichage des versets d'un chapitre
- [ ] **Navigation** livre → chapitre → versets avec `NavigationManager`
- [ ] **Composant Recherche** — champ texte + résultats cliquables
- [ ] **Design responsive** — MudBlazor gère le responsive (mobile + desktop)
- [ ] **Déploiement PWA** — test du service worker, chargement offline de base

## Phase 2 — Navigation & Confort de lecture

- [ ] **Drawer latéral** — navigation rapide par livre + chapitre
- [ ] **Boutons Précédent / Suivant** — chapitre précédent et suivant
- [ ] **Numérotation des versets** en exposant, bien formatée
- [ ] **Mode Nuit / Jour** — MudBlazor `MudThemeProvider`
- [ ] **Réglage de la police** — taille (petit/moyen/grand) + famille (serif/sans-serif)
- [ ] **Interlignage ajustable**
- [ ] **Mémorisation de la dernière position** de lecture (localStorage)
- [ ] **Barre de progression** du chapitre et du livre

## Phase 3 — Interactions utilisateur (IndexedDB)

- [ ] **Service `IndexedDbService`** — wrapper JS interop pour IndexedDB (open, get, set, delete, getAll)
- [ ] **Base IndexedDB `BibleAppDB`** avec stores : `bookmarks`, `notes`, `highlights`, `history`, `progress`
- [ ] **Marque-pages** — sauvegarder livre + chapitre + verset avec un label
- [ ] **Notes personnelles** — attacher un texte à un verset (stocké dans IndexedDB)
- [ ] **Surlignage** — 4-5 couleurs au choix, appliqué à un verset
- [ ] **Historique de lecture** — 50 dernières navigations
- [ ] **Progression de lecture** — % par livre, par testament, total
- [ ] **Stockage des Bibles dans IndexedDB** — une fois chargée, chaque livre JSON est mis en cache dans IndexedDB pour les visites suivantes (offline-ready)
- [ ] **Export des données utilisateur** — JSON (marque-pages, notes, surlignages)
- [ ] **Import des données utilisateur** — restaurer depuis un fichier JSON

## Phase 4 — Fonctionnalités avancées

- [ ] **Verset du jour** — aléatoire ou basé sur la date du jour (seed)
- [ ] **Index de recherche inversé** — à l'initialisation, parcourir tous les livres chargés et construire un `Dictionary<string, List<Result>>` mot → liste de versets
- [ ] **Recherche instantanée** — requête sur l'index inversé, résultats en temps réel (pas de boucle LINQ sur tout le texte)
- [ ] **Recherche avancée** — filtre par testament, par livre, recherche exacte, mots multiples (ET/OU)
- [ ] **Normalisation des mots** — lower case, suppression accents, stemming basique pour le français
- [ ] **Recherche floue** — tolérance aux fautes (distance de Levenshtein) : "Genes" → Genèse, "Matthieuu" → Matthieu
- [ ] **Partage** — copier un verset avec référence, partager via Web Share API
- [ ] **Plan de lecture** — lecture de la Bible en 1 an (lectures quotidiennes)
- [ ] **Concordance** — liste des occurrences d'un mot avec référence (alimentée par l'index inversé)
- [ ] **Navigation par référence** — taper "Genèse 1:1", "Jean 3:16", "Ps 23" pour y aller directement
- [ ] **Recherche par abréviations** — "Gn 1:1", "Jn 3:16", "1 Co 13", "Mt 5" reconnues automatiquement
- [ ] **Copie avec format propre** — copier un verset avec référence + version (ex: *Jean 3:16 (LSG) — Car Dieu a tant aimé...*)

## Phase 5 — Déploiement & Polissage

- [ ] **Audit Lighthouse** — performance, accessibilité, SEO
- [ ] **Favicon** + icônes PWA
- [ ] **Manifeste PWA** — nom, couleurs, écran splash
- [ ] **Stratégie de cache service worker par type de ressource :**
  - `data/index.json` → **cache-first** (liste des livres, petit, statique)
  - `data/books/*.json` → **cache-first** (livres, mis en cache dès la première visite)
  - `data/search-index.json` → **network-first** (peut être regénéré côté serveur)
  - `_framework/*.dll` → **cache-first** (assemblies Blazor, statiques)
  - Pages, CSS, JS → **stale-while-revalidate**
- [ ] **Préchargement intelligent** — après le chargement d'un livre, précharger en cache le livre suivant et le précédent
- [ ] **Cache des livres lus** — les livres déjà consultés sont en cache dans IndexedDB + service worker ; l'utilisateur peut les relire hors-ligne sans attente
- [ ] **Vidage sélectif du cache** — supprimer les vieux livres du cache si l'espace disque est limité (quota browser)
- [ ] **Deep linking** — routes comme `/lecture/genese/1/1` ou `/jean/3/16` pour partage direct
- [ ] **Déploiement** sur GitHub Pages / Azure Static Web Apps / Netlify
- [ ] **Page 404** personnalisée
- [ ] **Meta tags** pour le partage (Open Graph)
- [ ] **Sitemap** (si déploiement avec pré-rendu)

---

## Fonctionnalités Bonus

Choisis celles que tu veux, je les ajouterai au planning :

### Audio & Multimédia
- [ ] **Bible audio intégrée** — lecture chapitre par chapitre avec fichiers audio `.mp3`
- [ ] **Synchro texte- audio** — verset surligné pendant la lecture audio
- [ ] **Synthèse vocale (TTS)** — lire un verset avec Web Speech API (JS interop)

### Étude biblique
- [ ] **Dictionnaire biblique** — noms, lieux, thèmes expliqués
- [ ] **Références croisées** — cliquer sur un verset → versets liés
- [ ] **Strong (numéros grec/hébreu)** — concordance avec les mots originaux
- [ ] **Comparaison de traductions** — 2 versions côte à côte (si on ajoute une autre trad.)
- [ ] **Cartes géographiques** — lieux mentionnés dans un chapitre (via Leaflet/Mapbox)

### Social & Communauté
- [ ] **Génération d'image de verset** — carte de verset personnalisée (canvas HTML)
- [ ] **Partage d'image** — télécharger l'image générée
- [ ] **Quiz biblique** — questions chronométrées, score, classement localStorage
- [ ] **Défis de lecture** — lire X chapitres par jour, streak (série de jours consécutifs)

### Productivité & Personnalisation
- [ ] **Multi-langues UI** — français/anglais (fichiers `.resx`)
- [ ] **Polices personnalisables** — serif, sans-serif, manuscrite, dyslexie
- [ ] **Thèmes de couleur** — préréglages (sombre, sépia, vert doux, bleu nuit)
- [ ] **Mode veilleuse** — luminosité réduite, filtre bleu pour la nuit
- [ ] **Raccourcis clavier** — `← →` chapitres, `Ctrl+F` recherche, `Esc` fermer
- [ ] **Plein écran** — mode lecture immersive (fullscreen API)

### Statistiques & Gamification
- [ ] **Statistiques de lecture** — temps total, chapitres lus, livres terminés, mots lus
- [ ] **Calendrier de lecture** — contribution heatmap (comme GitHub) par jour
- [ ] **Badges / succès** — "Premier livre fini", "10 jours de suite", "Nouveau Testament complet"
- [ ] **Classement local** — comparaison avec soi-même (pas de multi-utilisateur)

### Techniques
- [ ] **Recherche par expression exacte** — guillemets "au commencement"
- [ ] **Recherche floue** — tolérance aux fautes d'orthographe
- [ ] **Export PDF** — chapitre ou livre entier en PDF (via jsPDF en JS interop)
- [ ] **Impression** — mise en page adaptée à l'impression (`@media print`)
- [ ] **Widget verset du jour** — embed HTML pour d'autres sites
- [ ] **API REST légère** — Minimal API ASP.NET pour servir les versets (optionnel)
- [ ] **Synchronisation cloud** — sauvegarder notes/surlignages via un compte (Azure Tables / SQLite)

---

## Architecture Blazor WASM — Structure de projet

```
BibleApp/
├── Program.cs
├── BibleApp.csproj
├── Tools/
│   └── SplitBibleJson.cs          (script CLI pour découper BibleLSFull.json en 66 fichiers)
├── wwwroot/
│   ├── index.html
│   ├── manifest.json
│   ├── service-worker.js
│   ├── service-worker.published.js
│   ├── data/
│   │   ├── index.json             (métadonnées des 66 livres — chargement immédiat)
│   │   └── books/
│   │       ├── genese.json
│   │       ├── exode.json
│   │       ├── psaumes.json
│   │       ├── matthieu.json
│   │       └── ... (66 fichiers, 1 par livre)
│   └── css/ (MudBlazor)
├── Models/
│   ├── Bible.cs
│   ├── Testament.cs
│   ├── Livre.cs
│   ├── Chapitre.cs
│   └── Verset.cs
├── Services/
│   ├── BibleService.cs            (chargement lazy des livres, cache mémoire LRU)
│   ├── SearchIndexService.cs      (construction + requête de l'index inversé)
│   ├── IndexedDbService.cs        (JS interop pour IndexedDB)
│   └── ProgressService.cs         (progression de lecture)
├── Pages/
│   ├── Index.razor                (accueil — liste des livres depuis index.json)
│   ├── Livre.razor                (sélection du chapitre)
│   ├── Lecture.razor              (lecture d'un chapitre avec lazy loading)
│   ├── Recherche.razor            (recherche par index inversé)
│   ├── Favoris.razor              (marque-pages, notes — IndexedDB)
│   ├── Progression.razor          (statistiques)
│   └── Parametres.razor           (thème, police, etc.)
├── Shared/
│   ├── MainLayout.razor
│   ├── NavMenu.razor
│   └── VersetCard.razor           (affichage d'un verset)
└── wwwroot/js/
    └── app.js                     (interop IndexedDB, Web Share, TTS, etc.)
```

---

*Dernière mise à jour : 11/05/2026*
