namespace TestingGrounds; 

internal class Program {
    static void Main(string[] args) {
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
}
