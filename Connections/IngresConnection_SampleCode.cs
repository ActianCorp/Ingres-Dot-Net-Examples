/*
 * This simple example shows how to connect to Ingres using the Ingres .NET Data Provider. 
 * The connection string defaults to Port=II7 and to the current user credentials. 
 */

using Ingres.Client;

void SampleCode()
{
	string myConnectionString =
		"Host=myserver; database=demodb";

	IngresConnection conn = new IngresConnection(myConnectionString);
	conn.Open();

//	<do work>

	conn.Close();
}
