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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ImageAssetGenerator
{
    public class AndroidAssetGenerator
    {
        private readonly List<Density> _densities;

        private struct Density
        {
            public string OutputFolder { get; set; }
            public float Multiplier { get; set; }
        }

        public AndroidAssetGenerator()
        {
            _densities = new List<Density>
            {
                new Density { OutputFolder = "drawable-mdpi", Multiplier = 1 },
                new Density { OutputFolder = "drawable-hdpi", Multiplier = 1.5f },
                new Density { OutputFolder = "drawable-xhdpi", Multiplier = 2 },
                new Density { OutputFolder = "drawable-xxhdpi", Multiplier = 3 },
                new Density { OutputFolder = "drawable-xxxhdpi", Multiplier = 4 },
            };
        }

        public Task GenerateAssetAsync(string inputFile, string outputDirectory, int baseWidth, int baseHeight)
        {
            Console.WriteLine($"Generating Android image asset from {inputFile}");

            return Task.Run(() =>
            {
                _densities.AsParallel().ForAll(density =>
                {
                    var destinationDirectory = Path.Combine(outputDirectory, density.OutputFolder);

                    if (!Directory.Exists(destinationDirectory))
                        Directory.CreateDirectory(destinationDirectory);

                    var outputFile = Path.Combine(outputDirectory, density.OutputFolder, Path.GetFileNameWithoutExtension(inputFile) + ".png");
                    var outputWidth = (int)(baseWidth * density.Multiplier);

                    //Output height may not be specified so leave it blank and let svgexport figure out a height for us based on the width
                    var outputHeight = baseHeight > 0 ? ((int)(baseHeight * density.Multiplier)).ToString() : string.Empty;

                    var svgExportParams = new ProcessStartInfo
                    {
                        CreateNoWindow = true,
                        WorkingDirectory = Directory.GetCurrentDirectory(),
                        FileName = "svgexport.cmd",
                        Arguments = $"{Path.Combine(inputFile)} {outputFile} {outputWidth}:{outputHeight}"
                    };

                    Process.Start(svgExportParams).WaitForExit();
                });
            });
        }
    }
}
