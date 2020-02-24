using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace WPFHelpers
{
    public static class VisualTreeHelperExtensions
    {
        public static T GetChildOfType<T>(this DependencyObject source)
            where T : DependencyObject
        {
            if (source == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(source); i++)
            {
                var child = VisualTreeHelper.GetChild(source, i);

                var result = (child as T) ?? GetChildOfType<T>(child);
                if (result != null) return result;
            }
            return null;
        }
        public static IEnumerable<T> GetParentsOfType<T>(this DependencyObject source)
            where T : DependencyObject
        {
            if (source == null)
            {
                yield break;
            }

            var visualParent = VisualTreeHelper.GetParent(source);
            T parent;

            while(visualParent != null)
            {
                parent = visualParent as T;
                if (parent != null)
                    yield return parent;

                visualParent = VisualTreeHelper.GetParent(visualParent);
            }

            yield break;
        }
        /// <summary>
        /// https://stackoverflow.com/questions/636383/how-can-i-find-wpf-controls-by-name-or-type
        /// Finds a Child of a given item in the visual tree. 
        /// </summary>
        /// <param name="parent">A direct parent of the queried item.</param>
        /// <typeparam name="T">The type of the queried item.</typeparam>
        /// <param name="childName">x:Name or Name of child. </param>
        /// <returns>The first parent item that matches the submitted type parameter. 
        /// If not matching item can be found, 
        /// a null parent is being returned.</returns>
        public static T GetChildByName<T>(this DependencyObject parent, string childName)
           where T : DependencyObject
        {
            // Confirm parent and childName are valid. 
            if (parent == null) return null;

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                T childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = GetChildByName<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child. 
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        // if the child's name is of the request name
                        foundChild = (T)child;
                        break;
                    }
                }
                else
                {
                    // child element found.
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }
        public static IEnumerable<T> GetChildrenWhichNameContains<T>(this DependencyObject parent, string childName, StringComparison stringComparison = StringComparison.InvariantCultureIgnoreCase)
           where T : DependencyObject
        {
            // Confirm parent and childName are valid. 
            if (parent == null) //return null;
                yield break;

            T foundChild = null;
            //List<T> foundChildren = new List<T>();

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                T childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = GetChildByName<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child. 
                    if (foundChild != null) //break;
                        yield return foundChild;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search
                    if (frameworkElement != null && frameworkElement.Name.IndexOf(childName, stringComparison) > -1)
                    {
                        // if the child's name is of the request name
                        foundChild = (T)child;
                        //break;
                        yield return foundChild;
                    }
                }
                else
                {
                    // child element found.
                    foundChild = (T)child;
                    //break;
                    yield return foundChild;
                }
            }

            //return foundChild;
            yield break;
        }
        //https://stackoverflow.com/questions/4030764/how-to-get-datagridcolumnheader-from-datagridcolumn
        public static DataGridColumnHeader GetColumnHeaderFromColumn(this DataGridColumn column, DataGrid dataGrid)
        {
            List<DataGridColumnHeader> columnHeaders = GetVisualChildCollection<DataGridColumnHeader>(dataGrid);
            foreach (DataGridColumnHeader columnHeader in columnHeaders)
            {
                if (columnHeader.Column == column)
                {
                    return columnHeader;
                }
            }
            return null;
        }
        public static List<T> GetVisualChildCollection<T>(object parent) where T : Visual
        {
            List<T> visualCollection = new List<T>();
            GetVisualChildCollection(parent as DependencyObject, visualCollection);
            return visualCollection;
        }
        private static void GetVisualChildCollection<T>(DependencyObject parent, List<T> visualCollection) where T : Visual
        {
            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child is T)
                {
                    visualCollection.Add(child as T);
                }
                else if (child != null)
                {
                    GetVisualChildCollection(child, visualCollection);
                }
            }
        }
    }
}
