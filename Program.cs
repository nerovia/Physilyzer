using SadConsole;
using SadConsole.Configuration;

Settings.WindowTitle = "Physilyzer";

// Configure how SadConsole starts up
Builder startup = new Builder()
	.SetScreenSize(90, 30)
	.SetStartingScreen(game => new Physilyzer.Optilyzer.Screen(game.ScreenCellsX, game.ScreenCellsY))
	.IsStartingScreenFocused(true)
	.ConfigureFonts(true);

// Setup the engine and start the game
Game.Create(startup);
Game.Instance.Run();
Game.Instance.Dispose();
