using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Tools;

namespace ClintMailDelivery;

internal class ModEntry : Mod
{
    private readonly string SaveKey = "TrackedTool";

    public override void Entry(IModHelper helper)
    {
        helper.Events.GameLoop.DayStarted += this.OnDayStarted;
        helper.Events.GameLoop.DayEnding += this.OnDayEnding;
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

        // Mod olmadan verilmiş, zaten hazır olan aleti yakala
        Tool? readyTool = Game1.player.toolBeingUpgraded.Value;
        if (readyTool != null && Game1.player.daysLeftForToolUpgrade.Value <= 0)
        {
            this.Monitor.Log($"{readyTool.GetType().Name} is ready, delivering to inventory!", LogLevel.Info);
            Tool? newTool = this.CreateTool(readyTool.GetType().Name, readyTool.UpgradeLevel + 1);
            if (newTool != null)
            {
                Game1.player.toolBeingUpgraded.Value = null;
                Game1.player.daysLeftForToolUpgrade.Value = 0;
                Game1.player.addItemToInventoryBool(newTool);
                this.Helper.Data.WriteSaveData<TrackedTool?>(this.SaveKey, null);
            }
            return;
        }

        TrackedTool? tracked = this.Helper.Data.ReadSaveData<TrackedTool>(this.SaveKey);
        if (tracked == null)
            return;

        tracked.DaysRemaining--;

        if (tracked.DaysRemaining <= 0)
        {
            if (Game1.player.toolBeingUpgraded.Value == null)
            {
                this.Helper.Data.WriteSaveData<TrackedTool?>(this.SaveKey, null);
                return;
            }

            Tool? newTool = this.CreateTool(tracked.ToolType, tracked.UpgradeLevel);
            if (newTool != null)
            {
                Game1.player.toolBeingUpgraded.Value = null;
                Game1.player.daysLeftForToolUpgrade.Value = 0;
                Game1.player.addItemToInventoryBool(newTool);
                this.Helper.Data.WriteSaveData<TrackedTool?>(this.SaveKey, null);
                this.Monitor.Log($"Delivered {tracked.ToolType} to inventory!", LogLevel.Info);
            }
        }
        else
        {
            this.Helper.Data.WriteSaveData(this.SaveKey, tracked);
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

internal class TrackedTool
{
    public string ToolType { get; set; } = "";
    public int UpgradeLevel { get; set; }
    public int DaysRemaining { get; set; }
}
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
