using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MySql.Data.MySqlClient;
using System.Data;
using System.Configuration;

namespace BakeHub.Data
{
    public class MySQLContext
    {
        private string connectionString =
            ConfigurationManager.ConnectionStrings["MysqlConnection"].ConnectionString;

        public IDbConnection Connection => new MySqlConnection
            (connectionString);
    }
}