# Twitch Faction Viewer System

## Overview

The Twitch Faction Viewer system allows Twitch viewers to participate in Per Aspera as faction leaders. Each viewer can:
- Create their own faction
- Team up with other viewers
- Make deals and trade resources
- Compete for points and rankings

## Architecture

### Core Components

1. **ViewerFaction** - Represents a Twitch viewer as a faction
   - Tracks resources, points, and team membership
   - Manages pending invitations and active deals

2. **ViewerTeam** - Alliance of multiple viewer factions
   - Shared resources and collaborative play
   - Team leader manages invitations

3. **ViewerDeal** - Agreements between two viewers
   - Resource exchanges
   - Time-limited proposals

4. **ViewerFactionManager** - Central management system
   - Manages all viewers, teams, and deals
   - Handles cleanup and statistics

5. **ViewerFactionCommands** - Twitch chat command processor
   - Processes all viewer commands
   - Sends responses via Twitch chat

6. **ViewerFactionIntegrationService** - Main integration service
   - Connects Twitch IRC to the faction system
   - Background cleanup and monitoring

## Available Commands

### Basic Commands

- `!join` - Join as a faction leader
  - Creates a new viewer faction
  - Grants starting resources
  - Example: `!join`

- `!status` - Show your faction status
  - Displays points, team, deals, and invitations
  - Example: `!status`

- `!help` - Show available commands
  - Lists all commands and their usage
  - Example: `!help`

### Team Commands

- `!team <username>` - Invite a viewer to your team
  - Creates a team if you don't have one
  - Sends invitation to target viewer
  - Example: `!team alice`

- `!accept <username>` - Accept a team invitation
  - Joins the sender's team
  - Can also accept deals
  - Example: `!accept bob`

- `!decline <username>` - Decline an invitation
  - Rejects team invitation or deal
  - Example: `!decline charlie`

- `!leave` - Leave your current team
  - Removes you from the team
  - Leader leaving disbands the team
  - Example: `!leave`

### Deal Commands

- `!deal <username> <terms>` - Propose a deal
  - Creates a time-limited deal proposal
  - Specify terms in natural language
  - Example: `!deal dave Trade 100 metal for 50 water`

### Information Commands

- `!alliances` or `!teams` - List all active teams
  - Shows team names and members
  - Displays team points
  - Example: `!alliances`

- `!factions` or `!viewers` - List active factions
  - Shows recently active viewers
  - Includes team membership
  - Example: `!factions`

- `!leaderboard` or `!top` - Show top factions
  - Ranks viewers by points
  - Shows top 10 factions
  - Example: `!leaderboard`

## Usage Example

### Basic Setup

```csharp
using PerAspera.SDK.TwitchIntegration;
using PerAspera.SDK.TwitchIntegration.Vendor.UnityTwitchChat;

// Configure Twitch connection
var config = new TwitchConnectionConfig
{
    OAuth = "oauth:your_oauth_token",
    Username = "your_bot_username",
    Channel = "your_channel_name"
};

// Create and start the service
var service = new ViewerFactionIntegrationService(config);
await service.StartAsync();

// Service is now running and processing commands
Console.WriteLine($"Status: {service.GetStatistics()}");
```

### Offline/Testing Mode

```csharp
// Create service without Twitch connection for testing
var service = new ViewerFactionIntegrationService(null);

// Manually interact with the faction manager
var manager = service.FactionManager;

// Create test viewers
var alice = manager.GetOrCreateViewer("alice", "Alice");
var bob = manager.GetOrCreateViewer("bob", "Bob");

// Create a team
var invitation = manager.SendTeamInvitation(alice, bob);
manager.AcceptTeamInvitation(bob, alice);

// Check team
var teams = manager.GetAllTeams();
Console.WriteLine($"Teams: {teams.Count}");
```

### Configuration Options

