//------------------------------------------------------------------------------
// <copyright file="AtPackage.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.Shell;

namespace At
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
    [InstalledProductRegistration("#110","#112","1.0",IconResourceID = 400)] // Info on this package for Help/About
    [Guid(PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules","SA1650:ElementDocumentationMustBeSpelledCorrectly",Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideService(typeof(AtLanguageService),ServiceName = "At Language Service")]
    [ProvideLanguageService(typeof(AtLanguageService),
                                     "At",
                                     106,             // resource ID of localized language name
                                     CodeSense = true,             // Supports IntelliSense
                                     RequestStockColors = true,    // false=custom colors
                                     EnableCommenting = true,      // Supports commenting out code
                                     EnableAsyncCompletion = true  // Supports background parsing
                                     )]
     [ProvideLanguageCodeExpansion(
             typeof(AtLanguageService),
             "At", // Name of language used as registry key.
             106,           // Resource ID of localized name of language service.
             "at",  // language key used in snippet templates.
             @"%InstallRoot%\At\SnippetsIndex.xml",  // Path to snippets index
             SearchPaths = @"%InstallRoot%\At\Snippets\%LCID%\Snippets\;" +
                           @"%TestDocs%\Code Snippets\At\At Code Snippets"
             )]
    [ProvideLanguageExtension(typeof(AtLanguageService),".at")]
    /*b
    [ProvideLanguageEditorOptionPageAttribute(
             "At Language",  // Registry key name for language
             "Options",      // Registry key name for property page
             "#242",         // Localized name of property page
             OptionPageGuid = "{A2FE74E1-FFFF-3311-4342-123052450768}"  // GUID of property page
             )]
    [ProvideLanguageEditorOptionPageAttribute(
             "At Language",  // Registry key name for language
             "Advanced",     // Registry key name for node
             "#243",         // Localized name of node
             )]
    [ProvideLanguageEditorOptionPageAttribute(
             "At Language",  // Registry key name for language
             @"Advanced\Indenting",     // Registry key name for property page
             "#244",         // Localized name of property page
             OptionPageGuid = "{A2FE74E2-FFFF-3311-4342-123052450768}"  // GUID of property page
             )]    
    [ProvideAutoLoad("ADFC4E64-0397-11D1-9F4E-00A0C911004F")]*/
    public sealed class AtPackage : Package, IOleComponent
    {
        uint componentId;

        /// <summary>AtPackage GUID string.</summary>
        public const string PackageGuidString = "0edabeea-182a-48c3-98ee-e0d57ff99d60";

        /// <summary>Initializes a new instance of the <see cref="AtPackage"/> class.</summary>
        public AtPackage()
        {
            Trace.WriteLine("new AtPackage()");  

            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
        }


        /// <summary>Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.</summary>
        protected override void Initialize()
        {
            Trace.WriteLine("AtPackage.Initialize()"); 
            base.Initialize();

            var svcContainer = (IServiceContainer) this;
            var langSvc = new AtLanguageService();
            langSvc.SetSite(this);
            svcContainer.AddService(typeof(AtLanguageService),langSvc,promote:true);

            // Registers a timer to call language service during idle periods.
            var mgr = (IOleComponentManager) GetService(typeof(SOleComponentManager));
            if (componentId == 0 && mgr != null)
            {
               var crinfo = new OLECRINFO[1];
                crinfo[0].cbSize            = (uint)Marshal.SizeOf(typeof(OLECRINFO));
                crinfo[0].grfcrf            = (uint)_OLECRF.olecrfNeedIdleTime |
                                              (uint)_OLECRF.olecrfNeedPeriodicIdleTime;
                crinfo[0].grfcadvf          = (uint)_OLECADVF.olecadvfModal     |
                                              (uint)_OLECADVF.olecadvfRedrawOff |
                                              (uint)_OLECADVF.olecadvfWarningsOff;
                crinfo[0].uIdleTimeInterval = 1000;

                int hr = mgr.FRegisterComponent(this, crinfo, out componentId);
            }
        }

        public int FDoIdle(uint grfidlef)
        {
            //Trace.Write($"AtPackage.FDoIdle({grfidlef})"); 
            bool bPeriodic = (grfidlef & (uint)_OLEIDLEF.oleidlefPeriodic) != 0;

            // Use typeof(AtLanguageService) because we need to
            // reference the GUID for our language service.
            LanguageService service = GetService(typeof(AtLanguageService)) as LanguageService;
            if (service != null)
            {
                service.OnIdle(bPeriodic);
            }

            return 0;
        }



        public int FContinueMessageLoop(uint uReason,
                                        IntPtr pvLoopData,
                                        MSG[] pMsgPeeked)
        {
            return 1;
        }

        public int FPreTranslateMessage(MSG[] pMsg)
        {
            return 0;
        }

        public int FQueryTerminate(int fPromptUser)
        {
            return 1;
        }

        public int FReserved1(uint dwReserved,
                              uint message,
                              IntPtr wParam,
                              IntPtr lParam)
        {
            return 1;
        }

        public IntPtr HwndGetWindow(uint dwWhich, uint dwReserved)
        {
            return IntPtr.Zero;
        }

        public void OnActivationChange(IOleComponent pic,
                                       int fSameComponent,
                                       OLECRINFO[] pcrinfo,
                                       int fHostIsActivating,
                                       OLECHOSTINFO[] pchostinfo,
                                       uint dwReserved)
        {
        }

        public void OnAppActivate(int fActive, uint dwOtherThreadID)
        {
        }

        public void OnEnterState(uint uStateID, int fEnter)
        {
        }

        public void OnLoseActivation()
        {
        }

        public void Terminate()
        {
        }
    }
}
