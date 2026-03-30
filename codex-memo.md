# Codex Memo

## Workspace actuel

- le projet actif est `C:\codex\TifBall\TifBall.v3.0`
- `C:\codex\TifBall` ne sert plus que de racine de travail
- le fichier de suivi courant est `C:\codex\TifBall\TifBall.v3.0\codex-memo.md`

## Etat technique confirmé

- la cible active du projet principal est désormais `net8.0`
- le projet principal build avec `dotnet build .\TifBall.csproj -c Debug`
- le lancement principal attendu est désormais `bin\Debug\net8.0\TifBall.exe`
- `Scores.dat` est maintenant créé à côté de l'exécutable
- `TifBall.settings.xml` et `Tifball.ini` relèvent de l'ancien runtime legacy et ne font plus partie du chemin principal
- le code MonoGame reste rangé dans le dossier `TifBall`, mais ce dossier ne porte plus de sous-projet séparé
- l'exécutable MonoGame courant est généré à la racine du projet dans `bin\Debug\net8.0\TifBall.exe`
- des tags de sauvegarde existent : `tifball-monogame-milestone-1` et `tifball-monogame-milestone-2`

## Correctifs déjà appliqués

- correction du bug d'initialisation de la position Y de fenêtre dans `Jeu`
- garde-fous ajoutés autour de l'initialisation audio / musique
- remplacement de `MainMenu` / `MenuItem` par `MenuStrip` / `ToolStripMenuItem`
- remplacement du système INI legacy par `GameSettings` avec persistance XML
- correction du chemin des highscores pour utiliser le dossier de l'exécutable au lieu du répertoire de lancement
- centralisation des chemins runtime dans `AppPaths` pour `Level.txt`, `Scores.dat`, `TifBall.settings.xml` et `startup-error.log`
- centralisation d'une première partie du chargement des ressources embarquées dans `EmbeddedResourceLoader`
- extraction d'un `GameSessionCoordinator` pour isoler une partie de l'initialisation audio et de la persistance des options hors de `FormTifBall`
- extraction d'un `RenderLoopCoordinator` pour isoler la logique de framerate capturé et de mode d'affichage hors de `FormTifBall`
- extraction d'un `GraphicsCoordinator` pour isoler l'initialisation du device DirectX, le reset et le rendu de base hors de `FormTifBall`
- extraction d'un `GameUiController` pour regrouper les interactions UI répétitives entre `FormTifBall` et `Jeu`
- factorisation des commandes de menu restantes de `FormTifBall` dans des helpers internes plus compacts
- extraction d'un `GameAudioManager` pour sortir de `Jeu` le chargement des sons, la musique de fond et les options audio
- extraction d'un `GameFlowState` pour sortir de `Jeu` l'état de flux principal (`mode`, pause, partie en cours, transition, partie perdue)
- extraction d'un `GameInputRouter` pour sortir de `Jeu` le routage des entrées souris/clavier selon le mode actif
- extraction d'un `GameSimulationCoordinator` pour sortir de `Jeu` l'orchestration de boucle, le framerate effectif et le coefficient de simulation
- extraction d'un `GameLifecycleCoordinator` pour sortir de `Jeu` le cycle de vie partie/niveau (vies, relance, progression de level)
- extraction d'un `GameCollisionCoordinator` pour sortir de `Jeu` l'orchestration des collisions balle/briques/tirs/bonus
- extraction d'un `GameBonusEffectsCoordinator` pour sortir de `Jeu` l'application des effets de bonus
- extraction d'un `GameRenderingCoordinator` pour sortir de `Jeu` l'orchestration du dessin de scène et des textes
- extraction d'un `GameGraphicsAssetsCoordinator` pour sortir de `Jeu` le chargement des textures, typos et fonds
- introduction d'un `GameSceneState` pour regrouper les objets et collections actifs de la scène derrière `Jeu`
- début de la phase audio MonoGame avec extraction d'un backend audio via `IGameAudioBackend` et `DirectXGameAudioBackend`
- début de la préparation graphique MonoGame avec extraction d'une interface `IGraphicsCoordinator` et réduction de l'empreinte DirectX dans `FormTifBall`
- création d'un projet parallèle `TifBall.MonoGameHost` en `net8.0` avec une fenêtre MonoGame minimale qui build et se lance
- premier rendu effectif côté `TifBall.MonoGameHost` avec chargement de `Raquette.png` et `Balle2.png`
- ajout d'un premier comportement gameplay côté `TifBall.MonoGameHost` : balle collée, lancement au clic, rebonds murs/raquette
- ajout d'une première grille de briques et d'une collision balle/brique basique dans `TifBall.MonoGameHost`
- remplacement de la grille hardcodée de `TifBall.MonoGameHost` par le chargement réel de `Level.txt` avec textures de briques par type
- ajout dans `TifBall.MonoGameHost` d'un premier cycle de niveau avec score, vies, briques multi-coups et passage au level suivant
- ajout d'une première vague de bonus jouables dans `TifBall.MonoGameHost` avec chute, capture par la raquette et effets simples (raquette +/-, vitesse balle +/-, vie +)
- ajout d'un deuxième lot de bonus dans `TifBall.MonoGameHost` avec support de plusieurs balles, `colle`, `3 balles` et `balle invincible`
- couverture du reste des bonus dans `TifBall.MonoGameHost` : taille de balle +/-, mitraillette, barre d'invincibilité, mode `hic` et bonus fruits / points
- ajout d'un gestionnaire audio dans `TifBall.MonoGameHost` pour les effets sonores gameplay (`Ping`, `Pong`, `Ploc`, `Hic`, `Laser`, `Recharge`, `Youhou`)
- diagnostic d'un asset legacy non compatible MonoGame : `Croque.wav` est un WAV compressé (format 85) non supporté par `SoundEffect.FromStream`, donc il n'est pas chargé dans l'hôte
- l'hôte MonoGame est revenu sur un chemin de lecture musique natif MonoGame (`Song` / `MediaPlayer`) et reste stable au lancement
- état actuel : la musique n'est pas encore effectivement lue dans cette configuration MonoGame/DesktopGL, même si l'exécutable reste vivant
- diagnostic confirmé au runtime : sur MonoGame/DesktopGL, `Song.FromUri` passe par `NVorbis` et échoue sur `musique.mp3` avec `Could not load the specified container`, ce qui confirme qu'un asset `musique.ogg` est requis pour cette voie
- `musique.ogg` a été ajoutée aux assets du projet et la lecture de fond est maintenant validée dans `TifBall.MonoGameHost`
- l'hôte MonoGame gère maintenant proprement la perte de vie, le game over avec redémarrage (`Entrée` ou clic gauche), le reset des bonus temporaires et la libération des textures au `Dispose`
- l'hôte MonoGame rejoue maintenant l'intro legacy au lancement avec `Fond2`, `TifBall`, les 42 frames de flash, `SabreLaser`, `Boing` et une transition vers les highscores
- l'hôte MonoGame couvre maintenant le flux joueur complet : intro, start, pause, game over, highscores legacy, saisie highscore legacy, HUD ingame et audio
- le HUD MonoGame utilise une police bitmap interne blanche, dans le cadre de jeu, avec `SCORE`, `LEVEL` et `VIES` calés comme le legacy
- l'écran des highscores MonoGame lit et écrit le même format `Scores.dat` legacy avec contrôle MD5
- le renderer MonoGame gère maintenant un mode plein écran centré (`F11` ou `Alt+Entrée`) avec cadre gris et fond extérieur gris foncé
- le rendu MonoGame est maintenant découpé par clipping à la zone de jeu, ce qui masque correctement bonus et balles hors cadre
- le chargement des niveaux MonoGame est maintenant tolérant aux lignes `-` du fichier `Level.txt`
- la physique de l'intro MonoGame a été réalignée avec le legacy
- les rebonds balle/brique MonoGame utilisent désormais une logique inspirée du legacy, par comptage des faces touchées, pour éviter les rebonds artificiels entre deux briques collées
- la colle MonoGame suit maintenant le point d'impact réel sur la raquette et relance ensuite la balle depuis cette position comme dans le legacy
- la barre d'invincibilité MonoGame renvoie maintenant correctement la balle sur son sommet et permet à une petite balle de rebondir entre la barre et le dessous de la raquette
- les tailles de balle et de raquette MonoGame ont retrouvé les paliers legacy : raquette `120 -> 240` / `120 -> 40` par pas de `20`, balle `20 -> 60` / `20 -> 10` par pas de `10`
- le bonus `Hic` MonoGame remet maintenant le curseur à la position logique correcte lors de l'entrée et de la sortie de l'inversion souris
- la colle et la mitraillette sont exclusives dans les deux sens
- les bonus qui libèrent la colle sont alignés sur le legacy : `Raquette +`, `Raquette -`, `3 Balles`, `Taille balle +`, `Taille balle -`, `Mitraillette`
- les briques indestructibles MonoGame disparaissent maintenant bien après `20` chocs, comme dans le legacy
- le chargement audio MonoGame est maintenant tolérant aux assets son incompatibles; `Croque.wav` n'empêche plus le lancement et retombe sur `Ploc` comme son de secours pour les bonus fruits
- la vitesse de balle MonoGame est maintenant réalignée sur le legacy: les bonus `Balle +` / `Balle -` persistent après rebond raquette, la relance après colle repart à la vitesse initiale, et une accélération discrète `+5%` est appliquée toutes les `5` briques touchées par balle
- la balle MonoGame démarre maintenant décalée sur la raquette comme le legacy, ce qui évite un premier tir vertical
- la saisie de highscore MonoGame a retrouvé la roue legacy `A-Z0-9` avec sprites, sons, clic souris et validation type flipper
- l'écran des meilleurs scores MonoGame utilise maintenant le fond legacy plein écran avec le formatage de colonnes et de couleurs du jeu original
- le HUD MonoGame a été poli sur plusieurs détails visuels, dont `CLIC POUR COMMENCER` et la ligne `SCORE / LEVEL / VIES`
- le titre de fenêtre MonoGame est simplifié à `TifBall v3.0 - <fps> fps`
- la transparence par couleur noire du legacy est maintenant réappliquée sur les sprites MonoGame pertinents
- le host MonoGame expose maintenant un vrai menu natif Windows dans la barre principale avec `Pause`, `Sons`, `Musique` et `Images de fond`
- la raquette MonoGame ne bouge plus pendant la pause, comme dans le legacy
- l'intro MonoGame a été réalignée sur le flux legacy réel : `TifBall.png` défile dans le cadre, la balle apparaît ensuite, et les anciennes `Presentationxx.jpg` ont été retirées du code et des assets
- plusieurs assets non utilisés ont été supprimés, dont `Presentation*.jpg`, `Balle.png`, `Tic.wav`, `BonusA.png`, `BonusT.png`, `BalleInvincible.png` et les sprites `Espace_*`
- l'overlay géant `LEVEL X` qui descend avant chaque niveau est restauré avec une durée raccourcie à `3` secondes
- le menu `? > A propos` du legacy est maintenant reporté dans l'hôte MonoGame via un panneau intégré affichant `Balle2.png`, `TifBall`, la version forcée `3.0.0` et le copyright
- le chargement des fonds de gameplay MonoGame est maintenant tolérant aux images absentes ou invalides; une `Image*.jpg` manquante n'empêche plus le host de démarrer et est simplement journalisée dans `monogame-host-error.log`
- l'écran de saisie highscore MonoGame a été réaligné sur les coordonnées legacy pour les flèches, les lettres précédente/courante/suivante et les boutons `EFF` / `OK`
- les assets MonoGame (`images`, `sons`, `musique.ogg`) sont maintenant embarqués dans `TifBall.dll` via `EmbeddedResource`, avec un chargeur central `EmbeddedAssetLoader`
- `Level.txt` a finalement été ressorti du package et reste copié à côté de l'exécutable MonoGame, comme dans le legacy
- la musique de fond MonoGame n'utilise plus `Song` / `MediaPlayer`; elle est maintenant décodée depuis l'OGG embarqué via `NVorbis` puis jouée en mémoire via `SoundEffectInstance`
- le dossier de sortie MonoGame n'a plus besoin d'exposer de dossier `Assets`; `Level.txt` reste visible à côté de l'exécutable, avec les DLL natives/managées et les fichiers locaux de runtime (`Scores.dat`, bonus local, logs)
- le projet root `TifBall.csproj` a été converti en projet MonoGame principal; l'ancien sous-projet MonoGame séparé et le build legacy `net48` ont été retirés du chemin principal
- le dossier source MonoGame a été renommé en `TifBall` et les namespaces/types ont été nettoyés pour supprimer le préfixe redondant `MonoGame`
- une passe complète de nettoyage du nommage legacy a été appliquée : identifiants source, assets graphiques, sons techniques et fichiers de configuration locaux sont maintenant en anglais
- l'IHM visible par le joueur reste en français; seuls les noms techniques internes ont été anglifiés
- les assets principaux ont notamment été normalisés vers `Ball.png`, `InvincibleBall.png`, `Paddle.png`, `Background2.jpg`, le dossier `Letters`, les sprites `PowerUp*` et la musique `music.ogg`
- le fichier local de test bonus a été renommé en `TifBall\power-up-test-settings.local.json`
- un mode de reprise rapide local a été ajouté via `power-up-test-settings.local.json` avec `startLevel`, `skipPresentation`, `skipInitialHighScores` et `autoStartBall` pour accélérer la comparaison manuelle MonoGame / legacy
- l'origine des tirs de mitraillette MonoGame a été réalignée sur la pointe visuelle des canons
- les anciens dossiers de build hérités de l'ex-sous-projet MonoGame (`TifBall\bin` et `TifBall\obj`) ont été supprimés; seuls `bin` et `obj` à la racine du projet restent utiles

