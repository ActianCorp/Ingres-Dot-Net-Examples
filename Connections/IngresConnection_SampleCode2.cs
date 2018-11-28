/*
 * This example uses a better programming practice of specifying 
 * a "using" scope to ensure that the IngresConnection is closed 
 * should an exception occur while doing database work. 
 * The "using" scope behaves like a try/finally block. 
 */

using Ingres.Client;

void SampleCode2()
{
	string myConnectionString =
		"Host=myserver;database=demodb";

	using (IngresConnection conn = new IngresConnection(myConnectionString))
	{
		conn.Open();

//		<do work>

	}  // end using IngresConnection
}
