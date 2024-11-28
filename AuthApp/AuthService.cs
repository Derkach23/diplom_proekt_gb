using Npgsql;
using System.Windows.Controls.Primitives;

public class AuthService
{
    private string connectionString = "Host=localhost;Username=postgres;Password=postgres;Database=docudb";

    public User Authenticate(string username, string password)
    {
        using (var connection = new NpgsqlConnection(connectionString))
        {
            connection.Open();
            using (var command = new NpgsqlCommand("SELECT Id, Username, Password, IsAdmin FROM users WHERE Username = @Username AND Password = @Password", connection))
            {
                command.Parameters.AddWithValue("username", username);
                command.Parameters.AddWithValue("password", password);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new User
                        {
                            Id = reader.GetInt32(0),
                            Username = reader.GetString(1),
                            Password = reader.GetString(2),
                            IsAdmin = !reader.IsDBNull(3) && reader.GetBoolean(3)
                        };
                    }
                }
            }
        }

        return null; // Возвращаем null, если пользователь не найден
    }
}
