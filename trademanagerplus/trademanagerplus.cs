using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ArchiSteamFarm.Core;
using ArchiSteamFarm.Json;
using ArchiSteamFarm.Plugins.Interfaces;
using ArchiSteamFarm.Steam;
using ArchiSteamFarm.Steam.Data;
using ArchiSteamFarm.Steam.Exchange;
using JetBrains.Annotations;

namespace TradeManagerPlus;

[UsedImplicitly]
internal sealed class PluginConfig
{
    [JsonInclude]
    [JsonPropertyName("TradeRules")]
    public IReadOnlyCollection<TradeRule> TradeRules { get; private init; } = new List<TradeRule>();
}

[UsedImplicitly]
internal sealed class TradeRule
{
    [JsonInclude]
    [JsonPropertyName("Type")]
    public string Type { get; private init; } = string.Empty;

    [JsonInclude]
    [JsonPropertyName("Conditions")]
    public Conditions Conditions { get; private init; } = new();

    [JsonInclude]
    [JsonPropertyName("ConditionLogic")]
    public string ConditionLogic { get; private init; } = "All";

    [JsonInclude]
    [JsonPropertyName("Description")]
    public string Description { get; private init; } = string.Empty;
}

[UsedImplicitly]
internal sealed class Conditions
{
    [JsonInclude]
    [JsonPropertyName("TheirItems")]
    public ItemConditions TheirItems { get; private init; } = new();

    [JsonInclude]
    [JsonPropertyName("MyItems")]
    public ItemConditions MyItems { get; private init; } = new();
}

[UsedImplicitly]
internal sealed class ItemConditions
{
    [JsonInclude]
    [JsonPropertyName("AppIDs")]
    public IReadOnlyCollection<uint> AppIDs { get; private init; } = new List<uint>();

    [JsonInclude]
    [JsonPropertyName("Types")]
    public IReadOnlyCollection<string> Types { get; private init; } = new List<string>();

    [JsonInclude]
    [JsonPropertyName("Rarities")]
    public IReadOnlyCollection<string> Rarities { get; private init; } = new List<string>();
}


#pragma warning disable CA1812 // ASF uses this class during runtime
[UsedImplicitly]
internal sealed class TradeManagerPlus : IGitHubPluginUpdates, IBotCommand2, IBotModules, IBotTradeOffer2
{
    private readonly ConcurrentDictionary<string, IReadOnlyCollection<TradeRule>> _botTradeRules = new();
    public string Name => nameof(TradeManagerPlus);
    public string RepositoryName => "killerboyyy777/trademanagerplus";
    public Version Version => typeof(TradeManagerPlus).Assembly.GetName().Version ?? throw new InvalidOperationException(nameof(Version));

    public Task OnLoaded()
    {
        ASF.ArchiLogger.LogGenericInfo($"Hello {Name}!");

        return Task.CompletedTask;
    }

    public Task<string?> OnBotCommand(Bot bot, EAccess access, string message, string[] args, ulong steamID = 0)
    {
        return Task.FromResult(args[0].ToUpperInvariant() switch
        {
            "TEST" => "Test command executed!",
            _ => null
        });
    }

    public Task OnBotInitModules(Bot bot, IReadOnlyDictionary<string, JsonElement>? additionalConfigProperties = null)
    {
        if (additionalConfigProperties == null)
        {
            _botTradeRules.TryRemove(bot.BotName, out _);
            return Task.CompletedTask;
        }

        if (!additionalConfigProperties.TryGetValue(Name, out JsonElement jsonElement))
        {
            _botTradeRules.TryRemove(bot.BotName, out _);
            return Task.CompletedTask;
        }

        PluginConfig? pluginConfig;
        try
        {
            pluginConfig = jsonElement.ToJsonObject<PluginConfig>();
        }
        catch (JsonException e)
        {
            bot.ArchiLogger.LogGenericError($"Error parsing {Name} config: {e.Message}");
            _botTradeRules.TryRemove(bot.BotName, out _);
            return Task.CompletedTask;
        }

        if (pluginConfig == null)
        {
            _botTradeRules.TryRemove(bot.BotName, out _);
            return Task.CompletedTask;
        }

        _botTradeRules[bot.BotName] = pluginConfig.TradeRules;

        return Task.CompletedTask;
    }

    public Task<bool> OnBotTradeOffer(Bot bot, TradeOffer tradeOffer, ParseTradeResult.EResult asfResult)
    {
        if (!_botTradeRules.TryGetValue(bot.BotName, out IReadOnlyCollection<TradeRule>? rules) || rules == null)
        {
            return Task.FromResult(false);
        }

        foreach (TradeRule rule in rules)
        {
            if (rule.Type.ToUpperInvariant() != "ACCEPT")
            {
                if (!string.IsNullOrEmpty(rule.Type))
                {
                    bot.ArchiLogger.LogGenericWarning($"Rule with description '{rule.Description}' has an unsupported type '{rule.Type}'. Only 'Accept' is supported.");
                }
                continue;
            }

            bool theirItemsMatch = AreItemsMatching(tradeOffer.ItemsToReceive, rule.Conditions.TheirItems, rule.ConditionLogic);
            bool myItemsMatch = AreItemsMatching(tradeOffer.ItemsToGive, rule.Conditions.MyItems, rule.ConditionLogic);

            if (theirItemsMatch && myItemsMatch)
            {
                bot.ArchiLogger.LogGenericInfo($"Trade offer {tradeOffer.TradeOfferID} matched a rule with description '{rule.Description}'. Accepting.");
                return Task.FromResult(true);
            }
        }

        return Task.FromResult(false);
    }

    private static bool AreItemsMatching(IReadOnlyCollection<Asset> items, ItemConditions conditions, string logic)
    {
        // If no conditions are specified for this set of items (e.g. MyItems), it's an automatic match.
        if (!conditions.AppIDs.Any() && !conditions.Types.Any() && !conditions.Rarities.Any())
        {
            return true;
        }

        // If there are conditions but no items to check against, it's a mismatch.
        if (items.Count == 0)
        {
            return false;
        }

        bool logicIsAll = logic.ToUpperInvariant() == "ALL";

        if (logicIsAll)
        { // ALL items must satisfy the conditions
            foreach (Asset item in items)
            {
                bool appIDMatch = !conditions.AppIDs.Any() || conditions.AppIDs.Contains(item.RealAppID);
                bool typeMatch = !conditions.Types.Any() || conditions.Types.Any(t => item.Type.ToString().Equals(t, StringComparison.OrdinalIgnoreCase));
                bool rarityMatch = !conditions.Rarities.Any() || conditions.Rarities.Any(r => item.Rarity.ToString().Equals(r, StringComparison.OrdinalIgnoreCase));

                if (!(appIDMatch && typeMatch && rarityMatch))
                {
                    return false; // One item did not match
                }
            }
            return true; // All items matched
        }

        // ANY item must satisfy the conditions
        foreach (Asset item in items)
        {
            bool appIDMatch = !conditions.AppIDs.Any() || conditions.AppIDs.Contains(item.RealAppID);
            bool typeMatch = !conditions.Types.Any() || conditions.Types.Any(t => item.Type.ToString().Equals(t, StringComparison.OrdinalIgnoreCase));
            bool rarityMatch = !conditions.Rarities.Any() || conditions.Rarities.Any(r => item.Rarity.ToString().Equals(r, StringComparison.OrdinalIgnoreCase));

            if (appIDMatch && typeMatch && rarityMatch)
            {
                return true; // Found at least one matching item
            }
        }
        return false; // No items matched
    }
}
#pragma warning restore CA1812 // ASF uses this class during runtime
