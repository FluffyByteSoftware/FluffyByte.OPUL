FluffyByte.OPUL
OPUL (Open Persistent Universal Layer) is an authoritative, polling-based game server engine written in C# for small-scale multiplayer RPGs. Designed for persistent fantasy worlds where player actions have permanent consequences, OPUL prioritizes simulation depth over scale.
Overview
OPUL powers a living, breathing 2D fantasy world similar to Project Zomboid or Ultima Online, where up to 8 players can mine, build, craft, fight monsters, and shape a persistent environment. The server runs all game logic server-side, treating clients as thin renderers for maximum security and consistency.
Core Principles

Finite Resources: Every tree chopped, ore mined, and tile destroyed is permanent
Living World: NPCs and systems operate continuously across the entire map, even when players aren't nearby
Authoritative Design: All game logic runs server-side; clients only render and send input
Quality Over Scale: Sacrifices player count and map size for deep simulation and world persistence
Binary Protocols: Fast, secure network communication using custom binary serialization

Technical Architecture
Network Layer

TCP: Reliable message delivery for chat, commands, and system notifications (~3 ticks/second)
UDP: Fast state synchronization with custom reliability layer (20 ticks/second for movement)
Multi-Rate Tick System: Independent update loops for different systems (movement, spawning, combat, world simulation)

World Persistence

Binary Map Format: Loads Unity-exported .fluff files with tile-based world data
Full State Simulation: Entire 256×256 tile world simulated in memory
Auto-Save System: Periodic persistence of world state and player data
Object Management: Separate serialization for dynamic entities (players, NPCs, buildings, items)

Tech Stack

Language: C# 9.0
Platform: .NET Console Application (cross-platform: Linux/Windows)
Architecture: Single-threaded with async I/O
Networking: Custom TCP/UDP implementation using .NET sockets
Deployment: Optimized for Linux servers, Windows-compatible for development

Features
✅ Implemented

Multi-rate tick system (movement, messaging, spawning, cleanup)
Custom TCP/UDP networking layer
Binary map loading from Unity exports
Scribe logging system with color-coded console output

🚧 In Development

Tile destruction and transformation system
Player position synchronization
NPC AI and pathfinding
Combat mechanics
Inventory and crafting systems

📋 Planned

Building placement and management
Monster spawning and AI
Weather and day/night cycles
Admin commands and moderation tools

Performance Targets

Max Players: 8 concurrent connections
Map Size: 256×256 tiles (65,536 tiles)
Update Rate: 20 Hz for movement, variable rates for other systems
Bandwidth: <1 Mbps per client
Platform: Designed for dedicated Linux servers

Why OPUL?
Most multiplayer game engines prioritize massive scale—hundreds of players, huge maps, and distributed systems. OPUL takes the opposite approach: by limiting scope to 8 players and a finite world, we can afford to simulate everything, everywhere, all the time.

No spatial partitioning tricks
No "active area" optimizations
No despawning distant objects
Every NPC makes decisions, every crop grows, every tile remembers

This creates a truly persistent, living world where player absence doesn't freeze time.
Project Status
Alpha Development - Core systems are being implemented. Not production-ready.
Philosophy

"Build a world that feels alive, even when no one is watching."

OPUL is a passion project exploring what's possible when you sacrifice scale for depth. It's designed for small communities who want a shared, persistent world that remembers every action.

Development

Developer: Solo dev + occasional artist
License: Unlicensed (free to use, and don't need to credit but I'd love it if you did)
