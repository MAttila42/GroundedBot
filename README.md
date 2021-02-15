# GroundedBot
Official Discord bot for the ProgramTan server network.

Feel free to use some code.

Official Documentation on [Trello](https://trello.com/b/Ns1WcpEB/groundedbot)

# Contributing Guide
So you would like to help developing the bot I see. Good. I'll try to cover everything you need to know below.

## Setting up
I recommend using [Visual Studio](https://visualstudio.microsoft.com/vs/community/). Don't forget to have [Git](https://git-scm.com/downloads) installed. [Clone](https://docs.github.com/en/github/creating-cloning-and-archiving-repositories/cloning-a-repository) this repository. The project uses Discord.Net 2.3.0.

The Bot won't work like this though, you will need a `BaseConfig.json` file next to the built Bot. The `BaseConfig.cs` has a summary for everything you need in it, but here's an example:
```json
{
	"Token": "YOUR-BOT'S-TOKEN",
	"Prefix": "&",
	"BotID": 763791179711512616,
	"Roles": {
		"Admin": [
			642864137960947755,
			728992546658975805,
			764230828275531796
		],
		"Mod": [
			727070093816758352, 
			728992546658975804
		],
		"PtanB": [
      			680465501599563794,
      			791662389568339998,
      			752836577016676522,
      			791662435505274881,
      			727070093816758352
		],
		"PtanS": [
      			791662389568339998,
      			752836577016676522,
      			791662435505274881,
      			727070093816758352
		],
		"PtanP": [
      			791662435505274881,
      			727070093816758352
		]
	},
	"Channels": {
		"BotTerminal": [
			775458009434030111
		],
		"Backups": [
			0
		],
    		"BotChannel": [
      			656872640958431232,
      			656923734069739521
    		],
    		"PingRequests": 781989837195968545,
    		"AnswerRequests": 781989530197950524,
    		"LevelUp": 0
	}
}
```
You should be able to just put 0's at places that you don't need.

The Bot is now fully funcitonal and you are ready to make some new features.

## File structure
Everything is organized in folders. The commands are in the `Commands` folder and in their proper category. 6 ccategories are planned: `Dev`, `Fun`, `Info`, `Mod`, `Music`, `Util`. Events are in the `Events` folder and Json handlers are in the `Json` folder. Anything that isn't in a folder doesn't really belong anywhere, or contains code that is accessible everywhere (`Program.cs`, `ScriptEngineJs.cs`).

## Program.cs
The start of the Bot. This contains many useful pieces of code. From top to bottom:

### Recieved Message and PingTime
PingTime is nothing special it is only used for the Ping command.

In `Message` the latest message is stored sent by a user. You can acces it anywhere with `Recieved.Message`.

### \_client
DiscrodSocketClient variable. The bot itself basically. You can access it anywhere with `Program._client`.

### MainAsync
This is where the bot starts and every event handler comes from. More in depth on that below.

### ClientLog
Puts some basic logging on the console sent by the client.

### MessageHandler
Here's what most of the fun happens.

If the recieved message is from a Bot then it won't do anything. If it is an answer to a Ping command then it runs that. Lastly, if it is from a user then it continues.

Below the `Events` comment are the message based event funcitons. If you make a new one then put it here.

If the recieved message starts with the Bot's prefix then it will continue to the commands.

Below the `Commands` comment there are further comments with categry names in alphabetical ordar (Please maintain that). Every command is in it's own if statement checking conditions. These can be wheter the user has permission to run that command or if it only be used in bot channels. More on how they work below.

### LeaveHandler
Events that are run when someone leaves the server.

### Ready
Events than are run when the bot starts.

### HourlyEvents
This starts from the Ready event and does the same thing every hour.

### Log
Without parameters it's a simple command logging. With a string paremeter it's an event log and display the given text. Accessible anywhere as `Program.Log()`.

### HasPerm
Needs a list of roles as parameter and determines if the user has any of those.

### BotChannel
Checks if the message was sent in a bot channel based on the BaseConfig

### GetRoleId
Gets the asked role's ID from given name, mention or ID.

### GetUserId
Gets the asked user's ID from given name, mention or ID.

## JSON
The Bot uses JSONs to save data. To access them you will need `using GroundedBot.Json;`. When making a new one please make it as similar to the others as possible for consistency. In depth explanation for each JSON below.

### BaseConfig
I've mentiond it multiple times, but what exactly is this? It's a simple JSON containing every non-variable values such as the Token, different roles or channels etc. Like I said in the beggining the `BaseConfig.cs` file contains summary for everything in it.

Then simply use `BaseConfig.GetConfig()`. You may save this in a variable or get the desired value rightaway.

### Members
Stores many values of each user as long as they are on the server. To get the list simply use `Members.PullData()`. It works the same as the BaseConfig. However you can change the values stored in this by editing the values of the pulled JSON, then Pushing it back into the file with `Members.PushData(list)`. Unique feature is the `GetMemberIndex`. Feed a list and a member, and it will return back their position in the list.

The constructor only needs a single parameter which is the ID.

### Other
All the other JSONs are essentially the same as the Members.

## Making a new command
Lets say you now want to make a new command. Here's what you need to have:
* AllowedRoles (optional, list of IDs from the BaseConfig)
* Basic information about it:
  * Aliases (list of phrases that would run this command)
  * Description (simple string)
  * Usages (list of ways to use the command)
  * Permission (simple string)
  * Trello (link to the Trello page for that command)
* `DoCommand` (function where the command starts)
  * Must have `await Program.Log();` preferably at the top.
  * Must have some sort of response.
  
Ok, you made a new command, what now?

Call it from the MessageHandler with the right condition. Example:
```cs
if (YourCommand.Aliases.Contains(command) && BotChannel() && HasPerm(YourCommand.AllowedRoles))
    YourCommand.DoCommand();
```
Next up add it to the commands list in `Commands.cs` so the users can get some help on how to use it. Example:
```cs
commands.Add(new Command("YourCommand", YourCommand.Aliases, YourCommand.Description, YourCommand.Usages, YourCommand.Permission, "CategoryName", YourCommand.Trello));
```
You can always take a look at already existing commands and I encourage you to do so. Please make everything consistent.

## Making a new event
The event should start from a function named `DoEvent`. Call it from its proper place.

## What to do?
Glad you asked. On [Trello](https://trello.com/b/Ns1WcpEB/groundedbot) I always put what features are planned to be added. Select one and get to work.

## I'm done, how can I put it in the bot?
Make a [Pull Request](https://docs.github.com/en/github/collaborating-with-issues-and-pull-requests/about-pull-requests). You can even do it from Visual Studio. I then take a look at it and maybe some tweaks later put it in the bot.

## Why would I help you?
idk

## Thank you
Thank you for contributing, I appreciate it. If I end up hiring someone as a Developer for this bot then it might just be You.

Love y'all <3
\- ExAtom
