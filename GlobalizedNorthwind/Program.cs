using System;
using System.IO;
using System.Text;

public class Example
{
    public static void Main()
    {
        //Compare different method for comparing/searching/sorting strings
        Console.WriteLine("Hello World!");

    }
    public static void Main2()
    {
        Console.WriteLine("CurrentCulture is {0}.", System.Globalization.CultureInfo.CurrentCulture.Name);
        Console.WriteLine("CurrentUICulture is {0}.", System.Globalization.CultureInfo.CurrentUICulture.Name);
        // To set current culture both of following works
        System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("fa-ir", false);
        System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("jp-JP", false);
        Console.WriteLine("CurrentCulture is {0}.", System.Globalization.CultureInfo.CurrentCulture.Name);
        Console.WriteLine("CurrentUICulture is {0}.", System.Globalization.CultureInfo.CurrentUICulture.Name);
    }

    public static void Main1()
    {
        Console.WriteLine("Encodings");
        Console.WriteLine("[1] ASCII");
        Console.WriteLine("[2] UTF-7");
        Console.WriteLine("[3] UTF-8");
        Console.WriteLine("[4] UTF-16 (Unicode)");
        Console.WriteLine("[5] UTF-32");
        Console.WriteLine("[any other key] Default");
        Console.Write("Press a number to choose an encoding: ");
        ConsoleKey input = Console.ReadKey(intercept: false).Key;
        Encoding encoding = input switch
        {
            ConsoleKey.D1 => Encoding.ASCII,
            ConsoleKey.D2 => Encoding.UTF7,
            ConsoleKey.D3 => Encoding.UTF8,
            ConsoleKey.D4 => Encoding.Unicode,
            ConsoleKey.D5 => Encoding.UTF32,
            _ => Encoding.Default,
        };
        Console.WriteLine();
        char[] message = { ':', '>', '﷼' };
        var nBytes = encoding.GetByteCount(message); // Get Byte Count
        var bytes = encoding.GetBytes(message); // Get Bytes used for encoding message in selected encoding. This method has a reverse which gets string for given byte[] in defined encoding
        Console.WriteLine($"Selected Encoding:{encoding.GetType().Name}"); // each encoding has a type with its encoding??
        Console.WriteLine($"Used Bytes:{nBytes}=>{bytes.Length}");
        Console.WriteLine();
        foreach (var b in bytes)
        {
            Console.WriteLine($"{b} {b.ToString("X")} {(char)b}"); // Display integer value of byte
            // Console.WriteLine($"{b.ToString("X")}"); // Display byte as 16 bit system
            // Console.WriteLine($"{(char)b}"); // Display byte as char
            // Console.WriteLine();
        }

        // To get a string from a bytes in specified Encoding
        var s = encoding.GetString(bytes);
        Console.WriteLine(s);


    }
}
