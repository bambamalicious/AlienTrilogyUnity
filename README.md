Based upon the incredible work happening at https://github.com/Thor110/AlienTrilogyResurrection and https://github.com/CsCodeDude/AlienTrilogyUnity to both byte by byte uncover the details of the game, and to port into Unity.

Huge thanks to both Thor110 and CsCodeDude for their help along the way, along with the work by Bobblen147 initially and the first attempts at source code by Lex Safanov.

This set of scripts and scene file allows you to load the files from an installation and inspect the maps & assets with the hope of uncovering the game's logic system towards a modern engine port of the game.

Screenshots of tool output below.

<img width="1609" height="859" alt="image" src="https://github.com/user-attachments/assets/d74e1407-da09-4e5c-9c2d-e7f45c2e3abc" />

<img width="982" height="625" alt="image" src="https://github.com/user-attachments/assets/e0af9a31-38cd-483e-abd6-de2c71959c32" />

**Installation**
Create a new Unity project and add all files (Map or Game). Open project and load sample scene.

In play mode, add level numbers **Only** (i.e 111) to the inspector, right click map puller and generate level and all data, or level alone.

Right click ObjSpawner to spawn object classes, or all objects.

Export to CSV will export all Collider data to CSV.

**Unity Port Progress**

File viewer implemented allowing loading of original game into port,

Videos decoded using FFmpeg and stored in streaming assets (On the fly decode just didn't play),

Sound effects decoded and loaded into asset library for immediate use,

Music pulled straight from CD audio,

Opening videos and transition to main menu complete.

<img width="687" height="387" alt="image" src="https://github.com/user-attachments/assets/0467e8a5-bab3-47f9-801d-85bee68fe6e1" />
<img width="689" height="388" alt="image" src="https://github.com/user-attachments/assets/71f7a765-87f1-42ce-a8e7-c577cb28cc91" />
<img width="682" height="384" alt="image" src="https://github.com/user-attachments/assets/34974f6c-a913-4c96-9ca7-ec9531580c83" />
<img width="899" height="505" alt="image" src="https://github.com/user-attachments/assets/3fe5efe1-fd1d-427f-9ed6-b20f2e03dd6b" />
<img width="894" height="506" alt="image" src="https://github.com/user-attachments/assets/9988c529-73db-47f3-ac4d-00f0a1bd6b93" />


**Next step**

Creating options menu and transitions. Refining scripts for future data pull,

Data manager to store all values and groundwork for SAving / Loading via PlayerPrefs.

**To Do**

Add .exe file to file viewer, and enable game to be "launched" from .exe in viewer,

Implement options menu,

Implement resolution options,

Implement controls and re-mapping (M&K, Controller & Touch, single output to read i.e if(InputManager.forward... go forward)),

Implement Language options as per original (Maybe look to on-the-fly translation as well?),

Implement original credits sequence,

Implement Classic / Modern switch,

Implement ALT map viewer and store data on a per-level basis,

Develop async loading screen to hide data load & map build,

Saving and loading system. (Playerprefs?),

The entire gameplay aspect.

**Unknowns**

Texture swapping is a problem to be fixed, along with destructable walls and the re-creation in Unity,

Lots of unknown byte actions remain in the original code. To be discovered by developing early (hopefully).

**Known Unity Port Bugs**

Existence of decoded videos can cause exceptions and failure to load. Clear all decoded video from streaming assets and attempt boot again,

3rd video only plays halfway then transitions. Fix inbound,

Moving out of the Unity window during video play can mess things up. Either skip or watch through. Catch to be implemented.

