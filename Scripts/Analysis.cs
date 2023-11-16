using Accord;
using Accord.MachineLearning.DecisionTrees;
using Accord.MachineLearning.DecisionTrees.Learning;
using Accord.Math;
using Accord.Math.Optimization.Losses;
using Accord.Statistics.Filters;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using UnityEngine;

public class Analysis : MonoBehaviour
{
    public static string StartAnalysis()
    {
        initDecisionTreeModel();
        return dataAnalysis();
    }

    private static string dataAnalysis()
    {
        string vrPredictedResult = null;
        using (var reader = new StreamReader("Assets/Scripts/LastPlayData.csv"))
        {
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var valuesCont = line.Split(',');
                string[] valuesDisc = { "", ""};

                //JUMPS
                if (Int32.Parse(valuesCont[0]) < 20)  //20
                {
                    valuesDisc[0] = "Few";
                }
                else if (Int32.Parse(valuesCont[0]) < 30)  //40
                {
                    valuesDisc[0] = "Some";
                }
                else
                {
                    valuesDisc[0] = "Many";
                }

                //TIME
                if (Int32.Parse(valuesCont[1]) < 20)
                {
                    valuesDisc[1] = "Quick";
                }
                else if (Int32.Parse(valuesCont[1]) < 40) //60000
                {
                    valuesDisc[1] = "Medium";
                }
                else
                {
                    valuesDisc[1] = "Slow";
                }


                Dictionary<string, string> dicValues = new Dictionary<string, string>();
                int i = 0;
                foreach (var vrPerFeature in lst_input_features)
                {
                    dicValues.Add(vrPerFeature, valuesDisc[i]);
                    i += 1;
                }

                vrPredictedResult = predict_the_class(dicValues);
                Debug.Log("Predicted result (engagement): " + vrPredictedResult + "\r\n" + "\r\n");

            }
            
        }
        return vrPredictedResult;
    }

    private static List<string> lst_input_features = new List<string>{"Jumps",
            "Time"};

    private static string predict_the_class(Dictionary<string, string> dicInput)
    {

        string[,] arrayString = new string[dicInput.Count, 2];
        int irIndex = 0;
        foreach (var vrPerRecord in dicInput)
        {
            arrayString[irIndex, 0] = vrPerRecord.Key;
            arrayString[irIndex, 1] = vrPerRecord.Value;
            irIndex++;
        }

        int[] query = myCodeBook.Transform(arrayString);

        // And then predict the label using
        int predicted = myTreeModel.Decide(query);  // result will be 0

        // We can translate it back to strings using
        string answer = myCodeBook.Revert("Engagement", predicted); // Answer will be: "No"
        return answer;
    }

    static DataTable dtStatic = new DataTable("my custom data table");
    static Codification myCodeBook;
    static DecisionTree myTreeModel;

    private static void initDecisionTreeModel()
    {
        dtStatic.Columns.Add("Game", "Jumps", "Time", "Engagement");

        using (var reader = new StreamReader("Assets/Scripts/data.csv"))
        {
            int i = 0;

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var valuesCont = line.Split(',');
                string[] valuesDisc = { "", "", "" };

                //JUMPS
                if (Int32.Parse(valuesCont[0]) < 20)
                {
                    valuesDisc[0] = "Few";
                }
                else if (Int32.Parse(valuesCont[0]) < 30)
                {
                    valuesDisc[0] = "Some";
                }
                else
                {
                    valuesDisc[0] = "Many";
                }

                //TIME
                if (Int32.Parse(valuesCont[1]) / 1000 < 20)
                {
                    valuesDisc[1] = "Quick";
                }
                else if (Int32.Parse(valuesCont[1]) / 1000 < 40)
                {
                    valuesDisc[1] = "Medium";
                }
                else
                {
                    valuesDisc[1] = "Slow";
                }

                //ENGAGEMENT
                if (Int32.Parse(valuesCont[2]) == 0)
                {
                    valuesDisc[2] = "Yes";
                }
                else if (Int32.Parse(valuesCont[2]) == 1)
                {
                    valuesDisc[2] = "Some";
                }
                else
                {
                    valuesDisc[2] = "No";
                }

                dtStatic.Rows.Add(i, valuesDisc[0], valuesDisc[1], valuesDisc[2]);
                i += 1;
            }
        }

        myCodeBook = new Codification(dtStatic);

        DataTable symbols = myCodeBook.Apply(dtStatic);
        int[][] inputs = symbols.ToJagged<int>("Jumps", "Time");
        int[] outputs = symbols.ToArray<int>("Engagement");
        var id3learning = new ID3Learning(){
                new DecisionVariable("Jumps",     3), // 3 possible values (Few, Some, Many)
                new DecisionVariable("Time", 3), // 3 possible values (Quick, Medium, Slow)
            };
        myTreeModel = id3learning.Learn(inputs, outputs);

        double error = new ZeroOneLoss(outputs).Loss(myTreeModel.Decide(inputs));

        Debug.Log("Learnt model training accuracy is: " + (100 - error).ToString("N2"));

    }
}
