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

	void Awake()
	{
		_sb = new StringBuilder();
		Application.logMessageReceived += DebugLogMessage;
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
		AddCommand(new TimeScaleCommand());
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
		_sb.AppendLine($"<color={color}><b>[{type}]</b> {condition}</color>");
		_logs.Add($"[{type}] {condition}\n\t{stackTrace.Replace("\n", "\n\t")}");
		if(consolePanel.IsOpen)
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
        if(Input.GetKeyUp(KeyCode.BackQuote) && !inputBox.isFocused)
		{
			if (consolePanel.IsOpen)
				consolePanel.Hide();
			else
				consolePanel.Show();
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
		var commandSplit = commandString.Split(' ');
		AddConsoleMessage($"> {commandString}", false);
		if (_commands.ContainsKey(commandSplit[0]))
		{
			_commands[commandSplit[0]].Execute(commandSplit);
		}else
		{
			AddConsoleMessage($"No such command '{commandSplit[0]}'");
		}
	}

	public void AddCommand(Command command)
	{
		if (!_commands.ContainsKey(command.name))
			_commands.Add(command.name, command);
	}

	public class Command
	{
		public string name;
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

		public virtual string GetHelpMessage()
		{
			return _helpMessage;
		}
	}

	public class HelpCommand : Command
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
					AddConsoleMessage($"{commands[i].name}:\t{commands[i].GetHelpMessage()}");
				}
				return;
			}
			if (INST._commands.ContainsKey(args[1]))
				AddConsoleMessage(INST._commands[args[1]].GetHelpMessage());
			else
				AddConsoleMessage($"No such command: '{args[1]}'");
		}

		public override string GetHelpMessage()
		{
			return "Shows the help message for the given command or lists all commands";
		}
	}

	public class TimeScaleCommand : Command
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
	}

	void OnDisable()
	{
		File.WriteAllLines("output.log", _logs);
	}
}
