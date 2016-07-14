using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;

namespace CsvCompare
{
    public static class Info
    {
        public static string Title
        {
            get
            {
                string result = string.Empty;
                Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();

                if (assembly != null)
                {
                    object[] customAttributes = assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                    if ((customAttributes != null) && (customAttributes.Length > 0))
                        result = ((AssemblyTitleAttribute)customAttributes[0]).Title;
                }

                return result;
            }
        }
        public static string Description
        {
            get
            {
                string result = string.Empty;
                Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();

                if (assembly != null)
                {
                    object[] customAttributes = assembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                    if ((customAttributes != null) && (customAttributes.Length > 0))
                        result = ((AssemblyDescriptionAttribute)customAttributes[0]).Description;
                }

                return result;
            }
        }
        public static string Company
        {
            get
            {
                string result = string.Empty;
                Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();

                if (assembly != null)
                {
                    object[] customAttributes = assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                    if ((customAttributes != null) && (customAttributes.Length > 0))
                        result = ((AssemblyCompanyAttribute)customAttributes[0]).Company;
                }

                return result;
            }
        }
        public static string Product
        {
            get
            {
                string result = string.Empty;
                Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();

                if (assembly != null)
                {
                    object[] customAttributes = assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                    if ((customAttributes != null) && (customAttributes.Length > 0))
                        result = ((AssemblyProductAttribute)customAttributes[0]).Product;
                }
                return result;
            }
        }
        public static string Copyright
        {
            get
            {
                string result = string.Empty;
                Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();

                if (assembly != null)
                {
                    object[] customAttributes = assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                    if ((customAttributes != null) && (customAttributes.Length > 0))
                        result = ((AssemblyCopyrightAttribute)customAttributes[0]).Copyright;
                }
                return result;
            }
        }
        public static string Trademark
        {
            get
            {
                string result = string.Empty;
                Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();

                if (assembly != null)
                {
                    object[] customAttributes = assembly.GetCustomAttributes(typeof(AssemblyTrademarkAttribute), false);
                    if ((customAttributes != null) && (customAttributes.Length > 0))
                        result = ((AssemblyTrademarkAttribute)customAttributes[0]).Trademark;
                }
                return result;
            }
        }
        public static string AssemblyVersion
        {
            get
            {
                Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                return assembly.GetName().Version.ToString();
            }
        }
        public static string Guid
        {
            get
            {
                string result = string.Empty;
                Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();

                if (assembly != null)
                {
                    object[] customAttributes = assembly.GetCustomAttributes(typeof(System.Runtime.InteropServices.GuidAttribute), false);
                    if ((customAttributes != null) && (customAttributes.Length > 0))
                        result = ((System.Runtime.InteropServices.GuidAttribute)customAttributes[0]).Value;
                }
                return result;
            }
        }
    }
}
