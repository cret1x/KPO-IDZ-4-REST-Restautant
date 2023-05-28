namespace RestRestaurant
{
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Status { get; set; }
        public string SpecialRequests { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        ICollection<Dish> Dishes { get; set; }

        public Order(int id, int user_id, string status, string special_requests, DateTime created_at, DateTime updated_at, ICollection<Dish> dishes)
        {
            Id = id;
            UserId = user_id;
            Status = status;    
            SpecialRequests = special_requests;
            CreatedAt = created_at;
            UpdatedAt = updated_at;
            Dishes = dishes;
        }
    }
}
