# Type Rider
Code used for the game "Type Rider". Following is a quick rundown of what each script does.

## Scripts

`Camera.cs` - Makes the camera follow the car smoothly and always keeps it in view.

`Car_Movement.cs` - Moves the car along the track. Handles the countdown before the race starts, and makes the car go faster or slower based on your typing.

`Restart_Button.cs` - Resets the whole game when you press a key or hit the restart button.

`Scene_Loader.cs` - Loads the main game scene when you start playing.

`Speed_Bar_UI.cs` - Shows a bar on screen that fills up as the car speeds up, and changes color from red to green.

`Typing_Mechanic.cs` - The main game mechanic. Shows a word, checks what you type, and speeds up or slows down the car depending on how well you type. The difficulty increases as you perform better.
