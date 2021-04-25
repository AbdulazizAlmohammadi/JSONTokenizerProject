using JSONTokenizerProject;
using System;
using System.Collections.Generic;
namespace JSONTokenizerProject
{

    class Program
    {
        static void Main(string[] args)
        {

            //JSON json = new JSON("{\n" +
            //    "   \"hello\": \"arr\",\n" +
            //    "   \"age\":[\n " +
            //    "       \"hh\" , 12 , [\n" +
            //    "        1 ,\n " +
            //    "       {\"key\":\"value\"}\n" +
            //    "                    ]\n" +
            //    "         ], \n" +
            //    "   \"ismarried\" : true\n" +
            //    "}");


            //JSON json = new JSON(@" [ 1 , 2 , {""ke5y"" : 55 }, {""TEST"" : {""INSIDE"": [4,{""ARR-_23"":9},4]}} , {""EXAMPLE"" : nuLL} ]");
            //JSON json = new JSON(@" true");

            JSON json = new JSON(@"{""key"": 1.7e+3 }");


            //JSON json1 = new JSON("false");
            //JSON json2 = new JSON("12");
            //JSON json3 = new JSON("null");
            //JSON json4 = new JSON("false");

            JSONValue js = json.getJSON();
            
            if (js is JBool)
            {
                JBool jbool = (JBool)js;
                Console.WriteLine(jbool.value);


            } else if(js is JArray)
            {
                JArray arr = (JArray)js;
            } else if(js is JObject)
            {
                JObject obj = (JObject)js;
                //Console.WriteLine(obj.value[0].key);
            }

          //  json.getObject();



        }
    }
}