using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord_Bot;
using System.Reflection;


public class Program
{
    public static Task Main(string[] args) => new Program().MainAsync();

    private DiscordSocketClient? _client;
    private CommandService? _commands;

    private CommandHandler? commandHandler;

    public async Task MainAsync()
    {
        var config = new DiscordSocketConfig
        {
            // Set the desired GatewayIntents
            GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
        };

        _client = new DiscordSocketClient(config);
        

        _client.Log += Log;

        _commands = new CommandService();
        await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), null);

        commandHandler = new CommandHandler(_client, _commands);
        await commandHandler.InstallCommandsAsync();

        //  You can assign your bot token to a string, and pass that in to connect.
        //  This is, however, insecure, particularly if you plan to have your code hosted in a public repository.
        //var token = "";

        // Some alternative options would be to keep your token in an Environment Variable or a standalone file.
        // var token = Environment.GetEnvironmentVariable("NameOfYourEnvironmentVariable");
        var token = File.ReadAllText(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)+"/token.txt");
        // var token = JsonConvert.DeserializeObject<AConfigurationClass>(File.ReadAllText("config.json")).Token;
        //_client.SlashCommandExecuted += SlashCommandHandler;
        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

       

        

        
        // Block this task until the program is closed.
        await Task.Delay(-1);
    }

    private Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }

    private async Task SlashCommandHandler(SocketSlashCommand command)
    {
        await command.RespondAsync($"You executed {command.Data.Name}");
    }
}
    