This is a game that aims to achieve a 3D sandbox environment where players can spawn in vegetation and animals and watch the animals show realistic behaviours suitable to their species and surroundings.

If you want to see the inner workings of the game or compile this project from the source, clone this repository and add its folder as a project in Unity Hub. Then open it as if you would open any local Unity project.

100 page documentation of the making process of this project: [A-level CS NEA - Yagiz Yilmaz (2024-2025).pdf](https://github.com/user-attachments/files/18203341/A-level.CS.NEA.-.Yagiz.Yilmaz.2024-2025.pdf)


# Features
* The animals are able to exhibit behaviours such as fleeing, wandering, hunting, sleeping, and mating. They correctly avoid pathfinding to the sea or the peaks of mountains.
* 3D game world that is procedurally generated with the use of Perlin noise. The terrain has realistic features such as spreading vegetation, deep and shallow seas, mountains, and plateaus.
* My AnimalAI class offers an easily extendable framework for adding more animals and functionality.
* Priority queue for managing the AI states and behaviours.
* The player can manually spawn in any object they desire.
* Save load system that stores the save file locally in a ".json" file. The save file can then be loaded to fully restore the previous gamestate
* Handmade pixel art for the spawnable objects.

# Demonstration Video
https://www.youtube.com/watch?v=FqfRRGODOvQ

![snapshot](https://github.com/user-attachments/assets/a259ee37-2026-4023-81e9-d8eda1a3bc0a)

# Controls
WASD - Movement
Mouse - Look around
Holding Shift - Increases movement speed
Holding Ctrl - Decreases movement speed
Esc - Bring up the pause menu
E - Bring up the spawn menu
Left click - Spawns the selected object at a suitable location in front of the player

#

Made for my A-level computer science NEA (2024-2025)

Made in Unity 2022.3 LTS
