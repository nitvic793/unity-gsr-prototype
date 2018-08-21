using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;


struct GSROutput
{
    public float time;
    public int gsrValue;
    public int count;
}

public class GSRReader : MonoBehaviour
{
    SerialPort port;
    bool isRunning = false;
    string outputFilename = "output.json";
    List<GSROutput> gsrValues = new List<GSROutput>();
    Queue<GSROutput> gsrQueue = new Queue<GSROutput>();

    public GSRReader()
    {
        port = new SerialPort();
        port.PortName = "COM4";
        port.BaudRate = 9600;
    }

    public void Run()
    {
        StartCoroutine(Poll());
        StartCoroutine(Read());
    }

    IEnumerator Poll()
    {
        isRunning = true;
        float totalTime = 0;
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        port.Open();
        int count = 1;
        while (isRunning)
        {
            try
            {
                var line = port.ReadLine();
                totalTime += stopwatch.ElapsedMilliseconds / 1000f;
                var lineVal = new GSROutput();
                if (string.IsNullOrEmpty(line)) continue;
                lineVal.time = totalTime;
                lineVal.gsrValue = Convert.ToInt32(line);
                lineVal.count = count;
                //gsrValues.Add(lineVal);

                gsrQueue.Enqueue(lineVal);
                stopwatch.Restart();

                count++;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e);
            }
            yield return new WaitForSeconds(0.01F);
        }
        port.Close();
    }

    public string Output { get; private set; } = "";
    public float Average { get; private set; } = 0F;
    public bool IsReady { get; private set; } = false;

    IEnumerator Read()
    {
        float sum = 0F;

        float prevAverage = 0F;
        int currentCount = 0;
        float prevTime = 0F;

        while(isRunning)
        {
            if (gsrQueue.Count == 0)
            {
                yield return new WaitForSeconds(0.01F);
                continue;
            }
            var item = gsrQueue.Dequeue();
            if (item.time <= 5)
            {
                sum += item.gsrValue;
            }
            else if (!IsReady)
            {
                sum += item.gsrValue;
                prevAverage = sum / item.count;
                IsReady = true;
                sum = 0;
                currentCount = item.count;
                prevTime = item.time;
                UnityEngine.Debug.Log("Base Line " + prevAverage);
            }
            else
            {
                if (item.time - prevTime >= 5F)
                {
                    Average = sum / (item.count - currentCount);
                    prevTime = item.time;
                    currentCount = item.count;
                    sum = 0;
                    if (Average > prevAverage)
                    {
                        Output = "up";
                    }
                    else
                    {
                        Output = "down";
                    }
                    prevAverage = Average;
                    UnityEngine.Debug.Log(Output + " " + Average);
                }
                else
                {
                    sum += item.gsrValue;
                }
            }

        }
    }

    public void Stop()
    {
        isRunning = false;
    }
}
