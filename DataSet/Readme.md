# .Net DataSet

This simple example shows how to access Ingres data and fill a DataSet using the IngresDataAdapter class of the Ingres .NET Data Provider. The code opens a connection, creates a SELECT command, sets it into an IngresDataAdapter, fills the data adapter, writes the Ingres result set out as an XML file, and closes the connection (implicitly by the using(IngresConnection) construct). 
