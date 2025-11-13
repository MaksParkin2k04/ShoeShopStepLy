namespace ShoeShop.Models {
    public interface IBasketShoppingService {
       BasketShopping GetBasketShopping();
        void SetBasketShopping(BasketShopping basketShopping);
        void Clear();
    }
}
