#define INGRES

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

using Ingres.Client;


namespace BooleanTest
{
	class DotNETBooleanTestProgram
	{

		static string regressionTestServer   = "myserver";
		static string regressionTestDatabase = "mydatabase";
		static string regressionTestUserID   = "myuserid";
		static string regressionTestPassword = "mypassword";

		static bool pauseAtEnd = true;


		static void Main(string[] args)
		{
			try
			{
				TestBoolean(
					" server=" + regressionTestServer +
					";db    =" + regressionTestDatabase +
					";userid=" + regressionTestUserID +
					";pwd   =" + regressionTestPassword + ";");


				Console.WriteLine();
				Console.WriteLine("Success!!");
				String instring2;
				if (pauseAtEnd)
				{
					Console.WriteLine("\nPress Enter to exit...");
					instring2 = Console.ReadLine();  //wait for input before close
				}
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
				//PrintErrorCollection(ex.Errors);
				Console.WriteLine("\nPress Enter to exit...");
				String instring = Console.ReadLine();  //wait for input before close
			}
			catch (Exception ex)
			{
				Console.WriteLine();
				Console.WriteLine(ex.GetType().ToString() + " Message: {0}\n", ex.Message);
				PrintException(ex);
				Console.WriteLine("StackTrace:\n" + ex.StackTrace);
				Console.WriteLine();
				Console.WriteLine("\nPress Enter to exit...");
				String instring = Console.ReadLine();  //wait for input before close
			}

		}  // Main

