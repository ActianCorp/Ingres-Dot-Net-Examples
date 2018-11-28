using System;
using System.Data;

using Ingres.Client;

public class MyClass
{
   public static void Main()
   {
	try
	{
	    string connection = @"Server=myserver;Port=B87;Database=demodb;User ID=ingres;Password=mypassword";
	    Type typ;
	    typ = typeof(String);
	    TestDbProc(connection,typ);
	}
	catch (Exception ex)
	{
	    Console.WriteLine(ex.Message);
	}
			
   }
	
static void TestDbProc(string connstring, Type type)
{
   int recsaffected;
   string strSeq;
   string strName;

   IngresConnection conn = new IngresConnection(connstring);
   IngresCommand cmd;

   Console.WriteLine("\nTestDbProc (using " + type.ToString()
      +") to database: " + conn.Database);

   conn.Open();

    cmd = new IngresCommand(
       "declare global temporary table session.tab (seq integer,"+
       "tabname varchar(32), tabowner varchar(32), numpages integer)"+
       " on commit preserve rows with norecovery", conn);
    recsaffected = cmd.ExecuteNonQuery();
    Console.WriteLine("ExecuteNonQuery(\"declare gtt\") returned " +
    recsaffected.ToString());

    cmd = new IngresCommand(
       "insert into session.tab (tabname,tabowner,numpages)"+
       " select table_name,table_owner,number_pages from iitables", conn);
    recsaffected = cmd.ExecuteNonQuery();
    Console.WriteLine("ExecuteNonQuery(\"insert into gtt\") returned " +
    recsaffected.ToString());


    cmd = new IngresCommand( 
        "{ call gttproc(gtt1 = session.tab)}", conn);
//        "{ call gttproc(session.tab)}", conn);
    cmd.CommandType = CommandType.Text;
    recsaffected = cmd.ExecuteNonQuery();
    Console.WriteLine("ExecuteNonQuery(\"execute proc\") returned " +
    recsaffected.ToString());
    
    cmd = new IngresCommand(
            "select seq,tabname from session.tab", conn);

        IngresDataReader reader = cmd.ExecuteReader();

        Console.Write(reader.GetName(0) + "\t");
        Console.Write(reader.GetName(1));
        Console.WriteLine();

        while (reader.Read())
        {
            strSeq= reader.IsDBNull(0)?
                "<none>":reader.GetInt32(0).ToString();
            strName  = reader.IsDBNull(1)?
                "<none>":reader.GetString(1);

            Console.WriteLine( strSeq + "\t" + strName );
        }  // end while loop through result set

        reader.Close();


    
    conn.Close();
} 
}

