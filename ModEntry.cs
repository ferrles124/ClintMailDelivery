using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Tools;

namespace ClintMailDelivery;

internal class ModEntry : Mod
{
    private readonly string SaveKey = "PendingTools";

    public override void Entry(IModHelper helper)
    {
        helper.Events.GameLoop.DayStarted += this.OnDayStarted;
        helper.Events.GameLoop.Saving += this.OnSaving;
    }

    private void OnDayStarted(object? sender, DayStartedEventArgs e)
    {
        if (!Context.IsWorldReady)
            return;

        // Bekleyen aletleri kontrol et
        List<PendingTool>? pending = this.Helper.Data.ReadSaveData<List<PendingTool>>(this.SaveKey);
        if (pending == null || pending.Count == 0)
            return;

        List<PendingTool> stillPending = new();

        foreach (PendingTool entry in pending)
        {
            entry.DaysRemaining--;

            if (entry.DaysRemaining <= 0)
            {
                // Aleti posta kutusuna gönder
                Tool? tool = this.CreateTool(entry.ToolType, entry.UpgradeLevel);
                if (tool != null)
                {
                    Game1.player.mailbox.Add("ClintMailDelivery_" + entry.ToolType);
                    Game1.player.addItemToInventoryBool(tool);
                    this.Monitor.Log($"Delivered {entry.ToolType} via mail.", LogLevel.Info);
                }
            }
            else
            {
                stillPending.Add(entry);
            }
        }

        this.Helper.Data.WriteSaveData(this.SaveKey, stillPending);
    }

    private void OnSaving(object? sender, SavingEventArgs e)
    {
        // Clint'teki aleti tespit et ve kaydet
        if (Game1.player.toolBeingUpgraded.Value is Tool tool)
        {
            List<PendingTool>? pending = this.Helper.Data.ReadSaveData<List<PendingTool>>(this.SaveKey) ?? new();

            bool alreadyTracked = pending.Exists(p => p.ToolType == tool.GetType().Name);
            if (!alreadyTracked && Game1.player.daysLeftForToolUpgrade.Value > 0)
            {
                pending.Add(new PendingTool
                {
                    ToolType = tool.GetType().Name,
                    UpgradeLevel = tool.UpgradeLevel + 1,
                    DaysRemaining = Game1.player.daysLeftForToolUpgrade.Value
                });

                this.Helper.Data.WriteSaveData(this.SaveKey, pending);
                this.Monitor.Log($"Tracking {tool.GetType().Name} upgrade, {Game1.player.daysLeftForToolUpgrade.Value} days remaining.", LogLevel.Info);
            }
        }
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

internal class PendingTool
{
    public string ToolType { get; set; } = "";
    public int UpgradeLevel { get; set; }
    public int DaysRemaining { get; set; }
}
