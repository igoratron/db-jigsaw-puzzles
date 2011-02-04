using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace jigsaw.View.Jigsaw
{
    public class JigsawTreemap : TreeView
    {
        public JigsawTreemap() : base()
        {
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new JigsawTreemapContainer();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is JigsawTreemapContainer;
        }
    }

    public class JigsawTreemapContainer : TreeViewItem
    {
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new JigsawTreemapPiece();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is JigsawTreemapContainer;
        }
    }

    public class JigsawTreemapPiece : TreeViewItem
    {
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new JigsawTreemapPiece();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is JigsawTreemapContainer;
        }
    }
}
