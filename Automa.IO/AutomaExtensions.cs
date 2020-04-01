using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using NetCookie = System.Net.Cookie;

namespace Automa.IO
{
    /// <summary>
    /// AutomaExtensions
    /// </summary>
    public static partial class AutomaExtensions
    {
        #region IWebDriver

        /// <summary>
        /// Javas the script click by element identifier.
        /// </summary>
        /// <param name="driver">The driver.</param>
        /// <param name="by">The by.</param>
        /// <returns>IWebElement.</returns>
        public static IWebElement JavaScriptClickByElementId(this IWebDriver driver, By by = null)
        {
            var element = driver.FindElement(by);
            if (element == null)
                return null;
            var elementId = element.GetAttribute("id");
            ((IJavaScriptExecutor)driver).ExecuteScript($"document.getElementById('{elementId}').click();");
            return element;
        }

        /// <summary>
        /// Waits for display.
        /// </summary>
        /// <param name="driver">The driver.</param>
        /// <param name="millisecondsToWait">The milliseconds to wait.</param>
        /// <param name="by">The by.</param>
        /// <returns>IWebElement.</returns>
        public static IWebElement WaitForDisplay(this IWebDriver driver, int millisecondsToWait = 2000, By by = null)
        {
            var element = driver.FindElement(by);
            if (element.Displayed)
                return element;
            var endTime = DateTime.Now.AddMilliseconds(millisecondsToWait);
            while (!element.Displayed && DateTime.Now <= endTime)
            {
                Thread.Sleep(1);
                element = driver.FindElement(by);
            }
            return element.Displayed ? element : null;
        }

        /// <summary>
        /// Waits for display.
        /// </summary>
        /// <param name="driver">The driver.</param>
        /// <param name="elements">The elements.</param>
        /// <param name="millisecondsToWait">The milliseconds to wait.</param>
        /// <param name="waitBys">The wait bys.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public static bool WaitForDisplay(this IWebDriver driver, out IWebElement[] elements, int millisecondsToWait = 2000, params By[] waitBys)
        {
            elements = waitBys.Select(driver.FindElement).ToArray();
            var endTime = DateTime.Now.AddMilliseconds(millisecondsToWait);
            while (!elements.Any(x => !x.Displayed) && DateTime.Now <= endTime)
            {
                Thread.Sleep(1);
                elements = waitBys.Select(driver.FindElement).ToArray();
            }
            return elements.Any(x => !x.Displayed);
        }

        /// <summary>
        /// Waits for URL.
        /// </summary>
        /// <param name="driver">The driver.</param>
        /// <param name="millisecondsToWait">The milliseconds to wait.</param>
        /// <param name="waitUrls">The wait urls.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public static bool WaitForUrl(this IWebDriver driver, int millisecondsToWait = 2000, params string[] waitUrls)
        {
            var url = driver.Url;
            var endTime = DateTime.Now.AddSeconds(millisecondsToWait);
            while (!waitUrls.Any(x => url.StartsWith(x)) && DateTime.Now <= endTime)
            {
                Thread.Sleep(1);
                url = driver.Url;
            }
            return waitUrls.Any(x => url.StartsWith(x));
        }

        #endregion

        #region String

        /// <summary>
        /// Indexes the of skip.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="value">The value.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="ignoreCase">if set to <c>true</c> [ignore case].</param>
        /// <returns>System.Int32.</returns>
        public static int IndexOfSkip(this string source, string value, int startIndex = 0, bool ignoreCase = true)
        {
            var stringComparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            var r = source.IndexOf(value, startIndex, stringComparison);
            return r != -1 ? r + value.Length : -1;
        }

        /// <summary>
        /// Extracts the span inner.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="endIndex">The end index.</param>
        /// <param name="ignoreCase">if set to <c>true</c> [ignore case].</param>
        /// <returns>System.String.</returns>
        public static string ExtractSpanInner(this string source, string start, string end = null, int startIndex = 0, int endIndex = int.MaxValue, bool ignoreCase = true)
        {
            var stringComparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            var startIdx = source.IndexOf(start, startIndex, stringComparison);
            if (startIdx == -1 || startIdx >= endIndex) return null;
            startIdx += start.Length;
            if (end == null) return endIndex != int.MaxValue ? source.Substring(startIdx, endIndex - startIdx - 1) : source.Substring(startIdx);
            // end
            var endIdx = source.IndexOf(end, startIdx, stringComparison);
            if (endIdx == -1 || endIdx >= endIndex) return endIndex != int.MaxValue ? source.Substring(startIdx, endIndex - startIdx - 1) : source.Substring(startIdx);
            return source.Substring(startIdx, endIdx - startIdx);
        }

