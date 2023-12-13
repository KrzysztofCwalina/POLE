
using Azure.Core.Pole.Tooling;

class Program
{
    private static void Main(string[] args)
    {
        if (args.Length < 2)
        {
            Usage();
            return;
        }

        var source = args[0];
        var destinationFolder = args[1];

        TypeSpecGenerator.Generate(source, destinationFolder);
    }

    private static void Usage()
    {
        Console.WriteLine("Usage: pole.exe <typeSpecFile> <outputFolder>");
    }
}