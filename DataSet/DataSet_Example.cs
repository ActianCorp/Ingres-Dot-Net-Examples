using System;
using System.Data;
using System.Data.Common;

using Ingres.Client;

class App
{
    static public void Main()
    {
        string myConnectionString =
        "Host=myserver.mycompany.com;" +
        "User Id=myname;PWD=mypass;" +
        "Database=mydatabase";

        using (DbConnection conn = new IngresConnection())
        {
            conn.ConnectionString = myConnectionString;
            conn.Open();   // open the Ingres connection

            string cmdtext =
                "select table_owner, table_name, " +
                " create_date from iitables " +
                " where table_type in ('T','V') and " +
                " table_name not like 'ii%' and" +
                " table_name not like 'II%'";

            DbCommand cmd = conn.CreateCommand();
            cmd.CommandText = cmdtext;


            DataSet ds = new DataSet("my_list_of_tables");

            //  read the data using the DataAdapter method
            DbDataAdapter adapter = new IngresDataAdapter();
            adapter.SelectCommand = cmd;
            adapter.Fill(ds);  // execute the query and fill the dataset

            //  write the dataset to an XML file
            ds.WriteXml("c:/temp/temp.xml");

        }   // close the connection
    }  // end Main()
}  // end class App