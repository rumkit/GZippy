using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GZippy.CommandLineOptions
{
    [Verb("decompress", HelpText = "Decompresses gzip archive to target file")]
    class DecompressOptions : OptionsBase
    {
    }
}
