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
private void OnDayEnding(object? sender, DayEndingEventArgs e)
    {
        if (!Context.IsWorldReady)
            return;

        Tool? tool = Game1.player.toolBeingUpgraded.Value;
        int daysLeft = Game1.player.daysLeftForToolUpgrade.Value;

        if (tool != null && daysLeft >= 0)
        {
            TrackedTool tracked = new()
            {
                ToolType = tool.GetType().Name,
                UpgradeLevel = tool.UpgradeLevel + 1,
                DaysRemaining = daysLeft
            };
            this.Helper.Data.WriteSaveData(this.SaveKey, tracked);
            this.Monitor.Log($"Tracking {tracked.ToolType}, {daysLeft} days left.", LogLevel.Info);
        }
    }
private void OnDayStarted(object? sender, DayStartedEventArgs e)
    {
        if (!Context.IsWorldReady)
            return;

        if (this.MailApi == null)
            return;

        // Mod olmadan verilmiş, zaten hazır olan aleti yakala
        Tool? readyTool = Game1.player.toolBeingUpgraded.Value;
        if (readyTool != null && Game1.player.daysLeftForToolUpgrade.Value <= 0)
        {
            this.SendToolMail(readyTool.GetType().Name, readyTool.UpgradeLevel + 1);
            Game1.player.toolBeingUpgraded.Value = null;
            Game1.player.daysLeftForToolUpgrade.Value = 0;
            this.Helper.Data.WriteSaveData<TrackedTool?>(this.SaveKey, null);
            return;
        }

        TrackedTool? tracked = this.Helper.Data.ReadSaveData<TrackedTool>(this.SaveKey);
        if (tracked == null)
            return;

        tracked.DaysRemaining--;

        if (tracked.DaysRemaining <= 0)
        {
            this.SendToolMail(tracked.ToolType, tracked.UpgradeLevel);
            Game1.player.toolBeingUpgraded.Value = null;
            Game1.player.daysLeftForToolUpgrade.Value = 0;
            this.Helper.Data.WriteSaveData<TrackedTool?>(this.SaveKey, null);
        }
        else
        {
            this.Helper.Data.WriteSaveData(this.SaveKey, tracked);
        }
    }
private void SendToolMail(string toolType, int upgradeLevel)
    {
        Tool? tool = this.CreateTool(toolType, upgradeLevel);
        if (tool == null)
            return;

        string letterId = $"ClintMailDelivery_{toolType}_{Game1.Date.TotalDays}";
        Letter letter = new(
            id: letterId,
            text: $"Merhaba @,^^Aletini yükselttim, umarım işine yarar.^^-Clint",
            items: new List<Item> { tool }
        );

        this.MailApi!.RegisterLetter(
            letter,
            condition: _ => true,
            callback: _ => this.Monitor.Log($"Player received {toolType}.", LogLevel.Info)
        );

        this.Monitor.Log($"Sent {toolType} via mail!", LogLevel.Info);
    }

    private Tool? CreateTool(string toolType, int upgradeLevel)
    {
        return toolType switch
        {
            nameof(Axe) => new Axe { UpgradeLevel = upgradeLevel },
            nameof(Pickaxe) => new Pickaxe { UpgradeLevel = upgradeLevel },
            nameof(Hoe) => new Hoe { UpgradeLevel = upgradeLevel },
            nameof(WateringCan) => new WateringCan { UpgradeLevel = upgradeLevel },
            _ => null
        };
    }
}

internal class TrackedTool
{
    public string ToolType { get; set; } = "";
    public int UpgradeLevel { get; set; }
    public int DaysRemaining { get; set; }
}