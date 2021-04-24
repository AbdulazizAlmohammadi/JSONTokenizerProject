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

            JSON json = new JSON(@"[ 1 , 2 , {""ke5y  ik@#$g,hadf "" : 55 }]");

            //JSON json1 = new JSON("false");
            //JSON json2 = new JSON("12");
            //JSON json3 = new JSON("null");
            //JSON json4 = new JSON("false");


            json.getObject();
        }
    }
}