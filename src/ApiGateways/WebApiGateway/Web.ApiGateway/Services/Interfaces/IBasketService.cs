using Web.ApiGateway.Models.Basket;
using System.Threading.Tasks;


namespace Web.ApiGateway.Services.Interfaces
{
    public interface IBasketService
    {
        Task<BasketData> GetById(string id);

        Task<BasketData> UpdateAsync(BasketData currentBasket);
    }
}
