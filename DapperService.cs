using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Reflection;
using System.Configuration;
namespace carservice
{
    public static class DapperService<T>
    {
        private static IDbConnection connection;
        private static string con = ConfigurationManager.AppSettings.Get("conString");
        private static string tableName = typeof(T).Name;
       
        public  static IEnumerable<T> Get()
        {
            using (connection = new SqlConnection(con))
            {
                connection.Open();
                var obj=connection.Query<T>("select * from "+tableName);
                connection.Close();
                return obj;
            }
        }
        public static IEnumerable<T> Get(string query)
        {
            using (connection = new SqlConnection(con))
            {
                connection.Open();
                var obj = connection.Query<T>(query);
                connection.Close();
                return obj;
            }
        }
        /// <summary>
        /// the obj propery for primary key should be named id or "obj names first char"+_id e.g(c_id)
        /// </summary>
        /// <param name="obj"></param>
        public static void Add(T obj)
        {
            using (connection = new SqlConnection(con))
            {
                connection.Open();
                connection.Execute(InsertQuery(obj),obj);
                connection.Close();
            }
        }
        public static void Add(string query,object obj)
        {
            using (connection = new SqlConnection(con))
            {
                connection.Open();
                connection.Execute(query,obj);
                connection.Close();
            }
        }
        public static T GetProcedure(string procedureName )
        {
            using (connection = new SqlConnection(con))
            {
                connection.Open();
                var obj = connection.Query<T>(procedureName,CommandType.StoredProcedure).SingleOrDefault();
                connection.Close();
                return obj;
            }
        }
        public static T GetProcedure(string procedureName, object parameters)
        {
            using (connection = new SqlConnection(con))
            {
                connection.Open();
                var obj = connection.Query<T>(procedureName,parameters,null,false,null,CommandType.StoredProcedure).SingleOrDefault();
                connection.Close();
                return obj;
            }
        }
        private static List<PropertyInfo> GetProperties(T obj)
        {
            Type type = obj.GetType();
            List<PropertyInfo> properties = new List<PropertyInfo>(type.GetProperties());
            if (properties != null)
            {
                return properties;
            }
            return null;
        }
        private static string InsertQuery(T obj)
        {
            List<PropertyInfo> properties = GetProperties(obj);
            if (properties != null)
            {
                string query = String.Format("INSERT INTO {0} VALUES ()", tableName);
                PropertyInfo lastProperty = properties.Last();
                foreach (PropertyInfo pr in properties)
                {
                    int set=query.Length-1;
                    string separator = pr.Equals(lastProperty)? "" : ",";
                    if (!(tableName[0].ToString() + "_id").Equals(pr.Name) && !pr.Name.Equals("id") )
                    {
                        query = query.Insert(set, "@"+pr.Name+separator);
                    }
                   
                }
                return query;
            }
            return null;
        }

    }
}
