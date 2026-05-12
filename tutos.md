# Déploiement sur Netlify — Bible Louis Segond

## 1. Créer un dépôt GitHub

1. Va sur [github.com/new](https://github.com/new)
2. Nomme le dépôt (ex: `bible-lsg`)
3. Exécute :

```bash
cd BibleApp
git remote add origin https://github.com/ton-compte/bible-lsg.git
git branch -M main
git push -u origin main
```

## 2. Configurer Netlify

### Via GitHub (recommandé)

1. Va sur [netlify.com](https://netlify.com) et connecte-toi avec GitHub
2. Clique **"Add new site" → "Import an existing project"**
3. Choisis ton dépôt `bible-lsg`
4. Configure :

| Champ | Valeur |
|-------|--------|
| **Build command** | `dotnet publish -c Release -o release` |
| **Publish directory** | `release/wwwroot` |

5. Clique **"Deploy site"**

### Via upload manuel

```bash
dotnet publish -c Release -o release
```
Puis glisse-dépose `release/wwwroot` sur Netlify.

## 3. Domaines (optionnel)

Dans Netlify : **Site settings → Domain management**

## Structure du projet

```
BibleApp/
├── .gitignore
├── netlify.toml
├── BibleApp.csproj
├── Program.cs
├── Models/
├── Services/
├── Pages/
├── Shared/
├── Layout/
└── wwwroot/
    ├── data/
    │   ├── index.json
    │   └── books/          ← 66 fichiers JSON (un par livre)
    ├── index.html
    ├── css/app.css
    ├── js/app.js
    └── service-worker.js
```
