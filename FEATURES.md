# Bibeli — Feuille de route fonctionnalités

Site : https://bibeli.vercel.app — PWA Blazor WebAssembly .NET 10 (MudBlazor 9).

## ✅ Implémenté (v2.5)

- **Crash au chargement corrigé** : service worker avec `skipWaiting` + `clients.claim`, cache-on-read, auto-récupération (bouton « Réessayer » qui vide le cache), loader honnête (20 s max).
- **Optimisation max** : `Cache-Control: immutable` sur `_framework/*`, pré-chargement limité, verset du jour pré-calculé (`verset-du-jour.json`), quiz plafonné, format compact des données.
- **SEO max** : Open Graph, Twitter cards, JSON-LD (WebSite + SearchAction, WebApplication), sitemap (77 URLs), robots.txt, manifest PWA, canonical, theme-color, hreflang.
- **Migration .NET 10** + MudBlazor 9.5.
- **Redesign UI/UX** : nouvelle palette indigo/or, typographie Crimson Pro pour les versets, mobile bottom-nav, accessibilité (focus-visible, 44 px min), `prefers-reduced-motion`.
- **Responsive mobile max** : bottom navigation, drawer plein écran, touch targets, safe-area.
- **Mémorisation (flashcards)** SM-2 + révision + suppression.
- **Statistiques** de lecture (chapitres, livres, AT/NT %, top livres, plan 1 an, streak).
- **Concordance thématique** (18 thèmes, versets clés).
- **Mode enfant** (grandes cartes, verset adapté).
- **Icônes réelles** (Material Icons) à la place des emojis.
- **Correction de données** : 4 livres à chapitre unique avaient le nom du livre comme numéro de chapitre (Abdias, Jude, 2 Jean, 3 Jean) → navigation cassée.

## 🚧 En cours / prochaines vagues

- Format compact des JSON (-23 %) + découpage par chapitre (-90 % par navigation).
- Index de recherche pré-calculé au build.
- Retrait de MudBlazor → HTML/CSS natif (-384 KB).
- `HttpClient` → interop `fetch` (-100 KB).
- Coquille HTML statique pour LCP instantané.
- Synchronisation cloud (Firebase) des favoris/notes/mémorisation.
- Comparaison de traductions (LSG + Darby + Segond 21).
- Audio TTS amélioré (vitesse, surlignage du verset en cours, téléchargement offline).
- Plan de lecture personnalisable.
- Mode présentation / prédication.
- Recherche sémantique.
- Export/import des données utilisateur (JSON).
- Accessibilité avancée (lecteur d'écran, contraste).

## Principes UX appliqués
Mobile-first, touch targets ≥ 44 px, focus-visible, prefers-reduced-motion, safe-area insets, contrast WCAG AA, hiérarchie visuelle claire, feedback immédiat, états de chargement honnêtes.