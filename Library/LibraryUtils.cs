using System;
using System.IO;
using System.Resources;
using System.Windows;
using System.Xml;
using static Library.Utils;

namespace Library
{
    internal static class LibraryUtils
    {
        #region languages

        /// <summary>
        /// The path to the general languages directory.
        /// </summary>
        internal static readonly string languagesDirectory = "pack://application:,,,/Library;component/Resources/Languages/";

        /// <summary>
        /// Sets the general resource dictionary for the specified window.
        /// </summary>
        /// <param name="w">The window to set the resource dictionary for.</param>
        internal static void setLibraryRDict(this Window w)
        {
            w.Resources.MergedDictionaries.Add(LibraryUtils.getRDict());
        }

        /// <summary>
        /// Gets the general message for the specified key from the resource dictionary for the current language.
        /// </summary>
        /// <param name="key">The key of the message to get.</param>
        /// <returns>The message, or null if the message does not exist.</returns>
        internal static string? getMessage(string key)
        {
            var rDict = LibraryUtils.getRDict();

            return rDict[key].ToString();
        }

        /// <summary>
        /// Gets the general resource dictionary for the current language.
        /// </summary>
        /// <param name="language">The language to get the resource dictionary for.</param>
        /// <returns>The general resource dictionary for the current language, or null if the resource dictionary does not exist.</returns>
        internal static ResourceDictionary getRDict(string? language = null)
        {
            if(language == null)
            {
                language = Utils.Language.get();
            }

            return Utils.Language.getRDict(language, LibraryUtils.languagesDirectory);
        }

        #endregion languages
    }
}