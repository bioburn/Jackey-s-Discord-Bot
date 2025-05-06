using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using Discord.Net;
using Discord.WebSocket;
using Discord;
using Microsoft.AspNetCore.Mvc;
using System.Text;

[ApiController]
[Route("api/[controller]")]
public class ShopifyWebhookController : ControllerBase
{
    private readonly DiscordSocketClient _discord;
    private static Dictionary<long, int> _previousStock = new();
    private static readonly string stockFile = "stock_data.json";

    public ShopifyWebhookController(DiscordSocketClient discord)  
    {
        _discord = discord;
        LoadStockFromFile(); 
    }

    [HttpPost("inventory")]
    public async Task<IActionResult> InventoryUpdate()
    {
        using var reader = new StreamReader(Request.Body, Encoding.UTF8);
        var body = await reader.ReadToEndAsync();

        Console.WriteLine("Shopify webhook received:\n" + body);

        var payload = JsonSerializer.Deserialize<Dictionary<string, object>>(body);

        if (payload != null &&
            payload.TryGetValue("inventory_item_id", out var idObj) &&
            payload.TryGetValue("available", out var stockObj) &&
            long.TryParse(idObj.ToString(), out var inventoryItemId) &&
            int.TryParse(stockObj.ToString(), out var available))
        {
            _previousStock.TryGetValue(inventoryItemId, out var previousAvailable);

            ulong channelId = 1119707379102122096;
            var channel = _discord.GetChannel(channelId) as IMessageChannel;

            if (channel != null)
            {
                if (available == 0 && previousAvailable > 0)
                {
                    await channel.SendMessageAsync($"❌ Product {inventoryItemId} is now OUT OF STOCK!");
                }
                else if (available > previousAvailable)
                {
                    await channel.SendMessageAsync($"📦 Inventory increased for product {inventoryItemId}: {previousAvailable} → {available}");
                }
            }

            _previousStock[inventoryItemId] = available;
            SaveStockToFile();
        }

        return Ok();
    }

    private void LoadStockFromFile()
    {
        if (System.IO.File.Exists(stockFile))
        {
            try
            {
                var json = System.IO.File.ReadAllText(stockFile);
                _previousStock = JsonSerializer.Deserialize<Dictionary<long, int>>(json) ?? new();
                Console.WriteLine("Loaded stock data from file.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading stock file: {ex.Message}");
            }
        }
    }

    private void SaveStockToFile()
    {
        try
        {
            var json = JsonSerializer.Serialize(_previousStock, new JsonSerializerOptions { WriteIndented = true });
            System.IO.File.WriteAllText(stockFile, json);
            Console.WriteLine("Saved stock data to file.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error writing stock file: {ex.Message}");
        }
    }
}
