using StardewModdingAPI;
using System;
using System.Collections.Generic;

namespace CustomCompanions.Framework.Interfaces
{
    public interface ISaveAnywhereApi
    {
        event EventHandler BeforeSave;
        event EventHandler AfterSave;
        event EventHandler AfterLoad;
    }
}
