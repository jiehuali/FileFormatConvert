using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
// https://docs.microsoft.com/zh-cn/visualstudio/extensibility/shipping-visual-studio-extensions?view=vs-2019
using EnvDTE;
using EnvDTE80;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.Shell.Interop;

namespace UTF8_VSIX
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [Guid(UTF8_VSIXPackage.PackageGuidString)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists)]

    public sealed class UTF8_VSIXPackage : Package
    {
        /// <summary>
        /// UTF8_VSIXPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "5d023c80-7183-4795-958e-8ed86b95a591";

        /// <summary>
        /// Initializes a new instance of the <see cref="UTF8_VSIXPackage"/> class.
        /// </summary>
        public UTF8_VSIXPackage()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
        }

        #region Package Members

        private EnvDTE.DocumentEvents documentEvents;

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

            var dte = GetService(typeof(DTE)) as DTE2;
            documentEvents = dte.Events.DocumentEvents;
            documentEvents.DocumentOpened += DocumentEvents_DocumentSaved;
            //documentEvents.DocumentSaved += DocumentEvents_DocumentSaved;
        }

        private void DocumentEvents_DocumentSaved(Document document)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            //MessageBox.Show(msg2, "Example Output");
            if (document.Kind != "{8E7B96A8-E33D-11D0-A6D5-00C04FB67F6A}")
            {
                // then it's not a text file
                return;
            }

            var path = document.FullName;
            var stream = new FileStream(path, FileMode.Open);
            var reader = new StreamReader(stream, System.Text.Encoding.Default, true);
            reader.Read();

            if (reader.CurrentEncoding != System.Text.Encoding.Default)
            {
                stream.Close();
                return;
            }

            string text;

            try
            {
                stream.Position = 0;
                reader = new StreamReader(stream, new UTF8Encoding(true, true));
                reader.ReadToEnd();
                stream.Close();

            }catch(DecoderFallbackException)
            {
                stream.Position = 0;
                reader = new StreamReader(stream, System.Text.Encoding.Default);
                text = reader.ReadToEnd();
                stream.Close();

                File.WriteAllText(path, text, new UTF8Encoding(true));
            }


            throw new NotImplementedException();
        }

        #endregion
    }
}