		/// <summary>
		/// Regression test on BOOLEAN data type
		/// </summary>
		/// <param name="connstring"></param>
		static void TestBoolean(string connstring)
		{
			IngresCommand cmd;
			IngresDataReader rdr;

			string title;
			DataTable dt, dtBool;
			string[][] tests;
			string tablename = "regressionbooleantesttable";
			string colname = "mybooleancolumn";

			Console.WriteLine("Test BOOLEAN Data Type\n");

			using (IngresConnection conn = new IngresConnection(connstring))
			{
				conn.Open();

				//Console.WriteLine("GetSchema(\"DataTypes\");");
				dt = conn.GetSchema(
					DbMetaDataCollectionNames.DataTypes);
				//    String[] tab3cols = new String[] {
				//    "TypeName",
				//    "ProviderDbType",
				//    "ColumnSize",
				//    "CreateFormat",
				//    "CreateParameters",
				//    "DataType",
				//    "MinimumScale",
				//    "MaximumScale",
				//    "IsLiteralSupported",
				//    "LiteralPrefix",
				//    "LiteralSuffix"
				//};
				//PrintDataTable(dt,tab3cols);

				dtBool = new DataTable("GetSchema_DataTypes_Boolean");
				foreach (DataColumn col in dt.Columns)  // copy col metadate of dt
				{
					dtBool.Columns.Add(
						new DataColumn(col.ColumnName, col.DataType));
				}  // end foreach (DataColumn col in dt.Columns)


				foreach (DataRow row in dt.Rows)
				{
					if (row.ItemArray[0].ToString().ToLowerInvariant()
						== "boolean")
						dtBool.Rows.Add(row.ItemArray);
				}  // end foreach (DataRow row in dt.Rows)

				if (dtBool.Rows.Count == 0)
				{
					Console.WriteLine("\tDBMS does not support BOOLEAN data type.");
					return;
				}

				//PrintDataTable(dtBool);

				title = "\n";
				tests = new string[][]
				{
					new string[]{"TypeName",              "boolean"},
					new string[]{"ProviderDbType",        "3"},
					new string[]{"ColumnSize",            "1"},
					new string[]{"CreateFormat",          "boolean"},
					new string[]{"CreateParameters",      "<DBNull>"},
					new string[]{"DataType",              "system.boolean"},
					new string[]{"IsAutoIncrementable",   "false"},
					new string[]{"IsBestMatch",           "true"},
					new string[]{"IsCaseSensitive",       "false"},
					new string[]{"IsFixedLength",         "true"},
					new string[]{"IsFixedPrecisionScale", "false"},
					new string[]{"IsLong",                "false"},
					new string[]{"IsNullable",            "true"},
					new string[]{"IsSearchable",          "true"},
					new string[]{"IsSearchableWithLike",  "false"},
					new string[]{"IsUnsigned",            "<DBNull>"},
					new string[]{"MaximumScale",          "<DBNull>"},
					new string[]{"MinimumScale",          "<DBNull>"},
					new string[]{"IsConcurrencyType",     "<DBNull>"},
					new string[]{"IsLiteralSupported",    "true"},
					new string[]{"LiteralPrefix",         "<DBNull>"},
					new string[]{"LiteralSuffix",         "<DBNull>"}
				};

				DiffOnRow(title + "    GetSchema(\"DataTypes\") (boolean)...\n",
					dtBool, 0, tests);  // compare


				try
				{
					cmd = conn.CreateCommand();
					cmd.CommandText = "drop table " + tablename;
					cmd.ExecuteNonQuery();
				}
				catch (Exception) { }  // ignore table not exist

				cmd = conn.CreateCommand();
				cmd.CommandText = "create table " + tablename +
					" (mykey integer not null primary key, " +
					colname + " boolean default true)";
				cmd.ExecuteNonQuery();

				cmd = conn.CreateCommand();
				cmd.CommandText = "insert into " + tablename +
					" values (1, true)";
				cmd.ExecuteNonQuery();

				cmd = conn.CreateCommand();
				cmd.CommandText = "insert into " + tablename +
					" values (2, false)";
				cmd.ExecuteNonQuery();

				cmd = conn.CreateCommand();
				cmd.CommandText = "insert into " + tablename +
					" values (3, unknown)";
				cmd.ExecuteNonQuery();

				bool myvalue = true;

				myvalue = true;
				cmd = conn.CreateCommand();
				cmd.CommandText = "insert into " + tablename +
					" values (11, ?)";
				cmd.Parameters.Add(new IngresParameter("mybool", IngresType.Boolean));
				cmd.Parameters[0].Value = myvalue;
				cmd.ExecuteNonQuery();

				myvalue = false;
				cmd = conn.CreateCommand();
				cmd.CommandText = "insert into " + tablename +
					" values (12, ?)";
				cmd.Parameters.Add(new IngresParameter("mybool", IngresType.VarChar));
				cmd.Parameters[0].Value = "false";
				cmd.ExecuteNonQuery();

				cmd = conn.CreateCommand();
				cmd.CommandText = "insert into " + tablename +
					" values (13, ?)";
				cmd.Parameters.Add(new IngresParameter("mybool", DbType.Boolean));
				cmd.Parameters[0].Value = DBNull.Value;
				cmd.ExecuteNonQuery();

				//Console.WriteLine("GetSchema(\"Columns\", {null, null, \""+tablename+"\", \""+colname+"\"});");
				dt = conn.GetSchema(
					"Columns", new string[4] { null, null, tablename, colname });
				//PrintDataTable(dt);

				title = "\n";
				tests = new string[][]
				{
					new string[]{"TABLE_CATALOG",           "<DBNull>"},
//					new string[]{"TABLE_SCHEMA",            "thoda04"},
					new string[]{"TABLE_NAME",              tablename},
					new string[]{"COLUMN_NAME",             colname},
					new string[]{"ORDINAL_POSITION",        "2"},
					new string[]{"COLUMN_DEFAULT",          "true"},
					new string[]{"IS_NULLABLE",             "yes"},
					new string[]{"DATA_TYPE",               "boolean"},
					new string[]{"CHARACTER_MAXIMUM_LENGTH","<DBNull>"},
					new string[]{"CHARACTER_OCTET_LENGTH",  "<DBNull>"},
					new string[]{"NUMERIC_PRECISION",       "<DBNull>"},
					new string[]{"NUMERIC_PRECISION_RADIX", "<DBNull>"},
					new string[]{"NUMERIC_SCALE",           "<DBNull>"},
					new string[]{"DATETIME_PRECISION",      "<DBNull>"}
				};

				DiffOnRow(title + "    GetSchema(\"Columns\") (boolean)...\n",
					dt, 0, tests);  // compare


				//Console.WriteLine("rdr.GetSchemaTable()");
				cmd = conn.CreateCommand();
				cmd.CommandText = "select mykey, " + colname + " from " + tablename +
					" ORDER BY mykey ";
				rdr = cmd.ExecuteReader();
				rdr.Close();

				cmd = conn.CreateCommand();
				cmd.CommandText = "select mykey, " + colname + " from " + tablename +
					" ORDER BY mykey ";
				rdr = cmd.ExecuteReader(CommandBehavior.KeyInfo);

				dt = rdr.GetSchemaTable();
				//PrintDataTable(dt);

				string s = "";
				Boolean sw;

				for (int i = 0; i < 6; i++)
				{
					if (rdr.Read() == false)
					{
						Console.WriteLine("   Premature EOF on reading rows!" +
							"  Rows read = " + i.ToString());
						break;
					}

					Int32 mykey = rdr.GetInt32(0);

					if (rdr.IsDBNull(1))
						s = "<null>";
					else
					{
						if (i % 2 == 0)
						{
							sw = rdr.GetBoolean(1);
							s = sw.ToString();
						}
						else
						{
							object obj = rdr.GetValue(1);
							s = obj.ToString();
						}
					}

					string expected = "Row " + mykey.ToString() + " is ";
					string actual = "Row " + mykey.ToString() + " is " + s;
					switch (mykey)
					{
						case 1:
							Expect("    GetBoolean", actual, expected + "True");
							break;
						case 2:
							Expect("    GetBoolean", actual, expected + "False");
							break;
						case 3:
							Expect("    GetBoolean", actual, expected + "<null>");
							break;
						case 11:
							Expect("    GetBoolean", actual, expected + "True");
							break;
						case 12:
							Expect("    GetBoolean", actual, expected + "False");
							break;
						case 13:
							Expect("    GetBoolean", actual, expected + "<null>");
							break;
						default:
							Console.WriteLine("    Unexpected row read.  Mykey = " + mykey.ToString());
							break;
					}
				}  // end for loop

				if (rdr.Read() == true)
				{
					Console.WriteLine("   Too many boolean rows in result-set!");
				}

				rdr.Close();

			}  // end using (IngresConnection conn)
		}  // end TestBoolean(string connstring)

