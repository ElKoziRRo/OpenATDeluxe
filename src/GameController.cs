using Godot;
using System;
using System.Threading.Tasks;

public class GameController : Node2D {
	#region Event Callbacks

	/// <summary>
	/// Gets called every in game tick
	/// </summary>
	public static Action onTick;
	public static Action onUnhandledInput;

	public static Action<GameMode> onGameModeChanged;

	#endregion

	GameMode currentGameMode;
	public GameMode CurrentGameMode { get => currentGameMode; private set { currentGameMode = value; } }

	public static GameController instance;

	private static ATTime currentTime = new ATTime(0, 0, 0, 0);
	public static ATTime CurrentTime { get => currentTime; private set => currentTime = value; }

	public static int TimeScale { get; set; }

	public static int currentPlayerID = 2;
	public static string CurrentPlayerTag {
		get {
			return "P" + currentPlayerID;
		}
	}


	public void SetUI(bool toggle) {
		UI.Visible = toggle;
	}

	[Export]
	public NodePath _taskbar;
	public Control taskbar;


	[Export]
	public NodePath _UI;
	public Control UI;


	public static string[] playerCompanyNames = { "Sunshine Airways", "Falcon Lines", "Phönix Travel", "Honey Airlines" };
	public static string[] playerNames = { "Tina Cortez", "Siggi Sorglos", "Igor Tuppolevsky", "Mario Zucchero" };

	public static bool canPlayerInteract = true;

	public static Random r = new Random();

	public static bool fastForward = false;

	public override void _Ready() {
		instance = this;
		taskbar = GetNode<Control>(_taskbar);
		UI = GetNode<Control>(_UI);

		RoomManager.ChangeRoom("RoomMainMenu", isAirport: false);
		GetTree().Connect("screen_resized", this, "OnScreenSizeChanged");

		EventManager.ScheduleEvent(new Event() { name = "Test Event", onQueue = () => { GD.Print("Queue!"); }, onEventStart = () => { GD.Print("EVENT!"); } }, new ATTime(0, 0, 0, 20, 0));
	}

	public override void _UnhandledInput(InputEvent @event) {
		if (@event is InputEventKey k) {
			fastForward = false;
			if (k.Scancode == (int)KeyList.Space) {
				//onUnhandledInput?.Invoke();
				fastForward = k.Pressed;
			}
		}
		if (@event is InputEventMouseButton m) {
			OnMouseClick(m);
		}
	}

	public static void OnMouseClick(InputEventMouseButton m) {
		if (m.IsPressed() && m.ButtonIndex == (int)ButtonList.Left) {
			onUnhandledInput?.Invoke();
		}
	}

	override public void _Process(float delta) {
		TimeScale = fastForward ? 20 : 1;

		float elapsedTicks = 30 * TimeScale * delta; //1 * delta = 1 tick per second

		if (CurrentGameMode == GameMode.InGame) {
			CurrentTime.Tick(elapsedTicks);
			Tick();
		}
	}

	public void Tick() {
		EventManager.HandleTick();
	}

	public void SetGameMode(GameMode mode) {
		CurrentGameMode = mode;
		onGameModeChanged?.Invoke(mode);

		switch (mode) {
			case (GameMode.InGame):
				SetUI(true);
				break;
			case (GameMode.Loading):
			case (GameMode.MainMenu):
				SetUI(false);
				break;
		}
	}

	public void OnScreenSizeChanged() {
		//GetViewport().SetSizeOverride(true, new Vector2(OS.GetWindowSize().x, GetViewportRect().Size.y));
		// Vector2 screenSize = OS.GetWindowSize();
		// Viewport viewport = GetViewport();

		// float scaleX = Mathf.Floor(screenSize.x / viewport.Size.x);
		// float scaleY = Mathf.Floor(screenSize.y / viewport.Size.y);

		// float scale = Mathf.Max(1, Mathf.Min(scaleX, scaleY));

		// Vector2 diffHalf = ((screenSize - (viewport.Size * scale)) / 2).Floor();

		// viewport.SetAttachToScreenRect(new Rect2(diffHalf, viewport.Size * scale));
	}
}

public enum GameMode {
	InGame,
	Loading,
	MainMenu,
}