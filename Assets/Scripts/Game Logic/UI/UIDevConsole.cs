using Amatsugu.Phos;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using TMPro;

using Unity.Entities;

using UnityEngine;
using UnityEngine.UI;

public class UIDevConsole : MonoBehaviour
{
	public static UIDevConsole INST;

	public UIPanel consolePanel;
	public TMP_InputField consoleOut;
	public TMP_Text cmdPredictText;
	public TMP_InputField inputBox;
	public ScrollRect scrollView;

	private StringBuilder _sb;
	private Dictionary<string, Command> _commands;
	private List<string> _commandNames;

	private List<string> _logs;
	private string _lastCmd;
	private string _curLogTimeStamp;
	private List<string> _curPredictionItems;
	private int _curPrediction;

	private void Awake()
	{
		_sb = new StringBuilder();
		Application.logMessageReceived += DebugLogMessage;
		_commands = new Dictionary<string, Command>();
		_commandNames = new List<string>();
		_logs = new List<string>();
		INST = this;
		_curLogTimeStamp = $"{DateTime.Now.ToString().Replace('/', '-').Replace(':', '.')}";
		AddDefaultCommands();
	}


	// Start is called before the first frame update
	private void Start()
	{
#if UNITY_EDITOR
			GameEvents.OnGameReady += DevMode;
#endif
		consolePanel.OnHide += () =>
		{
			GameEvents.InvokeOnDevConsoleClose();
			GameEvents.InvokeOnCameraUnFreeze();
		};

		consolePanel.OnShow += () =>
		{
			scrollView.verticalNormalizedPosition = 0;
			inputBox.ActivateInputField();
			UpdateConsoleText();
			GameEvents.InvokeOnDevConsoleOpen();
			GameEvents.InvokeOnCameraFreeze();
			Save();
		};

		inputBox.onSubmit.AddListener(s =>
		{
			if (s.Length == 0 || string.IsNullOrWhiteSpace(s))
				return;

			ParseCommand(inputBox.text);
			inputBox.text = "";
			inputBox.ActivateInputField();
		});

		inputBox.onValueChanged.AddListener(s =>
		{
			if (_curPredictionItems != null && s == _curPredictionItems[_curPrediction])
				return;
			_curPredictionItems = null;
			_curPrediction = 0;
			cmdPredictText.text = "";
			if (s.Length == 0)
				return;
			if (s.Contains(' '))
				return;
			var preview = GetCommandSuggestion(s);
			if(preview.Count >= 1)
				cmdPredictText.text = preview[0];
		});
	}

	private void DevMode()
	{
		ParseCommand("unlockAll");
		//ParseCommand("noResourceCost");
		GameEvents.OnGameReady -= DevMode;
	}

	private void AddDefaultCommands()
	{
		AddCommand(new Command("close", () => consolePanel.Hide(), "Closes the console"));
		AddCommand(new Command("quit", () => Application.Quit(0), "Quits the Game"));
		AddCommand(new HelpCommand());
		//Info
		AddCommand(new Command("seed", () => AddConsoleMessage(GameRegistry.GameMap.Seed.ToString()), "Displays the current map seed"));
		//Cheats
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
		AddCommand(new Command("unlockAll", () =>
		{
			foreach (var building in GameRegistry.BuildingDatabase.buildings.Values)
			{
				GameRegistry.UnlockBuilding(new BuildingIdentifier { id = building.id }, false);
			}
			AddConsoleMessage("Unlocked All Buildings");
		}, "Unlocks All Buildings"));
		AddCommand(new Command("fullCheat", () =>
		{
			ParseCommand("noResourceCost");
			ParseCommand("instantBuild");
			ParseCommand("instantResearch");
			ParseCommand("unlockAll");
		}, "Runs all the cheat commands"));
		//Graphics Settings
		AddCommand(new SetResolutionCommand());
		AddCommand(new Command("toggleFullscreen", () =>
		{
			Screen.fullScreen = !Screen.fullScreen;
			AddConsoleMessage($"Fullscreen: {Screen.fullScreen}");
		}, "Toggles fullscreen mode"));
		AddCommand(new SetWindowStateCommand());
		AddCommand(new TimeScaleCommand());
		AddCommand(new Command("nextWeather", WeatherSystem.SkipWeather, "Skips the current weather"));
		AddCommand(new Command("toggleVSync", () =>
		{
			QualitySettings.vSyncCount = (QualitySettings.vSyncCount + 1) % 3;
			switch (QualitySettings.vSyncCount)
			{
				case 0:
					AddConsoleMessage("Vsync: Off");
					break;

				case 1:
					AddConsoleMessage("Vsync: Every VBlank");
					break;

				case 2:
					AddConsoleMessage("Vsync: Every 2nd VBlank");
					break;
			}
		}, "toggles Vsync on/off"));
		//Debug
		AddCommand(new Command("regenLevel", () =>
		{
			GameEvents.InvokeOnMapRegen();
		}, "Destroys and unrenders the map"));

		AddCommand(new Command("toggleClouds", () =>
		{
			var e = GameRegistry.EntityManager.GetAllEntities(Unity.Collections.Allocator.Temp);
			for (int i = 0; i < e.Length; i++)
			{
				if(GameRegistry.EntityManager.HasComponent<CloudData>(e[i]))
				{
					if (GameRegistry.EntityManager.HasComponent<Disabled>(e[i]))
						GameRegistry.EntityManager.RemoveComponent<Disabled>(e[i]);
					else
						GameRegistry.EntityManager.AddComponent<Disabled>(e[i]);
				}
			}
		}, "Toggle cloud visivility"));
	}
	
