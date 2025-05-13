# 🌱 Maze Generator 2D

<table>
  <tr>
    <td><img src="real time 1.png" width="200"/></td>
    <td><img src="real time 2.png" width="200"/></td>
    <td><img src="real time 3.png" width="200"/></td>
    <td><img src="real time 4.png" width="200"/></td>
  </tr>
</table>

Welcome to **Maze Generator 2D**!

This project provides a procedural maze generator and difficulty evaluator. Create your own mazes and evaluate how challenging they are!

---

## 🚀 First Steps

1. Download the file: `MazeGenerator_Package.unitypackage`
2. Import it into Unity (Recommended Unity version: **2022.3.30**)

---

## 📦 What's Included

- A **Maze Cell prefab** for generating the maze layout  
- **Scripts** for generating mazes based on custom size and difficulty  
- A **player sprite** controlled with **WASD**  
- A **GUI** to:
  - Generate mazes
  - Set parameters
  - Evaluate difficulty  
  During the evaluation phase, the maze is solved twice:
    - Once by a simulated human agent.
    - Once by the A* algorithm.  
  The total steps of both runs are visualized in the GUI.

---

## 🛠️ How to Use the Generator

The generator includes multiple adjustable settings:

### Maze Cell Prefab
- The building block used to create the maze.
- You can design your own prefab, but ensure it matches the proportions of the original `MazeCell2D` prefab.

### Maze Width and Height
- Customize the maze's **width (X-axis)** and **height (Y-axis)**.

### Change in Real Time
- Enables the maze to **evolve 2 seconds after generation** (only for *Easy* and *Medium* difficulty).
- This is a custom implementation based on the **Depth-First Search (DFS)** algorithm.

### Watch Grow
- Enables a live visualization of the maze being built.

### Wait Seconds
- Sets the delay between building cycles.
- Fewer seconds = faster generation.

### Watch Human Evaluation
- Watch the human evaluation process in real time.
- Follows the same delay defined in **Wait Seconds**.

### Difficulty
- Choose between:
  - `Easy` – uses **Binary Tree** algorithm  
  - `Medium` – uses **Aldous-Broder** algorithm  
  - `Hard` – uses **Depth-First Search** algorithm

---

## 🧪 Evaluation Mode

During difficulty evaluation:
- The maze is solved by both a **human player** and an **A\*** pathfinding algorithm.
- The number of steps and paths taken are displayed for comparison.

---

Feel free to contribute, suggest improvements, or build your own version of the maze generator!
