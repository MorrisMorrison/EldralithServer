using System;
using Godot;

public class GDPrint
{
    public static string GetTimestamp()
    {
        DateTime currentDateTime = DateTime.Now;
        string formattedTime = currentDateTime.ToString("dd.MM.yyyy-HH:mm:ss");
        return formattedTime;
    }

    public static void Print(string text)
    {
        GD.Print($"{GetTimestamp()}: {text}");
    }

    public static void PrintErr(string text)
    {
        GD.PrintErr($"{GetTimestamp()}: {text}");
    }
}