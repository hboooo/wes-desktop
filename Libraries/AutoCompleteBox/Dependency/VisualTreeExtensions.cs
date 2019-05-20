using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
namespace System.Windows.Controls
{
    internal static class VisualTreeExtensions
    {
        internal static IEnumerable<DependencyObject> GetVisualChildren(this DependencyObject parent)
        {
            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                yield return VisualTreeHelper.GetChild(parent, i);
            }
            yield break;
        }
        internal static IEnumerable<FrameworkElement> GetLogicalChildrenBreadthFirst(this FrameworkElement parent)
        {
            Queue<FrameworkElement> queue = new Queue<FrameworkElement>(parent.GetVisualChildren().OfType<FrameworkElement>());
            while (queue.Count > 0)
            {
                FrameworkElement frameworkElement = queue.Dequeue();
                yield return frameworkElement;
                foreach (FrameworkElement current in frameworkElement.GetVisualChildren().OfType<FrameworkElement>())
                {
                    queue.Enqueue(current);
                }
            }
            yield break;
        }
    }
}
