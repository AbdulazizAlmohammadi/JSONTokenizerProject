using System;
using System.Collections.Generic;

namespace JSONTokenizerProject
{
    public delegate bool InputCondition(Input input);

    public class Input
    {
        private readonly string input;
        private readonly int length;
        private int position;
        private int lineNumber;

        //Properties
        public int Length
        {
            get
            {
                return this.length;
            }
        }

        public int Position
        {
            get
            {
                return this.position;
            }
        }

        public int NextPosition
        {
            get
            {
                return this.position + 1;
            }
        }

        public int LineNumber
        {
            get
            {
                return this.lineNumber;
            }
        }

        public char Character
        {
            get
            {
                if (this.position > -1) return this.input[this.position];
                else return '\0';
            }
        }

        public Input(string input)
        {
            this.input = input;
            this.length = input.Length;
            this.position = -1;
            this.lineNumber = 1;
        }

        public bool hasMore(int numOfSteps = 1)
        {
            if (numOfSteps <= 0) throw new Exception("Invalid number of steps");
            return (this.position + numOfSteps) < this.length;
        }

        public bool hasLess(int numOfSteps = 1)
        {
            if (numOfSteps <= 0) throw new Exception("Invalid number of steps");
            return (this.position - numOfSteps) > -1;
        }

        //callback -> delegate
        public Input step(int numOfSteps = 1)
        {
            if (this.hasMore(numOfSteps))
                this.position += numOfSteps;
            else
            {
                throw new Exception("There is no more step");
            }
            return this;
        }

        public Input back(int numOfSteps = 1)
        {
            if (this.hasLess(numOfSteps))
                this.position -= numOfSteps;
            else
            {
                throw new Exception("There is no more step");
            }
            return this;
        }

        public Input reset()
        {
            return this;
        }

        public char peek(int numOfSteps = 1)
        {
            if (this.hasMore(numOfSteps)) return this.input[this.Position + numOfSteps];
            return '\0';
        }

        public string loop(InputCondition condition)
        {
            string buffer = "";
            while (this.hasMore() && condition(this))
                buffer += this.step().Character;
            return buffer;
        }
    }

    public class Token
    {
        public int Position { set; get; }
        public int LineNumber { set; get; }
        public string Type { set; get; }
        public string Value { set; get; }

        public Token(int position, int lineNumber, string type, string value)
        {
            this.Position = position;
            this.LineNumber = lineNumber;
            this.Type = type;
            this.Value = value;
        }
    }

    public abstract class Tokenizable
    {
        public abstract bool tokenizable(Tokenizer tokenizer);

        public abstract Token tokenize(Tokenizer tokenizer);
    }

    public class Tokenizer
    {
        public List<Token> tokens;
        public bool enableHistory;
        public Input input;
        public Tokenizable[] handlers;

        public Tokenizer(string source, Tokenizable[] handlers)
        {
            this.input = new Input(source);
            this.handlers = handlers;
        }

        public Tokenizer(Input source, Tokenizable[] handlers)
        {
            this.input = source;
            this.handlers = handlers;
        }

        public Token tokenize()
        {
            foreach (var handler in this.handlers)
                if (handler.tokenizable(this)) return handler.tokenize(this);
            return null;
        }

        public List<Token> all()
        {
            return null;
        }
    }

    /// Tokenizers ///

    public class KeyWordsTokenizer : Tokenizable
    {
        public override bool tokenizable(Tokenizer t)
        {
            return Char.IsLetter(t.input.peek());
        }

        static bool isLetter(Input input)
        {
            return Char.IsLetter(input.peek());
        }

        public override Token tokenize(Tokenizer t)
        {
            Token token = new Token(t.input.Position, t.input.LineNumber, "", "");
            token.Value = t.input.loop(isLetter).ToLower();

            if (token.Value == "true" || token.Value == "false")
            {
                token.Type = "bool";
            }
            else if (token.Value == "null")
            {
                token.Type = "null";
            }
            else
            {
                throw new Exception("Invalid Value!");
            }
            return token;
        }
    }

    public class NumberTokenizer : Tokenizable
    {
        public override bool tokenizable(Tokenizer t)
        {
            return Char.IsDigit(t.input.peek());
        }

        private static bool isDigit(Input input)
        {
            char currentChar = input.Character;
            return Char.IsDigit(input.peek()) || input.peek() == 'e'
                || input.peek() == '.' || input.peek() == 'E' || input.peek() == '+' || input.peek() == '-';
        }

        public override Token tokenize(Tokenizer t)
        {
            return new Token(t.input.Position, t.input.LineNumber,
                "number", t.input.loop(isDigit));
        }
    }

    public class WhiteSpaceTokenizer : Tokenizable
    {
        public override bool tokenizable(Tokenizer t)
        {
            return Char.IsWhiteSpace(t.input.peek());
        }

        private static bool isWhiteSpace(Input input)
        {
            return Char.IsWhiteSpace(input.peek());
        }

        public override Token tokenize(Tokenizer t)
        {
            return new Token(t.input.Position, t.input.LineNumber,
                "whitespace", t.input.loop(isWhiteSpace));
        }
    }

    public class SpecialCharacterTokenizer : Tokenizable
    {
        private List<char> specialChar = new List<char> { '}', '{', ':', ',', ']', '[' };

        public override bool tokenizable(Tokenizer t)
        {
            char peek = t.input.peek();
            return specialChar.Contains(peek);
        }

        public override Token tokenize(Tokenizer t)
        {
            Token token = new Token(t.input.Position, t.input.LineNumber,
                "", "");
            char peek = t.input.peek();
            if (peek == '{')
            {
                token.Value += t.input.step().Character;
                token.Type = "openingBrace";
                return token;
            }
            else if (peek == '}')
            {
                token.Value += t.input.step().Character;
                token.Type = "closingBrace";
                return token;
            }
            else if (peek == ':')
            {
                token.Value += t.input.step().Character;
                token.Type = "colon";
                return token;
            }
            else if (peek == ',')
            {
                token.Value += t.input.step().Character;
                token.Type = "comma";
                return token;
            }
            else if (peek == '[')
            {
                token.Value += t.input.step().Character;
                token.Type = "openingBracket";
                return token;
            }
            else
            {
                token.Value += t.input.step().Character;
                token.Type = "closingBracket";
                return token;
            }
        }
    }

    public class StringTokenizer : Tokenizable
    {
        public override bool tokenizable(Tokenizer t)
        {
            return t.input.hasMore() && t.input.peek() == '"';
        }

        public bool isString(Tokenizer t)
        {
            return t.input.hasMore();
        }

        public override Token tokenize(Tokenizer t)
        {
            Token token = new Token(t.input.Position, t.input.LineNumber,
                "string", "");
            while (isString(t))
            {
                if (t.input.peek(2) == '"')
                {
                    token.Value += t.input.step().Character;
                    token.Value += t.input.step().Character;

                    break;
                }
                else token.Value += t.input.step().Character;
            }
            return token;
        }
    }
}