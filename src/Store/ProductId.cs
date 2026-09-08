using System.Collections.Generic;

namespace Bedrock.Game.Manager.Store;

static class ProductId
{
    internal const string Minecraft = "9NBLGGH2JHXJ";
    internal const string GamingServices = "9MWPM2CQNLHN";
    internal const string MinecraftPreview = "9P5X4QVLC2XR";
    internal const string MinecraftLegends = "9N98Z825TNFW";
    internal const string MinecraftDungeons = "9P8MK4NC0LJB";
    internal const string MinecraftDungeons2 = "9P5786PJB9RP";

    internal static readonly IEnumerable<string> s_minecraftGames =
    [
        Minecraft,
        MinecraftPreview,
        MinecraftLegends,
        MinecraftDungeons,
        MinecraftDungeons2,
    ];
}