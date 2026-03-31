namespace OrderManagement.Application.Cache;

public static class CacheKeys
{
    public static class Customers
    {
        public static string GetCustomerById(Guid id) => $"Customers:id:{id}";

        public const string GetAllCustomers = "Customers:all";

        public const string CustomerPattern = "Customers:";
    }

    public static class Products
    {
        public static string GetProductById(Guid id) => $"Products:id:{id}";

        public const string GetAllProducts = "Products:all";

        public const string ProductsPattern = "Products:";
    }

    public static class Orders
    {
        public static string GetOrderById(Guid id) => $"Orders:id:{id}";

        public const string GetAllOrders = "Orders:all";

        public const string OrderPattern = "Orders:";
    }

    public static class Users
    {
        public static string GetUserById(Guid id) => $"Users:id:{id}";

        public const string GetAllUsers = "Users:all";

        public const string UserPattern = "Users:";
    }
}
