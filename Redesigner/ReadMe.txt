
Redesigner

  Version 1.0

  Copyright (c) 2012 by Sean Werkema
  All rights reserved.

-------------------------------------------------------------------------------------------------

OVERVIEW

  ASP.NET is a powerful website development framework, but the .designer files it requires can
  be a pain to work with. They can often get broken, corrupted, or out-of-sync with the markup,
  leading to complicated problems that are often difficult to resolve.

  Redesigner is a command-line tool that can read and parse .aspx and .ascx files and generate
  .designer files for them. It runs quickly and reliably, and can display lots of useful
  information about how the .designer file was generated. It does not use or require Visual
  Studio, either!

  It has an open-source license (the New BSD license) and can be reused anywhere for nearly any
  purpose, for free!

-------------------------------------------------------------------------------------------------

USAGE

  Redesigner is a command-line tool, and is invoked like this:
  
    Redesigner.exe [-w website.dll] [-r path\to\root\of\website] [options] files.aspx ...

  A simple use to regenerate a .designer file for MyPage.aspx might look like this:

    Redesigner.exe -w MyWebsite\bin\Release\MyWebsite.dll -r MyWebsite MyWebsite\MyPage.aspx

  Note:  Your website *must* be able to be compiled!  Redesigner *must* be able to
  reflect against your website's DLL, so if you can't compile your website's DLL, do whatever
  you have to do to hack your website until you can get it to compile.
	
  Currently, Redesigner can generate .designer.cs files for .aspx and .ascx files.  It can
  handle <%@ Register %> directives found in markup, and registered namespaces and user
  controls found in your website's web.config.  It can handle server controls, user controls,
  and HTML controls, data-bound controls and template controls, and complex nested control
  properties.  It uses the same parsing regular expressions ASP.NET uses to ensure that it
  will analyze your markup the same way that ASP.NET will.

  If you want detailed information about what Redesigner is doing, you can add the --verbose
  option to the command-line.  This will cause Redesigner to print out a huge quantity of
  text describing what it found in your markup, where it found the controls while reflecting,
  and lots of useful information that may be valuable when tracking down errors in your markup.
  The --verbose option prints a lot of text, but that's better than just saying "Exception of
  type System.Exception was thrown", isn't it? :-)
  
-------------------------------------------------------------------------------------------------

OPTIONS

  --help            Show help on usage and command-line options.

  --website website.dll
  -w website.dll    Specify the path to the DLL which contains the website's code-behind.

  --root pathname
  -r pathname       Specify the path to the website's root (this is where the "web.config" file
                    should be located).

  --verify          Verify that the existing designer file(s) were generated correctly.

  --generate        Generate replacement designer file(s) for the given pages/controls.  (This
                    is the default behavior.)

-------------------------------------------------------------------------------------------------

TODO
  
  Future plans include:
  
    * Support for reading and processing .master files.

    * Outputting .designer.vb files (not just .designer.cs files).

    * A verification mode, for ensuring that existing .designer files are valid/undamaged.

    * Recursive walk through an entire project or a set of folders to generate missing,
       regenerate, or verify designer files for a large set of pages and controls.

    * Detection of control declarations that have been moved out of the .designer file and
       into the code-behind.
	   
-------------------------------------------------------------------------------------------------

LICENSE

  This software is released under the terms of the "New BSD License," as follows:

  Redistribution and use in source and binary forms, with or without modification, are permitted
  provided that the following conditions are met:

   * Redistributions of source code must retain the above copyright notice, this list of
     conditions and the following disclaimer.

   * Redistributions in binary form must reproduce the above copyright notice, this list of
     conditions and the following disclaimer in the documentation and/or other materials
     provided with the distribution.

  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR
  IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
  AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR
  CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
  CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
  SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
  THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
  OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
  POSSIBILITY OF SUCH DAMAGE.
