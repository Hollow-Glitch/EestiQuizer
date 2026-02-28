using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EestiQuizer.anki; 


internal class AnkiDatabase : IDisposable {
    SqliteConnection connection;
    private bool disposedValue;


    /// <summary>
    /// </summary>
    /// <param name="profileName">example "User 1"</param>
    internal AnkiDatabase(string profileName) {
        var dbPath = Environment.ExpandEnvironmentVariables($@"%APPDATA%\Anki2\{profileName}\collection.anki2");
        connection = new SqliteConnection($"Data Source={dbPath}");
        connection.Open();
    }


    public bool IsWordInDatabase_v2(string word, string tag)
    {
        var command = connection.CreateCommand();
        command.CommandText = """SELECT count(sfld) FROM notes WHERE tags like $tag AND sfld LIKE $word""";
        command.Parameters.AddWithValue("$word", $"%{word}%");
        command.Parameters.AddWithValue("$tag", $"%{tag}%");

        var sfld_o = command.ExecuteScalar();
        if (sfld_o is null) throw new InvalidOperationException();

        var sfld = (long)sfld_o;
        var isPresent = sfld > 0;
        return isPresent;
    }


    protected virtual void Dispose(bool disposing) {
        if (!disposedValue) {
            if (disposing) {
                // TODO: dispose managed state (managed objects)
            }
            connection.Dispose();
            disposedValue = true;
        }
    }


    // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    public void Dispose() {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
