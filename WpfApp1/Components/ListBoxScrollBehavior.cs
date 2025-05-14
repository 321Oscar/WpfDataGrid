using Microsoft.Xaml.Behaviors;
using System.ComponentModel;
using System.Windows.Controls;

namespace ERad5TestGUI.Components
{
    public class ListBoxScrollBehavior : Behavior<ListBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();

            ((ICollectionView)AssociatedObject.Items).CollectionChanged += ListViewScrollBehavior_CollectionChanged;
        }

        private void ListViewScrollBehavior_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (AssociatedObject.HasItems)
            {
                AssociatedObject.ScrollIntoView(AssociatedObject.Items[AssociatedObject.Items.Count - 1]);
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            ((ICollectionView)AssociatedObject.Items).CollectionChanged -= ListViewScrollBehavior_CollectionChanged;
        }
    }
}
