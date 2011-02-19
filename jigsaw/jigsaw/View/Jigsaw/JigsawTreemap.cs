using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using jigsaw.Model;

namespace jigsaw.View.Jigsaw
{
    public class JigsawTreemap : TreeView
    {
        private static Dictionary<Table, Piece> mapping;

        public JigsawTreemap() : base()
        {
            mapping = new Dictionary<Table, Piece>();
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new JigsawTreemapContainer();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is JigsawTreemapContainer;
        }

        public static void registerChild(Table t, Piece p)
        {
            if(!mapping.ContainsKey(t))
                mapping.Add(t, p);
        }

        public static Piece getChild(Table t)
        {
            return mapping[t];
        }
    }

    public class JigsawTreemapContainer : TreeViewItem
    {
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new Piece();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is Piece;
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            JigsawTreemap.registerChild((Table)item, (Piece)element);
            base.PrepareContainerForItemOverride(element, item);
        }
    }
}
