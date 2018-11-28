Public Class Form1

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Label1.Text = "Using the IngresDataReader object to retrieve data from an Ingres stored procedure"
        Dim x As Exception
        ingresConnection1.Open()
        IngresCommand1.Connection = ingresConnection1
        IngresCommand1.CommandText = "rpp_proc"
        IngresCommand1.CommandType = Data.CommandType.StoredProcedure
        ' pass in a parameter to the row producing procedure
        IngresCommand1.Parameters.Add(New Ingres.Client.IngresParameter("n_name", Ingres.Client.IngresType.VarChar)).Value = "Alcott, Scott"
        ' comment our above line to get all the data back
        'IngresCommand1.Parameters.Add(New Ingres.Client.IngresParameter("n_name", Ingres.Client.IngresType.VarChar)).Value = ""
        Dim rdr As Ingres.Client.IngresDataReader

        Try
            rdr = IngresCommand1.ExecuteReader()
            While (rdr.Read())
                'do something with the data - 
                'I just display them in the list box
                ListBox1.Items.Add(rdr.GetString(0) + ", " + rdr.GetString(1) + ", " + rdr.GetString(2) + ", $" + rdr.GetDecimal(3).ToString())
            End While
            rdr.Close()
        Catch x
            MessageBox.Show(x.Message.ToString)
        Finally
            ingresConnection1.Close()
        End Try
    End Sub
End Class
