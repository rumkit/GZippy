﻿using CommandLine;
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
                return HandleError(e);
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
                return HandleError(e);
            }

            return ErrorCode.Success;
        }

        private static ErrorCode HandleError(Exception e)
        {
            Console.WriteLine("Some error 0_o");
            Console.WriteLine(e.Message);
            return ErrorCode.Fail;
        }
    }
}
