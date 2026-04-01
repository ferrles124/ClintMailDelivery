using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace ClintMailDelivery;

public class Letter : ILetter
{
    public string Id { get; set; }
    public string Text { get; set; }
    public string GroupId { get; set; }
    public string Title { get; set; }
    public List<Item> Items { get; set; }
    public string Recipe { get; set; } = null;
    public int WhichBG { get; set; } = 0;
    public Texture2D LetterTexture { get; set; } = null;
    public int? TextColor { get; set; } = null;
    public Texture2D UpperRightCloseButtonTexture { get; set; } = null;
    public bool AutoOpen { get; set; } = false;
    public ITranslationHelper I18N { get; set; } = null;

    public Letter(string id, string text, List<Item> items)
    {
        this.Id = id;
        this.Text = text;
        this.Items = items;
        this.Title = "Clint";
        this.GroupId = null;
    }
}