using System;
using System.IO;
using Src.Frontend.Lexer;
using Src.Frontend.Parser;

public class Program {
    public static void Main(String[] args){
        Console.WriteLine("Zirion v0.01 alpha");
        string text = File.ReadAllText("C:\\Users\\3TP\\Downloads\\Zirion\\Zirion-main\\test.zir");
        var toks = new Lexer().Tokenize(text);
        foreach (var item in toks)
        {
            Console.WriteLine(item.ToString());
        }
        Parser parser = new();
        parser.Parse(toks);
    }
}