		/// <summary>
		/// Compare differences between the row in a DataTable and 
		/// the expected values in the tests string array.
		/// </summary>
		/// <param name="title">String with a title for the diff report.</param>
		/// <param name="dt">DataTable with the actual values.</param>
		/// <param name="rowNumber">Row number in the DataTable to test.</param>
		/// <param name="tests">Array of strings with expected values.</param>
		/// <returns></returns>
		static bool DiffOnRow(
			string title, DataTable dt, int rowNumber, string[][] tests)
		{
			string s;
			bool diffOccurred = false;

			for (int i = 0; i < tests.Length; i++)
			{
				if (dt.Rows[rowNumber][tests[i][0]] == DBNull.Value)
					s = "<DBNull>";
				else
					s = dt.Rows[rowNumber][tests[i][0]].ToString().ToLower();
				if (!Expect(title + "\t" + tests[i][0], s, tests[i][1]))
				{
					title = "";
					diffOccurred = true;
				}
			}
			return diffOccurred;
		}

		static bool Expect(string title, int actual, int expected)
		{
			if (expected != actual)
			{
				Console.WriteLine("UNEXPECTED: " + title +
					" actual: \"" + actual + "\"" +
					" expected: \"" + expected + "\"\n");
				return false;
			}
			return true;
		}

		static bool Expect(string title, string actual, string expected)
		{
#if    !(EDBC || INGRES)
			if (title == "ProviderType")
				return true;
#endif
			if (expected != actual)
			{
				Console.WriteLine(title);
				Console.WriteLine("\tUNEXPECTED: " +
					" actual: \"" + actual + "\"" +
					" expected: \"" + expected + "\"\n");
				return false;
			}
			return true;
		}


		static void PrintException(DbException ex)
		{
			Console.Write(ex.GetType().ToString() + ": " + ex.Message + "\n");
			//Console.Write("\t" + "Errors         = " +
			//    (ex.Errors != null ? ex.Errors.ToString() : "<null>") + "\n");
			Console.Write("\t" + "HelpLink       = " +
				(ex.HelpLink != null ? ex.HelpLink.ToString() : "<null>") + "\n");
			Console.Write("\t" + "Source = " +
				(ex.Source != null ? ex.Source.ToString() : "<null>") + "\n");
			Console.Write("\t" + "TargetSite = " +
				(ex.TargetSite != null ? ex.TargetSite.ToString() : "<null>") + "\n");
#if    EDBC
#elif  SQLSERVER
			Console.WriteLine("");
			Console.Write("\t" + "Class = " +
				(                    ex.Class.ToString()) + "\n");
			Console.Write("\t" + "Number = " +
				(                    ex.Number.ToString()) + "\n");
			Console.Write("\t" + "Server = " +
				(ex.Server    !=null?ex.Server.ToString():"<null>") + "\n");
			Console.Write("\t" + "State = " +
				(                    ex.State.ToString()) + "\n");
#elif  OLEDB
#endif
			PrintInnerException(ex, 1);
			Console.WriteLine("");
		}

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
		}

		static void PrintInnerException(Exception ex, int depth)
		{
			if (ex == null || depth > 8)
				return;

			string tabs = "";
			for (int i = 0; i < depth; i++)
				tabs += "\t";

			Console.Write(tabs + "InnerException (" + ex.GetType().ToString() + ")= " +
				(ex.InnerException != null ? ex.InnerException.ToString() : "<null>") + "\n");
			//Console.Write(tabs + "InnerException Source = " +
			//    (ex.Source != null ? ex.Source.ToString() : "<null>") + "\n");
			//Console.Write(tabs + "InnerException TargetSite = " +
			//    (ex.TargetSite != null ? ex.TargetSite.ToString() : "<null>") + "\n");

			if (ex.InnerException != null)
				PrintInnerException(ex.InnerException, depth + 1);
		}

	}  // end class DotNETBooleanTestProgram
}  // end namespace
