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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Redesigner.Library
{
	public class Common
	{
		/// <summary>
		/// The canonical name of the "web.config" file.
		/// </summary>
		private const string WebConfigFilename = "web.config";

		/// <summary>
		/// The canonical name of the "System.Web" assembly.
		/// </summary>
		public const string SystemWebAssemblyName = "System.Web, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";

		/// <summary>
		/// The standard tag registrations that are always included, regardless of whether they
		/// are mentioned in the "web.config" or markup.
		/// 
		/// Note that HTML server controls are handled specially and are not included here.
		/// </summary>
		private static readonly IEnumerable<TagRegistration> _standardTagRegistrations = new List<TagRegistration>
		{
			new TagRegistration
			{
				Kind = TagRegistrationKind.Namespace,
				AssemblyFilename = SystemWebAssemblyName,
				Namespace = "System.Web.UI.WebControls",
				TagPrefix = "asp",
			},
		};

		/// <summary>
		/// For the given set of .aspx or .ascx files, generate all of their designer files.
		/// </summary>
		/// <param name="compileContext">The context in which errors are to be reported.</param>
		/// <param name="filenames">The filenames to generate.</param>
		/// <param name="rootPath">The root disk path of the website (usually the same as the path to "web.config").</param>
		/// <param name="websiteDllFileName">The disk path to the website's DLL.</param>
		public static void GenerateDesignerFiles(ICompileContext compileContext, IEnumerable<string> filenames, string rootPath, string websiteDllFileName)
		{
			// Load and parse the "web.config".
			WebConfigReader webConfigReader = new WebConfigReader();
			try
			{
				webConfigReader.LoadWebConfig(compileContext, Path.Combine(rootPath, WebConfigFilename), rootPath);
			}
			catch (Exception e)
			{
				compileContext.Error("Cannot read {0}:\r\n{1}", Path.Combine(rootPath, WebConfigFilename), e.Message);
				return;
			}

			// Load any assemblies we know we'll need.  This includes the default assemblies, any declared
			// in the web.config, and, of course, the website's DLL itself.
			AssemblyLoader assemblyLoader = new AssemblyLoader();
			List<string> assemblyNames = new List<string>();
			assemblyNames.AddRange(_standardTagRegistrations.Where(r => !string.IsNullOrEmpty(r.AssemblyFilename)).Select(r => r.AssemblyFilename).Distinct());
			assemblyNames.AddRange(webConfigReader.TagRegistrations.Where(r => !string.IsNullOrEmpty(r.AssemblyFilename)).Select(r => r.AssemblyFilename).Distinct());
			string dllFullPath = Path.GetFullPath(websiteDllFileName);
			assemblyNames.Add(dllFullPath);
			string assemblyDirectory = Path.GetDirectoryName(dllFullPath);
			assemblyLoader.PreloadAssemblies(compileContext, assemblyNames, assemblyDirectory);
			assemblyLoader.PrimaryAssembly = assemblyLoader[dllFullPath];

			// Add the default tag registrations, including those from System.Web and any declared in the "web.config".
			List<TagRegistration> tagRegistrations = new List<TagRegistration>();
			tagRegistrations.AddRange(_standardTagRegistrations);
			tagRegistrations.AddRange(webConfigReader.TagRegistrations);

			// Spin through any user controls that were declared in the web.config and connect them to their actual
			// .NET class types via reflection.
			compileContext.Verbose("Resolving user controls declared in the web.config.");
			compileContext.VerboseNesting++;
			ResolveUserControls(compileContext, tagRegistrations, assemblyLoader, assemblyDirectory, rootPath, rootPath);
			compileContext.VerboseNesting--;
			compileContext.Verbose(string.Empty);

			// Now that all the setup is done, load and parse each individual markup file into its own .designer.cs output file.
			foreach (string filename in filenames)
			{
				compileContext.Verbose("Begin processing \"{0}\"...", filename);
				compileContext.Verbose("");
				compileContext.VerboseNesting++;

				GenerateDesignerForFilename(compileContext, filename, tagRegistrations, assemblyLoader, assemblyDirectory, rootPath);

				compileContext.VerboseNesting--;
				compileContext.Verbose("");
				compileContext.Verbose("End processing \"{0}\".", filename);
			}
		}

		/// <summary>
		/// Generate a replacement .designer.cs file for the given markup file, overwriting the existing
		/// .designer.cs file if there is one.
		/// </summary>
		public static void GenerateDesignerForFilename(ICompileContext compileContext, string filename, IEnumerable<TagRegistration> tagRegistrations, AssemblyLoader assemblies, string assemblyDirectory, string rootPath)
		{
			string designer;
			string designerFilename = filename + ".designer.cs";

			// Load the markup from the .aspx or .ascx file.
			MarkupReader markup = new MarkupReader();
			MarkupInfo markupInfo;
			try
			{
				markupInfo = markup.LoadMarkup(compileContext, filename, tagRegistrations, assemblies, assemblyDirectory, rootPath);
			}
			catch (Exception e)
			{
				compileContext.Error("{0}: Failed to load markup file:\r\n{1}", filename, e.Message);
				compileContext.Verbose("Stopping file processing due to exception.  Stack trace:\r\n{0}", e.StackTrace);
				return;
			}

			// Generate the output text for the new .designer.cs file.
			try
			{
				DesignerWriter designerWriter = new DesignerWriter();
				designer = designerWriter.CreateDesigner(compileContext, markupInfo);
			}
			catch (Exception e)
			{
				compileContext.Error("{0}: Cannot regenerate designer file:\r\n{1}", filename, e.Message);
				compileContext.Verbose("Stopping file processing due to exception.  Stack trace:\r\n{0}", e.StackTrace);
				return;
			}

			// Save the output .designer.cs file to disk.
			try
			{
				File.WriteAllText(designerFilename, designer, Encoding.UTF8);
			}
			catch (Exception e)
			{
				compileContext.Error("{0}: Cannot open designer file for writing:\r\n{1}", designerFilename, e.Message);
				compileContext.Verbose("Stopping file processing due to exception.  Stack trace:\r\n{0}", e.StackTrace);
				return;
			}
		}

		/// <summary>
		/// Given a set of tag registrations for user controls, attempt to connect those tag registrations to actual
		/// .NET class types in the main website assembly.  This will update the Typename field for each TagRegistration
		/// where a matching class type is found; or if no matching class type is found, this will throw an exception.
		/// </summary>
		/// <param name="compileContext">The context in which errors should be reported.</param>
		/// <param name="tagRegistrations">The set of user-control registrations to resolve to real class types.</param>
		/// <param name="assemblies">The full set of preloaded assemblies.</param>
		/// <param name="assemblyDirectory">The directory where the main website DLL can be found.</param>
		/// <param name="rootPath">The real disk path to the root of the website's virtual directory.</param>
		/// <param name="currentDirectory">The current directory (for resolving relative paths).</param>
		public static void ResolveUserControls(ICompileContext compileContext, IEnumerable<TagRegistration> tagRegistrations, AssemblyLoader assemblies, string assemblyDirectory, string rootPath, string currentDirectory)
		{
			foreach (TagRegistration tagRegistration in tagRegistrations.Where(t => t.Kind == TagRegistrationKind.SingleUserControl))
			{
				compileContext.Verbose("Registering user control <{0}:{1}> as \"{2}\".", tagRegistration.TagPrefix, tagRegistration.TagName, tagRegistration.SourceFilename);

				compileContext.VerboseNesting++;

				string filename = ResolveWebsitePath(compileContext, tagRegistration.SourceFilename, rootPath, currentDirectory);

				MarkupReader userControlReader = new MarkupReader();
				Tag userControlMainDirective = userControlReader.ReadMainDirective(compileContext, filename, assemblies, assemblyDirectory, rootPath);

				if (string.IsNullOrEmpty(userControlMainDirective.TagName)
					&& string.Compare(userControlMainDirective.TagName, "control", StringComparison.InvariantCultureIgnoreCase) != 0)
				{
					throw new RedesignerException("Cannot register user control \"{0}\":  Its main <% ... %> directive does not start with the \"Control\" keyword.  Is this actually a user control?", tagRegistration.SourceFilename);
				}

				string inheritsAttribute = userControlMainDirective["inherits"];
				if (string.IsNullOrEmpty(inheritsAttribute))
				{
					throw new RedesignerException("Cannot register user control \"{0}\":  Its main <% Control ... %> directive is missing the required Inherits=\"...\" attribute.", tagRegistration.SourceFilename);
				}

				tagRegistration.Typename = inheritsAttribute;

				compileContext.Verbose("User control registered as type \"{0}\".", inheritsAttribute);
				compileContext.VerboseNesting--;
			}
		}

		/// <summary>
		/// Given a virtual filename (possibly either rooted or relative), and a root path to the website,
		/// and the current disk directory, return a complete disk path for that virtual filename.
		/// </summary>
		/// <param name="compileContext">The context in which this is being resolved (for error-reporting).</param>
		/// <param name="virtualFilename">The virtual filename to resolve to a disk path to a real file.</param>
		/// <param name="rootPath">The root disk path to the website.</param>
		/// <param name="currentDirectory">The current directory (for evaluating relative paths).</param>
		/// <returns>The fully-resolved disk path to the given file.</returns>
		public static string ResolveWebsitePath(ICompileContext compileContext, string virtualFilename, string rootPath, string currentDirectory)
		{
			string filename;

			if (virtualFilename.StartsWith("~/") || virtualFilename.StartsWith(@"~\"))
			{
				// Rooted virtual path.
				filename = virtualFilename.Substring(2).Replace('/', '\\');
				return Path.Combine(rootPath, filename);
			}

			if (virtualFilename.StartsWith("/") || virtualFilename.StartsWith(@"\"))
				throw new RedesignerException("Illegal virtual path \"{0}\".  Virtual paths should be relative to the current path, or should start with \"~/\".", virtualFilename);

			// Relative virtual path.
			filename = virtualFilename.Replace('/', '\\');
			return Path.Combine(currentDirectory, filename);
		}

		/// <summary>
		/// Given a string, return another string where the original is repeated 'count' times.
		/// </summary>
		public static string RepeatString(string str, int count)
		{
			switch (count)
			{
				case 0: return string.Empty;
				case 1: return str;
				case 2: return str + str;
				case 3: return str + str + str;

				default:
					StringBuilder stringBuilder = new StringBuilder();
					for (int i = 0; i < count; i++)
					{
						stringBuilder.Append(str);
					}
					return stringBuilder.ToString();
			}
		}
	}
}
