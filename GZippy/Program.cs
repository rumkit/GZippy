using CommandLine;
using GZippy.CommandLineOptions;
using GZippy.Gzip;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GZippy
{
    static class Program
    {
        static int Main(string[] args)
        {
            var errorCode = Parser.Default.ParseArguments<DecompressOptions, CompressOptions>(args)
                .MapResult(
                    (DecompressOptions options) => Decompress(options),
                    (CompressOptions options) => Compress(options),
                    errs => ErrorCode.Fail);

            return (int)errorCode;
        }

        private static ErrorCode Compress(CompressOptions options)
        {
            try 
            {
                using (var source = File.OpenRead(options.SourceFileName))
                using (var destination = File.Create(options.DestinationFileName))
                {
                    var dispatcher = new Dispatcher(new GzipCompressionStrategy());
                    dispatcher.Compress(source, destination);
                }
            }
            catch(Exception e)
            {
                return HandleError(e, options);
            }
            
            return ErrorCode.Success;
        }        

        private static ErrorCode Decompress(DecompressOptions options)
        {
            try
            {
                using (var source = File.OpenRead(options.SourceFileName))
                using (var destination = File.Create(options.DestinationFileName))
                {
                    var dispatcher = new Dispatcher(new GzipCompressionStrategy());
                    dispatcher.Decompress(source, destination);                    
                }
            }
            catch (Exception e)
            {
                return HandleError(e, options);
            }

            return ErrorCode.Success;
        }

        private static ErrorCode HandleError(Exception e, OptionsBase options)
        {
            switch(e)
            {
                case FileNotFoundException _:
                    Console.WriteLine($"Could not find input file: {options.SourceFileName}. Make sure file exists.");
                    break;
                case IOException _:
                    Console.WriteLine($"Could not access output file: {options.DestinationFileName}. Please close all programs using this file.");
                    break;
                case UnsupportedFileFormatException _:
                case InvalidDataException _:
                    Console.WriteLine($"Archive seems to be damaged or has incorrect format.");
                    break;
                case Exception ex:
                    Console.WriteLine("Unexpected error occured. Show this to your programmer");
                    Console.WriteLine("Error:");
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("Stacktrace:");
                    Console.WriteLine(ex.StackTrace);
                    break;
            }                                    
            return ErrorCode.Fail;
        }
    }
}
