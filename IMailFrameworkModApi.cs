using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace ClintMailDelivery;

public interface ILetter
{
    string Id { get; }
    string Text { get; }
    string GroupId { get; }
    string Title { get; }
    List<Item> Items { get; }
    string Recipe { get; }
    int WhichBG { get; }
    Texture2D LetterTexture { get; }
    int? TextColor { get; }
    Texture2D UpperRightCloseButtonTexture { get; }
    bool AutoOpen { get; }
    ITranslationHelper I18N { get; }
}

public interface IMailFrameworkModApi
{
    void RegisterLetter(ILetter iLetter, Func<ILetter, bool> condition, Action<ILetter> callback = null, Func<ILetter, List<Item>> dynamicItems = null);
    ILetter GetLetter(string id);
}