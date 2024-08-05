using Newtonsoft.Json;
using ProductShop.Data;
using ProductShop.Models;
using System.Linq;

namespace ProductShop
{
    public class StartUp
    {
        public static void Main()
        {
            ProductShopContext context = new ProductShopContext();

            #region JsonToString
            string userJson = File.ReadAllText("../../../Datasets/users.json");

            string productJson = File.ReadAllText("../../../Datasets/products.json");

            string categoryJson = File.ReadAllText("../../../Datasets/categories.json");

            string categoryProductJson = File.ReadAllText("../../../Datasets/categories-products.json");
            #endregion

            Console.WriteLine(GetUsersWithProducts(context));
        }

        //01.
        public static string ImportUsers(ProductShopContext context, string inputJson)
        {
            var users = JsonConvert.DeserializeObject<User[]>(inputJson);

            context.Users.AddRange(users);
            context.SaveChanges();

            return $"Successfully imported {users.Length}";
        }

        //02.
        public static string ImportProducts(ProductShopContext context, string inputJson)
        {
            var products = JsonConvert.DeserializeObject<Product[]>(inputJson);

            if (products != null)
            {
                context.Products.AddRange(products);
                context.SaveChanges();
            }

            return $"Successfully imported {products?.Length}";
        }

        //03.
        public static string ImportCategories(ProductShopContext context, string inputJson)
        {
            var allCategories = JsonConvert.DeserializeObject<Category[]>(inputJson);

            var categories = allCategories
                .Where(c => c.Name != null)
                .ToArray();

            if (categories != null)
            {
                context.Categories.AddRange(categories);
                context.SaveChanges();
            }

            return $"Successfully imported {categories?.Length}";
        }

        //04.
        public static string ImportCategoryProducts(ProductShopContext context, string inputJson)
        {
            var categoriesProducts = JsonConvert.DeserializeObject<CategoryProduct[]>(inputJson);

            if (categoriesProducts != null)
            {
                context.CategoriesProducts.AddRange(categoriesProducts);
                context.SaveChanges();
            }

            return $"Successfully imported {categoriesProducts?.Length}";
        }

        //05.
        public static string GetProductsInRange(ProductShopContext context)
        {
            var products = context.Products
                .Where(p => p.Price >= 500 && p.Price <= 1000)
                .Select(p => new
                {
                    name = p.Name,
                    price = p.Price,
                    seller = p.Seller.FirstName + " " + p.Seller.LastName
                })
                .OrderBy(p => p.price)
                .ToList();

            string json = JsonConvert.SerializeObject(products, Formatting.Indented);

            return json;
        }

        //06.
        public static string GetSoldProducts(ProductShopContext context)
        {
            var users = context.Users
                .Where(u => u.ProductsSold.Any() && u.ProductsSold.Where(p => p.Buyer != null).Any())
                .OrderBy(u => u.LastName)
                    .ThenBy(u => u.FirstName)
                .Select(u => new
                {
                    firstName = u.FirstName,
                    lastName = u.LastName,
                    soldProducts = u.ProductsSold
                        .Select(p => new
                        {
                            name = p.Name,
                            price = p.Price,
                            buyerFirstName = p.Buyer.FirstName,
                            buyerLastName = p.Buyer.LastName
                        })
                        .ToList()
                })
                .ToList();

            string json = JsonConvert.SerializeObject(users, Formatting.Indented);

            return json;
        }

        //07.
        public static string GetCategoriesByProductsCount(ProductShopContext context)
        {
            var categories = context.Categories
                .OrderByDescending(c => c.CategoriesProducts.Count())
                .Select(c => new
                {
                    category = c.Name,
                    productsCount = c.CategoriesProducts.Count(),
                    averagePrice = c.CategoriesProducts
                        .Average(c => c.Product.Price)
                        .ToString("F2"),
                    totalRevenue = c.CategoriesProducts
                        .Sum(c => c.Product.Price)
                        .ToString("F2")
                })
                .ToList();

            string json = JsonConvert.SerializeObject(categories, Formatting.Indented);

            return json;
        }

        //08.
        public static string GetUsersWithProducts(ProductShopContext context)
        {
            var usersWithProduct = context.Users
                .Where(u => u.ProductsSold.Any(p => p.BuyerId != null))
                .Select(u => new
                {
                    firstName = u.FirstName,
                    lastName = u.LastName,
                    age = u.Age,
                    soldProducts = u.ProductsSold
                        .Where(p => p.BuyerId != null)
                        .Select(p => new
                        {
                            name = p.Name,
                            price = p.Price
                        })
                        .ToArray()
                })
                .OrderByDescending(u => u.soldProducts.Count())
                .ToArray();

            var output = new
            {
                usersCount = usersWithProduct.Count(),
                users = usersWithProduct
                .Select(u => new
                {
                    u.firstName,
                    u.lastName,
                    u.age,
                    soldProducts = new
                    {
                        count = u.soldProducts.Count(),
                        products = u.soldProducts
                    }
                })
            };


               

            string json = JsonConvert.SerializeObject(output, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore
            });

            return json;
        }
    }
}