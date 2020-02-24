using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Xml;

namespace WPFHelpers
{
    public static class ExceptionExtensions
    {
        public static void ShowException(this Exception e, string additionalMessagePrefix = "")
        {
            var msg = $@"{additionalMessagePrefix}
                Error: 
{e.Message} ;

                Trace: 
{e.StackTrace} ;";

            Exception innerEx = e.InnerException;

            while (innerEx != null)
            {
                msg = msg + $@"
                
                InnerException: 
{(e.InnerException != null ? innerEx.Message : "")} ; 
                
                InnerException Trace:
{(e.InnerException != null ? innerEx.StackTrace : "")} ;";

                innerEx = innerEx.InnerException;
            }

            Task.Run(() => Log.Instance.WriteAsync(msg));
            System.Windows.MessageBox.Show(msg);
        }
        public static void ShowExceptions(this IEnumerable<Exception> es, string additionalMessagePrefix = "")
        {
            StringBuilder sb = new StringBuilder();
            foreach (var e in es)
            {
                sb.Append($@"{additionalMessagePrefix}
                Error: 
{e.Message} ;

                Trace: 
{e.StackTrace} ;");

                Exception innerEx = e.InnerException;

                while (innerEx != null)
                {
                    sb.Append(sb + $@"
                
                InnerException: 
{(e.InnerException != null ? innerEx.Message : "")} ; 
                
                InnerException Trace:
{(e.InnerException != null ? innerEx.StackTrace : "")} ;");

                    innerEx = innerEx.InnerException;
                }

                sb.AppendLine("***********************************************");
                sb.AppendLine("***********************************************");
            }

            var msg = sb.ToString();
            Task.Run(() => Log.Instance.WriteAsync(msg));
            System.Windows.MessageBox.Show(msg);
        }
    }

    public static class ObjectExtensions
    {
        public static T XamlDeepClone<T>(this T element)
        {
            var xaml = XamlWriter.Save(element);
            using (var xamlString = new StringReader(xaml))
            using (var xmlTextReader = new XmlTextReader(xamlString))
            {
                var deepCopyObject = (T)XamlReader.Load(xmlTextReader);
                return deepCopyObject;
            }
        }
    }
}
