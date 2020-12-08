using Godot;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Text;

public class DialogueWindow : Control {
	//public Label textLabel;

	public VBoxContainer lineContainer;
	public MarginContainer container;

	public NinePatchRect speechbubble;
	public NinePatchRect rightTexture;

	public PackedScene linePrefab;
	public List<Control> lines;
	public List<Control> linesDebug;

	public TextureRect head;

	Vector2 baseSize, basePosition, headPosition;

	public string speechbubbleLinePrefab = "res://Prefabs/Speech/SpeechbubbleLinePrefab.tscn";

	public Vector2 HeadPosition { get => headPosition; set => headPosition = value; }

	public Vector2 GlobalOffset, HeadOffset;

	public override void _Ready() {
		//textLabel = GetNode<Label>("Label");
		lineContainer = GetNode<VBoxContainer>("Content");

		// container = GetNode<MarginContainer>("Margin");
		lineContainer.Connect("resized", this, nameof(OnContainerSizeChange));

		speechbubble = GetNode<NinePatchRect>("SpeechbubbleFlexible");
		//rightTexture = GetNode<NinePatchRect>("SpeechbubbleFlexible/RightSide/Flip/Texture");
		head = GetNodeOrNull<TextureRect>("Head");

		linePrefab = (PackedScene)ResourceLoader.Load(speechbubbleLinePrefab);

		lines = new List<Control>();
		linesDebug = new List<Control>();

		baseSize = RectSize;
		basePosition = RectPosition;

		HeadPosition = head?.RectGlobalPosition ?? default(Vector2);

		this.Update();
		this.Hide();
	}

	public void OnContainerSizeChange() {
		Vector2 innerMargin = new Vector2(75, 20);
		//Add padding:
		speechbubble.RectPosition = lineContainer.RectPosition - innerMargin / 2;
		speechbubble.RectSize = lineContainer.RectSize * 2 + innerMargin * 2;

		//Move to the right position, with the new margin in mind
		RectSize = baseSize - innerMargin;
		RectPosition = basePosition + innerMargin / 2 + GlobalOffset;

		//Move the Speechbubble down a bit when the box is bigger than normal 
		//to mimic the original behavior for longer player text.
		//Except when we are in our office, there we have a different speechbubble!
		if (DialogueSystem.currentlyTalking == GameController.CurrentPlayerTag && RoomManager.currentRoom != "RoomOffice") {
			float heightDifference = speechbubble.RectSize.y - 61;
			heightDifference = Mathf.Min(heightDifference, 140);

			RectPosition = RectPosition + new Vector2(0, heightDifference) / 2;
		}

		PositionHead();

		//Force redraw and repositioning
		speechbubble.Hide();
		speechbubble.Show();
	}

	protected virtual void PositionHead() {
		Vector2 headPos = HeadPosition;
		headPos.y = speechbubble.RectGlobalPosition.y + speechbubble.RectSize.y / 2 - 3;
		head?.SetGlobalPosition(headPos + HeadOffset);
	}

	public static string FillWildcards(string fullText, params string[] values) {
		var aStringBuilder = new StringBuilder(fullText);

		int index = 0;
		int offset = 0;
		foreach (Match m in Regex.Matches(fullText, @"%[a-z]*")) {
			int wildcardPos = m.Index + offset;

			aStringBuilder.Remove(wildcardPos, 2);//!Account for the actual size, not just %s -> e.g. %li 
			aStringBuilder.Insert(wildcardPos, values[index]);
			offset += values[index].Length - 2;

			index++;
		}

		fullText = aStringBuilder.ToString();
		return fullText;
	}

	public static string GetFullCleanTrText(int id, Dialogue dialogue, string[] wildcards = null) {
		string text = CleanOffInstruction(TranslationServer.Translate(dialogue.dialogueGroup + ">" + id));
		if (wildcards != null)
			text = FillWildcards(text, wildcards);
		return text;
	}

	public void PrepareBubbleHeadText(int currentActor, Dialogue dialogue) {
		ClearLines();

		lines.Clear();

		HBoxContainer line = (HBoxContainer)linePrefab.Instance();
		line.GetNode<Control>("Control").Visible = false;

		Label textLabel = line.GetNode<Label>("Label");

		lineContainer.AddChild(line);
		lines.Add(line);
		//TODO: Add positioning to dialogue actor

		string text = GetFullCleanTrText(dialogue.CurrentNode.textId, dialogue, dialogue.CurrentNode.wildcards);
		textLabel.CallDeferred("set_text", text);

		lineContainer.Hide();
		lineContainer.Show();
	}

	private void ClearLines() {
		if (lines != null) {
			foreach (Control l in lines) {
				l.Hide();
				l.CallDeferred("queue_free");
			}
			// lineContainer.RectSize = new Vector2(lineContainer.RectSize.x, 0);
			// lineContainer.RectPosition = new Vector2(0, 0);
		}
	}

	//TODO: Add positioning to dialogue actor
	public void PrepareBubbleOptionsText(int currentActor, Dialogue dialogue) {
		ClearLines();

		lines.Clear();
		linesDebug.Clear();

		string text = "";
		int optionIndex = 0;
		foreach (DialogueOption option in dialogue.CurrentNode.options) {
			HBoxContainer line = (HBoxContainer)linePrefab.Instance();

			Label textLabel = line.GetNode<Label>("Label");

			lineContainer.AddChild(line);
			lines.Add(line);

			LineElement newLine = new LineElement();
			newLine.Name = "Line";
			int opt = optionIndex;
			newLine.onClick += () => DialogueSystem.SelectOption(opt);
			newLine.MouseFilter = MouseFilterEnum.Pass;
			newLine.RectMinSize = line.RectSize;
			newLine.RectPosition = line.RectPosition;
			linesDebug.Add(newLine);
			textLabel.AddChild(newLine);

			//text += "* ";
			text = GetFullCleanTrText(option.TextId, dialogue, option.wildcards);
			//text += '\n';
			textLabel.CallDeferred("set_text", text);

			//AddLines(textLabel.GetLineCount() - lineCount, optionIndex);
			//lineCount = textLabel.GetLineCount();

			optionIndex++;
		}
		lineContainer.Hide();
		lineContainer.Show();
	}
	public void PrepareBubbleAnswerText(int optionIndex, int currentActor, Dialogue dialogue) {
		ClearLines();

		lines.Clear();
		HBoxContainer line = (HBoxContainer)linePrefab.Instance();
		line.GetNode<Control>("Control").Visible = false;

		Label textLabel = line.GetNode<Label>("Label");

		lineContainer.AddChild(line);
		lines.Add(line);

		string text = GetFullCleanTrText(dialogue.CurrentNode.options[optionIndex].TextId, dialogue, dialogue.CurrentNode.options[optionIndex].wildcards);

		textLabel.CallDeferred("set_text", text);

		lineContainer.Hide();
		lineContainer.Show();
	}

	public virtual void OnStartTalking() {

	}
	public virtual void OnStopTalking() {

	}

	public static string CleanOffInstruction(string text) {
		string pattern = @" ?\[\[([^\]]*)\]\]";
		return Regex.Replace(text, pattern, "");
	}

	override public void _Process(float delta) {
		Update();
	}

	override public void _Draw() {
		// float i = 0;
		// foreach (Control c in linesDebug) {
		// 	i += 0.25f;
		// 	Rect2 r = c.GetRect();
		// 	DrawRect(r, Color.FromHsv(i, 1, 1, 0.25f), true);

		// }
	}
}
