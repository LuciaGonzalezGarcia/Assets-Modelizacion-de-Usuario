using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class DataManeger : MonoBehaviour
{
    public static int jumps;
    public static string engagement;


    void Awake()
    {
        try
        {
            engagement = Analysis.StartAnalysis();

            StreamReader sr = new StreamReader("Assets/Scripts/LastPlayData.csv");
            var line = sr.ReadLine();
            var valuesCont = line.Split(',');

            //JUMPS
            if (Int32.Parse(valuesCont[0]) < 20)
            {
                jumps = 2;
            }
            else if (Int32.Parse(valuesCont[0]) < 30)
            {
                jumps = 1;
            }
            else
            {
                jumps = 0;
            }

        }
        catch (Exception e)
        {
            Debug.Log("Exception: " + e.Message);
        }
        finally
        {
            Debug.Log("Executing finally block.");
        }


    }

    public static void Write(int jumps, float time)
    {
        try
        {
            StreamWriter sw = new StreamWriter("Assets/Scripts/LastPlayData.csv");
            
            sw.WriteLine(jumps.ToString() +" ,"+ ((int)time).ToString());
            sw.Close();
        }
        catch (Exception e)
        {
            Console.WriteLine("Exception: " + e.Message);
        }
        finally
        {
            Console.WriteLine("Executing finally block.");
        }
    }
}
