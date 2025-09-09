# Sliding Puzzle Game

A challenging sliding puzzle game built in Unity, featuring AI-powered solvers, a timer system, and an immersive detective-themed storyline.

## 🎮 Game Overview

In this game, you play as a detective solving a mystery by completing sliding puzzles. Each puzzle represents a clue in the case. Use your skills to slide tiles into the correct order before time runs out!

## ✨ Features

- **Multiple Puzzle Sizes**: Choose from 3x3, 4x4, 5x5, or 6x6 grids
- **AI Solvers**: Three different AI algorithms (BFS, DFS, A* ) to solve puzzles automatically
- **Hint System**: Get intelligent hints with visual bubbles showing the next move
- **Timer & Lives System**: 5-minute timer per puzzle with 3 lives system
- **Immersive Story**: Detective-themed narrative with typewriter effects
- **Sound Effects**: Atmospheric background music and sound effects
- **Responsive UI**: Adapts to different screen sizes

## 🚀 Installation & Setup

### Prerequisites
- Unity 2021.3 or later
- Git

### Steps
1. Clone the repository:
   ```bash
   git clone https://github.com/yatharth1999/Sliding-Game.git
   cd Sliding-Game
   ```

2. Open the project in Unity:
   - Launch Unity Hub
   - Click "Open" and select the `Sliding-Game` folder
   - Wait for Unity to import assets

3. Open the main scene:
   - Navigate to `Assets/Scenes/`
   - Open `Intro.unity`

4. Build and run:
   - File > Build Settings
   - Select your platform (PC, Mac, WebGL, etc.)
   - Click "Build and Run"

## 🎯 How to Play

1. **Start Game**: Launch the game and follow the introductory story
2. **Select Difficulty**: Choose puzzle size (3x3 to 6x6)
3. **Choose AI Solver**: Pick BFS, DFS, or A* for automatic solving
4. **Solve the Puzzle**:
   - Click adjacent tiles to slide them
   - Arrange tiles in numerical order (1-8 for 3x3, etc.)
   - Bottom-right should be empty
5. **Use Hints**: Click the hint button for AI-suggested moves
6. **Beat the Timer**: Complete before 5 minutes run out
7. **Manage Lives**: You have 3 lives - lose one if time expires

### Controls
- **Mouse Click**: Select and move tiles
- **Hint Button**: Get AI-powered hint
- **Solve Button**: Let AI solve the puzzle automatically
- **Next Button**: Proceed to next puzzle (appears when solved)

## 🧠 AI Algorithms

### Breadth-First Search (BFS)
- Explores all possible moves level by level
- Guarantees shortest solution
- Can be slow for larger puzzles

### Depth-First Search (DFS)
- Explores deep into one path before backtracking
- Memory efficient
- Limited to 20 moves depth

### A* Search
- Uses Manhattan distance heuristic
- Fastest and most efficient
- Recommended for all puzzle sizes

## 🛠️ Technologies Used

- **Unity Engine**: Game development framework
- **C#**: Programming language
- **TextMesh Pro**: UI text rendering
- **Unity UI System**: User interface
- **Audio System**: Sound effects and music

## 📁 Project Structure

```
Sliding-Game/
├── Assets/
│   ├── Scenes/          # Unity scenes
│   ├── Script/          # C# scripts
│   │   ├── GameMgr.cs
│   │   ├── ImageSlidingPuzzle.cs
│   │   ├── PanelSwitcher.cs
│   │   ├── SlidingPuzzleSolver.cs
│   │   └── TypeWriter.cs
│   ├── Images/          # Puzzle images
│   ├── Sounds/          # Audio files
│   ├── prefab/          # Game objects
│   └── TextMesh Pro/    # UI fonts
├── ProjectSettings/     # Unity project settings
└── README.md
```

## 🎨 Customization

### Adding New Images
1. Place image files in `Assets/Images/`
2. Update `imageTextures` list in `ImageSlidingPuzzle.cs`
3. Ensure images are readable in Unity

### Modifying Timer
- Change `timeRemaining` in `PanelSwitcher.cs`
- Default: 300 seconds (5 minutes)

### Adjusting Lives
- Modify `lives` in `GameMgr.cs`
- Default: 3 lives

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch: `git checkout -b feature-name`
3. Commit changes: `git commit -m "Add feature"`
4. Push to branch: `git push origin feature-name`
5. Create a Pull Request

## 📝 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🙏 Acknowledgments

- Unity Technologies for the game engine
- Open-source community for algorithms and assets
- Detective story inspiration from classic mystery games

## 📞 Support

If you encounter issues:
1. Check the Unity console for error messages
2. Ensure all assets are properly imported
3. Verify script references in the Inspector
4. Check that audio files are assigned

For questions or suggestions, please open an issue on GitHub.

---

**Enjoy solving the mystery! 🕵️‍♂️**
