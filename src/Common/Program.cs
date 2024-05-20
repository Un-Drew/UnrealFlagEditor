using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UnrealFlagEditor
{
	internal static class Program
	{
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
		static int Main(string[] args)
		{
            if (args.Length > 0 && string.Compare(args[0], "-PrintTest", true) == 0)
			{
                AttachToParentConsole();
                DoPrintTest(args);

                return 0;
            }

            string headlessInstructions = null;
            string package = null;

			System.Collections.IEnumerator enumerator = args.GetEnumerator();

			while (enumerator.MoveNext())
			{
				string current = (string)enumerator.Current;
                switch (current.ToUpperInvariant())
				{
					case "-HEADLESSINSTRUCTIONS":
						if (!enumerator.MoveNext())
						{
                            AttachToParentConsole();
                            Console.ForegroundColor = ConsoleColor.Red;
                            WriteConsoleMessage(@"ERROR: Expected path argument after '-HeadlessInstructions'. e.g.: -HeadlessInstructions C:\My\File\Path.xml");
                            Console.ResetColor();
                            return 1;
						}
						headlessInstructions = (string)enumerator.Current;
						break;
					case "-PACKAGE":
                        if (!enumerator.MoveNext())
                        {
                            AttachToParentConsole();
                            Console.ForegroundColor = ConsoleColor.Red;
                            WriteConsoleMessage(@"ERROR: Expected path argument after '-Package'. e.g.: -Package C:\My\File\Path.u");
                            Console.ResetColor();
                            return 1;
                        }
                        package = (string)enumerator.Current;
						break;
					default:
                        // Unrecognised argument. This could be a file argument passed by "Open With" in Windows.
                        package = null;
                        try
                        {
                            if (File.Exists(current))
                            {
                                package = current;
                            }
                        }
                        catch { }
                        if (package == null)
                        {
                            AttachToParentConsole();
                            Console.ForegroundColor = ConsoleColor.Red;
                            WriteConsoleMessage($"ERROR: Unexpected argument {current}");
                            Console.ResetColor();
                            return 1;
                        }
                        break;
                }
			}

            if (headlessInstructions != null)
            {
                AttachToParentConsole();

                if (package == null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    WriteConsoleMessage($"ERROR: A package is required for headless mode. Please specify one using a '-Package' argument.");
                    Console.ResetColor();
                    return 1;
                }

                return EditorHeadless.PerformHeadlessXmlFile(package, headlessInstructions);
            }
            else
            {
                OpenTheForm(package);
                return 0;
            }
		}

        static public void DoPrintTest(string[] args)
        {
            WriteConsoleMessage("");

            foreach (var a in args)
            {
                WriteConsoleMessage(a);
            }
            WriteConsoleMessage("");

            WriteConsoleMessage($"Current working directory: {Directory.GetCurrentDirectory()}");

            WriteConsoleMessage("");
        }

		static public void OpenTheForm(string package)
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new EditorForm(package));
		}

        static public void WriteConsoleMessage(string s)
        {
            Console.WriteLine($"[FLAG EDITOR] {s}");
        }

        [DllImport("kernel32.dll")]
        static extern bool AttachConsole(int dwProcessId);
        private const int ATTACH_PARENT_PROCESS = -1;

        // Allows the app to print using Console.WriteLine in (what's likely) the cmd prompt that opened this app.
        static public void AttachToParentConsole()
        {
            AttachConsole(ATTACH_PARENT_PROCESS);
        }
	}
}

/*
 * A log of Hat versions.
 * 
 * +--------------------------------------------------+-----+-------+---------+-------------+----------------+----------------+
 * | Build - M:Manifest (Name)                        | Ver.| Licen.| Eng. V. |   Cook. V.  | Custom Specif. | Optionals List |
 * +--------------------------------------------------+-----+-------+---------+-------------+----------------+----------------+
 * +--------------------------------------------------+-----+-------+---------+-------------+----------------+----------------+
 * | M:2270812390036347554 (Current)                  | 893 |   5   |  12097  |     137     | TRUE           | TRUE           |
 * +--------------------------------------------------+-----+-------+---------+-------------+----------------+----------------+
 * | ................................................ |     |       |         |             |                |                |
 * +--------------------------------------------------+-----+-------+---------+-------------+----------------+----------------+
 * | M:2100581822495945111 (DLC 1)                    | 882 |   5   |  12097  |     136     | TRUE           | TRUE           |
 * +--------------------------------------------------+-----+-------+---------+-------------+----------------+----------------+
 * | ................................................ |     |       |         |             |                |                |
 * +--------------------------------------------------+-----+-------+---------+-------------+----------------+----------------+
 * | M:2130909360457865983 (ModdingBeta 6 Jan 2018)   | 878 |   5   |  12097  |     136     | TRUE           | TRUE           |
 * +--------------------------------------------------+-----+-------+---------+-------------+----------------+----------------+
 * | ................................................ |     |       |         |             |                |                |
 * +--------------------------------------------------+-----+-------+---------+-------------+----------------+----------------+
 * | M:5462424799735342480 (ModdingBeta 1 Nov 2017)   | 877 |   5   |  12097  |     136     | TRUE           | FALSE          |
 * +--------------------------------------------------+-----+-------+---------+-------------+----------------+----------------+
 * | ................................................ |     |       |         |             |                |                |
 * +--------------------------------------------------+-----+-------+---------+-------------+----------------+----------------+
 * | M:6348723431270073514 (ModdingBeta 26 Oct 2017)  | 877 |   5   |  12097  |     136     | TRUE           | FALSE          |
 * +--------------------------------------------------+-----+-------+---------+-------------+----------------+----------------+
 * | ................................................ |     |       |         |             |                |                |
 * +--------------------------------------------------+-----+-------+---------+-------------+----------------+----------------+
 * | M:4435011916194978256 (1.0)                      | 877 |   5   |  12097  |     136     | TRUE           | FALSE          |
 * +--------------------------------------------------+-----+-------+---------+-------------+----------------+----------------+
 * | ................................................ |     |       |         |             |                |                |
 * +--------------------------------------------------+-----+-------+---------+-------------+----------------+----------------+
 * | (Betas)                                          | 868 |   0   |  12097  |     136     | FALSE          | FALSE          |
 * +--------------------------------------------------+-----+-------+---------+-------------+----------------+----------------+
 * | ................................................ |     |       |         |             |                |                |
 * +--------------------------------------------------+-----+-------+---------+-------------+----------------+----------------+
 * | (Alphas)                                         | 867 |   0   |  10900  |     136     | FALSE          | FALSE          |
 * +--------------------------------------------------+-----+-------+---------+-------------+----------------+----------------+
 */

