# Codex Memo

## Workspace actuel

- le projet actif est `C:\codex\TifBall\TifBall.v4.0`
- `C:\codex\TifBall` reste la racine de travail
- le fichier de suivi courant est `C:\codex\TifBall\TifBall.v4.0\codex-memo.md`

## Etat de départ v4.0

- `TifBall.v4.0` est une copie de travail créée après la finalisation de la migration MonoGame
- la migration vers `MonoGame` est considérée comme terminée côté `v3.0`
- cette base `v4.0` repart d'un dépôt Git neuf, sans l'historique de migration précédent
- la branche initiale du nouveau dépôt est `main`

## Etat technique confirmé

- le projet principal cible `net8.0`
- le build principal passe avec `dotnet build .\TifBall.csproj -c Debug`
- le build principal passe aussi avec `dotnet build .\TifBall.csproj -c Release`
- l'exécutable principal est généré dans `bin\Debug\net8.0\TifBall.exe` et `bin\Release\net8.0\TifBall.exe`
- `Scores.dat` est créé à côté de l'exécutable au runtime
- `Level.txt` reste copié à côté de l'exécutable
- les assets graphiques et sonores sont embarqués dans `TifBall.dll`
- le fichier local `power-up-test-settings.local.json` peut être utilisé en debug pour accélérer certains tests manuels
- le build `Release` ne doit pas embarquer ce fichier local de test dans son dossier de sortie final

## Structure utile

- le projet principal est [TifBall.csproj](C:\codex\TifBall\TifBall.v4.0\TifBall.csproj)
- le code source MonoGame est dans [TifBall](C:\codex\TifBall\TifBall.v4.0\TifBall)
- les assets source sont dans [Assets](C:\codex\TifBall\TifBall.v4.0\Assets)
- les sorties de build actives sont dans [bin](C:\codex\TifBall\TifBall.v4.0\bin) et [obj](C:\codex\TifBall\TifBall.v4.0\obj)
- les anciens dossiers obsolètes `TifBall\bin` et `TifBall\obj` ont été supprimés

## Dépôt Git v4.0

- dépôt Git neuf initialisé dans `C:\codex\TifBall\TifBall.v4.0`
- branche active : `main`
- commit racine actuel :
  - `d5cf03e` `Initial state: TifBall v4.0`
- remote configuré :
  - `origin` -> `https://github.com/Tifred25/TifBall.v.4.0.git`
- un fichier `AGENTS.md` à la racine formalise désormais la façon de travailler sur ce dépôt

## Points de vigilance

- le fichier local `TifBall\power-up-test-settings.local.json` n'est pas versionné mais peut être copié dans un dossier de sortie si on n'y prend pas garde
- le build `Release` doit être vérifié avant publication pour s'assurer que ce fichier local de test n'est pas présent dans `bin\Release\net8.0`
- le projet a hérité d'une base de code stabilisée en fin de migration; les futures évolutions doivent éviter de réintroduire des reliquats de migration ou de legacy inutile
- si une nouvelle publication majeure est préparée, penser à garder la documentation alignée sur `v4.0` et non plus sur `v3.0`
- `codex-memo.md` doit être mis à jour à chaque commit, conformément à `AGENTS.md`

## Prochaine étape recommandée

1. définir clairement l'objectif produit de `v4.0`
2. nettoyer la documentation restante qui serait encore trop liée à la phase de migration
3. avancer ensuite par commits fonctionnels courts à partir de cette base propre
