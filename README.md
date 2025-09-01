# 🎮 Eternal Legion - Tactical Card Game (Unity)

Bienvenue dans le projet **Eternal Legion**, un jeu de cartes tactique en 3D inspiré de Krosmaga, hearthstone, etc . Ce README décrit la structure du projet et l’organisation des dossiers dans Unity.

---

## 📁 Structure du projet

```
/Eternal Legion
└── Assets/
    ├── Art/
    ├── Audio/
    ├── Materials/
    ├── Prefabs/
    ├── Scenes/
    ├── Scripts/
    ├── Resources/
    └── Plugins/
```

---

## 📂 Détail des dossiers

### 🎨 `Art/`

Contient tous les **éléments visuels (assets graphiques)** du jeu.

* `Art/Characters/` : Illustrations ou modèles 2D/3D des unités et héros.
* `Art/Cards/` : Visuels des cartes (illustrations, icônes, frames).
* `Art/Board/` : Visuels du plateau, cases, décors.
* `Art/UI/` : Éléments d'interface utilisateur (boutons, icônes, fonds).

---

### 🔊 `Audio/`

Contient les **effets sonores et musiques**.

* `Audio/SFX/` : Sons de sorts, attaques, interactions.
* `Audio/Music/` : Musiques de fond, thèmes de jeu.

---

### 🧱 `Materials/`

Regroupe les **matériaux Unity** appliqués aux objets 3D ou 2D (shader, textures...).

---

### 🧩 `Prefabs/`

Contient tous les **objets préfabriqués** utilisés en jeu.

* `Prefabs/Units/` : Préfabs d’unités invoquées sur le plateau. (3d)
* `Prefabs/Cards/` : Préfabs de cartes utilisables. (2d)
* `Prefabs/Demi-Plateaux/` : Préfabs de cases du plateau. demi plateaux a joindres entre eux
* `Prefabs/Decors` : Préfabs des decors et elements entourants le plateau

---

### 🗺️ `Scenes/`

Contient les **scènes Unity**, comme l’écran principal + menus ou la scène de jeu.

---

### 📜 `Scripts/`

Contient tout le **code source C#** du jeu.

* `Scripts/Core/` : Comportements centraux (GameManager, TurnManager, BoardManager).
* `Scripts/Cards/` : Logique des cartes (Card, CardEffect, DeckManager...).
* `Scripts/Units/` : Classes des unités, compétences et stats.
* `Scripts/UI/` : Gestion de l’interface utilisateur (HUD, affichage des cartes...).
* `Scripts/Utils/` : Fichiers d'aide et constantes globales.

---

### 📦 `Resources/`

Contient les **données de gameplay chargées dynamiquement** (via `Resources.Load`).

* `Resources/Cards/` : Fichiers `.json` décrivant les propriétés de chaque carte (coût, effet, stats...). -> db mongo ??
* `Resources/Units/` : Fichiers de données sur les unités (PV, attaques, compétences...).

📝 **Note :** Ce dossier est destiné à la configuration des éléments du jeu. Il est distinct du dossier `Art/` qui contient les visuels.

---

### 🧩 `Plugins/`

Espace pour inclure des **librairies tierces**, packages ou outils externes (animations, UI frameworks, etc.).

---

## ✅ Bonnes pratiques

* **Données dans `Resources/`**, **visuels dans `Art/`**
* Utiliser les `Prefabs` pour les objets réutilisables
* Organiser le code par fonctionnalité pour une meilleure maintenabilité
* Modulariser les effets de cartes et les compétences pour faciliter l’équilibrage

---

### 📦 Gérer les fichiers lourds avec Git LFS

Si vous avez des fichiers trop lourds (>100 Mo) ou si GitHub bloque votre `git push`, utilisez **Git LFS** :


1. **Installer Git LFS** (une seule fois - déjà fait) :

   ```bash
   git lfs install  #linux 
   ```

2. **Ajouter les types de fichiers à suivre** (exemples) :

   ```bash
   git lfs track "*.dll"
   git lfs track "*.dylib"
   git lfs track "*.so"
   ```

3. **Committer le fichier `.gitattributes` généré** :

   ```bash
   git add .gitattributes
   git commit -m "Track large files with Git LFS"
   ```

4. **Ajoutez et push normalement vos fichiers :**

   ```bash
   git add .
   git commit -m "Add large assets"
   git push
   ```

> ⚠️ Si le fichier a déjà été commité sans LFS, supprimez le commit avec `git reset --soft HEAD~1` et recommencez.

---

Voici un **guide pratique complet pour travailler à plusieurs sur Unity avec Git**, suivi d’un `.gitignore` optimisé pour Unity.

---

# 👥 Guide pratique – Travailler à plusieurs sur Unity avec Git

## 📦 Prérequis

* ✅ Avoir **Git installé**
* ✅ Avoir **Git LFS installé et initialisé** (`git lfs install`)
* ✅ Cloner le projet Unity dans un dossier vide
* ✅ Avoir un `.gitignore` adapté à Unity (voir plus bas)
* ✅ Utiliser Unity **avec le même versioning exact** (via Unity Hub)

---

## 🔁 Organisation du travail

### 🎯 Règles de base

| À faire                                             | Pourquoi                             |
| --------------------------------------------------- | ------------------------------------ |
| Une branche par feature / personne                  | Travail parallèle sans conflit       |
| Ne pas modifier la même scène/prefab à plusieurs    | Pour éviter des conflits binaires    |
| Faire des commits fréquents et clairs               | Facilite le suivi et les corrections |
| Tirer (`pull`) avant de pousser (`push`)            | Pour éviter des conflits             |
| Discuter avant de travailler sur les mêmes éléments | Coordination efficace                |

---

## 💡 Astuces Unity

* **Créez plusieurs scènes** : `MainMenu.unity`, `BattleScene_Laurick.unity`, etc.
* **Externalisez les données** : utilisez des `ScriptableObject` pour éviter les conflits dans les scènes.
* **Modifiez les Prefabs dans des scènes séparées**, puis testez dans la scène principale.

---

## ⚠️ Fichiers à éviter dans Git

Ne versionnez **jamais** les dossiers suivants :

* `Library/` : données temporaires locales
* `Temp/` : fichiers de build temporaires
* `Logs/` : journaux d'exécution
* `Build/` : les versions compilées du jeu
* `UserSettings/` : préférences utilisateur spécifiques à la machine

---

## 🧾 Exemple de `.gitignore` pour Unity

```gitignore
# Unity
[Ll]ibrary/
[Tt]emp/
[Oo]bj/
[Bb]uild/
[Bb]uilds/
[Ll]ogs/
[Mm]emoryCaptures/
UserSettings/
.vscode/
*.csproj
*.unityproj
*.sln
*.suo
*.tmp
*.user
*.userprefs
*.pidb
*.booproj
*.svd
*.pdb
*.mdb
*.opendb
*.VC.db
.DS_Store
*.apk
*.unitypackage
.idea/

```

---

## ✅ Recommandations finales

* Activez **Git LFS** pour tous les fichiers binaires Unity :

  ```bash
  git lfs track "*.png" "*.fbx" "*.dll" "*.dylib" "*.so" "*.wav" "*.mp4"
  ```
* Sauvegardez régulièrement votre scène pendant le développement.
* Mettez en place un **README.md avec les règles de contribution**.

---

Souhaite-tu que je te génère ce fichier `.gitignore` prêt à l’emploi et l’ajoute à un README amélioré ?
oici un **guide pratique complet pour travailler à plusieurs sur Unity avec Git**, suivi d’un `.gitignore` optimisé pour Unity.

---