using System;
using System.Threading.Tasks;
using ArchiSteamFarm.Core;
using ArchiSteamFarm.Plugins.Interfaces;
using ArchiSteamFarm.Steam;
using JetBrains.Annotations;

namespace TradeManagerPlus;

#pragma warning disable CA1812 // ASF uses this class during runtime
[UsedImplicitly]
internal sealed class TradeManagerPlus : IGitHubPluginUpdates, IBotCommand2 {
	public string Name => nameof(TradeManagerPlus);
	public string RepositoryName => "killerboyyy777/TradeManagerPlus";
	public Version Version => typeof(TradeManagerPlus).Assembly.GetName().Version ?? throw new InvalidOperationException(nameof(Version));

	public Task OnLoaded() {
		ASF.ArchiLogger.LogGenericInfo($"Hello {Name}!");

		return Task.CompletedTask;
	}

	public Task<string?> OnBotCommand(Bot bot, EAccess access, string message, string[] args, ulong steamID = 0) {
		return Task.FromResult(args[0].ToUpperInvariant() switch {
			"TEST" => "Test command executed!",
			_ => null
		});
	}
}
#pragma warning restore CA1812 // ASF uses this class during runtime
