# Repository Guidelines

## Project Structure & Module Organization

This is a Godot 4.6 C# template project targeting `net10.0` through `Godot.NET.Sdk/4.6.2`. Main project files live at the repository root: `project.godot`, `GFramework-Godot-Template.sln`, and `GFramework-Godot-Template.csproj`.

- `scripts/` contains C# gameplay, framework integration, CQRS handlers, UI controllers, settings, data, and utilities.
- `global/` contains autoload scenes and scripts such as `GameEntryPoint`, `SceneRoot`, `UiRoot`, and audio/input managers.
- `scenes/` contains Godot `.tscn` scenes, including `scenes/tests/` for manual test/demo scenes.
- `assets/`, `resource/`, `config/`, and `schemas/` store art, fonts, shaders, themes, localization/config YAML, and JSON schemas.
- `script_templates/` contains Godot C# script templates and is treated as generated-style code.

## Build, Test, and Development Commands

- `dotnet restore` restores NuGet packages.
- `dotnet tool restore` restores local .NET tools used by CI.
- `dotnet build --no-restore` builds the C# project after restore.
- `dotnet build` is the normal local build command when dependencies may have changed.
- Open `project.godot` in Godot 4.6+ to run the main scene and inspect scene/resource wiring.

CI runs restore, tool restore, and build on pushes and pull requests to `main` or `master`, plus a TruffleHog secret scan.

## Coding Style & Naming Conventions

Use UTF-8 files. C# nullable reference types are enabled and the project uses preview language features. Prefer existing namespaces under `GFrameworkGodotTemplate.scripts.*` and keep new files close to the feature they implement.

Follow the repository naming rules: variables use lower camel case, constants use `UPPER_SNAKE_CASE`, and folders/files use lowercase snake_case for Godot assets and scenes. C# types remain PascalCase, matching existing files such as `SceneRouter.cs` and `OpenPauseMenuCommandHandler.cs`.

## Testing Guidelines

There is no dedicated unit-test framework configured. Validate changes with `dotnet build` and, for scene/UI/gameplay behavior, run the relevant Godot scene manually. Place manual test scenes in `scenes/tests/` and companion scripts in `scripts/tests/` when needed.

## Commit & Pull Request Guidelines

Recent history mostly follows Conventional Commits, for example `feat(ui): ...`, `refactor(core): ...`, and `chore(deps): ...`. Use a concise scope when helpful and keep messages imperative or descriptive.

Pull requests should include a short summary, validation steps (`dotnet build`, Godot scene tested), linked issues when applicable, and screenshots or clips for UI/visual changes. Do not commit `.godot/`, IDE metadata, generated build output, or local secrets.
