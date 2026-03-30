# TifBallRework

Migration MonoGame désormais retenue comme projet principal de TifBall.

## Etat actuel

- dépôt Git propre réinitialisé
- build principale sur `net8.0`
- exécutable principal généré en `TifBall.exe`
- `Scores.dat` est généré à côté de l'exécutable
- `Level.txt` reste volontairement externe à côté de l'exécutable, comme dans le legacy
- les assets graphiques et sonores sont embarqués dans la DLL MonoGame

## Build

```powershell
dotnet build .\TifBall.csproj -c Debug
```

## Exécution

```powershell
.\bin\Debug\net8.0\TifBall.exe
```

## Notes

- les sorties de build et les fichiers runtime locaux sont ignorés par Git
- `TifBall.csproj` est maintenant le projet principal MonoGame
- le dossier `TifBall` ne porte plus de sous-projet séparé; il sert uniquement de dossier source pour le code MonoGame
- le fichier de suivi du travail est `C:\codex\TifBall\TifBall.v3.0\codex-memo.md`
