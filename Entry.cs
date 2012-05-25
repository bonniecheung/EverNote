using System;
using Oracle.DataAccess.Client;
using System.Data;
using System.Data.Odbc;


public class Entry
{
    String Guid;
    String line1;
    String line2;
    String line3;

    public void setGuid(String Guid)
    {
        this.Guid = Guid;
    }

    public void setLine1(String line1)
    {
        this.line1 = line1;
    }

    public void setLine2(String line2)
    {
        this.line2 = line2;
    }

    public void setLine3(String line3)
    {
        this.line3 = line3;
    }

    public void displayGuid()
    {
        Console.WriteLine(Guid);
    }

    public void displayLines()
    {
        Console.WriteLine(line1);
        Console.WriteLine(line2);
        Console.WriteLine(line3);
    }

    public String getGuid()
    {
        return Guid;
    }

    public String getLine1()
    {
        return line1;
    }

    public String getLine2()
    {
        return line2;
    }

    public String getLine3()
    {
        return line3;
    }

    public void InsertRow(string connectionString, string insertSQL)
    {

        using (OdbcConnection connection =
                   new OdbcConnection(connectionString))
        {
            // The insertSQL string contains a SQL statement that
            // inserts a new row in the source table.
            OdbcCommand command = new OdbcCommand(insertSQL, connection);

            // Open the connection and execute the insert command.
            try
            {
                connection.Open();
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            // The connection is automatically closed when the
            // code exits the using block.
        }


    }

}