# DarkHome (Technical Prototype)

A narrative-driven, first-person psychological horror prototype combining Visual Novel dialogue mechanics, RPG-style quest progression, and dynamic Sanity management. 

> ** Disclaimer for Reviewers:** This repository serves strictly as a **Technical Demonstration**. It is designed to showcase system architecture, decoupled gameplay programming, custom tool development, and the practical application of design patterns in Unity. **There is no public playable build; please refer to the Gameplay Demo video below to see these systems in action.**

🔗 **Gameplay Demo:** [Watch on YouTube](https://youtu.be/2JUmByys2Rw)

---

## Architecture & Design Patterns

The codebase is engineered with scalability and decoupling in mind. Core design patterns utilized include:
* **Observer Pattern:** A centralized `EventManager` handles all cross-module communication, completely decoupling the UI, audio, and game logic.
* **Finite State Machine (FSM):** Heavily utilized for isolated, predictable behavior logic (AI, Player Sanity, Interaction States).
* **Singleton Pattern:** Strictly limited to persistent global managers (`GameManager`, `SaveLoadManager`, `SceneTransitionManager`).
* **Data-Driven Design:** Extensive use of `ScriptableObjects` (for Dialogue, Quests, Items, Flags) to separate data from logic.

---

## Core Modules & Systems

### 1. Flag System & Event Triggers
The backbone of narrative branching and world state management.
* Handles global, runtime, and previous-chapter flags.
* `EventTriggerManager` listens to flag changes to dynamically alter the environment, spawn NPCs, or update interactive objects.

### 2. Dialogue & Choice System (Visual Novel Style)
A fully custom, data-driven dialogue engine.
* Parses dialogue nodes and branching choices dynamically.
* Choices evaluate current `Flags` in real-time before displaying, allowing for deep cause-and-effect narrative trees.

### 3. Quest & Chapter Progression
* **Quest System:** Tracks sequential and parallel objectives. Completing quests updates the Flag system and triggers narrative events. *(Note: Continuously refactored for performance, phasing out legacy handlers).*
* **Chapter Manager:** Handles the macro-progression of the game, securely loading distinct story acts.

### 4. NPC & Enemy AI (Hierarchical FSM)
* **NPCs:** Utilize custom State Machines for idle, talking, moving, and reacting to player actions.
* **Enemies:** NavMesh-driven behaviors with states for patrol, line-of-sight scanning (`ScannerTarget`), chase, and rage, integrated with dynamic audio cues.

### 5. Interaction Framework (`BaseObject`)
An interface-based architecture (`IInteractable`, `IPressInteractable`, `IHoldInteractable`) using physics raycasting.
* **Scalability:** Every interactive object (doors, clues, puzzles) inherits from `BaseObject`, making it trivial to implement new mechanics without altering the core interaction raycaster.

### 6. Seamless Scene Transition
* Utilizes **Additive Scene Loading** via `SceneTransitionManager` to load/unload environments without destroying persistent core managers.
* Includes safe player teleportation, spawn point assignment, and fade-in/out visual logic operating on unscaled time.

### 7. Save/Load System (JSON Serialization)
* Implements an `IDataPersistence` interface.
* Serializes complex runtime data (Player Stats, Objects(Flags), Active Flags, Quest States) into local JSON files, ensuring stable data restoration across different scenes and sessions.

### 8. Custom Editor Tools
To streamline development and content creation, several custom editor tools were built:
* **CSV Importers:** Automated tools to convert `.csv` files into ScriptableObjects for Dialogues, Quests, Events, and Items.
* **Quest & Dialogue Editors:** Custom Unity Editor windows to visualize and manage data-driven assets efficiently.

---

## Technical Details
* **Engine:** Unity 6000.0.28f1
* **Language:** C#
* **Version Control:** Unity Version Control (Plastic SCM) was used for daily development. This GitHub repository acts as a consolidated snapshot.