        /// <summary>
        /// Extracts the span.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="endIndex">The end index.</param>
        /// <param name="ignoreCase">if set to <c>true</c> [ignore case].</param>
        /// <returns>System.String.</returns>
        /// <exception cref="IndexOutOfRangeException">start
        /// or
        /// end</exception>
        public static string ExtractSpan(this string source, string start, string end = null, int startIndex = 0, int endIndex = int.MaxValue, bool ignoreCase = true)
        {
            var stringComparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            var startIdx = source.IndexOf(start, startIndex, stringComparison);
            if (startIdx == -1) return null;
            if (end == null) return endIndex != int.MaxValue ? source.Substring(startIdx, endIndex - startIdx - 1) : source.Substring(startIdx);
            // end
            var endIdx = source.IndexOf(end, startIdx, stringComparison);
            if (endIdx == -1 || endIdx >= endIndex) return endIndex != int.MaxValue ? source.Substring(startIdx, endIndex - startIdx - 1) : source.Substring(startIdx);
            return source.Substring(startIdx, endIdx - startIdx + end.Length);
        }

        /// <summary>
        /// To the HTML document.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>HtmlDocument.</returns>
        public static HtmlDocument ToHtmlDocument(this string source)
        {
            var r = new HtmlDocument();
            r.LoadHtml(source);
            return r;
        }

        /// <summary>
        /// Expands the path and query.
        /// </summary>
        /// <param name="source">The path and query.</param>
        /// <param name="attributes">The attributes.</param>
        /// <returns>System.String.</returns>
        public static string ExpandPathAndQuery(this string source, object attributes)
        {
            if (attributes == null)
                return source;
            var b = new StringBuilder(source);
            object value;
            string valueAsString;
            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(attributes))
                if ((value = descriptor.GetValue(attributes)) != null && !string.IsNullOrEmpty(valueAsString = value.ToString()))
                    b.Append($"&{descriptor.Name}={valueAsString}");
            return b.ToString();
        }

        #endregion

        #region Action

        /// <summary>
        /// Timeouts the invoke.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="millisecondsTimeout">The timeout milliseconds.</param>
        /// <exception cref="System.TimeoutException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public static void TimeoutInvoke(this Action source, int millisecondsTimeout)
        {
            if (millisecondsTimeout == 0)
                source();
            var task = Task.Run(source);
            if (Task.WhenAny(task, Task.Delay(millisecondsTimeout)).Result != task)
                throw new TimeoutException();
        }

        /// <summary>
        /// Timeouts the invoke.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="millisecondsTimeout">The timeout milliseconds.</param>
        /// <returns>TResult.</returns>
        /// <exception cref="System.TimeoutException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public static TResult TimeoutInvoke<TResult>(this Func<TResult> source, int millisecondsTimeout)
        {
            if (millisecondsTimeout == 0)
                return source();
            var task = Task.Run(source);
            if (Task.WhenAny(task, Task.Delay(millisecondsTimeout)).Result == task) return task.Result;
            else throw new TimeoutException();
        }

        #endregion

        #region ITryMethod

