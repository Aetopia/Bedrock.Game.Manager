using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Bedrock.Game.Manager.Deploy;
using Windows.ApplicationModel.Store.Preview.InstallControl;
using Windows.Management.Deployment;
using Windows.Services.Store;
using static Windows.Services.Store.StoreCanLicenseStatus;

namespace Bedrock.Game.Manager.Store;

static class MicrosoftStore
{
    static readonly PackageManager s_manager = new();
    static readonly StoreContext s_context = StoreContext.GetDefault();
    static readonly IEnumerable<string> s_kinds = ["Application", "Game"];

    static async Task<bool> HasLicenseAsync(string productId)
    {
        var result = await s_context.CanAcquireStoreLicenseAsync(productId);

        if (result.ExtendedError is { })
            throw result.ExtendedError;

        if (result.Status is Licensable)
            return true;

        return await InstallManager.AcquireLicenseAsync(productId);
    }

    static async Task<StoreProduct?> GetProductAsync(string productId)
    {
        if (!await HasLicenseAsync(productId))
            return null;

        var result = await s_context.GetStoreProductsAsync(s_kinds, [productId]);

        if (result.ExtendedError is { })
            throw result.ExtendedError;

        return result.Products.Values.First();
    }

    internal static async IAsyncEnumerable<StoreProduct> GetProductsAsync(IEnumerable<string> productIds)
    {
        await foreach (var result in Task.WhenEach(productIds.Select(GetProductAsync)))
            if (await result is { } product) yield return product;
    }

    extension(ProductInfo info)
    {
        internal bool IsInstalled => IsInstalled(info.PackageFamilyName);
    }

    extension(AppInstallItem item)
    {
        internal bool IsInstalled => IsInstalled(item.PackageFamilyName);
    }

    static bool IsInstalled(string packageFamilyName)
    {
        return s_manager.FindPackagesForUser(string.Empty, packageFamilyName).Any();
    }
}