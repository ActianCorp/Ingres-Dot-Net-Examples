using System.Data;
using Ingres.Client;

static void ReaderDemo(string connstring)
{
    using (IngresConnection conn = new IngresConnection(connstring))
    {

        string strNumber;
        string strName;
        string strSSN;

        conn.Open();

        IngresCommand cmd = new IngresCommand(
            "select number, name, ssn  from personnel", conn);

        IngresDataReader reader = cmd.ExecuteReader();

        Console.Write(reader.GetName(0) + "\t");
        Console.Write(reader.GetName(1) + "\t");
        Console.Write(reader.GetName(2));
        Console.WriteLine();

        while (reader.Read())
        {
            strNumber= reader.IsDBNull(0)?
                "<none>":reader.GetInt32(0).ToString();
            strName  = reader.IsDBNull(1)?
                "<none>":reader.GetString(1);
            strSSN   = reader.IsDBNull(2)?
                "<none>":reader.GetString(2);

            Console.WriteLine(
                strNumber + "\t" + strName + "\t" + strSSN);
        }  // end while loop through result set

        reader.Close();
    }  // end using (IngresConnection)
}