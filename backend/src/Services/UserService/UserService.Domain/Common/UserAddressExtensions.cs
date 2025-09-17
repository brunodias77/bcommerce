using UserService.Domain.Entities;

namespace UserService.Domain.Common;


public static class UserAddressExtensions
{
    public static string GetFormattedAddress(this UserAddress address)
    {
        var complement = !string.IsNullOrEmpty(address.Complement) ? $", {address.Complement}" : "";
        return $"{address.Street}, {address.StreetNumber}{complement} - {address.Neighborhood}, {address.City}/{address.StateCode} - CEP: {address.PostalCode}";
    }
}