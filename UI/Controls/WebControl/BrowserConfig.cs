// ******************************************************************************************
//     Assembly:                Badger
//     Author:                  Terry D. Eppler
//     Created:                 09-09-2024
// 
//     Last Modified By:        Terry D. Eppler
//     Last Modified On:        09-09-2024
// ******************************************************************************************
// <copyright file="BrowserConfig.cs" company="Terry D. Eppler">
//     Baby is a light-weight, full-featured, web-browser built with .NET 6 and is written
//     in C#.  The baby browser is designed for budget execution and data analysis.
//     A tool for EPA analysts and a component that can be used for general browsing.
// 
//     Copyright ©  2020 Terry D. Eppler
// 
//    Permission is hereby granted, free of charge, to any person obtaining a copy
//    of this software and associated documentation files (the “Software”),
//    to deal in the Software without restriction,
//    including without limitation the rights to use,
//    copy, modify, merge, publish, distribute, sublicense,
//    and/or sell copies of the Software,
//    and to permit persons to whom the Software is furnished to do so,
//    subject to the following conditions:
// 
//    The above copyright notice and this permission notice shall be included in all
//    copies or substantial portions of the Software.
// 
//    THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
//    INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//    FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT.
//    IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
//    DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
//    ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
//    DEALINGS IN THE SOFTWARE.
// 
//    You can contact me at:  terryeppler@gmail.com or eppler.terry@epa.gov
// </copyright>
// <summary>
//   BrowserConfig.cs
// </summary>
// ******************************************************************************************

namespace Bocifus
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using CefSharp;

    /// <summary>
    /// 
    /// </summary>
    [ SuppressMessage( "ReSharper", "MemberCanBeInternal" ) ]
    [ SuppressMessage( "ReSharper", "FieldCanBeMadeReadOnly.Global" ) ]
    [ SuppressMessage( "ReSharper", "InconsistentNaming" ) ]
    [ SuppressMessage( "ReSharper", "UnusedType.Global" ) ]
    public static class BrowserConfig
    {
        /// <summary>
        /// 
        /// </summary>
        public static string Branding = "Baby Browser";

        /// <summary>
        /// 
        /// </summary>
        public static string AcceptLanguage = "en-US,en;q=0.9";

        /// <summary>
        ///  UserAgent to fix issue with Google account authentication
        /// </summary>
        public static string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64)"
            + " AppleWebKit/537.36 (KHTML, like Gecko)"
            + " Chrome/104.0.5112.102 Safari/537.36 /CefSharp Browser" + Cef.CefSharpVersion;

        /// <summary>
        /// 
        /// </summary>
        public static string Homepage =
            "file:///C:/Users/terry/source/repos/Baby/WebPages/index.html";

        /// <summary>
        /// 
        /// </summary>
        public static string NewTab = "https://www.google.com";

        /// <summary>
        /// 
        /// </summary>
        public static string Internal = "baby";

        /// <summary>
        /// 
        /// </summary>
        public static string Downloads = "baby://storage/downloads.html";

        /// <summary>
        /// 
        /// </summary>
        public static string FileNotFound = "baby://storage/errors/notFound.html";

        /// <summary>
        /// 
        /// </summary>
        public static string CannotConnect = "baby://storage/errors/cannotConnect.html";

        /// <summary>
        /// 
        /// </summary>
        public static string Google = "https://www.google.com/search?q=";

        /// <summary>
        /// 
        /// </summary>
        public static string EPA = "https://www.google.com/search?q=site:epa.gov ";

        /// <summary>
        /// 
        /// </summary>
        public static string GPO = "https://www.google.com/search?q=site:gpo.gov ";

        /// <summary>
        /// 
        /// </summary>
        public static string GovInfo = "https://www.google.com/search?q=site:govinfo.gov ";

        /// <summary>
        /// The congressional search URL
        /// </summary>
        public static string CRS = "https://www.google.com/search?q=site:crsreports.congress.gov ";

        /// <summary>
        /// 
        /// </summary>
        public static string LOC = "https://www.google.com/search?q=site:loc.gov ";

        /// <summary>
        /// 
        /// </summary>
        public static string OMB = "https://www.google.com/search?q=site:whitehouse.gov ";

        /// <summary>
        /// 
        /// </summary>
        public static string Treasury = "https://www.google.com/search?q=site:fiscal.treasury.gov ";

        /// <summary>
        /// 
        /// </summary>
        public static string NASA = "https://www.google.com/search?q=site:nasa.gov ";

        /// <summary>
        /// 
        /// </summary>
        public static string NOAA = "https://www.google.com/search?q=site:noaa.gov ";

        /// <summary>
        /// 
        /// </summary>
        public static string PyPI = "https://www.google.com/search?q=site:pypi.org ";

        /// <summary>
        /// 
        /// </summary>
        public static string NuGet = "https://www.google.com/search?q=site:nuget.org ";

        /// <summary>
        /// 
        /// </summary>
        public static string GitHub = "https://www.google.com/search?q=site:github.com ";

        /// <summary>
        /// 
        /// </summary>
        public static bool WebSecurity = true;

        /// <summary>
        /// 
        /// </summary>
        public static bool CrossDomainSecurity = true;

        /// <summary>
        /// 
        /// </summary>
        public static bool WebGl = true;

        /// <summary>
        /// 
        /// </summary>
        public static bool ApplicationCache = true;

        /// <summary>
        /// 
        /// </summary>
        public static bool Proxy = false;

        /// <summary>
        /// 
        /// </summary>
        public static string ProxyIp = "123.123.123.123";

        /// <summary>
        /// 
        /// </summary>
        public static int ProxyPort = 123;

        /// <summary>
        /// 
        /// </summary>
        public static string ProxyUsername = "username";

        /// <summary>
        /// 
        /// </summary>
        public static string ProxyPassword = "pass";

        /// <summary>
        /// 
        /// </summary>
        public static string ProxyBypassList = "";
    }
}