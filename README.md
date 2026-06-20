# RAXY Quest System

RAXY Quest System provides a modular quest foundation for Unity projects: quest data, requirements, objectives, progress tracking, and world quest objects.

## Features

- **QuestSO / QuestDatabaseSO** — quest definitions with requirements, objective sets, and rewards
- **QuestManagerBase** — take quests, track progress, complete quests, and emit quest events
- **QuestObjectManagerBase** — spawn and manage world quest objects from quest data
- **ObjectiveProgressBase** — extensible objective progress contract for game-specific handlers
- **IQuestFactory** — factory hook for creating objective progress instances

## Setup

1. Create a concrete manager extending `QuestManagerBase` (e.g. `QuestManager`).
2. Create a concrete object manager extending `QuestObjectManagerBase` (e.g. `QuestObjectManager`).
3. Assign `QuestDatabaseSO` and a GameObject with `IQuestFactory` to the quest manager.
4. Implement game-specific `ObjectiveProgressBase` subclasses for kill, collect, talk, etc.
5. Call `InitAllQuest()` at bootstrap, then `TakeQuest(questId)` when starting quests.

## Dependencies

- **RAXY Inventory** (`com.raxy.inventory`) — quest rewards via `ItemAmountContainer`
- **RAXY Utility** (`com.raxy.utility`) — `CustomDebug`
- **RAXY Localization** (`com.raxy.utility.localization`) — localized quest and objective text
- **UniTask** (`com.cysharp.unitask`) — async localization cache refresh
- **Odin Inspector** (project plugin) — editor attributes; runtime works without Odin if attributes are stripped

## Notes

Game-specific objective handlers, quest requirements, UI, save/load, and bootstrap wiring should live in your project, not in this package.
