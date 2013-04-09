using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace Amido.Azure.Resources
{
    internal static class SecUtility {
        internal const int Infinite = Int32.MaxValue;

        internal static bool ValidateParameter(ref string param, bool checkForNull, bool checkIfEmpty, bool checkForCommas, int maxSize) {
            if(param == null) {
                return !checkForNull;
            }

            param = param.Trim();
            if((checkIfEmpty && param.Length < 1) ||
                 (maxSize > 0 && param.Length > maxSize) ||
                 (checkForCommas && param.Contains(","))) {
                return false;
            }

            return true;
        }

        internal static void CheckParameter(ref string param, bool checkForNull, bool checkIfEmpty, bool checkForCommas, int maxSize, string paramName) {
            if(param == null) {
                if(checkForNull) {
                    throw new ArgumentNullException(paramName);
                }

                return;
            }

            param = param.Trim();
            if(checkIfEmpty && param.Length < 1) {
                throw new ArgumentException(string.Format(CultureInfo.InstalledUICulture, "The parameter '{0}' must not be empty.", paramName), paramName);
            }

            if(maxSize > 0 && param.Length > maxSize) {
                throw new ArgumentException(string.Format(CultureInfo.InstalledUICulture, "The parameter '{0}' is too long: it must not exceed {1} chars in length.", paramName, maxSize.ToString(CultureInfo.InvariantCulture)), paramName);
            }

            if(checkForCommas && param.Contains(",")) {
                throw new ArgumentException(string.Format(CultureInfo.InstalledUICulture, "The parameter '{0}' must not contain commas.", paramName), paramName);
            }
        }

        internal static void CheckArrayParameter(ref string[] param, bool checkForNull, bool checkIfEmpty, bool checkForCommas, int maxSize, string paramName) {
            if(param == null) {
                throw new ArgumentNullException(paramName);
            }

            if(param.Length < 1) {
                throw new ArgumentException(string.Format(CultureInfo.InstalledUICulture, "The array parameter '{0}' should not be empty.", paramName), paramName);
            }

            var values = new Hashtable(param.Length);
            for(int i = param.Length - 1; i >= 0; i--) {
                CheckParameter(ref param[i], checkForNull, checkIfEmpty, checkForCommas, maxSize,
                    paramName + "[ " + i.ToString(CultureInfo.InvariantCulture) + " ]");
                if(values.Contains(param[i])) {
                    throw new ArgumentException(string.Format(CultureInfo.InstalledUICulture, "The array '{0}' should not contain duplicate values.", paramName), paramName);
                }
                
                values.Add(param[i], param[i]);
            }
        }

        // the table storage system currently does not support the StartsWith() operation in 
        // queries. As a result we transform s.StartsWith(substring) into s.CompareTo(substring) > 0 && 
        // s.CompareTo(NextComparisonString(substring)) < 0
        // we assume that comparison on the service side is as ordinal comparison
        internal static string NextComparisonString(string s) {
            if(string.IsNullOrEmpty(s)) 
            {
                throw new ArgumentException("The string argument must not be null or empty!");
            }
            var last = s[s.Length - 1];
            if(last + 1 > char.MaxValue) 
            {
                throw new ArgumentException("Cannot convert the string.");
            }
            // don't use "as" because we want to have an explicit exception here if something goes wrong
            last = (char)(last + 1);
            var ret = s.Substring(0, s.Length - 1) + last;
            return ret;
        }

        // we use a normal character as the separator because of string comparison operations
        // these have to be valid characters
        internal const char KeySeparator = 'a';
        internal static readonly string KeySeparatorString = new string(KeySeparator, 1);
        internal const char EscapeCharacter = 'b';
        internal static readonly string EscapeCharacterString = new string(EscapeCharacter, 1);

        // Some characters can cause problems when they are contained in columns 
        // that are included in queries. We are very defensive here and escape a wide range 
        // of characters for key columns (as the key columns are present in most queries)
        internal static bool IsInvalidKeyCharacter(char c) {
            return ((c < 32)
                || (c >= 127 && c < 160)
                || (c == '#')
                || (c == '&')
                || (c == '+')
                || (c == '/')
                || (c == '?')
                || (c == ':')
                || (c == '%')
                || (c == '\\')
                );
        }

        internal static string CharToEscapeSequence(char c)
        {
            var ret = EscapeCharacterString + string.Format(CultureInfo.InvariantCulture, "{0:X2}", (int)c);
            return ret;
        }

        internal static string Escape(string s) {
            if(string.IsNullOrEmpty(s)) {
                return s;
            }
            var ret = new StringBuilder();
            foreach(var c in s) {
                if(c == EscapeCharacter || c == KeySeparator || IsInvalidKeyCharacter(c)) {
                    ret.Append(CharToEscapeSequence(c));
                }
                else {
                    ret.Append(c);
                }
            }
            return ret.ToString();
        }

        internal static string UnEscape(string s) {
            if(string.IsNullOrEmpty(s)) {
                return s;
            }
            var ret = new StringBuilder();
            for(var i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if(c == EscapeCharacter) {
                    if(i + 2 >= s.Length) {
                        throw new FormatException("The string " + s + " is not correctly escaped!");
                    }
                    int ascii = Convert.ToInt32(s.Substring(i + 1, 2), 16);
                    ret.Append((char)ascii);
                    i += 2;
                }
                else {
                    ret.Append(c);
                }
            }
            return ret.ToString();
        }

        internal static string CombineToKey(string s1, string s2) {
            return s1.ToLower() + s2.ToLower();
            //return Escape(s1) + KeySeparator + Escape(s2);
        }

        internal static string EscapedFirst(string s) {
            return Escape(s) + KeySeparator;
        }

        internal static string GetFirstFromKey(string key) {
            Debug.Assert(key.IndexOf(KeySeparator) != -1);
            string first = key.Substring(0, key.IndexOf(KeySeparator));
            return UnEscape(first);
        }

        internal static string GetSecondFromKey(string key) {
            Debug.Assert(key.IndexOf(KeySeparator) != -1);
            string second = key.Substring(key.IndexOf(KeySeparator) + 1);
            return UnEscape(second);
        }
    }


    /// <summary>
    /// This delegate defines the shape of a provider retry policy. 
    /// Provider retry policies are only used to retry when a row retrieved from a table 
    /// was changed by another entity before it could be saved to the data store.A retry policy will invoke the given
    /// <paramref name="action"/> as many times as it wants to in the face of 
    /// retriable InvalidOperationExceptions.
    /// </summary>
    /// <param name="action">The action to retry</param>
    /// <returns></returns>
    public delegate void ProviderRetryPolicy(Action action);


    /// <summary>
    /// We are using this retry policies for only one purpose: the ASP providers often read data from the server, process it 
    /// locally and then write the result back to the server. The problem is that the row that has been read might have changed 
    /// between the read and write operation. This retry policy is used to retry the whole process in this case.
    /// </summary>
    /// <summary>
    /// Provides definitions for some standard retry policies.
    /// </summary>
    public static class ProviderRetryPolicies {

        public static readonly TimeSpan StandardMinBackoff = TimeSpan.FromMilliseconds(100);
        public static readonly TimeSpan StandardMaxBackoff = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Policy that does no retries i.e., it just invokes <paramref name="action"/> exactly once
        /// </summary>
        /// <param name="action">The action to retry</param>
        /// <returns>The return value of <paramref name="action"/></returns>
        internal static void NoRetry(Action action) {
            action();
        }
    }
}