using CommandLine;
using GZippy.CommandLineOptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GZippy
{
    static class Program
    {
        static int Main(string[] args)
        {
            var errorCode = Parser.Default.ParseArguments<DecompressOptions,CompressOptions>(args)
                .MapResult(
                    (DecompressOptions options) => Decompress(options),
                    (CompressOptions options) => Compress(options),
                    errs => ErrorCode.Fail);
            
            return (int)errorCode;
        }

        private static ErrorCode Compress(CompressOptions options)
        {
            using(var source = File.OpenRead(options.SourceFileName))
            using (var destination = File.OpenWrite(options.DestinationFileName))
            {
                var dispatcher = new Dispatcher();
                dispatcher.Compress(source,destination);
                Console.WriteLine("ready");
            }
            return ErrorCode.Success;
        }

        private static ErrorCode Decompress(DecompressOptions options)
        {
            return ErrorCode.Success;
        }
    }
}
