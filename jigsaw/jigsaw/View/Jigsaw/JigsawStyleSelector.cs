using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using jigsaw.Model;
using System.Windows;

namespace jigsaw.View.Jigsaw
{
    class JigsawStyleSelector : StyleSelector
    {
        public override System.Windows.Style SelectStyle(object item, System.Windows.DependencyObject container)
        {

            if (item is Table)
            {
                return (Style)((TreeViewItem)container).FindResource("pieceStyle");
            }
            else
            {
                return (Style)((TreeViewItem)container).FindResource("panelStyle");
            }
        }
    }
}
