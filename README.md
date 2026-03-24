# VR Aim Trainer

Projet réalisé dans le cadre du cours de **Réalité augmentée et réalité virtuelle** — Master 2, Ynov Campus.

## Résumé

VR Aim Trainer est une application de réalité virtuelle développée avec Unity, destinée au Meta Quest 3. Le joueur évolue dans un stand de tir intérieur où il doit toucher un maximum de cibles en un temps limité. L'application propose plusieurs armes avec des comportements différents (semi-automatique, automatique, une ou deux mains) et des cibles fixes. L'objectif est d'entraîner sa précision et sa réactivité dans un environnement immersif.

## Informations techniques

| Élément        | Détail                  |
|----------------|-------------------------|
| Moteur         | Unity 6000.3.10f1 (6.3 LTS) |
| Plateforme     | Meta Quest 3            |
| SDK            | Meta XR / OVR           |
| Langage        | C#                      |

## Fonctionnalités

### Session de jeu

- Lancement d'une partie via le bouton [A] de la manette droite.
- Partie chronométrée (durée définie dans Unity).
- Écran d'accueil affichant le nom du jeu et les contrôles disponibles.
- Écran de fin de partie affichant le score final avec possibilité de rejouer.

### Système de scoring

- Compteur de points incrémentant de 1 à chaque cible touchée.
- Affichage en temps réel du score et du temps restant via un HUD placé en haut à gauche du champ de vision.
- Barre de progression du timer avec changement de couleur selon le temps restant (ambre, orange, rouge).

### Armes

- Menu de sélection d'arme en VR accessible via le bouton [Y] (manette gauche), uniquement hors partie.
- Interface sous forme de cartes flottantes suivant le regard du joueur, avec laser de pointage depuis la main droite.
- Sélection confirmée avec la gâchette droite.
- Plusieurs armes disponibles, chacune avec ses propres caractéristiques (cadence de tir, mode de tir, prise à une ou deux mains).
- Prise en main de l'arme avec le grip droit ; possibilité de poser l'arme.
- Prise à deux mains : la main gauche peut saisir la poignée avant de l'arme pour stabiliser la visée. L'arme s'oriente alors vers la position de la main gauche.

### Tir et projectiles

- Tir déclenché par la gâchette droite ; le projectile est instancié depuis l'extrémité du canon (muzzle point).
- Deux modes de tir : semi-automatique (un tir par pression) et automatique (tir continu tant que la gâchette est maintenue).
- Visée assistée avec la gâchette gauche (aim down sights) et lissage de la rotation de l'arme.
- Détection des impacts par raycast continu entre les positions successives du projectile, évitant le passage à travers les cibles à haute vitesse.
- Retour haptique (vibration de la manette droite) à chaque tir.
- Effet visuel de muzzle flash (lumière ponctuelle orange à l'extrémité du canon).
- Traînée visuelle derrière chaque projectile.

### Cibles

- Cible de base sous forme de cube rouge, apparaissant à une position aléatoire dans une zone définie.
- Respawn automatique d'une nouvelle cible à une position aléatoire après chaque impact.

### Environnement

- Salle de tir générée au lancement : sol, murs, plafond, lignes de tir orange au sol.
- Éclairage composé de barres lumineuses au plafond et de lumières latérales orange émissives.

### Audio

- Son de tir généré (simulation d'un crack supersonique, d'une composante basse et d'un bruit d'explosion).
- Son d'impact généré lors de la collision avec une cible.
- Audio spatialisé en 3D pour les sons de tir et d'impact.

## Contrôles

| Entrée                    | Action                              |
|---------------------------|-------------------------------------|
| [A] manette droite        | Lancer / Rejouer une partie         |
| [Y] manette gauche        | Ouvrir / Fermer le menu des armes   |
| Gâchette droite (index)   | Tirer                               |
| Grip droit                | Ramasser / Poser l'arme             |
| Gâchette gauche (index)   | Viser (aim down sights)             |
| Grip gauche               | Saisir la poignée avant (deux mains)|
| [B] manette droite        | Changer le mode de tir              |

## Répartition du travail

| Membre                          | Contributions                                                                                                  |
|---------------------------------|----------------------------------------------------------------------------------------------------------------|
| Kévin MARGUET                   | Système de scoring, HUD en jeu (timer, barre de progression, score), salle de tir, éclairage, son d'impact, audio spatialisé |
| Adélia FATHIPOURSASANSARA       | Mise en place des cibles et respawn automatique                  |
| Loan FRANÇOIS                   | Gestion de session de jeu, écran d'accueil et de fin de partie, menu de sélection d'arme, prise en main de l'arme, prise à deux mains, configuration des armes |
| Valentin VANHOVE                | Système de tir, modes de tir (semi-auto / auto), visée, détection de collision des projectiles, retour haptique, muzzle flash, traînée visuelle, son de tir procédural |
