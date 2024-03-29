﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace YTAutoMusic
{
    using System;


    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources
    {

        private static global::System.Resources.ResourceManager resourceMan;

        private static global::System.Globalization.CultureInfo resourceCulture;

        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources()
        {
        }

        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager
        {
            get
            {
                if (object.ReferenceEquals(resourceMan, null))
                {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("YTAutoMusic.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }

        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture
        {
            get
            {
                return resourceCulture;
            }
            set
            {
                resourceCulture = value;
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to 
        ///&apos;n&apos; - new playlist
        ///Create new playlist from a YouTube playlist. Place files in a new folder.
        ///
        ///&apos;a&apos; - append playlist
        ///Update a playlist to include new music from YouTube playlist. Requires an existing playlist folder.
        ///
        ///&apos;c&apos; - copy playlist
        ///Copy an existing playlist to a new location while preserving the XSPF playlist file
        ///.
        /// </summary>
        internal static string helpText
        {
            get
            {
                return ResourceManager.GetString("helpText", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to How to use a YTAutoMusic playlist:
        ///
        ///1) To play the music in the playlist. Open the XSPF file on your media player of choice. I recommend VLC.
        ///
        ///2) If you update the playlist on YouTube, you can open the batch file &apos;Append Playlist.bat&apos; to automaticly update the local playlist.
        ///
        ///3) Moving the playlist to a different location break functionality. The XSPF file and the mp3 files inside the tracks folder should still work, but the batch file won&apos;t work anymore. To fix this use the copy command in YTAutoMus [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string readmeText
        {
            get
            {
                return ResourceManager.GetString("readmeText", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to What do you want to do?
        ///&apos;n&apos; - new playlist | &apos;a&apos; - append playlist | &apos;q&apos; - quit | &apos;c&apos; copy | &apos;h&apos; - help.
        /// </summary>
        internal static string responses
        {
            get
            {
                return ResourceManager.GetString("responses", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to  __     _________            _        __  __           _      
        /// \ \   / /__   __|/\        | |      |  \/  |         (_)     
        ///  \ \_/ /   | |  /  \  _   _| |_ ___ | \  / |_   _ ___ _  ___ 
        ///   \   /    | | / /\ \| | | | __/ _ \| |\/| | | | / __| |/ __|
        ///    | |     | |/ ____ \ |_| | || (_) | |  | | |_| \__ \ | (__ 
        ///    |_|     |_/_/    \_\__,_|\__\___/|_|  |_|\__,_|___/_|\___|
        ///                                                              
        ///-- .- -.. . -... -.-- .- .. -.. . -. -... .-. .- -.. .-.. . -.--.
        /// </summary>
        internal static string splash
        {
            get
            {
                return ResourceManager.GetString("splash", resourceCulture);
            }
        }
    }
}