## Etat du dépôt Git

- dépôt Git neuf initialisé dans `C:\codex\TifBall\TifBall.v3.0`
- remote configuré sur `https://github.com/Tifred25/TifBallRework.git`
- branche active : `main`
- tags de sauvegarde locaux : `tifball-monogame-milestone-1`, `tifball-monogame-milestone-2`
- derniers commits :
  - `424cac4` `Tweak: improve MonoGame manual test flow and shot alignment`
  - `7d169c9` `Refactor: rename legacy French source assets to English`
  - `55e5d4e` `Chore: normalize line endings`
  - `1f2b474` `Chore: stop tracking local bonus settings`
  - `9255de1` `Refactor: promote MonoGame as main project`
  - `14eb811` `Feat: embed MonoGame assets in assembly`
  - `c944c4b` `Fix: restore legacy about menu`
  - `4063c85` `Tweak: shorten level transition timing`
  - `c3e4e08` `Fix: restore legacy level transition overlay`
  - `4190ae9` `Chore: remove unused intro assets`
  - `609d950` `Fix: freeze paddle movement during pause`
  - `2750abe` `Fix: restore native MonoGame options menu`
  - `ecaafe1` `Fix: simplify MonoGame window title`
  - `da88419` `Fix: polish MonoGame HUD layout`
  - `f524587` `Fix: restore legacy MonoGame high scores`
  - `b68f1d1` `Fix: restore legacy MonoGame score entry`
  - `fade667` `Fix: offset initial MonoGame ball launch`
  - `4a5bf57` `Fix: align MonoGame ball speed with legacy`

