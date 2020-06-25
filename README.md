# Mobile App Image Asset Tools

This repository contains two simple command-line tools useful when generating/working with image assets for mobile apps in iOS and Android.

## ImageAssetGenerator
Converts a SVG base image asset into a series of density dependent PNG variants and (optionally) optimises them using [Tinify](https://tinypng.com/) if an API key is provided.

For Android, the output files will be contained with drawable-mdpi, drawable-hdpi etc. folders so that the contents of the output folder can simply be copied across into the app's /res directory.

iOS output are not grouped into separate folders and instead will be of the form <input_file>.png, <input_file>@2x.png and <input_file>@3x.png for adding into an iOS asset catalog.

This tool was written to scratch a personal itch during a separate piece of work so whilst there is some error checking/handling, it's very basic or non-existant so use it at your own risk.

##### Prerequisites
* Node.js's [svgexport](https://www.npmjs.com/package/svgexport) module to be installed which does the heavy lifting of converting from SVG to PNG.
* (Optional) a Tinify account to access their API for optimising PNG files. This can reduce output file sizes significantly.

##### Command Line Arguments
* `/input:` (Required) Path to the input SVG file. Relative paths supported.
* `/outputdirectory:` (Required) Directory to write the generated files to. Android and iOS images will be created in "Android" and "iOS" folders underneath this location.
* `/basewidth:` (Required) The width of the output image in the MDPI (Android) and 1x (iOS) density. All other variations for higher densities are based on this value.
* `/baseheight:` (Optional) Like `/basewidth` but for the output image height. May be left blank to preserve any aspect ratio of the input file in the output.
* `/optimise` (Optional) Use Tinify to reduce file size of the output PNG files.
* `/tinifykey:` (Optional, unless `/optimise` is specified) Tinify API key used to communicate with their API for optimising output PNG files.
* `/android` (Optional) Produce output images scaled for MDPI, HDPI, XHDPI, XXHDPI and XXXHDPI densities
* `/ios` (Optional) Produce output images scaled for 1x, 2x and 3x

_Note: Although `/android` and `\ios` are both optional, at least one of these options must be specified. Specify both together is valid._

##### Example
Generate optimised iOS and Android images:

`ImageAssetGenerator.exe /input:SomeFile.svg /outputdirectory:Generated /basewidth:32 /optimise /tinifykey:<redacted> /android /ios`

## PNGOptimiser
Batch optimises all PNG and JPG files in the specified directory (and its subdirectories) using Tinify to reduce file size.

##### Example
`PNGOptimiser <image directory> <Tinify API key>`
