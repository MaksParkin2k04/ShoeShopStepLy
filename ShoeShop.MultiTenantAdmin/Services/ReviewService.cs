using Microsoft.Data.SqlClient;
using ShoeShop.MultiTenantAdmin.Models;
using ShoeShop.MultiTenantAdmin.Data;

namespace ShoeShop.MultiTenantAdmin.Services {
    public class ReviewService {
        private readonly string _connectionString;

        public ReviewService(IConfiguration configuration) {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException();
        }

        public async Task<bool> AddReviewAsync(Guid productId, string userId, int rating, string comment) {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
                INSERT INTO ProductReviews (Id, ProductId, UserId, Rating, Comment, CreatedAt)
                VALUES (@Id, @ProductId, @UserId, @Rating, @Comment, @CreatedAt)";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", Guid.NewGuid());
            command.Parameters.AddWithValue("@ProductId", productId);
            command.Parameters.AddWithValue("@UserId", userId);
            command.Parameters.AddWithValue("@Rating", rating);
            command.Parameters.AddWithValue("@Comment", comment);
            command.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

            return await command.ExecuteNonQueryAsync() > 0;
        }

        public async Task<List<ProductReview>> GetProductReviewsAsync(Guid productId) {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
                SELECT r.Id, r.ProductId, r.UserId, r.Rating, r.Comment, r.CreatedAt, u.UserName
                FROM ProductReviews r
                LEFT JOIN AspNetUsers u ON r.UserId = u.Id
                WHERE r.ProductId = @ProductId
                ORDER BY r.CreatedAt DESC";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@ProductId", productId);

            var reviews = new List<ProductReview>();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync()) {
                reviews.Add(new ProductReview {
                    Id = reader.GetGuid(0),
                    ProductId = reader.GetGuid(1),
                    UserId = reader.GetString(2),
                    Rating = reader.GetInt32(3),
                    Comment = reader.IsDBNull(4) ? "" : reader.GetString(4),
                    CreatedAt = reader.GetDateTime(5),
                    User = new ApplicationUser { UserName = reader.IsDBNull(6) ? "Аноним" : reader.GetString(6) }
                });
            }
            return reviews;
        }

        public async Task<double> GetAverageRatingAsync(Guid productId) {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = "SELECT AVG(CAST(Rating AS FLOAT)) FROM ProductReviews WHERE ProductId = @ProductId";
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@ProductId", productId);

            var result = await command.ExecuteScalarAsync();
            return result == DBNull.Value ? 0 : Convert.ToDouble(result);
        }

        public async Task<int> GetReviewCountAsync(Guid productId) {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = "SELECT COUNT(*) FROM ProductReviews WHERE ProductId = @ProductId";
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@ProductId", productId);

            return (int)await command.ExecuteScalarAsync();
        }

        public async Task<bool> AddAdminReplyAsync(Guid reviewId, string adminId, string reply) {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
                INSERT INTO ReviewReplies (Id, ReviewId, AdminId, Reply, CreatedAt)
                VALUES (@Id, @ReviewId, @AdminId, @Reply, @CreatedAt)";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", Guid.NewGuid());
            command.Parameters.AddWithValue("@ReviewId", reviewId);
            command.Parameters.AddWithValue("@AdminId", adminId);
            command.Parameters.AddWithValue("@Reply", reply);
            command.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

            return await command.ExecuteNonQueryAsync() > 0;
        }

        public async Task<ReviewReply?> GetReviewReplyAsync(Guid reviewId) {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
                SELECT r.Id, r.ReviewId, r.AdminId, r.Reply, r.CreatedAt, u.UserName
                FROM ReviewReplies r
                LEFT JOIN AspNetUsers u ON r.AdminId = u.Id
                WHERE r.ReviewId = @ReviewId";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@ReviewId", reviewId);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync()) {
                return new ReviewReply {
                    Id = reader.GetGuid(0),
                    ReviewId = reader.GetGuid(1),
                    AdminId = reader.GetString(2),
                    Reply = reader.GetString(3),
                    CreatedAt = reader.GetDateTime(4),
                    Admin = new ApplicationUser { UserName = reader.IsDBNull(5) ? "Администратор" : reader.GetString(5) }
                };
            }
            return null;
        }

        public async Task<bool> CanUserReviewProductAsync(string userId, Guid productId) {
            if (!Guid.TryParse(userId, out var customerGuid)) {
                return false;
            }
            
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            // Проверяем оплаченные заказы (статус >= Paid)
            var query = @"
                SELECT COUNT(*)
                FROM Orders o
                INNER JOIN OrderDetails od ON o.Id = od.OrderId
                WHERE o.CustomerId = @CustomerId 
                AND od.ProductId = @ProductId 
                AND o.Status >= 1";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@CustomerId", customerGuid);
            command.Parameters.AddWithValue("@ProductId", productId);

            var count = (int)await command.ExecuteScalarAsync();
            return count > 0;
        }
    }
}
