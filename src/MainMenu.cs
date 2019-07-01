using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class MainMenu : Node2D {
	public static MainMenu instance;

	[Export]
	public NodePath _textGrid;
	public GridContainer textGrid;
	public Control[,] grid;

	[Export]
	public NodePath _klackerPlayer;
	public AudioStreamPlayer klackerPlayer;

	const int CharA = 'A';
	const int CharZ = 'Z';
	public const int MaxTextLength = 24;
	public const string TextImagesPath = "res://Images/room/klacker/";
	public readonly static string SoundPath = GFXLibrary.pathToAirlineTycoonD + "/SOUND/";
	public const string FilePrefixNormal = "KL_", FilePrefixDark = "KD_";

	public Dictionary<char, string> exceptions = new Dictionary<char, string>();

	List<Task> klackerTasks;
	public Dictionary<SceneName, List<MenuItem>> menuScenes;

	public enum SceneName {
		MainMenu,
		Settings,
		FreeGame,
		Campaign,
	}


	Random r = new Random(DateTime.Now.Millisecond);

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		instance = this;

		MusicController.isInMainMenu = true;
		MusicController.instance.SetSong("at2");

		textGrid = GetNode<GridContainer>(_textGrid);
		klackerPlayer = GetNode<AudioStreamPlayer>(_klackerPlayer);

		GameController.instance?.SetTaskbar(false);

		PopulateGrid();
		PopulateExceptions();
		PopulateMenuScenes();


		PrepareMenuScene(SceneName.MainMenu);
	}

	private void PopulateExceptions() {
		exceptions.Add('Ä', "AE");
		exceptions.Add('&', "AMP");
		exceptions.Add('\'', "ANF1");
		exceptions.Add('"', "ANF2");
		exceptions.Add('!', "AUSR");
		exceptions.Add('~', "BULL");
		exceptions.Add(':', "DPKT");
		exceptions.Add('?', "FRAGE");
		exceptions.Add('=', "GLCH");
		exceptions.Add('(', "KLA");
		exceptions.Add(')', "KLZ");
		exceptions.Add(',', "KOMMA");
		exceptions.Add('#', "KREUZ");
		exceptions.Add('<', "LESS");
		exceptions.Add('*', "MAL");
		exceptions.Add('-', "MINUS");
		exceptions.Add('>', "MORE");
		exceptions.Add('Ö', "OE");
		exceptions.Add('.', "PKT");
		exceptions.Add('+', "PL");
		exceptions.Add(';', "SEMI");
		exceptions.Add('/', "SLASH");
		exceptions.Add(' ', "SP");
	}

	private void PopulateMenuScenes() {
		menuScenes = new Dictionary<SceneName, List<MenuItem>>();
		klackerTasks = new List<Task>();

		menuScenes.Add(
			SceneName.MainMenu,
			new List<MenuItem>() {
				new MenuItem(
					"Main Menu:",
					MenuItem.EntryType.Header),
				new MenuItem(
					"",
					MenuItem.EntryType.HeaderBar),

				new MenuItem(
					"Free Game",
					MenuItem.EntryType.Link) {OnClick = ()=>{
						RoomManager.ChangeRoom("", true);
						GameController.instance.SetTaskbar(true);}},
				new MenuItem(
					"Campaigns",
					MenuItem.EntryType.LinkBlocked),
				new MenuItem(
					"Network Game",
					MenuItem.EntryType.LinkBlocked),
				new MenuItem(
					"Load Game!",
					MenuItem.EntryType.LinkBlocked),

				new MenuItem(),

				new MenuItem(
					"Home Airport",
					MenuItem.EntryType.LinkBlocked),
				new MenuItem(
					"Options",
					MenuItem.EntryType.Link) {SceneToChangeTo = SceneName.Settings},

				new MenuItem(),

				new MenuItem(
					"Intro",
					MenuItem.EntryType.LinkBlocked),
				new MenuItem(
					"Credits",
					MenuItem.EntryType.LinkBlocked),
				new MenuItem(
					"Highscores",
					MenuItem.EntryType.LinkBlocked),

				new MenuItem(),

				new MenuItem(
					"Quit Game!",
					MenuItem.EntryType.Link)  {SceneToChangeTo = SceneName.MainMenu},
			});
		menuScenes.Add(
			SceneName.Settings,
			new List<MenuItem>() {
				new MenuItem(
					"Settings",
					MenuItem.EntryType.Header
				),
				new MenuItem(
					"",
					MenuItem.EntryType.HeaderBar
				),
				new MenuItem(
					"< Back",
					MenuItem.EntryType.MoveLeft
				)   {SceneToChangeTo = SceneName.MainMenu},
				new MenuItem(
					"Next >",
					MenuItem.EntryType.MoveRight
				)
			});

	}

	private void PrepareMenuScene(SceneName name) {
		List<MenuItem> scene = menuScenes[name];
		for (int line = 0; line < scene.Count; line++) {
			string text = scene[line].text;
			for (int x = 0; x < text.Length; x++) {
				string output = CheckExceptions(text[x]);

				Sprite s = CreateTextSprite(scene[line], output);

				int xPos = x, yPos = line;
				switch (scene[line].type) {
					case MenuItem.EntryType.MoveLeft:
						yPos = grid.GetUpperBound(1);
						break;
					case MenuItem.EntryType.MoveRight:
						yPos = grid.GetUpperBound(1);
						xPos = grid.GetUpperBound(0) - text.Length + x + 1;
						break;
				}

				grid[xPos, yPos].AddChild(s);
				((CharacterItem)grid[xPos, yPos]).AssignedMenuItem = scene[line];
				//grid[x, line].Connect("mouse_entered", grid[x, line], "OnMouseEnter");
				//((CharacterItem)grid[x, line]).OnMouseEnter();

				klackerTasks.Add(AnimateText(scene[line], s, text[x]));
			}
		}

		PlayKlackers();
	}

	public void ChangeScene(SceneName name) {

		foreach (Control g in grid) {
			CharacterItem characterItem = (g as CharacterItem);
			characterItem.AssignedMenuItem = null;

			if (characterItem.IsConnected("mouse_entered", g, "MouseEntered")) {
				characterItem.Disconnect("mouse_entered", g, "MouseEntered");
				characterItem.Disconnect("mouse_exited", g, "MouseExited");
			}

			if (g.GetChildCount() == 1)
				g.GetChild(0)?.QueueFree();
		}

		PrepareMenuScene(name);
	}

	private async Task AnimateText(MenuItem item, Sprite text, char current) {
		int turns = 0;
		int maxTurns = 5 + r.Next(-2, 5);

		while (turns < maxTurns) {
			turns++;

			int randomStart = r.Next(CharA, CharZ);

			string output = CheckExceptions((char)randomStart);

			if (!IsInstanceValid(text))
				return;
			text.Texture = (Texture)ResourceLoader.Load(GetFilePath(output, item.TypeFace));

			await Task.Delay(60);
		}


		if (IsInstanceValid(text))
			text.Texture = (Texture)ResourceLoader.Load(GetFilePath(CheckExceptions(current), item.TypeFace));
	}

	private async Task PlayKlackers() {
		AudioStreamSample[] audioFiles = new AudioStreamSample[3];
		audioFiles[0] = new AudioStreamSample();
		audioFiles[1] = new AudioStreamSample();
		audioFiles[2] = new AudioStreamSample();

		List<AudioStreamPlayer> oneShotAudios = new List<AudioStreamPlayer>();

		byte[] data = System.IO.File.ReadAllBytes(SoundPath + "Klack0.raw");

		audioFiles[0].SetData(data);
		audioFiles[0].MixRate = 44100;
		data = System.IO.File.ReadAllBytes(SoundPath + "Klack1.raw");

		audioFiles[1].SetData(data);
		audioFiles[1].MixRate = 44100;

		data = System.IO.File.ReadAllBytes(SoundPath + "Klack2.raw");
		audioFiles[2].SetData(data);
		audioFiles[2].MixRate = 44100;

		klackerPlayer.SetStream(audioFiles[0]);

		Task t = Task.WhenAll(klackerTasks);



		while (!t.IsCompleted) {
			AudioStreamPlayer p = new AudioStreamPlayer();
			oneShotAudios.Add(p);
			AddChild(p);
			p.SetStream(audioFiles[r.Next(0, 2)]);
			p.Play();
			await Task.Delay(60);
		}

		foreach (var player in oneShotAudios) {
			player.QueueFree();
		}
	}

	private static bool IsLetterInAlphabet(char letter) {
		return letter > CharA && letter < CharZ;
	}

	private string CheckExceptions(char input) {
		string output = input.ToString();

		if (exceptions.ContainsKey(input)) {
			output = exceptions[input];
		}

		return output;
	}



	private void PopulateGrid() {
		// 24x16
		grid = new Control[24, 16];

		for (int y = 0; y < 16; y++) {
			for (int x = 0; x < 24; x++) {
				grid[x, y] = CreateControl();
				grid[x, y].Name = "X" + x + " Y" + y;
			}
		}
	}

	public Control CreateControl() {
		CharacterItem c = new CharacterItem();
		c.RectMinSize = new Vector2(16, 22);

		textGrid.AddChild(c);
		c.SetOwner(textGrid);
		return c;
	}
	public Sprite CreateTextSprite(MenuItem item, string character) {
		Sprite s = new Sprite();
		s.Name = character;
		s.Centered = false;
		s.Texture = (Texture)ResourceLoader.Load(GetFilePath(character, item.TypeFace));
		return s;
	}

	private static string GetFilePath(string character, string typeFace) {
		return TextImagesPath + typeFace + character.ToUpper() + ".res";
	}

	override public void _ExitTree() {
		GameController.instance.SetTaskbar(true);
	}
}

