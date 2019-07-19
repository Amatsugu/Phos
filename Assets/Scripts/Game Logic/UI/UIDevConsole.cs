using System;
using System.Collections;
using System.Collections.Generic;
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

	void Awake()
	{
		_sb = new StringBuilder();
		Application.logMessageReceived += DebugLogMessage;
		_commands = new Dictionary<string, Command>();
		INST = this;
		AddDefaultCommands();
	}

	void AddDefaultCommands()
	{
		AddCommand(new Command("close", () => consolePanel.Hide(), "closes the console"));
		AddCommand(new HelpCommand());
		AddCommand(new Command("seed", () => AddConsoleMessage(Map.ActiveMap.Seed.ToString()), "displays the current map seed"));
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
		if(consolePanel.IsActive)
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
			if (consolePanel.IsActive)
				consolePanel.Hide();
			else
				consolePanel.Show();
		}

		if((Input.GetKeyUp(KeyCode.Return) || Input.GetKeyUp(KeyCode.KeypadEnter)) && inputBox.isFocused)
		{
			ParseCommand(inputBox.text);
			inputBox.text = "";
			inputBox.ActivateInputField();
		}
    }

	public static void AddConsoleMessage(string message)
	{
		INST._sb.AppendLine(message);
		if(INST.consolePanel.IsActive)
			INST.UpdateConsoleText();
	}

	void ParseCommand(string commandString)
	{
		var commandSplit = commandString.Split(' ');
		if (_commands.ContainsKey(commandSplit[0]))
		{
			AddConsoleMessage(commandSplit[0]);
			_commands[commandSplit[0]].Execute(commandSplit);
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
}
