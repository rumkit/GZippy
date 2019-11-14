using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GZippy.CommandLineOptions
{
    abstract class OptionsBase
    {
        [Value(0, HelpText="path to source file", MetaName = "filepath", Required = true)]
        public string SourceFileName { get;set;}
        [Value(1, HelpText = "path to destination file", MetaName = "filepath", Required = true)]
        public string DestinationFileName { get;set;}
    }
}
