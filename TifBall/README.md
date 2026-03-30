# TifBall MonoGame

Ce dossier contient désormais les sources MonoGame du projet principal `TifBall.csproj`.

Etat actuel :
- fenêtre MonoGame opérationnelle
- gameplay de breakout déjà partiellement reporté
- chargement des textures gameplay depuis des ressources embarquées
- chargement de `Level.txt` depuis le dossier de sortie, comme dans le legacy
- effets sonores gameplay branchés via `SoundEffect`
- musique de fond lue depuis `music.ogg` embarqué, décodé via `NVorbis` puis joué en mémoire
- gestion plus stable du cycle de partie : reset propre des bonus/armes, perte de vie, game over et redémarrage
- reprise de l'intro legacy avec séquence de présentation animée, flash `TifBall`, balle rebondissante et transition vers les highscores

Notes audio :
- la musique de fond MonoGame est embarquée dans la DLL puis décodée en mémoire à l'exécution
- les diagnostics de chargement/lecture sont écrits dans `monogame-audio.log` à côté de l'exécutable

Packaging :
- les assets graphiques et sonores sont maintenant embarqués dans `TifBall.dll`
- `Level.txt` reste volontairement externe et copié à côté de l'exécutable, comme dans le legacy
- le dossier de sortie n'a plus besoin d'un dossier `Assets` visible pour fonctionner

Test manuel rapide :
- créer `power-up-test-settings.local.json` à côté de l'exécutable ou partir de `power-up-test-settings.sample.json`
- options de reprise rapide disponibles : `startLevel`, `skipPresentation`, `skipInitialHighScores`, `autoStartBall`
- ce fichier permet aussi de forcer les taux de spawn et les poids de bonus pour accélérer la comparaison MonoGame / legacy

Commande de build :

```powershell
dotnet build .\TifBall.csproj
```