        /// <summary>
        /// Tries the function.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="action">The action.</param>
        /// <param name="closeAfter">if set to <c>true</c> [close after].</param>
        /// <param name="tag">The tag.</param>
        /// <param name="loginTimeoutInSeconds">The login timeout in seconds.</param>
        /// <returns>T.</returns>
        /// <exception cref="ArgumentNullException">action</exception>
        /// <exception cref="System.ArgumentNullException">action</exception>
        public static T TryFunc<T>(this ITryMethod source, Func<T> action, bool closeAfter = true, object tag = null, decimal loginTimeoutInSeconds = -1M)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            source.EnsureAccess(AccessMethod.TryFunc, AccessMode.Preamble, ref tag);
            var value = action();
            if (value == null)
                return default(T);
            if (source.EnsureAccess(AccessMethod.TryFunc, AccessMode.Request, ref tag, value))
            {
                source.TryLogin(closeAfter, tag, loginTimeoutInSeconds);
                return action();
            }
            return value;
        }

        /// <summary>
        /// Tries the function.
        /// </summary>
        /// <typeparam name="T1">The type of the 1.</typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="action">The action.</param>
        /// <param name="t1">The t1.</param>
        /// <param name="closeAfter">if set to <c>true</c> [close after].</param>
        /// <param name="tag">The tag.</param>
        /// <param name="loginTimeoutInSeconds">The login timeout in seconds.</param>
        /// <returns>T.</returns>
        /// <exception cref="ArgumentNullException">action</exception>
        /// <exception cref="System.ArgumentNullException">action</exception>
        public static T TryFunc<T1, T>(this ITryMethod source, Func<T1, T> action, T1 t1, bool closeAfter = true, object tag = null, decimal loginTimeoutInSeconds = -1M)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            source.EnsureAccess(AccessMethod.TryFunc, AccessMode.Preamble, ref tag);
            var value = action(t1);
            if (value == null)
                return default(T);
            if (source.EnsureAccess(AccessMethod.TryFunc, AccessMode.Request, ref tag, value))
            {
                source.TryLogin(closeAfter, tag, loginTimeoutInSeconds);
                return action(t1);
            }
            return value;
        }

        /// <summary>
        /// Tries the function.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="exceptionType">Type of the exception.</param>
        /// <param name="action">The action.</param>
        /// <param name="closeAfter">if set to <c>true</c> [close after].</param>
        /// <param name="tag">The tag.</param>
        /// <param name="loginTimeoutInSeconds">The login timeout in seconds.</param>
        /// <returns>T.</returns>
        /// <exception cref="ArgumentNullException">action</exception>
        /// <exception cref="System.ArgumentNullException">action</exception>
        public static T TryFunc<T>(this ITryMethod source, Type exceptionType, Func<T> action, bool closeAfter = true, object tag = null, decimal loginTimeoutInSeconds = -1M)
        {
            if (exceptionType == null)
                exceptionType = typeof(Exception);
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            source.EnsureAccess(AccessMethod.TryFunc, AccessMode.Preamble, ref tag);
            try { return action(); }
            catch (Exception e)
            {
                if (exceptionType.IsAssignableFrom(e.GetType()) && source.EnsureAccess(AccessMethod.TryFunc, AccessMode.Exception, ref tag, e.Message))
                {
                    source.TryLogin(closeAfter, tag, loginTimeoutInSeconds);
                    return action();
                }
                else throw e;
            }
        }
        /// <summary>
        /// Tries the function.
        /// </summary>
        /// <typeparam name="TException">The type of the t exception.</typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="action">The action.</param>
        /// <param name="closeAfter">if set to <c>true</c> [close after].</param>
        /// <param name="tag">The tag.</param>
        /// <param name="loginTimeoutInSeconds">The login timeout in seconds.</param>
        /// <returns>T.</returns>
        public static T TryFunc<TException, T>(this ITryMethod source, Func<T> action, bool closeAfter = true, object tag = null, decimal loginTimeoutInSeconds = -1M) where TException : Exception => source.TryFunc(typeof(TException), action, closeAfter, tag, loginTimeoutInSeconds);

        /// <summary>
        /// Tries the function.
        /// </summary>
        /// <typeparam name="T1">The type of the 1.</typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="exceptionType">Type of the exception.</param>
        /// <param name="action">The action.</param>
        /// <param name="t1">The t1.</param>
        /// <param name="closeAfter">if set to <c>true</c> [close after].</param>
        /// <param name="tag">The tag.</param>
        /// <param name="loginTimeoutInSeconds">The login timeout in seconds.</param>
        /// <returns>T.</returns>
        /// <exception cref="ArgumentNullException">action</exception>
        /// <exception cref="System.ArgumentNullException">action</exception>
        public static T TryFunc<T1, T>(this ITryMethod source, Type exceptionType, Func<T1, T> action, T1 t1, bool closeAfter = true, object tag = null, decimal loginTimeoutInSeconds = -1M)
        {
            if (exceptionType == null)
                exceptionType = typeof(Exception);
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            source.EnsureAccess(AccessMethod.TryFunc, AccessMode.Preamble, ref tag);
            try { return action(t1); }
            catch (Exception e)
            {
                if (exceptionType.IsAssignableFrom(e.GetType()) && source.EnsureAccess(AccessMethod.TryFunc, AccessMode.Exception, ref tag, e.Message))
                {
                    source.TryLogin(closeAfter, tag, loginTimeoutInSeconds);
                    return action(t1);
                }
                else throw e;
            }
        }
        /// <summary>
        /// Tries the function.
        /// </summary>
        /// <typeparam name="TException">The type of the t exception.</typeparam>
        /// <typeparam name="T1">The type of the t1.</typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="action">The action.</param>
        /// <param name="t1">The t1.</param>
        /// <param name="closeAfter">if set to <c>true</c> [close after].</param>
        /// <param name="tag">The tag.</param>
        /// <param name="loginTimeoutInSeconds">The login timeout in seconds.</param>
        /// <returns>T.</returns>
        public static T TryFunc<TException, T1, T>(this ITryMethod source, Func<T1, T> action, T1 t1, bool closeAfter = true, object tag = null, decimal loginTimeoutInSeconds = -1M) where TException : Exception => source.TryFunc(typeof(TException), action, t1, closeAfter, tag, loginTimeoutInSeconds);

        /// <summary>
        /// Tries the action.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="exceptionType">Type of the exception.</param>
        /// <param name="action">The action.</param>
        /// <param name="closeAfter">if set to <c>true</c> [close after].</param>
        /// <param name="tag">The tag.</param>
        /// <param name="loginTimeoutInSeconds">The login timeout in seconds.</param>
        /// <exception cref="ArgumentNullException">action</exception>
        /// <exception cref="System.ArgumentNullException">action</exception>
        public static void TryAction(this ITryMethod source, Type exceptionType, Action action, bool closeAfter = true, object tag = null, decimal loginTimeoutInSeconds = -1M)
        {
            if (exceptionType == null)
                exceptionType = typeof(Exception);
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            source.EnsureAccess(AccessMethod.TryAction, AccessMode.Preamble, ref tag);
            try { action(); }
            catch (Exception e)
            {
                if (exceptionType.IsAssignableFrom(e.GetType()) && source.EnsureAccess(AccessMethod.TryAction, AccessMode.Exception, ref tag, e.Message))
                {
                    source.TryLogin(closeAfter, tag, loginTimeoutInSeconds);
                    action();
                }
                else throw e;
            }
        }
        /// <summary>
        /// Tries the action.
        /// </summary>
        /// <typeparam name="TException">The type of the t exception.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="action">The action.</param>
        /// <param name="closeAfter">if set to <c>true</c> [close after].</param>
        /// <param name="tag">The tag.</param>
        /// <param name="loginTimeoutInSeconds">The login timeout in seconds.</param>
        public static void TryAction<TException>(this ITryMethod source, Action action, bool closeAfter = true, object tag = null, decimal loginTimeoutInSeconds = -1M) where TException : Exception => source.TryAction(typeof(TException), action, closeAfter, tag, loginTimeoutInSeconds);

        /// <summary>
        /// Tries the action.
        /// </summary>
        /// <typeparam name="T1">The type of the 1.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="exceptionType">Type of the exception.</param>
        /// <param name="action">The action.</param>
        /// <param name="t1">The t1.</param>
        /// <param name="closeAfter">if set to <c>true</c> [close after].</param>
        /// <param name="tag">The tag.</param>
        /// <param name="loginTimeoutInSeconds">The login timeout in seconds.</param>
        /// <exception cref="ArgumentNullException">action</exception>
        /// <exception cref="System.ArgumentNullException">action</exception>
        public static void TryAction<T1>(this ITryMethod source, Type exceptionType, Action<T1> action, T1 t1, bool closeAfter = true, object tag = null, decimal loginTimeoutInSeconds = -1M)
        {
            if (exceptionType == null)
                exceptionType = typeof(Exception);
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            source.EnsureAccess(AccessMethod.TryAction, AccessMode.Preamble, ref tag);
            try { action(t1); }
            catch (Exception e)
            {
                if (exceptionType.IsAssignableFrom(e.GetType()) && source.EnsureAccess(AccessMethod.TryAction, AccessMode.Exception, ref tag, e.Message))
                {
                    source.TryLogin(closeAfter, tag, loginTimeoutInSeconds);
                    action(t1);
                }
                else throw e;
            }
        }
        /// <summary>
        /// Tries the action.
        /// </summary>
        /// <typeparam name="TException">The type of the t exception.</typeparam>
        /// <typeparam name="T1">The type of the t1.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="action">The action.</param>
        /// <param name="t1">The t1.</param>
        /// <param name="closeAfter">if set to <c>true</c> [close after].</param>
        /// <param name="tag">The tag.</param>
        /// <param name="loginTimeoutInSeconds">The login timeout in seconds.</param>
        public static void TryAction<TException, T1>(this ITryMethod source, Action<T1> action, T1 t1, bool closeAfter = true, object tag = null, decimal loginTimeoutInSeconds = -1M) where TException : Exception => source.TryAction(typeof(TException), action, t1, closeAfter, tag, loginTimeoutInSeconds);

        /// <summary>
        /// Tries the pager function.
        /// </summary>
        /// <typeparam name="TCursor">The type of the cursor.</typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="action">The action.</param>
        /// <param name="cursor">The cursor.</param>
        /// <param name="nextCursor">The next cursor.</param>
        /// <param name="closeAfter">if set to <c>true</c> [close after].</param>
        /// <param name="tag">The tag.</param>
        /// <param name="loginTimeoutInSeconds">The login timeout in seconds.</param>
        /// <returns>IEnumerable&lt;T&gt;.</returns>
        /// <exception cref="ArgumentNullException">action
        /// or
        /// nextCursor</exception>
        /// <exception cref="System.ArgumentNullException">action
        /// or
        /// nextCursor</exception>
        public static IEnumerable<T> TryPagerFunc<TCursor, T>(this ITryMethod source, Func<TCursor, T> action, TCursor cursor, Func<TCursor, T, TCursor> nextCursor, bool closeAfter = true, object tag = null, decimal loginTimeoutInSeconds = -1M)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            if (nextCursor == null)
                throw new ArgumentNullException(nameof(nextCursor));
            source.EnsureAccess(AccessMethod.TryPager, AccessMode.Preamble, ref tag);
            while (cursor != null)
            {
                var values = action(cursor);
                if (values == null)
                    yield break;
                if (source.EnsureAccess(AccessMethod.TryPager, AccessMode.Request, ref tag, values))
                {
                    source.TryLogin(closeAfter, tag, loginTimeoutInSeconds);
                    values = action(cursor);
                }
                cursor = nextCursor(cursor, values);
                yield return values;
            }
        }

        /// <summary>
        /// Tries the pager function.
        /// </summary>
        /// <typeparam name="TCursor">The type of the cursor.</typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="exceptionType">Type of the exception.</param>
        /// <param name="action">The action.</param>
        /// <param name="cursor">The cursor.</param>
        /// <param name="nextCursor">The next cursor.</param>
        /// <param name="closeAfter">if set to <c>true</c> [close after].</param>
        /// <param name="tag">The tag.</param>
        /// <param name="loginTimeoutInSeconds">The login timeout in seconds.</param>
        /// <returns>IEnumerable&lt;T&gt;.</returns>
        /// <exception cref="ArgumentNullException">action
        /// or
        /// nextCursor</exception>
        /// <exception cref="System.ArgumentNullException">action
        /// or
        /// nextCursor</exception>
        public static IEnumerable<T> TryPagerFunc<TCursor, T>(this ITryMethod source, Type exceptionType, Func<TCursor, T> action, TCursor cursor, Func<TCursor, T, TCursor> nextCursor, bool closeAfter = true, object tag = null, decimal loginTimeoutInSeconds = -1M)
        {
            if (exceptionType == null)
                exceptionType = typeof(Exception);
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            if (nextCursor == null)
                throw new ArgumentNullException(nameof(nextCursor));
            source.EnsureAccess(AccessMethod.TryPager, AccessMode.Preamble, ref tag);
            while (cursor != null)
            {
                var values = default(T);
                try { values = action(cursor); }
                catch (Exception e)
                {
                    if (exceptionType.IsAssignableFrom(e.GetType()) && source.EnsureAccess(AccessMethod.TryPager, AccessMode.Exception, ref tag, e.Message))
                    {
                        source.TryLogin(closeAfter, tag, loginTimeoutInSeconds);
                        values = action(cursor);
                    }
                    else throw e;
                }
                cursor = nextCursor(cursor, values);
                yield return values;
            }
        }
        /// <summary>
        /// Tries the pager function.
        /// </summary>
        /// <typeparam name="TException">The type of the t exception.</typeparam>
        /// <typeparam name="TCursor">The type of the t cursor.</typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="action">The action.</param>
        /// <param name="cursor">The cursor.</param>
        /// <param name="nextCursor">The next cursor.</param>
        /// <param name="closeAfter">if set to <c>true</c> [close after].</param>
        /// <param name="tag">The tag.</param>
        /// <param name="loginTimeoutInSeconds">The login timeout in seconds.</param>
        /// <returns>IEnumerable&lt;T&gt;.</returns>
        public static IEnumerable<T> TryPagerFunc<TException, TCursor, T>(this ITryMethod source, Func<TCursor, T> action, TCursor cursor, Func<TCursor, T, TCursor> nextCursor, bool closeAfter = true, object tag = null, decimal loginTimeoutInSeconds = -1M) where TException : Exception => source.TryPagerFunc(typeof(TException), action, cursor, nextCursor, closeAfter, tag, loginTimeoutInSeconds);

        #endregion

        #region IHasCookies

        const byte COOKIEMAGIC = 0x12;

        class CookieShim
        {
            public string Name { get; set; }
            public string Value { get; set; }
            public string Domain { get; set; }
            public string Path { get; set; }
            public DateTime Expires { get; set; }
        }

        /// <summary>
        /// Gets the cookies.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>System.String.</returns>
        public static byte[] GetCookies(this IHasCookies source, CookieStorageType storageType = CookieStorageType.Json)
        {
            if (source.Cookies == null || source.Cookies.Count == 0)
                return new byte[0];
            switch (storageType)
            {
                case CookieStorageType.Json:
                    return Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(source.Cookies.Cast<NetCookie>().Select(x =>
                        new CookieShim { Name = x.Name, Value = x.Value, Domain = x.Domain, Path = x.Path, Expires = x.Expires })
                        .ToArray()));
                case CookieStorageType.Binary:
                    using (var s = new MemoryStream())
                    {
                        var w = new BinaryWriter(s);
                        foreach (var x in source.Cookies.Cast<NetCookie>())
                        {
                            w.Write(COOKIEMAGIC);
                            w.Write(x.Name); w.Write(x.Value); w.Write(x.Path); w.Write(x.Domain);
                            w.Write(x.Expires.Ticks);
                        }
                        s.Position = 0; return s.ToArray();
                    }
                default: throw new ArgumentOutOfRangeException(nameof(storageType), storageType.ToString());
            }
        }

        /// <summary>
        /// Sets the cookies.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="value">The value.</param>
        public static void SetCookies(this IHasCookies source, byte[] value, CookieStorageType storageType = CookieStorageType.Json)
        {
            source.Cookies = new CookieCollection();
            if (value == null || value.Length == 0)
                return;
            switch (storageType)
            {
                case CookieStorageType.Json:
                    foreach (var x in JsonConvert.DeserializeObject<CookieShim[]>(Encoding.ASCII.GetString(value)).Select(x =>
                        new NetCookie(x.Name, x.Value, x.Path, x.Domain) { Expires = x.Expires }))
                        source.Cookies.Add(x);
                    return;
                case CookieStorageType.Binary:
                    using (var s = new MemoryStream(value))
                    {
                        var r = new BinaryReader(s);
                        while (r.PeekChar() == COOKIEMAGIC)
                        {
                            r.ReadByte();
                            source.Cookies.Add(new NetCookie(r.ReadString(), r.ReadString(), r.ReadString(), r.ReadString())
                            {
                                Expires = new DateTime(r.ReadInt64())
                            });
                        }
                        return;
                    }
                default: throw new ArgumentOutOfRangeException(nameof(storageType), storageType.ToString());
            }
        }

        /// <summary>
        /// Cookies the get set.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="cookies">The cookies.</param>
        /// <returns>CookieCollection.</returns>
        public static CookieCollection CookieGetSet(this IHasCookies source, CookieCollection cookies)
        {
            if (cookies != null)
            {
                source.Cookies = cookies;
                source.CookiesFlush();
            }
            return source.Cookies;
        }

        /// <summary>
        /// Merges the specified cookies.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="cookies">The cookies.</param>
        public static void CookieMerge(this IHasCookies source, CookieCollection cookies)
        {
            var cookiesSet = new HashSet<string>(cookies.Cast<NetCookie>().Select(x => x.Name));
            var newCookies = new CookieCollection { cookies };
            foreach (NetCookie cookie in source.Cookies)
                if (!cookiesSet.Contains(cookie.Name))
                    newCookies.Add(cookie);
            source.Cookies = newCookies;
        }

        /// <summary>
        /// Downloads the preamble.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="method">The method.</param>
        /// <param name="url">The URL.</param>
        /// <param name="body">The post data.</param>
        /// <param name="contentType">Type of the content.</param>
        /// <param name="interceptRequest">The intercept request.</param>
        /// <param name="updateCookies">if set to <c>true</c> [update cookies].</param>
        /// <param name="onError">The on error.</param>
        /// <param name="timeoutInSeconds">The timeout in seconds.</param>
        /// <returns>HttpWebResponse.</returns>
        /// <exception cref="ArgumentOutOfRangeException">method</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">method</exception>
        public static HttpWebResponse DownloadPreamble(this IHasCookies source, HttpMethod method, string url, object body, string contentType = null, Action<HttpWebRequest> interceptRequest = null, bool updateCookies = false, Action<HttpStatusCode, string> onError = null, decimal timeoutInSeconds = -1M)
        {
            HttpWebRequest rq;
            if (method == HttpMethod.Get && body != null)
            {
                var prefix = url.Contains("?") ? "&" : "?";
                if (body is string bodyString && !string.IsNullOrEmpty(bodyString))
                    url += prefix + bodyString;
                else if (body is FormUrlEncodedContent bodyContent)
                {
                    var headers = bodyContent.Headers;
                    if (headers.ContentType != null && contentType == null) contentType = headers.ContentType.ToString();
                    url += prefix + bodyContent.ReadAsStringAsync().Result;
                }
                else
                    throw new ArgumentOutOfRangeException(nameof(body), body.ToString());
            }
            // request body
            var cookies = source.Cookies;
            rq = (HttpWebRequest)WebRequest.Create(url);
            rq.Method = method.ToString().ToUpperInvariant();
            rq.CookieContainer = new CookieContainer();
            rq.CookieContainer.Add(cookies);
            rq.UserAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36";
            rq.AllowWriteStreamBuffering = true;
            rq.ProtocolVersion = HttpVersion.Version11;
            rq.AllowAutoRedirect = true;
            rq.ContentType = contentType ?? "application/x-www-form-urlencoded; charset=UTF-8";
            if (timeoutInSeconds >= 0)
                rq.Timeout = (int)(timeoutInSeconds * 1000);
            interceptRequest?.Invoke(rq);
            if (method != HttpMethod.Get && body != null)
            {
                if (body is string bodyString && !string.IsNullOrEmpty(bodyString))
                {
                    var send = Encoding.Default.GetBytes(bodyString);
                    rq.ContentLength = send.Length;
                    if (!string.IsNullOrEmpty(DownloadDebugFilePattern))
                        using (var s = File.OpenWrite(string.Format(DownloadDebugFilePattern, DateTime.Now.Ticks, method)))
                            s.Write(send, 0, send.Length);
                    using (var s = rq.GetRequestStream())
                        s.Write(send, 0, send.Length);
                }
                else if (body is HttpContent bodyContent)
                {
                    var headers = bodyContent.Headers;
                    if (headers.ContentLength != null) rq.ContentLength = headers.ContentLength.Value;
                    if (headers.ContentType != null) rq.ContentType = headers.ContentType.ToString();
                    if (!string.IsNullOrEmpty(DownloadDebugFilePattern))
                        using (var s = File.OpenWrite(string.Format(DownloadDebugFilePattern, DateTime.Now.Ticks, method)))
                            bodyContent.CopyToAsync(s).Wait();
                    using (var s = rq.GetRequestStream())
                        bodyContent.CopyToAsync(s).Wait();
                }
                else
                    throw new ArgumentOutOfRangeException(nameof(body), body.ToString());
            }

            // request execute
            try
            {
                var rs = (HttpWebResponse)rq.GetResponse();
                if (updateCookies)
                {
                    source.CookieMerge(rs.Cookies);
                    source.CookiesFlush();
                }
                return rs;
            }
            catch (WebException e)
            {
                if (!string.IsNullOrEmpty(DownloadDebugFilePattern) || onError != null)
                    using (var rs = (HttpWebResponse)e.Response)
                    using (var data = rs.GetResponseStream())
                    {
                        if (!string.IsNullOrEmpty(DownloadDebugFilePattern))
                        {
                            using (var s = File.OpenWrite(string.Format(DownloadDebugFilePattern, DateTime.Now.Ticks, rs.StatusCode)))
                                data.CopyTo(s);
                            if (data.CanSeek)
                                data.Position = 0;
                        }
                        if (onError != null)
                            using (var r = new StreamReader(data))
                                onError(rs.StatusCode, r.ReadToEnd());
                    }
                throw;
            }
        }

        /// <summary>
        /// Downloads the data.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="method">The method.</param>
        /// <param name="url">The URL.</param>
        /// <param name="body">The post data.</param>
        /// <param name="contentType">Type of the content.</param>
        /// <param name="interceptRequest">The intercept request.</param>
        /// <param name="updateCookies">if set to <c>true</c> [update cookies].</param>
        /// <param name="onError">The on error.</param>
        /// <param name="timeoutInSeconds">The timeout in seconds.</param>
        /// <returns>System.String.</returns>
        public static string DownloadData(this IHasCookies source, HttpMethod method, string url, object body = null, string contentType = null, Action<HttpWebRequest> interceptRequest = null, bool updateCookies = true, Action<HttpStatusCode, string> onError = null, decimal timeoutInSeconds = -1M)
        {
            var rs = source.DownloadPreamble(method, url, body, contentType, interceptRequest, updateCookies, onError, timeoutInSeconds);
            using (var r = new StreamReader(rs.GetResponseStream()))
                return r.ReadToEnd();
        }

        /// <summary>
        /// Downloads the json.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="method">The method.</param>
        /// <param name="url">The URL.</param>
        /// <param name="body">The post data.</param>
        /// <param name="contentType">Type of the content.</param>
        /// <param name="interceptRequest">The intercept request.</param>
        /// <param name="updateCookies">if set to <c>true</c> [update cookies].</param>
        /// <param name="onError">The on error.</param>
        /// <returns>JToken.</returns>
        public static JToken DownloadJson(this IHasCookies source, HttpMethod method, string url, object body = null, string contentType = null, Action<HttpWebRequest> interceptRequest = null, bool updateCookies = true, Action<HttpStatusCode, string> onError = null, decimal timeoutInSeconds = -1M)
        {
            var d = source.DownloadData(method, url, body, contentType, interceptRequest, updateCookies, onError, timeoutInSeconds);
            return JToken.Parse(d);
        }

        /// <summary>
        /// Downloads the file.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="filePath">The file path.</param>
        /// <param name="method">The method.</param>
        /// <param name="url">The URL.</param>
        /// <param name="body">The post data.</param>
        /// <param name="contentType">Type of the content.</param>
        /// <param name="interceptRequest">The intercept request.</param>
        /// <param name="interceptResponse">The intercept response.</param>
        /// <param name="interceptFilename">The intercept filename.</param>
        /// <param name="updateCookies">if set to <c>true</c> [update cookies].</param>
        /// <param name="onError">The on error.</param>
        /// <param name="timeoutInSeconds">The timeout in seconds.</param>
        /// <returns>System.String.</returns>
        public static string DownloadFile(this IHasCookies source, string filePath, HttpMethod method, string url, object body = null, string contentType = null, Action<HttpWebRequest> interceptRequest = null, Action<Stream, Stream> interceptResponse = null, Func<string, string> interceptFilename = null, bool updateCookies = false, Action<HttpStatusCode, string> onError = null, decimal timeoutInSeconds = -1M)
        {
            var rs = source.DownloadPreamble(method, url, body, contentType, interceptRequest, updateCookies, onError, timeoutInSeconds);
            return CompleteDownload(rs, filePath, null, interceptResponse, interceptFilename);
        }

        /// <summary>
        /// Downloads the file.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="stream">The stream.</param>
        /// <param name="method">The method.</param>
        /// <param name="url">The URL.</param>
        /// <param name="body">The post data.</param>
        /// <param name="contentType">Type of the content.</param>
        /// <param name="interceptRequest">The intercept request.</param>
        /// <param name="interceptResponse">The intercept response.</param>
        /// <param name="interceptFilename">The intercept filename.</param>
        /// <param name="updateCookies">if set to <c>true</c> [update cookies].</param>
        /// <param name="onError">The on error.</param>
        /// <param name="timeoutInSeconds">The timeout in seconds.</param>
        /// <returns>System.String.</returns>
        public static string DownloadFile(this IHasCookies source, Stream stream, HttpMethod method, string url, object body = null, string contentType = null, Action<HttpWebRequest> interceptRequest = null, Action<Stream, Stream> interceptResponse = null, Func<string, string> interceptFilename = null, bool updateCookies = false, Action<HttpStatusCode, string> onError = null, decimal timeoutInSeconds = -1M)
        {
            var rs = source.DownloadPreamble(method, url, body, contentType, interceptRequest, updateCookies, onError, timeoutInSeconds);
            return CompleteDownload(rs, null, stream, interceptResponse, interceptFilename);
        }

        /// <summary>
        /// Gets or sets the download debug path.
        /// </summary>
        /// <value>The download debug path.</value>
        public static string DownloadDebugFilePattern { get; set; }

        /// <summary>
        /// Pathifies the name of the file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>System.String.</returns>
        public static string PathifyFileName(string fileName) => fileName.Replace(':', '_');

        static string DownloadFileName(HttpWebResponse rs)
        {
            var contentDisposition = rs.GetResponseHeader("content-disposition");
            int filenameIdx, semicolonIdx;
            if (contentDisposition != null && (filenameIdx = contentDisposition.IndexOf("filename=") + 9) > 9)
                return contentDisposition.Substring(filenameIdx, (semicolonIdx = contentDisposition.IndexOf(";", filenameIdx)) > -1 ? semicolonIdx - filenameIdx : contentDisposition.Length - filenameIdx).Replace("\"", "");
            return null;
        }

        static string CompleteDownload(HttpWebResponse rs, string filePath, Stream stream, Action<Stream, Stream> interceptResponse, Func<string, string> interceptFilename)
        {
            var fileName = DownloadFileName(rs);
            if (string.IsNullOrEmpty(fileName))
                using (var r = new StreamReader(rs.GetResponseStream()))
                    return r.ReadToEnd();
            // file path
            var closeStream = stream == null;
            if (filePath != null)
            {
                fileName = PathifyFileName(fileName);
                if (interceptFilename != null)
                    fileName = interceptFilename(fileName);
                fileName = Path.Combine(filePath, fileName);
                stream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            }
            // download file
            var deleteFile = false;
            try
            {
                var buffer = new byte[4096];
                using (var input = rs.GetResponseStream())
                {
                    interceptResponse?.Invoke(stream, input);
                    var size = input.Read(buffer, 0, buffer.Length);
                    while (size > 0)
                    {
                        stream.Write(buffer, 0, size);
                        size = input.Read(buffer, 0, buffer.Length);
                    }
                    deleteFile = true;
                }
            }
            finally
            {
                if (closeStream && stream != null)
                {
                    stream.Flush();
                    stream.Close();
                    stream = null;
                    if (!deleteFile)
                        File.Delete(fileName);
                }
            }
            return fileName;
        }

        #endregion
    }
}