public class MenuItem {
	public string text;
	public string TypeFace {
		get {
			switch (type) {
				case (EntryType.LinkBlocked):
					return MainMenu.FilePrefixDark;
				default:
					return MainMenu.FilePrefixNormal;
			}
		}
	}


	public enum EntryType {
		Undefined,
		Space,
		Link,
		LinkBlocked,
		MoveLeft,
		MoveRight,
		TextField,
		HeaderBar,
		Header,
	}

	public EntryType type = EntryType.Undefined;

	public Action OnClick;

	private MainMenu.SceneName _sceneToChangeTo;
	public MainMenu.SceneName SceneToChangeTo {
		get => _sceneToChangeTo;
		set {
			_sceneToChangeTo = value;

			if (OnClick == null)
				OnClick += () => MainMenu.instance.ChangeScene(value);
		}
	}


	public MenuItem() {
		text = "";
		OnClick = null;
		type = EntryType.Space;
	}

	public MenuItem(string text, Action onClick) {
		this.text = text;
		OnClick = onClick;
	}

	public MenuItem(string text, EntryType type, bool fillWithType = true) {
		this.text = text;
		this.type = type;

		if (fillWithType) {
			switch (type) {
				case (EntryType.HeaderBar):
					this.text = "========================";
					break;
				case (EntryType.Link):
				case (EntryType.LinkBlocked):
					this.text = " ~ " + text;
					break;

			}
		}

		OnClick = null;
	}
	public MenuItem(string text, EntryType type, Action onClick, bool fillWithType = true) : this(text, type, fillWithType) {
		OnClick = onClick;
	}
}
