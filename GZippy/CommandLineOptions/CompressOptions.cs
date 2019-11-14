using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GZippy.CommandLineOptions
{
    [Verb("Compress", HelpText="compresses specified file to gzip archive")]
    class CompressOptions : OptionsBase
    {
    }
}
