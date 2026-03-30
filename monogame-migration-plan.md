# Plan de migration vers MonoGame

Date: 2026-03-24

Projet:
- `C:\codex\TifBall\TifBall.v3.0`

Objectif:
- sortir `TifBall` de `Microsoft.DirectX*`
- migrer vers une stack moderne basée sur `MonoGame`
- préserver autant que possible la logique métier déjà refactorée

## Principes

- faire une migration incrémentale
- éviter une réécriture globale
- garder des jalons lançables aussi souvent que possible
- isoler d'abord les dépendances techniques, pas la logique de jeu

## Etat de départ

La base actuelle est déjà mieux préparée qu'au début :

- `FormTifBall` est allégé
- `Jeu` est découpé en coordinateurs
- `GameSceneState` existe
- audio, rendu, collisions, boucle, cycle de vie ont été mieux isolés

Ce qui reste encore couplé à Managed DirectX :

- pipeline graphique dans `GraphicsCoordinator`
- son/musique dans `GameAudioManager`
- chargement de textures et rendu dans plusieurs classes de scène
- rendu texte dans `Jeu`, `HighScores`, `Presentation`, `SaisieScore`, `ScoreBonus`

## Cible technique

Choix retenu :
- `MonoGame`

Pourquoi :
- migration plus naturelle depuis l'architecture actuelle
- proche d'un modèle sprites/game loop en C#
- effort de transition plus faible que vers un moteur complet comme `Godot`

## Stratégie globale

### Phase 1. Préparer des abstractions internes minimales

But :
- réduire la dépendance directe du code métier à l'API graphique concrète

Travail :
- introduire une abstraction minimale de texture/sprite/font si nécessaire
- identifier ce qui doit être remplacé par :
  - texture 2D MonoGame
  - `SpriteBatch`
  - `SpriteFont`
  - `SoundEffect`
  - `Song` / `MediaPlayer`

Sortie attendue :
- une couche interne plus stable que l'API DirectX actuelle

### Phase 2. Sortir l'audio de Managed DirectX

But :
- enlever `Microsoft.DirectX.DirectSound` et `AudioVideoPlayback` en premier

Travail :
- remplacer les sons courts par `MonoGame SoundEffect`
- remplacer la musique de fond MP3 par l'équivalent MonoGame
- garder l'API publique actuelle de `GameAudioManager` autant que possible

Sortie attendue :
- plus aucune dépendance audio à `Microsoft.DirectX.*`

Pourquoi commencer par là :
- l'audio est déjà bien isolé
- risque plus faible que le rendu
- donne un premier gain concret sans casser l'affichage

### Phase 3. Introduire l'hébergement MonoGame

But :
- disposer d'une boucle MonoGame parallèle ou remplaçante

Travail possible :
- créer un projet ou un hôte MonoGame dédié
- décider si on garde temporairement WinForms comme shell ou non
- faire un spike minimal qui affiche une fenêtre MonoGame et rend un fond

Question à trancher :
- conserver un shell WinForms temporaire
- ou basculer plus vite vers un hôte MonoGame pur

Recommandation :
- spike minimal MonoGame d'abord
- décider ensuite si WinForms reste temporairement ou non

### Phase 4. Remplacer le pipeline de rendu

But :
- remplacer `GraphicsCoordinator` et l'initialisation Direct3D

Travail :
- remplacer `Device`, `PresentParameters`, `Sprite`, `TextureLoader`
- introduire `GraphicsDeviceManager`, `Texture2D`, `SpriteBatch`
- recâbler `GameRenderingCoordinator` et `GameGraphicsAssetsCoordinator`

Sortie attendue :
- scène principale affichée via MonoGame

### Phase 5. Migrer les objets de scène

But :
- faire disparaître les dépendances Direct3D dans les classes métier

Classes à migrer progressivement :
- `Balle`
- `BarreInvincibilite`
- `Bonus`
- `Brique`
- `Mitraillette`
- `Raquette`
- `TirMitraillette`
- puis `HighScores`, `Presentation`, `SaisieScore`, `ScoreBonus`

Travail :
- remplacer `Texture`
- remplacer `TextureLoader.FromStream`
- remplacer `Sprite.Draw2D`
- adapter le rendu texte

### Phase 6. Nettoyage final

But :
- supprimer Managed DirectX du projet

Travail :
- retirer les références `Microsoft.DirectX*` du `.csproj`
- retirer les DLL legacy du projet
- retester build et lancement
- marquer un jalon stable

## Ordre d'exécution recommandé

1. documenter les équivalences DirectX -> MonoGame
2. migrer l'audio
3. faire un spike d'hébergement/rendu MonoGame
4. migrer le pipeline de rendu principal
5. migrer les classes de scène
6. supprimer les dépendances Managed DirectX restantes

## Equivalences principales à viser

- `Device` -> `GraphicsDevice`
- `Sprite` -> `SpriteBatch`
- `Texture` -> `Texture2D`
- `TextureLoader.FromStream` -> chargement `Texture2D` depuis stream/contenu
- `Direct3D.Font` -> `SpriteFont`
- `SecondaryBuffer` -> `SoundEffect`
- `AudioVideoPlayback.Audio` -> `Song` + `MediaPlayer`

## Risques principaux

- le rendu texte demandera une vraie adaptation
- `Presentation`, `SaisieScore` et `HighScores` sont plus riches que le simple gameplay
- le cycle de chargement des textures devra être revu
- l'intégration exacte avec WinForms n'est pas forcément la bonne destination finale

## Jalon de départ

Point de départ recommandé pour la migration :
- tag Git `stable-refactor-base`
- ou plus récent si on veut partir après l'introduction de `GameSceneState`

## Prochaine tâche concrète

Tâche recommandée immédiatement :
- préparer la migration audio vers MonoGame en gardant l'API de `GameAudioManager`
