using System;
using System.Linq.Expressions;
using System.Xml.Linq;
using Global;
using lang.Src.Frontend.AST;
using Src.Frontend.Parser.AST;

namespace Src.Frontend.Parser{

    public class Parser{
        private static readonly HashSet<TokenType> AllowedTypes = new()
    {
    TokenType.K_void, TokenType.K_int, TokenType.K_string,
    TokenType.K_float, TokenType.K_char
    };
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
                case TokenType.k_fn:
                    return ParseFunction();
                default:
                    Logger.Error("An error occurred while parsing the file <"+current.Row+", "+current.Col+">", Logger.INVALID_TOKEN);
                    return null;
            }
        }

        private Node ParseBodyStmt()
        {
            Token current = Peek();
            switch (current.Type)
            {
                case TokenType.k_return:
                    return ParseReturn();
                default:
                    Logger.Error("An error occurred while parsing the file <" + current.Row + ", " + current.Col + ">", Logger.INVALID_TOKEN);
                    return null;
            }
        }
        private Return ParseReturn()
        {
            Consume(TokenType.k_return, "Expected 'return' at " + DrawInfo(), Logger.INVALID_TOKEN);
            Expr node = ParseExpr();
            Consume(TokenType.Semi, "Expected ';' at " + DrawInfo(), Logger.INVALID_TOKEN);
            return new Return(node);
        }


        private Expr ParseExpr()
        {
    
            Expr left = ParseTerm();


            while (Peek().Type == TokenType.Operator &&
                   (Peek().Value == "+" || Peek().Value == "-"))
            {
                Token operatorToken = NextToken();

              
                if (left == null)
                {
                    Logger.Error("Operator " + operatorToken.Value + " must have an expression on the left side at "+DrawInfo(),Logger.INVALID_SYNTAX);
                    return null; 
                }

                Expr right = ParseTerm(); 
                left = new BinaryOperation(left, right, operatorToken.Value); 
            }

            return left;
        }






        private Expr ParseTerm()
        {
            Expr left = ParseFactor();  

    
            while (Peek().Type == TokenType.Operator &&
                   (Peek().Value == "*" || Peek().Value == "/"))
            {
                Token op = NextToken();  
                Expr right = ParseFactor();  
                left = new BinaryOperation(left, right, op.Value);  
            }

            return left;
        }


        private Expr ParseFactor()
        {
            Expr expr = ParsePrimary(); 


            while (Peek().Type == TokenType.Operator &&
                   (Peek().Value == "*" || Peek().Value == "/" || Peek().Value == "%"))
            {
                Token operatorToken = NextToken();

             
                if (expr == null)
                {
                    Logger.Error(operatorToken.Value + " must have an expression on the left side at "+DrawInfo(),Logger.INVALID_SYNTAX);
                    return null;  
                }

                Expr right = ParsePrimary(); 
                expr = new BinaryOperation(expr, right, operatorToken.Value); 
            }

            return expr;
        }




        private Expr ParsePrimary()
        {
            Token Current = Peek();

            switch (Current.Type)
            {
                case TokenType.k_true:
                    NextToken();
                    return new ConstBool(true);
                case TokenType.k_false:
                    NextToken();
                    return new ConstBool(false);
                case TokenType.ConstInt:
                    NextToken();
                    return new ConstInt(int.Parse(Current.Value));
                case TokenType.ConstFloat:
                    NextToken();
                    return new ConstFloat(float.Parse(Current.Value));
                case TokenType.ConstString:
                    NextToken();
                    return new ConstString(Current.Value);
                case TokenType.ConstChar:
                    NextToken();
                    return new ConstChar(char.Parse(Current.Value));
                default:
                    return null;
            }
        }

        private Function ParseFunction()
        {
            Consume(TokenType.k_fn, "Expected 'fn'.");
            string name = Consume(TokenType.Id, "Expected function name at "+ DrawInfo(), Logger.INVALID_TOKEN).Value;
            Consume(TokenType.LP, "Expected '(' at "+DrawInfo(),Logger.INVALID_TOKEN);
            List<Arg> args = ParseArgs();
            Consume(TokenType.RP, "Expected ')' at "+DrawInfo(),Logger.INVALID_TOKEN);
            Consume(TokenType.Colon, "Expected ':' at "+DrawInfo(),Logger.INVALID_TOKEN);
            string returnType = ParseType();
            Body body = ParseBody();
            return new Function(name, returnType, args, body);
        }

        private string DrawInfo()
        {
            Token cur = Peek();
            return "<" + cur.Row + ", " + cur.Col + ">";
        }
        private Body ParseBody()
        {
            Consume(TokenType.LB, "Expected '{' at <" + Peek().Row + ", " + Peek().Col + ">", Logger.INVALID_TOKEN);
            List<Node> nodes = new();
            while (Peek().Type != TokenType.RB)
            {
                nodes.Add(ParseBodyStmt());
            }
            Consume(TokenType.RB, "Expected '}' at <" + Peek().Row + ", " + Peek().Col + ">", Logger.INVALID_TOKEN);
            return new Body(nodes);
        }
        private string ParseType()
        {
            Token token = Peek();
            if (AllowedTypes.Contains(token.Type))
            {
                return Consume(token.Type).Value;
            }
            return Consume(TokenType.Id, "Expected type identifier at <"+ Peek().Row+", "+Peek().Col+">",Logger.INVALID_TOKEN).Value;
        }
        private List<Arg> ParseArgs()
        {
            List<Arg> args = new();
            while (Peek().Type != TokenType.RP)
            {
                string type = ParseType();
                string name = Consume(TokenType.Id, "Expected argument name at "+DrawInfo(), Logger.INVALID_TOKEN).Value;
                args.Add(new Arg(name, type));

                if (Peek().Type == TokenType.Comma)
                {
                    Consume(TokenType.Comma);
                }
            }
            return args;
        }
        private Import ParseImport(){
            Consume(TokenType.k_import, "");
            string @namespace = Consume(TokenType.Id, "Expected Identifiter at <"+Peek().Row+", "+Peek().Col+">",Logger.INVALID_TOKEN).Value;
            Consume(TokenType.DoubleColon, "Expected '::' at <" + Peek().Row + ", " + Peek().Col + ">",Logger.INVALID_TOKEN);
            string name = Consume(TokenType.Id, "Expected Identifiter at <" + Peek().Row + ", " + Peek().Col + ">", Logger.INVALID_TOKEN).Value;
            Consume(TokenType.Semi, "Expected ';' at <" + Peek().Row + ", " + Peek().Col + ">",Logger.INVALID_TOKEN);
            return new Import(name,@namespace);
        }

        private Token Consume(TokenType type, string errorMessage = "", int EC = 0)
        {
            if (Peek().Type== type)
            {
                return NextToken();
            }

            Logger.Error(errorMessage,EC);
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