## Ménage déjà fait

- suppression du dépôt Git copié depuis un autre emplacement
- création d'un `.gitignore` adapté au projet
- suppression des `Thumbs.db` versionnés
- suppression des sorties de build et caches obsolètes
- nettoyage des fichiers parasites à la racine de `C:\codex\TifBall`
- suppression des anciens dossiers `TifBall\bin` et `TifBall\obj` devenus obsolètes après la promotion de `TifBall.csproj` à la racine

## Points de vigilance

- le projet legacy `net48` dépend encore de `Microsoft.DirectX*` au runtime, même si les anciennes DLL DirectX ne sont plus versionnées à la racine du dépôt
- le host MonoGame est maintenant très avancé, mais il reste encore des écarts fins possibles de physique ou de rendu à détecter uniquement en test manuel
- le son legacy exact des bonus fruits n'est toujours pas restauré : `Croque.wav` reste incompatible avec `SoundEffect.FromStream`, seule une retombée de secours est maintenant en place
- la nouvelle accélération progressive des balles MonoGame est alignée sur la lecture du code legacy, mais mérite encore une validation manuelle en situation multiballe
- le fichier local `TifBall\power-up-test-settings.local.json` n'est pas versionné et peut influencer les fréquences de bonus pendant les tests
- le menu `A propos` MonoGame est proche du legacy mais reste un overlay intégré au rendu MonoGame, pas une vraie boîte de dialogue WinForms native

## Prochaine étape recommandée

1. reprendre par une nouvelle passe de comparaison fine MonoGame / legacy en jeu réel
2. cibler en priorité les derniers écarts visuels ou de comportement encore visibles à l'œil
3. ne toucher à l'architecture qu'après validation du comportement gameplay restant

## Décision de migration

- la cible retenue est désormais `MonoGame`
- la logique est de faire une migration incrémentale depuis la base refactorée actuelle, pas une réécriture complète
- `Godot` n'est pas retenu pour ce projet de migration, car le coût de refonte serait plus élevé
- l'audio est maintenant préparé pour recevoir un backend MonoGame sans dépendance DirectX dans `GameAudioManager`
- un hôte MonoGame minimal existe désormais en parallèle pour tester la migration réelle sans casser le projet legacy
- le projet est maintenant à un jalon MonoGame suffisamment stable pour être repris plus tard à partir du tag `tifball-monogame-milestone-2`
