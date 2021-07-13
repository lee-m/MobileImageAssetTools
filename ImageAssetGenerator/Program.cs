//Copyright(c) 2019-2021 Lee Millward

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using TinifyAPI;

namespace ImageAssetGenerator
{
    class Program
    {
        private static readonly string InputFileSwitch = "/input:";
        private static readonly string OutputDirectorySwitch = "/outputdirectory:";
        private static readonly string BaseWidthSwitch = "/basewidth:";
        private static readonly string BaseHeightSwitch = "/baseheight:";
        private static readonly string OptimiseSwitch = "/optimise";
        private static readonly string iOSSwitch = "/ios";
        private static readonly string AndroidSwitch = "/android";
        private static readonly string TinifyKeySwitch = "/tinifykey:";

        static void Main(string[] args)
        {
            var inputFile = "";
            var outputDirectory = "";
            var baseWidth = 0;
            var baseHeight = -1;
            var optimise = false;
            var iOS = false;
            var android = false;
            var tinifyKey = "";

            foreach(var arg in args)
            {
                if (arg.StartsWith(InputFileSwitch))
                    inputFile = arg.Substring(InputFileSwitch.Length);
                else if (arg.StartsWith(OutputDirectorySwitch))
                    outputDirectory = arg.Substring(OutputDirectorySwitch.Length);
                else if (arg.StartsWith(BaseWidthSwitch))
                    baseWidth = int.Parse(arg.Substring(BaseWidthSwitch.Length));
                else if (arg.StartsWith(BaseHeightSwitch))
                    baseHeight = int.Parse(arg.Substring(BaseHeightSwitch.Length));
                else if (arg.StartsWith(OptimiseSwitch))
                    optimise = true;
                else if (arg.StartsWith(iOSSwitch))
                    iOS = true;
                else if (arg.StartsWith(AndroidSwitch))
                    android = true;
                else if (arg.StartsWith(TinifyKeySwitch))
                    tinifyKey = arg.Substring(TinifyKeySwitch.Length);
            }

            if(string.IsNullOrEmpty(inputFile))
            {
                Console.WriteLine("No input file specified");
                return;
            }

            if(string.IsNullOrEmpty(outputDirectory))
            {
                Console.WriteLine("No output directory specified");
                return;
            }

            if(optimise && string.IsNullOrEmpty(tinifyKey))
            {
                Console.WriteLine($"Tinify API key is required when {OptimiseSwitch} is specified");
                return;
            }

            if(!File.Exists(inputFile))
            {
                Console.WriteLine("Input file does not exist");
                return;
            }

            if(!android && !iOS)
            {
                Console.WriteLine($"One of {AndroidSwitch} or {iOSSwitch} must be specified");
                return;
            }

            if (iOS && !Directory.Exists(Path.Combine(outputDirectory, "iOS")))
                Directory.CreateDirectory(Path.Combine(outputDirectory, "iOS"));

            if (android && !Directory.Exists(Path.Combine(outputDirectory, "Android")))
                Directory.CreateDirectory(Path.Combine(outputDirectory, "Android"));

            var platformTasks = new List<Task>();

            if (android)
                platformTasks.Add(new AndroidAssetGenerator().GenerateAssetAsync(inputFile, Path.Combine(outputDirectory, "Android"), baseWidth, baseHeight));

            if (iOS)
                platformTasks.Add(new iOSAssetGenerator().GenerateAssetAsync(inputFile, Path.Combine(outputDirectory, "iOS"), baseWidth, baseHeight));

            Task.WhenAll(platformTasks)
                .ContinueWith((t) =>
                {
                    if (optimise)
                        OptimisePNGImages(outputDirectory, tinifyKey);
                })
                .Wait();
        }

        private static void OptimisePNGImages(string outputDirectory, string tinifyKey)
        {
            Tinify.Key = tinifyKey;

            var pngFiles = Directory.GetFiles(outputDirectory, "*.png", SearchOption.AllDirectories);

            Task.WaitAll(pngFiles.AsParallel().Select(async file =>
            {
                var source = Tinify.FromFile(file);
                await source.ToFile(file);

            }).ToArray());
        }
    }
}
