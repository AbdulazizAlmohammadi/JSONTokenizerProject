using System;
using System.Collections.Generic;
using System.Text;

namespace JSONTokenizerProject
{

    public class JSON
    {
        private Input input;
        Tokenizer t;
        

        public JSON(string input)
        {
            this.input = new Input(input);
            this.t = new Tokenizer(this.input, new Tokenizable[]
            {
                new StringTokenizer(),
                new KeyWordsTokenizer(),
                new NumberTokenizer(),
                new WhiteSpaceTokenizer(),
                new SpecialCharacterTokenizer(),
            });
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

            } else if(token.Type == "bool")
            {
                JBool bol = new JBool();
                if (token.Value == "true")
                    bol.value = true;
                else bol.value = false;

                return bol;
            }
            else if (token.Type == "null")
            {
                JNull nul = new JNull();
                nul.value = null;
                return nul;
            }
            else
            {
                throw new Exception("not a valid value");
            }

            

        }

        public JSONValue getJSON()
        {
            JKeyValue keyValue = new JKeyValue();
            List<JKeyValue> list = new List<JKeyValue> { };
            JSONValue jValue ;



            Token token;
            while(char.IsWhiteSpace(t.input.peek()))
            {
                token = t.tokenize();
            }

            if (t.input.peek() == '{')
            {

                jValue = getObject();

            }
            else if (t.input.peek() == '[') 
            {
                jValue = getArray();
            }
            else
            {
                token = t.tokenize();
                 if(token.Type == "bool" || token.Type == "null") 
                {
                    jValue = this.parse(token);
                    Console.Write(token.Value);
                }
                else throw new Exception("not object");
            }

          
           

            return jValue;
        }

        public JObject getObject()
        {
            JKeyValue keyValue = new JKeyValue();
            List<JKeyValue> list = new List<JKeyValue> { };
            JObject jsonObject = new JObject();
            Token token;
            token = t.tokenize();

            Console.Write(token.Value);

            while (this.input.hasMore())
            {

                token = t.tokenize();

                while (token != null && token.Type == "whitespace")
                {
                    Console.Write(token.Value);
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
                    Console.Write(token.Value);
                    token = t.tokenize();
                }


                // token == :
                if (token.Type == "colon")
                {
                    Console.Write(token.Value);
                    //
                }
                else throw new Exception("Error no colon");

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
                else if (t.input.peek() == '[')
                {

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

                keyValue = new JKeyValue();
                //check for , or }
                token = t.tokenize();
                while (token.Type == "whitespace")
                {

                    Console.Write(token.Value);
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
            jsonObject.value = list;
            return jsonObject;
        }
        public JArray getArray()
        {
            JArray arr = new JArray();
            arr.values = new List<JSONValue> { };
           
            Token token = t.tokenize();
            Console.Write(token.Value);
            
            while (t.input.hasMore())
            {
                while (char.IsWhiteSpace(t.input.peek()))
                {
                    
                    token = t.tokenize();
                    Console.Write(token.Value);

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
                    Console.Write(token.Value);

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
        public List<JKeyValue> value;
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
}
