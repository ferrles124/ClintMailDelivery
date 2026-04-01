using System.Collections.Generic;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Tools;

namespace ClintMailDelivery;

internal class ModEntry : Mod
{
    private readonly string SaveKey = "TrackedTool";
    private IMailFrameworkModApi? MailApi;

    public override void Entry(IModHelper helper)
    {
        helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        helper.Events.GameLoop.DayStarted += this.OnDayStarted;
        helper.Events.GameLoop.DayEnding += this.OnDayEnding;
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        this.MailApi = this.Helper.ModRegistry.GetApi<IMailFrameworkModApi>("DIGUS.MailFrameworkMod");
        if (this.MailApi == null)
            this.Monitor.Log("Mail Framework Mod not found! Letters will not work.", LogLevel.Warn);
        else
            this.Monitor.Log("Mail Framework Mod found!", LogLevel.Info);
    }