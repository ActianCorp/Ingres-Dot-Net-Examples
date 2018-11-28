using System;
using System.Data;
using System.Data.Common;
using System.Transactions;
using Ingres.Client;

/*
 * This example demonstrates Ingres .NET Data Provider support for
 * the .NET Framework's System.Transactions.TransactionScope class.
 * The example also shows use of the IngresConnection.EnlistTransaction() method.
 * The example uses two demodb databases to demonstrate a two-phase coordinated
 * commit between multiple databases.
 * 
 * The support is in Ingres release 9.3 and later, in the Ingres
 * .NET Data Provider's Ingres.Client.dll file version 2.1.930.13 and later.
 * Support for client 64-bit Windows platforms requires
 * .NET Data Provider's Ingres.Client.dll file version 2.1.1000.44 or later.
 */

namespace MyNamespace
{
	class TransactionScopeExample
	{
		static void Main(string[] args)
		{
			String connectionString1 =
				"host=(local);database=demodb1";
//				"host=myServer;port=II7;database=demodb1;user id=myUserName;pwd=myPassword";
			String commandText1     = "update country set ct_id = 999 where ct_code = 'GB'";

			String connectionString2 =
				"host=(local);database=demodb2";
//				"host=myServer;port=II7;database=demodb2;user id=myUserName;pwd=myPassword";
			String commandText2     = "update country set ct_id = 999 where ct_code = 'GB'";

			String commandTextReset = "update country set ct_id = 229 where ct_code = 'GB'";

			try
			{
				int i;

				// validate the initial state of the Ingres databases before the test changes it
				i = GetScalarInt(connectionString1, 
					"select ct_id from country where ct_code = 'GB'");
				Expect("Database1 initial state incorrect.", i, 229);
				i = GetScalarInt(connectionString2,
					"select ct_id from country where ct_code = 'GB'");
				Expect("Database2 initial state incorrect.", i, 229);

				Console.WriteLine("\n");
				Console.WriteLine("Testing TransactionScope and marked Complete()...");
				// update the Ingres databases within a Transaction using TransactionScope
				// using a distributed transaction via MSDTC
				ExecuteNonQueryWithinTransactionScope(
					connectionString1,
					connectionString2,
					commandText1,
					commandText2);

				// verify that update did occur
				i = GetScalarInt(connectionString1,
					"select ct_id from country where ct_code = 'GB'");
				Expect("Database1 initial state incorrect.", i, 999);
				i = GetScalarInt(connectionString2,
					"select ct_id from country where ct_code = 'GB'");
				Expect("Database2 initial state incorrect.", i, 999);


				Console.WriteLine("Resetting database back to initial state...");
				ExecuteNonQuery(connectionString1, commandTextReset);
				ExecuteNonQuery(connectionString2, commandTextReset);

				Console.WriteLine("\n");
				Console.WriteLine("Testing TransactionScope with missing Complete()...");
				// update the databases within a Transaction using TransactionScope
				// but don't mark the TransactionScope complete.
				ExecuteNonQueryWithinTransactionScope(
					connectionString1,
					connectionString2,
					commandText1,
					commandText2, false);

				// verify that update did NOT occur
				i = GetScalarInt(connectionString1,
					"select ct_id from country where ct_code = 'GB'");
				Expect("Database1 initial state incorrect.", i, 229);
				i = GetScalarInt(connectionString2,
					"select ct_id from country where ct_code = 'GB'");
				Expect("Database2 initial state incorrect.", i, 229);

				Console.WriteLine("Resetting database back to initial state...");
				ExecuteNonQuery(connectionString1, commandTextReset);
				ExecuteNonQuery(connectionString2, commandTextReset);

				Console.WriteLine("\n");
				Console.WriteLine("Testing TransactionScope manually Enlisted using EnlistTransaction...");
				// update the databases within a Transaction that the application
				// manually enlists in by adding ";enlist=false" to the connection
				// string, calling EnlistTransaction(tx),
				// and issuing TransactionScope.Complete().
				ExecuteNonQueryWithinExplicitTransactionScope(
					connectionString1,
					connectionString2,
					commandText1,
					commandText2, true);

				// verify that update did occur
				i = GetScalarInt(connectionString1,
					"select ct_id from country where ct_code = 'GB'");
				Expect("Database1 initial state incorrect.", i, 999);
				i = GetScalarInt(connectionString2,
					"select ct_id from country where ct_code = 'GB'");
				Expect("Database2 initial state incorrect.", i, 999);


				Console.WriteLine("Resetting database back to initial state...");
				ExecuteNonQuery(connectionString1, commandTextReset);
				ExecuteNonQuery(connectionString2, commandTextReset);

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


		/// <summary>
		/// Connect to two databases while automatically enlisting in the ambient Transaction,
		/// and execute two non-query SQL commands under the TransactionScope.
		/// Commit the Transaction by marking the TransactionScope complete.
		/// </summary>
		/// <param name="connString1">ConnectionString for database 1.</param>
		/// <param name="connString2">ConnectionString for database 2.</param>
		/// <param name="commandText1">Non-query CommandText to execute against database 1.</param>
		/// <param name="commandText2">Non-query CommandText to execute against database 2.</param>
		static void ExecuteNonQueryWithinTransactionScope(
			string connString1,  string connString2,
			string commandText1, string commandText2)
		{
			ExecuteNonQueryWithinTransactionScope(
			connString1, connString2,
			commandText1, commandText2, true);
		}


		/// <summary>
		/// Connect to two databases while automatically enlisting in the ambient Transaction,
		/// and execute two non-query SQL commands under the TransactionScope.
		/// Commit the Transaction by marking the TransactionScope complete,
		/// if issueScopeComplete argument is set true for testing purposes.
		/// </summary>
		/// <param name="connString1">ConnectionString for database 1.</param>
		/// <param name="connString2">ConnectionString for database 2.</param>
		/// <param name="commandText1">Non-query CommandText to execute against database 1.</param>
		/// <param name="commandText2">Non-query CommandText to execute against database 2.</param>
		/// <param name="issueScopeComplete">If true, issue TransactionScope.Complete()</param>
		static void ExecuteNonQueryWithinTransactionScope(
			string connString1,  string connString2,
			string commandText1, string commandText2,
			bool   issueScopeComplete)
		{
			Console.WriteLine("\n\tUpdateUsingTransactionScope...\n");

			using (TransactionScope scope = new TransactionScope())
			{

				using (IngresConnection conn1 = new IngresConnection(connString1))
				{
					using (IngresConnection conn2 = new IngresConnection(connString2))
					{
						Console.WriteLine("\tIngresConnection1.Open()...");
						// Open the connection to the database and 
						// enlist in the ambient Transaction using MSDTC
						conn1.Open();
						Console.WriteLine("\tIngresConnection1.Open() complete\n");

						Console.WriteLine("\tIngresConnection2.Open()...");
						// Open the connection to the database and 
						// enlist in the ambient Transaction using MSDTC
						conn2.Open();
						Console.WriteLine("\tIngresConnection2.Open() complete\n");

						try
						{

							IngresCommand cmd1 = conn1.CreateCommand();
							cmd1.CommandText = commandText1;
							cmd1.ExecuteNonQuery();

							IngresCommand cmd2 = conn2.CreateCommand();
							cmd2.CommandText = commandText2;
							cmd2.ExecuteNonQuery();

							// mark the Transaction complete
							// mark the Transaction complete
							if (issueScopeComplete)  // test debug flag for testing scope.Complete
							{
								Console.WriteLine("\tTransactionScope completing...");
								scope.Complete();
								Console.WriteLine("\tTransactionScope complete\n");
							}
							// note: TransactionScope will not be committed until
							// TransactionScope Dispose() is called.
						}
						catch (Exception ex)
						{
							string s = ex.ToString();
							Console.WriteLine("\n\tApplication throws Exception!!  " + s + "\n");
							throw;
						}
					}  // end using (IngresConnection2)  // closes and disposes conn2
				}  // end using (IngresConnection1)      // closes and disposes conn1

				Console.WriteLine("\tTransactionScope.Dispose()...");
			}  // end using (TransactionScope)  // calls System.Transactions.Dispose() --> System.Transactions.CommittableTransaction.Commit
			Console.WriteLine("\tTransactionScope.Dispose() complete\n");

		}  // end ExecuteNonQueryWithinTransactionScope


		/// <summary>
		/// Connect to two databases while manually enlisting in the ambient Transaction
		/// by appending ";enlist=false" to the connection strings and issuing EnlistTransaction().
		/// Execute two non-query SQL commands under the TransactionScope.
		/// Commit the Transaction by marking the TransactionScope complete,
		/// if issueScopeComplete argument is set true for testing purposes.
		/// </summary>
		/// <param name="connString1">ConnectionString for database 1.</param>
		/// <param name="connString2">ConnectionString for database 2.</param>
		/// <param name="commandText1">Non-query CommandText to execute against database 1.</param>
		/// <param name="commandText2">Non-query CommandText to execute against database 2.</param>
		/// <param name="issueScopeComplete">If true, issue TransactionScope.Complete()</param>
		static void ExecuteNonQueryWithinExplicitTransactionScope(
			string connString1, string connString2,
			string commandText1, string commandText2,
			bool issueScopeComplete)
		{
			Console.WriteLine("\n\tUpdateUsingTransactionScope...\n");

			using (TransactionScope scope = new TransactionScope())
			{

				using (IngresConnection conn1 = 
					new IngresConnection(connString1 + ";enlist=false"))
				{
					using (IngresConnection conn2 = 
						new IngresConnection(connString2 + ";enlist=false"))
					{
						Console.WriteLine("\tIngresConnection1.Open()...");
						// Open the connection to the database and 
						// enlist in the ambient Transaction using MSDTC
						conn1.Open();
						Console.WriteLine("\tIngresConnection1.Open() complete\n");

						Console.WriteLine("\tIngresConnection2.Open()...");
						// Open the connection to the database and 
						// enlist in the ambient Transaction using MSDTC
						conn2.Open();
						Console.WriteLine("\tIngresConnection2.Open() complete\n");

						Console.WriteLine("\tManually enlist in Transaction.Current...");
						// manually enlist in the current ambient transaction;
						Transaction tx = Transaction.Current;
						conn1.EnlistTransaction(tx);
						conn2.EnlistTransaction(tx);
						Console.WriteLine("\tEnlisted.\n");

						try
						{

							IngresCommand cmd1 = conn1.CreateCommand();
							cmd1.CommandText = commandText1;
							cmd1.ExecuteNonQuery();

							IngresCommand cmd2 = conn2.CreateCommand();
							cmd2.CommandText = commandText2;
							cmd2.ExecuteNonQuery();

							// mark the Transaction complete
							if (issueScopeComplete)  // test debug flag for testing scope.Complete
							{
								Console.WriteLine("\tTransactionScope completing...");
								scope.Complete();
								Console.WriteLine("\tTransactionScope complete\n");
							}
							// note: TransactionScope will not be committed until
							// TransactionScope Dispose() is called.
						}
						catch (Exception ex)
						{
							string s = ex.ToString();
							Console.WriteLine("\n\tApplication throws Exception!!  " + s + "\n");
							throw;
						}
					}  // end using (IngresConnection2)  // closes and disposes conn2
				}  // end using (IngresConnection1)      // closes and disposes conn1

				Console.WriteLine("\tTransactionScope.Dispose()...");
			}  // end using (TransactionScope)  // calls System.Transactions.Dispose() --> System.Transactions.CommittableTransaction.Commit
			Console.WriteLine("\tTransactionScope.Dispose() complete\n");

		}  // end ExecuteNonQueryWithinExplicitTransactionScope


		/// <summary>
		/// Open a database and execute a non-query SQL string.
		/// </summary>
		/// <param name="connString"></param>
		/// <param name="commandText"></param>
		static void ExecuteNonQuery(
			string connString, string commandText)
		{
			using (IngresConnection conn = new IngresConnection(connString))
			{
				conn.Open();

				IngresCommand cmd = conn.CreateCommand();
				cmd.CommandText = commandText;
				cmd.ExecuteNonQuery();
			}  // end using (IngresConnection conn)
		}  // end ExecuteNonQuery


		/// <summary>
		/// Open a database, execute a query, and get the first column as an integer.
		/// </summary>
		/// <param name="connString"></param>
		/// <param name="commandText"></param>
		/// <returns></returns>
		static int GetScalarInt(String connString, String commandText)
		{
			using (IngresConnection conn = new IngresConnection(connString))
			{
				conn.Open();

				DbCommand cmd = conn.CreateCommand();
				cmd.CommandText = commandText;

				//          read the data using the DataReader method
				DbDataReader datareader = cmd.ExecuteReader();
				if (datareader.HasRows == false)
					throw new Exception("No rows returned for: " + commandText);

				datareader.Read();  // get the row

				int i = datareader.GetInt32(0);
				return i;
			}
		} // end GetScalarInt



		/// <summary>
		/// Write out debugging information from the Exception.
		/// </summary>
		/// <param name="ex"></param>
		static void PrintException(Exception ex)
		{
			Console.Write(ex.GetType().ToString() + ": " + ex.Message + "\n");
			Console.Write("\t" + "HelpLink       = " +
				(ex.HelpLink != null ? ex.HelpLink.ToString() : "<null>") + "\n");
			Console.Write("\t" + "Source = " +
				(ex.Source != null ? ex.Source.ToString() : "<null>") + "\n");
			Console.Write("\t" + "TargetSite = " +
				(ex.TargetSite != null ? ex.TargetSite.ToString() : "<null>") + "\n");
			PrintInnerException(ex, 1);
			Console.WriteLine("");
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

			Console.Write(tabs + "InnerException (" + ex.GetType().ToString() + ")= " +
				(ex.InnerException != null ? ex.InnerException.ToString() : "<null>") + "\n");

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
}