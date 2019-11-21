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
                    var dispatcher = new Dispatcher(new GzipCompressionStrategy(),
                        new MultipartGzipFormatter());
                    dispatcher.Compress(source, destination);
                }
            }
            catch(Exception e)
            {
                return HandleError((dynamic)e, options);
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
                    var dispatcher = new Dispatcher(new GzipCompressionStrategy(),
                        new MultipartGzipFormatter());
                    dispatcher.Decompress(source, destination);                    
                }
            }
            catch (Exception e)
            {
                return HandleError((dynamic)e, options);
            }

            return ErrorCode.Success;
        }

        private static ErrorCode HandleError(FileNotFoundException e, OptionsBase options)
        {
            Console.WriteLine($"Could not find input file: {options.SourceFileName}. Make sure file exists.");
            return ErrorCode.Fail;
        }

        private static ErrorCode HandleError(UnsupportedFileFormatException e, OptionsBase options)
        {
            Console.WriteLine($"Archive seems to be damaged or has incorrect format.");
            return ErrorCode.Fail;
        }
        private static ErrorCode HandleError(InvalidDataException e, OptionsBase options)
        {
            Console.WriteLine($"Archive seems to be damaged or has incorrect format.");
            return ErrorCode.Fail;
        }

        private static ErrorCode HandleError(Exception e, OptionsBase options)
        {
            Console.WriteLine("Unexpected error occured. Show this to your programmer");
            Console.WriteLine("Error:");
            Console.WriteLine(e.Message);
            Console.WriteLine("Stacktrace:");
            Console.WriteLine(e.StackTrace);
            return ErrorCode.Fail;
        }
    }
}
