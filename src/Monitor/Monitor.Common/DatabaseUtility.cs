using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using Common.Logging;

namespace Monitor.Common
{
    public class DatabaseUtility
    {
        private static ILog logger = LogManager.GetCurrentClassLogger();

        public static DataSet Query(string connectionString, string sqlSyntax, string whereClause)
        {
            //Create a SqlConnection to the Northwind database.
            using (SqlConnection connection =
                       new SqlConnection(connectionString))
            {
                DataSet dataSet = new DataSet();
                try
                {
                    //Create a SqlDataAdapter for the Suppliers table.
                    SqlDataAdapter adapter = new SqlDataAdapter();

                    // Open the connection.
                    connection.Open();

                    // Create a SqlCommand to retrieve Suppliers data.
                    SqlCommand command = new SqlCommand(sqlSyntax + whereClause, connection);
                    command.CommandType = CommandType.Text;

                    // Set the SqlDataAdapter's SelectCommand.
                    adapter.SelectCommand = command;

                    // Fill the DataSet.
                    adapter.Fill(dataSet);

                }
                catch (Exception e)
                {
                    logger.Error(e);
                    throw new Exception(e.Message);
                }
                finally
                {
                    // Close the connection.
                    connection.Close();
                }

                return dataSet;
            }
        }
    }
}
