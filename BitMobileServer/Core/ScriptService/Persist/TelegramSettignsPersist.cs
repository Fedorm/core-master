using System;
using System.Data;
using System.Data.SqlClient;
using Telegram;

namespace ScriptService.Persist
{
    class TelegramSettignsPersist : ITelegramPersist
    {
        private readonly string _connectionString;

        public TelegramSettignsPersist(string connectionString)
        {
            _connectionString = connectionString;

        }

        public void Save(string phone, byte[] authKey, long serverSalt)
        {
            using (var c = new SqlConnection(_connectionString))
            using (var command = new SqlCommand("[admin].SaveTelegramSettings", c))
            {
                c.Open();

                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add("@Phone", SqlDbType.VarChar, 10);
                command.Parameters["@Phone"].Value = phone;
                command.Parameters.Add("@AuthKey", SqlDbType.VarChar, 4000);
                command.Parameters["@AuthKey"].Value = BytesToString(authKey);
                command.Parameters.Add("@ServerSalt", SqlDbType.BigInt);
                command.Parameters["@ServerSalt"].Value = serverSalt;

                command.ExecuteNonQuery();
            }
        }

        public bool TryGet(string phone, out byte[] authKey, out long serverSalt)
        {
            using (var c = new SqlConnection(_connectionString))
            using (var command = new SqlCommand("SELECT AuthKey, ServerSalt FROM [admin].Telegram WHERE Phone=@phone", c))
            {
                c.Open();

                command.Parameters.Add("@Phone", SqlDbType.VarChar, 10);
                command.Parameters["@Phone"].Value = phone;

                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    authKey = StringToBytes(reader.GetString(0));
                    serverSalt = reader.GetInt64(1);
                    return true;
                }
                authKey = null;
                serverSalt = 0;
                return false;
            }
        }

        private string BytesToString(byte[] bytes)
        {
            return Convert.ToBase64String(bytes);
        }

        private byte[] StringToBytes(string base64String)
        {
            return Convert.FromBase64String(base64String);
        }
    }
}
