﻿using System;
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

    public class IdTokenizer : Tokenizable
    {
        public override bool tokenizable(Tokenizer t)
        {
            char currentCharacter = t.input.peek();
            //Console.WriteLine(currentCharacter);
            return Char.IsLetter(currentCharacter) || currentCharacter == '_';
        }

        private static bool isId(Input input)
        {
            char currentCharacter = input.peek();
            return Char.IsLetterOrDigit(currentCharacter) || currentCharacter == '_';
        }

        public override Token tokenize(Tokenizer t)
        {
            return new Token(t.input.Position, t.input.LineNumber,
                "identifier", t.input.loop(isId));
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

            return Char.IsDigit(input.peek()) || input.peek() == '.';
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
            return t.input.hasMore() && t.input.peek() == '\"';
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
                if (t.input.peek(2) == '\"')
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

    ///JSONs

    public class JSON
    {
        private Input input;

        public JSON(string input)
        {
            this.input = new Input(input);
        }

        public JSONValue parse(Token token)
        {
            if (token.Type == "number")
            {
                JNumber number = new JNumber();
                number.value = double.Parse(token.Value);
                return number;
            }
            else if (token.Type == "openingBrace")
            {
                JObject o = getObject();
                return o;
            }
            else if (token.Type == "string")
            {
                JString str = new JString();
                str.value = token.Value;

                return str;
            }

            return new JString();
        }

        public JObject getObject()
        {
            JKeyValue keyValue = new JKeyValue();
            List<JKeyValue> list = new List<JKeyValue> { };
            JObject jsonObject = new JObject();
            Tokenizer t = new Tokenizer(this.input, new Tokenizable[]
            {
                new StringTokenizer(),
               // new IdTokenizer(),
                new NumberTokenizer(),
                new WhiteSpaceTokenizer(),
                new SpecialCharacterTokenizer(),
            });
            Token token;

            token = t.tokenize();

            if (token.Type == "openingBrace")
            {
                Console.Write(token.Value);

                while (this.input.hasMore())
                {
                    token = t.tokenize();

                    while (token != null && token.Type == "whitespace")
                    {
                        token = t.tokenize();
                    }
                    //key token
                    if (token != null && token.Type == "string")
                    {
                        Console.Write(token.Value);
                        keyValue.key = token.Value;
                    }
                    else throw new Exception("Not a key");

                    token = t.tokenize();
                    while (token != null && token.Type == "whitespace")
                    {
                        token = t.tokenize();
                    }

                    // token == :
                    if (token.Type == "colon")
                    {
                        Console.Write(token.Value);
                        // throw new Exception("Error no colon");
                    }

                    while (char.IsWhiteSpace(t.input.peek()))
                    {
                        token = t.tokenize();
                        Console.Write(token.Value);
                    }
                    //token == value
                    if (t.input.peek() == '{')
                    {
                        JObject o = getObject();
                        keyValue.value = o;
                    }
                    else if (token.Type == "string")
                    {
                        JString str = new JString();
                        str.value = token.Value;
                        keyValue.value = str;
                    }
                    else if (t.input.peek() == '[')
                    {
                        Console.Write(t.input.peek());
                        JArray arr = getArray();
                        keyValue.value = arr;
                    }
                    else
                    {
                        token = t.tokenize();
                        Console.Write(token.Value);
                        keyValue.value = this.parse(token);
                    }
                    list.Add(keyValue);

                    //check for , or }
                    token = t.tokenize();
                    while (token.Type == "whitespace")
                    {
                        token = t.tokenize();
                    }
                    if (token.Type != "comma" && token.Type != "closingBrace")
                    {
                        break;
                    }
                    else if (token.Type == "closingBrace")
                    {
                        Console.Write(token.Value);
                        break;
                    }
                    else
                    {
                        Console.Write(token.Value);
                    }
                }
            }
            else
            {
                throw new Exception("not object");
            }

            jsonObject.values = list;

            return jsonObject;
        }

        public JArray getArray()
        {
            JArray arr = new JArray();
            arr.values = new List<JSONValue> { };
            Tokenizer t = new Tokenizer(this.input, new Tokenizable[]
            {
                new StringTokenizer(),
                // new IdTokenizer(),
                new NumberTokenizer(),
                new WhiteSpaceTokenizer(),
                new SpecialCharacterTokenizer(),
            });
            Token token = t.tokenize();
            Console.Write(token.Value);

            while (t.input.hasMore())
            {
                while (char.IsWhiteSpace(t.input.peek()))
                {
                    token = t.tokenize();
                }

                //Values check
                if (t.input.peek() == '{')
                {
                    JObject o = getObject();
                    arr.values.Add(o);
                }
                else if (t.input.peek() == '[')
                {
                    JArray jarr = getArray();
                    arr.values.Add(jarr);
                }
                else
                {
                    token = t.tokenize();
                    Console.Write(token.Value);

                    arr.values.Add(this.parse(token));
                }

                //comma chick and ]
                while (char.IsWhiteSpace(t.input.peek()))
                {
                    token = t.tokenize();
                }
                token = t.tokenize();
                if (token.Type != "comma" && token.Type != "closingBracket")
                {
                    throw new Exception("Invalid");
                }
                else if (token.Type == "closingBracket")
                {
                    Console.Write(token.Value);
                    break;
                }
                else
                {
                    Console.Write(token.Value);
                }
            }

            return arr;
        }
    }

    public abstract class JSONValue
    {
    }

    public class JObject : JSONValue
    {
        public List<JKeyValue> values;
    }

    public class JArray : JSONValue
    {
        public List<JSONValue> values;
    }

    public class JString : JSONValue
    {
        public string value;
    }

    public class JNumber : JSONValue
    {
        public double value;
    }

    public class JBool : JSONValue
    {
        public bool value;
    }

    public class JNull : JSONValue
    {
        public object value;
    }

    public class JKeyValue
    {
        public string key;
        public JSONValue value;
    }

    //handlers return token

    internal class Program
    {
        private static void Main(string[] args)
        {
            JSON json = new JSON("{ \"Hello\": \"arr\",\"age\":[\"hh\" , 12 , [1,{\"key\":\"value\"}]]} ");

            json.getObject();

            /*
             Jbject o = getObj(
             `
                 {
                     "id" : 123321,
                     "name" : "ali",
                     "address" :
                         {
                         }
                 }
             `);
             */
        }
    }
}