using Microsoft.Data.Sqlite;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace RestRestaurant
{
    public class DatabaseManager
    {
        private static DatabaseManager instance;

        private DatabaseManager() { }

        public static DatabaseManager GetInstance()
        {
            if (instance == null)
            {
                instance = new DatabaseManager();
            }
            return instance;
        }

        /// <summary>
        /// Creates new order in DB
        /// </summary>
        /// <param name="uid">user id</param>
        /// <param name="special_requests">special requests</param>
        /// <param name="dishes">dishes</param>
        public void CreateOrder(int uid, string special_requests, ICollection<Dish> dishes)
        {
            DateTime now = DateTime.Now;
            using (var connection = new SqliteConnection("Data Source=usersdata.db"))
            {
                connection.Open();
                SqliteCommand command = new SqliteCommand();
                command.Connection = connection;
                command.CommandText = $"INSERT OR REPLACE INTO orders (id, user_id, status, special_requests, created_at, updated_at) VALUES ((SELECT id FROM orders WHERE user_id = {uid}), {uid}, 'pending', '{special_requests}', '{now}', '{now}');";
                int oid = command.ExecuteNonQuery();
                foreach (var dish in dishes)
                {
                    command.Connection = connection;
                    command.CommandText = $"INSERT INTO orders_dish (order_id, dish_id, quantity, price) VALUES ({oid}, {dish.Id}, {dish.Quantity}, {dish.Price * dish.Quantity});";
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Returns order from Db by id
        /// </summary>
        /// <param name="id">Order id</param>
        /// <returns>Order?</returns>
        public Order? GetOrder(int id)
        {
            using (var connection = new SqliteConnection("Data Source=usersdata.db"))
            {
                connection.Open();
                SqliteCommand command = new SqliteCommand();
                command.Connection = connection;
                command.CommandText = $"SELECT * FROM orders WHERE id={id}";
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    int user_id = Convert.ToInt32(reader["user_id"]);
                    string status = JsonSerializer.Serialize(reader["status"]);
                    string special_requests = JsonSerializer.Serialize(reader["special_requests"]);
                    string created_at = JsonSerializer.Serialize(reader["created_at"]);
                    string updated_at = JsonSerializer.Serialize(reader["updated_at"]);
                    reader.Close();
                    return new Order(id, user_id, status, special_requests, DateTime.Parse(created_at), DateTime.Parse(updated_at), null);
                }
                
                return null;
            }
        }

        /// <summary>
        /// Calc hash
        /// </summary>
        /// <param name="inputString">String</param>
        /// <returns>Hash</returns>
        public static byte[] GetHash(string inputString)
        {
            using (HashAlgorithm algorithm = SHA256.Create())
                return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        /// <summary>
        /// Calc hash
        /// </summary>
        /// <param name="inputString">String</param>
        /// <returns>Hash</returns>
        public static string GetHashString(string inputString)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetHash(inputString))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }

        /// <summary>
        /// Creates all tables
        /// </summary>
        public void CreateTables()
        {
            using (var connection = new SqliteConnection("Data Source=usersdata.db"))
            {
                connection.Open();
                SqliteCommand command = new SqliteCommand();
                command.Connection = connection;
                command.CommandText = "CREATE TABLE IF NOT EXISTS users (id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,username TEXT NOT NULL UNIQUE,email TEXT NOT NULL UNIQUE,password_hash TEXT NOT NULL,role TEXT NOT NULL, created_at TIMESTAMP, updated_at TIMESTAMP);";
                command.ExecuteNonQuery();
                command.CommandText = "CREATE TABLE IF NOT EXISTS session (id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,user_id INTEGER NOT NULL,session_token TEXT NOT NULL,expires_at TIMESTAMP NOT NULL,FOREIGN KEY (user_id) REFERENCES users(id));";
                command.ExecuteNonQuery();
                command.CommandText = "CREATE TABLE IF NOT EXISTS dish (id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,name TEXT NOT NULL,description TEXT,price DECIMAL(10, 2) NOT NULL,quantity INTEGER NOT NULL, is_available BOOLEAN NOT NULL, created_at TIMESTAMP,updated_at TIMESTAMP);";
                command.ExecuteNonQuery();
                command.CommandText = "CREATE TABLE IF NOT EXISTS orders (id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE, user_id INTEGER NOT NULL, status TEXT NOT NULL, special_requests TEXT, created_at TIMESTAMP, updated_at TIMESTAMP, FOREIGN KEY (user_id) REFERENCES users(id));";
                command.ExecuteNonQuery();
                command.CommandText = "CREATE TABLE IF NOT EXISTS orders_dish (id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE, order_id INTEGER NOT NULL, dish_id INTEGER NOT NULL, quantity INTEGER NOT NULL, price DECIMAL(10, 2) NOT NULL, FOREIGN KEY (order_id) REFERENCES orders(id), FOREIGN KEY (dish_id) REFERENCES dish(id));";
                command.ExecuteNonQuery();

                command.CommandText = $"INSERT INTO dish (name, description, price, quantity, is_available) VALUES ('Soup', 'desc', 100, 2, true);";
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Creates new user
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="email">Elail</param>
        /// <param name="password">Password</param>
        public void CreateUser(string name, string email, string password)
        {
            string passHash = GetHashString(password);
            DateTime now = DateTime.Now;
            using (var connection = new SqliteConnection("Data Source=usersdata.db"))
            {
                connection.Open();
                SqliteCommand command = new SqliteCommand();
                command.Connection = connection;
                command.CommandText = $"INSERT INTO users (username, email, password_hash, role, created_at, updated_at) VALUES ('{name}', '{email}', '{passHash}', 'customer', '{now}', '{now}')";
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Check user pass
        /// </summary>
        /// <param name="email">Email</param>
        /// <param name="password">Password</param>
        /// <returns></returns>
        public int CheckUser(string email, string password)
        {
            string passHash = GetHashString(password);
            using (var connection = new SqliteConnection("Data Source=usersdata.db"))
            {
                connection.Open();
                SqliteCommand command = new SqliteCommand();
                command.Connection = connection;
                command.CommandText = $"SELECT id FROM users WHERE email = '{email}' AND password_hash = '{passHash}'";
                int id = Convert.ToInt32(command.ExecuteScalar());
                Console.WriteLine(id);
                return id;
            }
        }

        /// <summary>
        /// Checks if user exists
        /// </summary>
        /// <param name="name">Usernname</param>
        /// <param name="email">Email</param>
        /// <returns>Exists</returns>
        public bool CheckUserEmail(string name, string email)
        {
            using (var connection = new SqliteConnection("Data Source=usersdata.db"))
            {
                connection.Open();
                SqliteCommand command = new SqliteCommand();
                command.Connection = connection;
                command.CommandText = $"SELECT count(*) FROM users WHERE email = '{email}' OR username = '{name}'";
                int count = Convert.ToInt32(command.ExecuteScalar());
                return count > 0;
            }
        }

        public void UpdateToken(string token, int uid, DateTime expires)
        {
            using (var connection = new SqliteConnection("Data Source=usersdata.db"))
            {
                connection.Open();
                SqliteCommand command = new SqliteCommand();
                command.Connection = connection;
                command.CommandText = $"INSERT OR REPLACE INTO session (id, user_id, session_token, expires_at) VALUES ((SELECT id FROM session WHERE user_id = {uid}), {uid}, '{token}', '{expires}');";
                command.ExecuteNonQuery();
            }
        }

        public User? GetUserByToken(string token)
        {
            using (var connection = new SqliteConnection("Data Source=usersdata.db"))
            {
                connection.Open();
                SqliteCommand command = new SqliteCommand();
                command.Connection = connection;
                command.CommandText = $"SELECT user_id FROM session WHERE session_token='{token}'";
                int uid = Convert.ToInt32(command.ExecuteScalar());
                if (uid > 0)
                {
                    command.CommandText = $"SELECT * FROM users WHERE id={uid}";
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        int id = Convert.ToInt32(reader["id"]);
                        string name = JsonSerializer.Serialize(reader["username"]);
                        string email = JsonSerializer.Serialize(reader["email"]);
                        string password = JsonSerializer.Serialize(reader["password_hash"]);
                        string role = JsonSerializer.Serialize(reader["role"]);
                        reader.Close();
                        return new User(id, name, email, password, role);
                    }
                }
                return null;
            }
        }


    }
}
