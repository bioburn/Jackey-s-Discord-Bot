using Discord;
using Discord.Audio;
using Discord.Commands;
using System.Diagnostics;


namespace Discord_Bot
{
    public class VoiceChatModule : ModuleBase<SocketCommandContext>
    {
        // ~play stellar stellar.wav
        [Command("play", RunMode = RunMode.Async)]
        [Summary("Plays a youtube video.")]

        public async Task JoinChannel(string url,IVoiceChannel channel = null)
        {
            // Get the voice channel the user is in
            var voiceChannel = (Context.Message.Author as IGuildUser)?.VoiceChannel;

           
            if (voiceChannel == null) { await Context.Channel.SendMessageAsync("User must be in a voice channel, or a voice channel must be passed as an argument."); return; }

            // For the next step with transmitting audio, you would want to pass this Audio Client in to a service.
            try
            {
                Console.WriteLine("Connecting to voice..."); 
                var audioClient = await voiceChannel.ConnectAsync();
                Console.WriteLine("Connected!");
                await SendAsyncURL(audioClient, url);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Voice connection failed: {ex.Message}");
            }



            //await SendAsync(audioClient, "test.mp3");
            //await SendAsyncURL(audioClient, "https://www.youtube.com/watch?v=k0rj-Y0nMcc");
            //await SendAsyncURL(audioClient, url);
        }

        private Process CreateStream(string path)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
            });
        }

        private async Task SendAsync(IAudioClient client, string path)
        {
            // Create FFmpeg
            using (var ffmpeg = CreateStream(path))
            using (var output = ffmpeg.StandardOutput.BaseStream)
            using (var discord = client.CreatePCMStream(AudioApplication.Mixed))
            {
                try { await output.CopyToAsync(discord); }
                catch(Exception ex) { Console.WriteLine($"{ex.Message}"); }
                finally { await discord.FlushAsync(); }
            }
        }

        private async Task SendAsyncURL(IAudioClient client, string url)
        {
            // Create FFmpeg
            using (var output = ConvertURLToPcm(url))
            using (var discord = client.CreatePCMStream(AudioApplication.Mixed))
            {
                try { await output.CopyToAsync(discord); }
                catch (Exception ex) { Console.WriteLine($"{ex.Message}"); }
                finally { await discord.FlushAsync(); }
            }
        }

        private Stream ConvertURLToPcm(string url)
        {
            string args = $"/C yt-dlp --verbose --ignore-errors -o - {url} | ffmpeg -err_detect ignore_err -i pipe:0 -ac 2 -f s16le -ar 48000 pipe:1";
            var ffmpeg = Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = args,
                RedirectStandardOutput = true,
                UseShellExecute = false
            });
            return ffmpeg.StandardOutput.BaseStream;
        }

    }
}
