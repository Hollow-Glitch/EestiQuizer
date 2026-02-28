using Microsoft.Data.Sqlite;

namespace TestingGrounds; 

internal class Program {
    static void Main(string[] args) {
        var tag = "generated";

        var word = "sõber";
        //var word = "sex";

        var isPresent = IsWordInDatabase_v2(word, tag, "User 1");
        Console.WriteLine($"{word} isPresent={isPresent}");
    }


    static void DictionaryCheck () {
        var dict = new Dictionary<string, int>();
        dict.Add("a", 0);
        dict["a"]++;
        dict.TryGetValue("a", out var weight2);
        weight2++;
        foreach(var kvp in dict) {
            Console.WriteLine($"{kvp.Key} {kvp.Value}");
        }
        Console.WriteLine($"{dict["a"]}");
    }


    public static bool IsWordInDatabase_v0(string word, string profileName)
    {
        //var profileName = "User 1"; //<< example
        var dbPath = Environment.ExpandEnvironmentVariables($@"%APPDATA%\Anki2\{profileName}\collection.anki2");

        using (var connection = new SqliteConnection($"Data Source={dbPath}"))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT sfld FROM notes WHERE sfld LIKE $word";
            command.Parameters.AddWithValue("$word", $"%{word}%");

            bool isPresent;
            string message;
            //var reader = command.ExecuteReader();
            //foreach (var item in reader) {
            //    if (item is null) continue;

            //    Console.WriteLine($"{item}");
            //}
            if (command.ExecuteScalar() is {} sfld) {
                message = (string) sfld;
                isPresent = true;
            } else {
                message = "not found";
                isPresent = false;
            }
            Console.WriteLine(message);
            return isPresent;
        }
    }


    public static bool IsWordInDatabase_v1(string word, string profileName)
    {
        //var profileName = "User 1"; //<< example
        var dbPath = Environment.ExpandEnvironmentVariables($@"%APPDATA%\Anki2\{profileName}\collection.anki2");

        using (var connection = new SqliteConnection($"Data Source={dbPath}"))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = """SELECT count(sfld) FROM notes WHERE sfld LIKE $word""";
            command.Parameters.AddWithValue("$word", $"%{word}%");

            var sfld_o = command.ExecuteScalar();
            if (sfld_o is null) throw new InvalidOperationException();

            var sfld = (long)sfld_o;
            var isPresent = sfld > 0;
            return isPresent;
        }
    }


    public static bool IsWordInDatabase_v2(string word, string tag, string profileName)
    {
        //var profileName = "User 1"; //<< example
        var dbPath = Environment.ExpandEnvironmentVariables($@"%APPDATA%\Anki2\{profileName}\collection.anki2");

        using (var connection = new SqliteConnection($"Data Source={dbPath}"))
        {
            connection.Open();
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
    }

}