```csharp
var service = new ViewerFactionIntegrationService(config);

// Configure faction manager settings
service.FactionManager.MaxTeamSize = 5;
service.FactionManager.MaxDealsPerViewer = 3;
service.FactionManager.DefaultDealDuration = TimeSpan.FromMinutes(5);

// Customize starting resources
service.FactionManager.StartingResources = new Dictionary<string, float>
{
    { "resource_metal", 100f },
    { "resource_silicon", 100f },
    { "resource_water", 50f }
};
```

## Integration with Game Systems

The viewer faction system is designed to integrate with Per Aspera's game systems:

### Future Enhancements

1. **Game Faction Integration**
   - Connect ViewerFaction to game Faction objects
   - Synchronize resources with actual game resources
   - Allow viewers to control in-game factions

2. **Command Extensions**
   - Build buildings via commands
   - Research technologies
   - Trade with other factions
   - Military actions

3. **Event Integration**
   - React to game events
   - Trigger viewer challenges
   - Award points for achievements

4. **Persistence**
   - Save faction state to database
   - Load previous sessions
   - Track historical statistics

## Command Flow

```
Twitch Chat Message
    ↓
TwitchConnection (IRC)
    ↓
ViewerFactionIntegrationService
    ↓
ViewerFactionCommands (Parse & Route)
    ↓
ViewerFactionManager (Execute)
    ↓
Update ViewerFaction / ViewerTeam / ViewerDeal
    ↓
Send Response to Twitch Chat
```

## Example Scenarios

### Scenario 1: Creating a Team

1. Alice joins: `!join`
2. Bob joins: `!join`
3. Alice invites Bob: `!team bob`
4. Bob sees invitation notification
5. Bob accepts: `!accept alice`
6. Alice and Bob are now in a team

### Scenario 2: Making a Deal

1. Alice and Bob are in separate factions
2. Alice proposes: `!deal bob 100 metal for 50 water`
3. Bob reviews: `!status` (shows pending deal)
4. Bob accepts: `!accept alice`
5. Resources are exchanged automatically

### Scenario 3: Competition

1. Multiple viewers join and form teams
2. Viewers compete for points through game actions
3. Check rankings: `!leaderboard`
4. Top teams get recognition

## Technical Details

### Thread Safety

All operations in ViewerFactionManager are thread-safe using locks to prevent race conditions.

### Expiration and Cleanup

- Deals expire after 5 minutes by default
- Invitations expire after 5 minutes by default
- Cleanup runs automatically every minute
- Expired items are removed automatically

### Rate Limiting

Currently not implemented, but recommended additions:
- Command cooldowns per user
- Global rate limiting
- Spam protection

### Error Handling

- All commands have try-catch blocks
- Errors are logged and reported to user
- Service continues running on errors
- Connection errors trigger auto-reconnect

## Testing

### Manual Testing

Test commands in offline mode:

```csharp
var service = new ViewerFactionIntegrationService(null);
var commands = new ViewerFactionCommands(
    service.FactionManager, 
    (user, msg) => Console.WriteLine($"[{user}] {msg}")
);

// Simulate commands
commands.ProcessMessage("alice", "Alice", "!join");
commands.ProcessMessage("bob", "Bob", "!join");
commands.ProcessMessage("alice", "Alice", "!team bob");
commands.ProcessMessage("bob", "Bob", "!accept alice");
commands.ProcessMessage("alice", "Alice", "!status");
```

### Integration Testing

Test with real Twitch connection in a private channel before going live.

## Troubleshooting

### Connection Issues

- Verify OAuth token is valid and starts with "oauth:"
- Check username and channel are correct
- Ensure bot account has joined the channel
- Check firewall settings for IRC port 6667

### Commands Not Working

- Verify command prefix (default: !)
- Check command spelling and syntax
- Ensure user has used !join first
- Check logs for error messages

### Performance Issues

- Monitor active viewer count
- Check for memory leaks in long-running sessions
- Review cleanup timer frequency
- Consider implementing pagination for large lists

## License

MIT License - See project LICENSE file for details.
