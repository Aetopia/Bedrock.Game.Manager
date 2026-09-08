using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Services.Store;

namespace Bedrock.Game.Manager.Store;

sealed class ProductInfo
{
    internal string Title { get; }
    internal StoreImage? Image { get; }

    internal string ProductId { get; }
    internal string PackageFamilyName { get; }

    ProductInfo(StoreProduct product, string packageFamilyName)
    {
        Title = product.Title;
        ProductId = product.StoreId;
        PackageFamilyName = packageFamilyName;

        Image = product.Images.FirstOrDefault(Predicate);
        static bool Predicate(StoreImage _) => _.ImagePurposeTag is "Logo";
    }

    internal static Task<ProductInfo> GetAsync(string productId)
    {
        return GetAsync([productId]).FirstAsync().AsTask();
    }

    internal static async IAsyncEnumerable<ProductInfo> GetAsync(IEnumerable<string> productIds)
    {
        await foreach (var product in MicrosoftStore.GetProductsAsync(productIds))
        {
            using var document = JsonDocument.Parse(product.ExtendedJsonData);

            if (!document.RootElement.TryGetProperty("Properties", out var properties))
                continue;

            if (!properties.TryGetProperty(nameof(PackageFamilyName), out var packageFamilyName))
                continue;

            if (packageFamilyName.GetString() is not { } value)
                continue;

            yield return new(product, value);
        }
    }
}