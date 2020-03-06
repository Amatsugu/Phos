using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIDevConsole : MonoBehaviour
{
	public static UIDevConsole INST;

	public UIPanel consolePanel;
	public TMP_Text consoleOut;
	public TMP_InputField inputBox;
	public ScrollRect scrollView;

	private StringBuilder _sb;
	private Dictionary<string, Command> _commands;

	private List<string> _logs;
	private string _lastCmd;

	void Awake()
	{
		_sb = new StringBuilder();
		Application.logMessageReceived += DebugLogMessage;
		Application.logMessageReceivedThreaded += DebugLogMessage;
		_commands = new Dictionary<string, Command>();
		_logs = new List<string>();
		INST = this;
		AddDefaultCommands();
	}

	void AddDefaultCommands()
	{
		AddCommand(new Command("close", () => consolePanel.Hide(), "Closes the console"));
		AddCommand(new HelpCommand());
		AddCommand(new Command("seed", () => AddConsoleMessage(Map.ActiveMap.Seed.ToString()), "Displays the current map seed"));
		AddCommand(new Command("instantBuild", () =>
		{
			GameRegistry.Cheats.INSTANT_BUILD = !GameRegistry.Cheats.INSTANT_BUILD;
			AddConsoleMessage($"instantBuild: <b>{GameRegistry.Cheats.INSTANT_BUILD}</b>");
		}, "Toggles instant build"));
		AddCommand(new Command("instantResearch", () =>
		{
			GameRegistry.Cheats.INSTANT_RESEARCH = !GameRegistry.Cheats.INSTANT_RESEARCH;
			AddConsoleMessage($"instantResearch: <b>{GameRegistry.Cheats.INSTANT_RESEARCH}</b>");
		}, "Toggles instant research"));
		AddCommand(new Command("noResourceCost", () =>
		{
			GameRegistry.Cheats.NO_RESOURCE_COST = !GameRegistry.Cheats.NO_RESOURCE_COST;
			AddConsoleMessage($"noResourceCost: <b>{GameRegistry.Cheats.NO_RESOURCE_COST}</b>");
		}, "Toggles resource cost"));
		AddCommand(new SetResolutionCommand());
		AddCommand(new Command("toggleFullscreen", () =>
		{
			Screen.fullScreen = !Screen.fullScreen;
			AddConsoleMessage($"Fullscreen: {Screen.fullScreen}");
		}, "Toggles fullscreen mode"));
		AddCommand(new SetWindowStateCommand());
		AddCommand(new TimeScaleCommand());
		AddCommand(new Command("nextWeather", WeatherSystem.SkipWeather, "Skips the current weather"));
		AddCommand(new Command("regenLevel", () =>
		{
			EventManager.InvokeEvent("OnMapRegen");
		}, "Destroys and unrenders the map"));
	}

    // Start is called before the first frame update
    void Start()
    {
		consolePanel.OnHide += () =>
		{
			GameRegistry.BuildUI.enabled = true;
			GameRegistry.CameraController.enabled = true;
			GameRegistry.InteractionUI.enabled = true;
		};

		consolePanel.OnShow += () =>
		{
			GameRegistry.BuildUI.enabled = false;
			GameRegistry.CameraController.enabled = false;
			GameRegistry.InteractionUI.enabled = false;
			scrollView.verticalNormalizedPosition = 0;
			inputBox.ActivateInputField();
			UpdateConsoleText();
		};

		inputBox.onSubmit.AddListener(s => {
			if (s.Length == 0 || string.IsNullOrWhiteSpace(s))
				return;
			
			ParseCommand(inputBox.text);
			inputBox.text = "";
			inputBox.ActivateInputField();
		});

		inputBox.onValueChanged.AddListener(s =>
		{
			if (inputBox.text == "`")
			{
				consolePanel.Hide();
				inputBox.text = "";
			}
		});
		consolePanel.Show();
		consolePanel.Hide();
	}

	private void DebugLogMessage(string condition, string stackTrace, LogType type)
	{
		var color = "#ffffff";
		switch(type)
		{
			case LogType.Warning:
				color = "#ffff00";
				break;
			case LogType.Assert:
				color = "#ff0000";
				break;
			case LogType.Error:
				color = "#ee0000";
				break;
			case LogType.Exception:
				color = "#ff0000";
				break;
		}
		if (_sb.Length >= 10000)
			_sb.Clear();
		_sb.AppendLine($"<color={color}><b>[{type}]</b> {condition}</color>");
		_logs.Add($"[{type}] {condition}\n\t{stackTrace.Replace("\n", "\n\t")}");
		UpdateConsoleText();
	}

	public void UpdateConsoleText()
	{
		consoleOut.SetText(_sb);
		scrollView.verticalNormalizedPosition = 0;
	}

	// Update is called once per frame
	void Update()
    {
        if(Input.GetKeyDown(KeyCode.BackQuote) && !inputBox.isFocused)
		{
			if (consolePanel.IsOpen)
				consolePanel.Hide();
			else
				consolePanel.Show();
		}
		if (Input.GetKeyUp(KeyCode.UpArrow) && inputBox.isFocused && inputBox.text != _lastCmd)
		{
			inputBox.text = _lastCmd;
			inputBox.caretPosition = inputBox.text.Length;
		}

		if(Input.GetKeyUp(KeyCode.F4))
		{
			ScreenCapture.CaptureScreenshot($"{Application.dataPath}/Phos {Time.time}.png");
		}
    }

	public static void AddConsoleMessage(string message, bool indent = true)
	{
		if (indent)
			INST._sb.Append('\t');
		INST._sb.AppendLine(message);
		if(INST.consolePanel.IsOpen)
			INST.UpdateConsoleText();
	}

	void ParseCommand(string commandString)
	{
		_lastCmd = commandString;
		var commandSplit = commandString.Split(' ');
		AddConsoleMessage($"> {commandString}", false);
		if (_commands.ContainsKey(commandSplit[0].ToLower()))
		{
			_commands[commandSplit[0].ToLower()].Execute(commandSplit);
		}else
		{
			AddConsoleMessage($"No such command '{commandSplit[0]}'");
		}
	}

	internal void AddCommand(Command command)
	{
		if (!_commands.ContainsKey(command.name.ToLower()))
			_commands.Add(command.name.ToLower(), command);
	}

	internal class Command
	{
		public string name;
		public virtual string HelpMessage => _helpMessage;
		private readonly Action _action;
		private readonly string _helpMessage;

		public Command(string name)
		{
			this.name = name;
		}

		public Command(string name, Action action, string helpMessage) : this(name)
		{
			_action = action;
			_helpMessage = helpMessage;
		}

		public virtual void Execute(string[] args)
		{
			_action?.Invoke();
		}

	}

	private class HelpCommand : Command
	{
		public HelpCommand() : base("help")
		{
			
		}

		public override void Execute(string[] args)
		{
			if (args.Length == 1)
			{
				var commands = INST._commands.Values.ToArray();
				for (int i = 0; i < commands.Length; i++)
				{
					AddConsoleMessage($"{commands[i].name}:\t{commands[i].HelpMessage}");
				}
				return;
			}
			if (INST._commands.ContainsKey(args[1]))
				AddConsoleMessage(INST._commands[args[1]].HelpMessage);
			else
				AddConsoleMessage($"No such command: '{args[1]}'");
		}

		public override string HelpMessage => "Shows the help message for the given command or lists all commands";
	}

	private class TimeScaleCommand : Command
	{
		public TimeScaleCommand() : base("timescale")
		{
		}

		public override void Execute(string[] args)
		{
			if(args.Length == 2)
			{
				if (float.TryParse(args[1], out float t))
					Time.timeScale = t;
				else
					AddConsoleMessage($"Invalid input \"{args[1]}\"");
			}
			AddConsoleMessage($"Timescale: {Time.timeScale}");

		}

		public override string HelpMessage => "Sets the timescale";
	}

	private class SetResolutionCommand : Command
	{
		public SetResolutionCommand() : base("setResolution")
		{
			
		}

		public override void Execute(string[] args)
		{
			if (args.Length < 3)
			{
				AddConsoleMessage("Invalid input, Usage:\n\t\tsetResolution <width> <height>");
				return;
			}
			if (int.TryParse(args[1], out int w))
			{
				if (int.TryParse(args[2], out int h))
				{
					Screen.SetResolution(w, h, Screen.fullScreen);
					AddConsoleMessage($"Resolution: {Screen.currentResolution}");
				}else
					AddConsoleMessage($"[{args[2]} is not a number]");
			}
			else
				AddConsoleMessage($"[{args[1]} is not a number]");


		}

		public override string HelpMessage => "Sets the current resolution.";
	}

	private class SetWindowStateCommand : Command
	{
		public SetWindowStateCommand() : base("setWindowState")
		{
		}

		public override string HelpMessage => $"Set the current window state. ({string.Join(" | ", Enumerable.Range(0,3).Select(i => $"[{i}]{(FullScreenMode)i}"))})";

		public override void Execute(string[] args)
		{
			if(args.Length == 1)
			{
				PrintInvalid();
				return;
			}
			if (int.TryParse(args[1], out var modeId))
			{
				if (modeId < 0 || modeId > 3)
				{
					PrintInvalid();
					return;
				}
				Screen.fullScreenMode = (FullScreenMode)modeId;
				AddConsoleMessage($"Fullscreen Mode: {Screen.fullScreenMode}");
			}
			else
				PrintInvalid();

		}

		private void PrintInvalid()
		{
			AddConsoleMessage("Invalid input");
			AddConsoleMessage("Usage setWindowState <0-3>");
			AddConsoleMessage(string.Join(" | ", Enumerable.Range(0, 3).Select(i => $"[{i}]{(FullScreenMode)i}")));
		}
	}


	void OnDisable()
	{
		File.WriteAllLines("output.log", _logs);
		Application.logMessageReceived -= DebugLogMessage;
	}
}
