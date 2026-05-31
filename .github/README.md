# Per Aspera Modding — GitHub Copilot Toolkit

This folder contains the complete **GitHub Copilot customization kit** for Per Aspera modding.

## What's included

### 🤖 Agents (`agents/`)

22 specialized agents — VS Code automatically loads them based on your request:

| Agent | Use for |
|---|---|
| `@per-aspera-onboarding` | First steps, environment setup, routing guide |
| `@per-aspera-debugging` | BepInEx log analysis, error diagnosis |
| `@per-aspera-sdk-coordinator` | All SDK tasks (GameAPI, Events, Overrides, Wrappers, Commands) |
| `@per-aspera-bepinex` | BepInEx plugin development |
| `@per-aspera-bepinx-core` | Advanced HarmonyX patches |
| `@per-aspera-sdk-ui` | Unity IMGUI/GUI panels |
| `@per-aspera-yaml` | YAML datamodel modification |
| `@per-aspera-architecture` | System design & patterns |
| `@per-aspera-ci-cd` | GitHub Actions automation |

### 📚 Skills (`skills/`)

8 slash-command knowledge packages — type `/per-aspera-` in chat:

| `/skill` | Content |
|---|---|
| `/per-aspera-project-setup` | .csproj template, sdkDLL.props, GUID, deploy path |
| `/per-aspera-il2cpp-gotchas` | 10 IL2CPP pitfalls with exact code fixes |
| `/per-aspera-sdk-quickref` | SDK access patterns, events, minimal plugin template |
| `/per-aspera-debug-workflow` | Log locations, error anatomy, debug cycle |
| `/per-aspera-events-sdk` | EnhancedEventBus, GameEvents, subscription patterns |
| `/per-aspera-commands-sdk` | IGameCommand, CommandResult, builder pattern |
| `/per-aspera-wrappers-sdk` | WrapperFactory, Atmosphere/Planet/Building access |
| `/per-aspera-climate-sdk` | AtmosphereSimulator, temperature, water cycle |

---

## How to use (new modders)

### Step 1 — Clone and add as workspace folder

```bash
git clone https://github.com/PerAsperaMods/.github.git
```

In VS Code: **File → Add Folder to Workspace** → select the cloned folder.

That's it. Copilot will automatically load all agents and skills.

### Step 2 — Start with onboarding

In VS Code chat, type:
```
@per-aspera-onboarding I'm new, help me set up my first mod
```

### Step 3 — Use slash commands for quick reference

```
/per-aspera-project-setup  ← get the .csproj template
/per-aspera-il2cpp-gotchas ← if you hit an exception
/per-aspera-sdk-quickref   ← daily SDK lookup
```

---

## ⚠️ Path references

Skills reference documentation at `F:\ModPeraspera\` — the standard Per Aspera dev workspace path.
If you use a different path, update your workspace accordingly or treat the paths as relative references
to documentation in the [Organization Wiki](https://github.com/PerAsperaMods/.github/tree/main).
