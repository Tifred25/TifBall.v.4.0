# AGENTS

## Objet

Ce fichier décrit la façon de travailler entre le propriétaire du projet et Codex sur ce dépôt.
Il doit rester précis, concret et fidèle au fonctionnement réel du projet.

## Périmètre

- ces règles s'appliquent au dépôt `C:\codex\TifBall\TifBall.v4.0`
- les actions automatiques de Codex doivent rester limitées au dossier de travail du projet
- rien ne doit être supprimé, déplacé ou renommé en dehors de ce dépôt sans demande explicite

## Langue

- la langue de travail par défaut est le français
- les messages de suivi, les explications, les résumés et les documents de collaboration doivent être rédigés en français, sauf besoin spécifique contraire
- le code source, les identifiants techniques, les noms de types, de méthodes, de variables, de fichiers techniques et les messages destinés au développement doivent rester en anglais, sauf contrainte particulière déjà présente dans le projet

## Git

- Codex peut créer des commits automatiquement quand le travail demandé est terminé et vérifié
- Codex peut pousser automatiquement sur le remote configuré sans redemander une validation séparée
- les messages de commit doivent rester courts, explicites et cohérents avec la modification réellement livrée
- les actions Git doivent rester non destructives sauf demande explicite du propriétaire du projet
- en cas de doute sur une opération risquée ou irréversible, Codex doit s'arrêter et demander confirmation

## Suppressions et renommages

- Codex peut supprimer, déplacer ou renommer des fichiers si cela est utile au travail en cours
- cette autorisation est limitée au dossier de travail du projet
- si une suppression ou un renommage présente un risque fonctionnel ou une ambiguïté forte, Codex doit demander confirmation avant d'agir

## Codex Memo

- `codex-memo.md` fait partie du workflow normal du dépôt
- Codex doit mettre à jour `codex-memo.md` à chaque commit
- le mémo doit refléter l'état réel du projet, les décisions récentes, les points de vigilance et la prochaine étape utile
- le mémo ne doit pas dériver en journal verbeux inutile; il doit rester exploitable rapidement

## Attendu de travail

- Codex doit privilégier les modifications concrètes et terminées plutôt que les réponses purement théoriques
- après une modification de code, Codex doit vérifier le résultat autant que possible, au minimum par build ou contrôle adapté au changement
- pour les changements importants, Codex doit laisser le dépôt dans un état propre, compréhensible et publiable
- les correctifs doivent viser une portée minimale et sûre avant toute refactorisation plus large

## Documentation de collaboration

- ce fichier doit être versionné et évoluer avec le projet
- si la façon de travailler change, `AGENTS.md` doit être mis à jour explicitement
- sur un nouveau projet, ce fichier peut servir de base, mais il doit être réadapté au contexte réel du dépôt concerné
