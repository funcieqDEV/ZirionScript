using System;
using Global;
using Src.Frontend.Parser.AST;

namespace Src.Frontend.Parser{

    public class Parser{
        private List<Token> tokens;
        private uint pos = 0;
        public Parser(){ 
            tokens = new();
        }
        public Root Parse(List<Token> tokens){
            this.tokens = tokens;
            Root root = new Root();
            while(Peek().Type != TokenType.EOF){
                root.nodes.Add(ParseStmt());
            }
            return root;
        }

        private Node ParseStmt(){
            Token current = Peek();
            switch (current.Type)
            {
                case TokenType.k_import:
                    return ParseImport();

                default:
                    Logger.Error("An error occurred while parsing the file <"+current.Row+", "+current.Col+">", Logger.INVALID_TOKEN);
                    return null;
            }
        }   


        private Import ParseImport(){
            Consume(TokenType.k_import, "");
            string @namespace = Consume(TokenType.Id, "Expected Identifiter at <"+Peek().Row+", "+Peek().Col+">").Value;
            return null;
        }

        private Token Consume(TokenType type, string errorMessage = "")
        {
            if (Peek().Type== type)
            {
                return NextToken();
            }

            Logger.Error(errorMessage,Logger.INVALID_TOKEN);
            return tokens[tokens.Count-1];

        }

        private bool IsAtEnd(){
            return pos >= tokens.Count;
        }
        private Token NextToken()
        {
            if (!IsAtEnd()) pos++;
            return tokens[(int)pos];
        }
private Token Peek()
{
    if (IsAtEnd()) 
    {
        if (tokens.Count == 0)
        {
            throw new InvalidOperationException("Cannot peek at an empty token list.");
        }
        return tokens[tokens.Count - 1]; 
    }
    return tokens[(int)pos];
}

    }

}