	private void AddCommand(Command command)
	{
		if (!_commands.ContainsKey(command.name.ToLower()))
		{
			_commands.Add(command.name.ToLower(), command);
			_commandNames.Add(command.name);
		}
	}

	// Update is called once per frame
	private void Update()
	{
		var openPressed = Input.GetKeyDown(KeyCode.BackQuote) || Input.GetKeyDown(KeyCode.Backslash) || Input.GetKeyDown(KeyCode.Slash);
		if (openPressed && !inputBox.isFocused)
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

		if(Input.GetKeyUp(KeyCode.Tab))
		{
			if (_curPredictionItems == null)
			{
				_curPredictionItems = GetCommandSuggestion(inputBox.text);
				if(_curPredictionItems.Count == 0)
					_curPredictionItems = null;
				else
					inputBox.text = _curPredictionItems[_curPrediction = 0];
			}else
			{
				inputBox.text = _curPredictionItems[_curPrediction = (_curPrediction + 1) % _curPredictionItems.Count];
			}
			cmdPredictText.text = "";
			inputBox.caretPosition = inputBox.text.Length;
		}

		if (Input.GetKeyUp(KeyCode.F4))
		{
			ScreenCapture.CaptureScreenshot($"{Application.dataPath}/Phos {Time.time}.png");
		}
	}

	private void DebugLogMessage(string condition, string stackTrace, LogType type)
	{
		var color = "#ffffff";
		switch (type)
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
		{
			_sb.Clear();
			Save();
		}
		_sb.AppendLine($"<color={color}><b>[{type}]</b> {condition}</color>");
		_logs.Add($"[{type}] {condition}\n\t{stackTrace.Replace("\n", "\n\t")}");
		UpdateConsoleText();
	}

	public void UpdateConsoleText()
	{
		consoleOut.text = _sb.ToString();
		scrollView.verticalNormalizedPosition = 0;
	}

	public List<string> GetCommandSuggestion(string query)
	{
		query = query.ToLower();
		var result = _commandNames.Where(n =>
		{
			var qL = query.Length;
			if (n.Length < qL)
				return false;
			return (n.ToLower().Substring(0, qL) == query);
		}).ToList();
		return result;
	}

	public static void AddConsoleMessage(string message, bool indent = true)
	{
		if (indent)
			INST._sb.Append('\t');
		INST._sb.AppendLine(message);
		if (INST.consolePanel.IsOpen)
			INST.UpdateConsoleText();
	}

	private void ParseCommand(string commandString)
	{
		_lastCmd = commandString;
		var commandSplit = commandString.Split(' ');
		AddConsoleMessage($"> {commandString}", false);
		if (_commands.ContainsKey(commandSplit[0].ToLower()))
		{
			_commands[commandSplit[0].ToLower()].Execute(commandSplit);
		}
		else
		{
			AddConsoleMessage($"No such command '{commandSplit[0]}'");
		}
	}
	private void Save()
	{
#if !UNITY_ENGINE
		File.WriteAllLines($"output - {_curLogTimeStamp}.log", _logs);
#endif
	}

	private void OnApplicationQuit()
	{
		Save();
	}

	private void OnDisable()
	{
		Application.logMessageReceived -= DebugLogMessage;
	}

	private void OnDestroy()
	{
		OnDisable();
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
			if (args.Length == 2)
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
				}
				else
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

		public override string HelpMessage => $"Set the current window state. ({string.Join(" | ", Enumerable.Range(0, 3).Select(i => $"[{i}]{(FullScreenMode)i}"))})";

		public override void Execute(string[] args)
		{
			if (args.Length == 1)
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

}