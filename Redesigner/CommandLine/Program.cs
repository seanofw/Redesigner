//-------------------------------------------------------------------------------------------------
//
//  Redesigner
//
//  Copyright (c) 2012 by Sean Werkema
//  All rights reserved.
//
//  This software is released under the terms of the "New BSD License," as follows:
//
//  Redistribution and use in source and binary forms, with or without modification, are permitted
//  provided that the following conditions are met:
//
//   * Redistributions of source code must retain the above copyright notice, this list of
//     conditions and the following disclaimer.
//
//   * Redistributions in binary form must reproduce the above copyright notice, this list of
//     conditions and the following disclaimer in the documentation and/or other materials
//     provided with the distribution.
//
//  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR
//  IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
//  AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR
//  CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
//  CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
//  SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
//  THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
//  OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
//  POSSIBILITY OF SUCH DAMAGE.
//
//-------------------------------------------------------------------------------------------------

using System;
using System.Linq;

using Redesigner.Library;

namespace Redesigner.CommandLine
{
	/// <summary>
	/// The core startup driver class for the program.
	/// </summary>
	public class Program
	{
		/// <summary>
		/// The main entry point of the program.  Decodes command-line parameters, and kicks off actions.
		/// </summary>
		public static void Main(string[] args)
		{
			const string ProgramName = "Redesigner.exe";

			const string Usage = @"Usage: redesigner [-w site.dll] [-r path\to\site] [options] files.aspx ...";

			// Process the command line into meaningful work.
			CommandLineArguments commandLineArguments;
			try
			{
				commandLineArguments = new CommandLineArguments(args);
			}
			catch (Exception e)
			{
				Console.WriteLine("{0}: Command-line error:\r\n{1}", ProgramName, e.Message);
				return;
			}

			// Begin doing the actual compiling task.
			ICompileContext compileContext = new CommandLineCompileContext(ProgramName, commandLineArguments);
			compileContext.Verbose("{0} begin.", ProgramName);
			compileContext.Verbose("");

			switch (commandLineArguments.Action)
			{
				case ProgramAction.Nothing:
					compileContext.Verbose("Action: Do nothing.");
					compileContext.Verbose("");
					break;

				case ProgramAction.Help:
					compileContext.Verbose("Action: Show help when the user is confuzzled.");
					compileContext.Verbose("");
					Console.Write(string.Format(Usage + @"

Options:
  --help            Show help (you're looking at it).

  --website website.dll
  -w website.dll    The path to the DLL that contains the website's
                    code-behind. [required]

  --root pathname
  -r pathname       Specify the path to the website's root, where the web.config
                    file is located. [required]

  --verify          Verify that the existing designer file(s) were
                    generated correctly.

  --generate        Generate replacement designer file(s) for the given
                    pages/controls. [default]

"));
					break;

				case ProgramAction.Generate:
					compileContext.Verbose("Action: Generate new designer files.");
					compileContext.Verbose("");
					if (!commandLineArguments.Filenames.Any())
					{
						Console.WriteLine(Usage);
						break;
					}
					Common.GenerateDesignerFiles(compileContext, commandLineArguments.Filenames, commandLineArguments.RootPath, commandLineArguments.WebsiteDllFileName);
					break;

				case ProgramAction.Verify:
					compileContext.Verbose("Action: Verify existing designer files.");
					compileContext.Verbose("");
					if (!commandLineArguments.Filenames.Any())
					{
						Console.WriteLine(Usage);
						break;
					}

					compileContext.Error("The 'verify' feature is not yet implemented.");
					break;
			}
		}
	}
}
