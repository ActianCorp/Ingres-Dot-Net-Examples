using System;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;

/*
 * The String data type in .NET applications is Unicode based (UTF-16 encoding).
 * This ODBC.NET C# sample program demonstrates how to INSERT Strings
 * into an Ingres varchar datatype.  The application assumes that the
 * String value contains only values in the subset of ASCII characters
 * within the Unicode universe of characters.  If the full range of 
 * Unicode values are needed in the database then the Ingres database
 * should be created with a "createdb -n" option and nchar, nvarchar, and
 * long nvarchar Ingres data types should be used.
 *
 * The crux of the example is to set OdbcParameter.OdbcType to 
 * OdbcType.VarChar, rather than the default of OdbcType.NChar.
 * Internally, the Microsoft .NET Data Provider for ODBC (ODBC.NET)
 * will issue an SQLBindParameter(...,SQL_C_WCHAR, SQL_VARCHAR,...)
 * to the Ingres ODBC Driver.  The driver will convert the Unicode string
 * value to a multi-byte string and pass it the DBMS for insertion
 * into the Ingres varchar column.
 * 
 * To connect to your own DSN, modify the connection string with your
 * own value of "DSN=myDSN".
 *
 */

namespace MyNamespace
{
class OdbcNetExample
{
	static void Main(string[] args)
	{
		try
		{
			String connstring = "DSN=myDSN";

			TestODBCVarCharCode(connstring);

			Console.WriteLine();
			Console.WriteLine("Success!!");
		}

		catch (DbException ex)
		{
			Console.WriteLine();
			Console.WriteLine(ex.GetType().ToString() + " Message: {0}\n", ex.Message);
			Console.WriteLine("TargetSite: " +
				(ex.TargetSite != null ? ex.TargetSite.ToString() : "<null>"));
			PrintException(ex);
			Console.WriteLine("StackTrace:\n" + ex.StackTrace);
			Console.WriteLine();
		}  // end catch (DbException ex)

		catch (Exception ex)
		{
			Console.WriteLine();
			Console.WriteLine(ex.GetType().ToString() + " Message: {0}\n", ex.Message);
			PrintException(ex);
			Console.WriteLine("StackTrace:\n" + ex.StackTrace);
			Console.WriteLine();
		}  // end catch (Exception ex)

		finally
		{
			Console.WriteLine("\nPress Enter to exit...");
			Console.ReadLine();  //wait for input before application exit
		}

	}  // end Main


	static void TestODBCVarCharCode(String connstring)
	{
		Console.WriteLine("\nTestODBCVarCharCode...\n");

		using (OdbcConnection conn = new OdbcConnection(connstring))
		{
			OdbcCommand command;
			string sql;

			conn.Open();

			try
			{
				sql = "drop table chardata";
				command = new OdbcCommand(sql, conn);
				command.ExecuteNonQuery();
				command.Dispose();
			}
			catch { }  // ignore if table does not exist

			sql = "create table chardata (" +
				"a_integer integer not null primary key, " +
				"a_smallint smallint not null, " +
				"a_varchar_20 varchar(20) not null, " +
				"a_long_varchar long varchar not null)";
			command = new OdbcCommand(sql, conn);
			command.ExecuteNonQuery();
			command.Dispose();

			sql = "insert into chardata (a_integer, a_smallint,a_varchar_20, a_long_varchar) " +
				"values (1,1, ?, ?)";

			OdbcParameter p1 = new OdbcParameter("p1", OdbcType.VarChar);
			p1.Value = "abc";

			OdbcParameter p2 = new OdbcParameter("p2", OdbcType.VarChar);
			p2.Value = "xyz";

			command = new OdbcCommand(sql, conn);
			command.Parameters.Add(p1);
			command.Parameters.Add(p2);
			command.ExecuteNonQuery();
		}  // end using OdbcConnection
	}



	/// <summary>
	/// Write out debugging information from the Exception.
	/// </summary>
	/// <param name="ex"></param>
	static void PrintException(Exception ex)
	{
		Console.WriteLine(ex.GetType().ToString() + ": " + ex.Message);
		Console.WriteLine("\t" + "HelpLink       = " +
			(ex.HelpLink != null ? ex.HelpLink.ToString() : "<null>"));
		Console.WriteLine("\t" + "Source = " +
			(ex.Source != null ? ex.Source.ToString() : "<null>"));
		Console.WriteLine("\t" + "TargetSite = " +
			(ex.TargetSite != null ? ex.TargetSite.ToString() : "<null>"));
		PrintInnerException(ex, 1);
		Console.WriteLine();
	}  // end PrintException


	/// <summary>
	/// Write out debugging information from the inner Exception.
	/// </summary>
	/// <param name="ex"></param>
	/// <param name="depth"></param>
	static void PrintInnerException(Exception ex, int depth)
	{
		if (ex == null || depth > 8)
			return;

		string tabs = "";
		for (int i = 0; i < depth; i++)
			tabs += "\t";

		Console.WriteLine(tabs + "InnerException (" + ex.GetType().ToString() + ")= " +
			(ex.InnerException != null ? ex.InnerException.ToString() : "<null>"));

		if (ex.InnerException != null)
			PrintInnerException(ex.InnerException, depth + 1);
	}  // end PrintInnerException


	/// <summary>
	/// Compare actual string results against expected string results.
	/// </summary>
	/// <param name="title"></param>
	/// <param name="actual"></param>
	/// <param name="expected"></param>
	static void Expect(string title, string actual, string expected)
	{
		if (expected != actual)
			Console.WriteLine("UNEXPECTED: " + title +
				" actual: \"" + actual + "\"" +
				" expected: \"" + expected + "\"\n");
	}


	/// <summary>
	/// 		/// Compare actual integer results against expected integer results.
	/// </summary>
	/// <param name="title"></param>
	/// <param name="actual"></param>
	/// <param name="expected"></param>
	static void Expect(string title, int actual, int expected)
	{
		if (expected != actual)
			Console.WriteLine("UNEXPECTED: " + title +
				" actual: \"" + actual + "\"" +
				" expected: \"" + expected + "\"\n");
	}

}  // end class Program
}  // end namespace