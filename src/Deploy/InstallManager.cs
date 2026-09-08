using System;
using System.Linq;
using System.Threading.Tasks;
using Bedrock.Game.Manager.Store;
using Windows.ApplicationModel.Store.Preview.InstallControl;
using static System.String;
using static System.StringComparison;
using static System.Threading.Tasks.TaskCreationOptions;
using static System.Threading.Tasks.TaskScheduler;
using static Windows.ApplicationModel.Store.Preview.InstallControl.AppInstallState;
using static Windows.ApplicationModel.Store.Preview.InstallControl.GetEntitlementStatus;

namespace Bedrock.Game.Manager.Deploy;

static class InstallManager
{
    static readonly AppInstallManager s_manager = new();

    internal static async Task<bool> AcquireLicenseAsync(string productId)
    {
        ReadOnlySpan<Task<GetEntitlementResult>> tasks =
        [
            s_manager.GetFreeUserEntitlementAsync(productId, Empty, Empty).AsTask(),
            s_manager.GetFreeDeviceEntitlementAsync(productId, Empty, Empty).AsTask()
        ];

        var items = await Task.WhenAll(tasks);
        return items.Any(Predicate);

        static bool Predicate(GetEntitlementResult result) => result.Status is Succeeded;
    }

    static Task<AppInstallItem?> GetAsync(ProductInfo product) => Task.Factory.StartNew(static state =>
    {
        AppInstallItem? result = null;

        if (state is not ProductInfo product)
            return null;

        foreach (var item in s_manager.AppInstallItems)
        {
            if (item.GetCurrentStatus().InstallState is Error)
            {
                item.Cancel();
                continue;
            }

            if (product.ProductId.Equals(item.ProductId, OrdinalIgnoreCase))
                result = item;
        }

        return result;
    }, product, default, DenyChildAttach, Default);

    internal static async Task<InstallRequest<T>?> InstallAsync<T>(ProductInfo info, T progress) where T : IProgress<(string, int)>
    {
        var item = await GetAsync(info);

        if (item is null) if (info.IsInstalled)
            item = await s_manager.UpdateAppByPackageFamilyNameAsync(info.PackageFamilyName);
        else
            item = await s_manager.StartAppInstallAsync(info.ProductId, Empty, false, false);

        if (item is { })
        {
            s_manager.MoveToFrontOfDownloadQueue(item.ProductId, Empty);
            return new InstallRequest<T>(info, item, progress);
        }

        return null;
    }
}