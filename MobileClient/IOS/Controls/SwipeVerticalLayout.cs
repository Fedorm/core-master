using System.Drawing;
using BitMobile.Application.Controls;
using BitMobile.Common.Controls;
using BitMobile.Common.StyleSheet;
using BitMobile.UI;

namespace BitMobile.Controls
{
    [Synonym("svl")]
    [MarkupElement(MarkupElementAttribute.ControlsNamespace, "SwipeVerticalLayout")]
    public class SwipeVerticalLayout : CustomSwipeLayout
    {
        private float _alignOffset;

        protected override IBound LayoutChildren(IStyleSheet stylesheet, IBound styleBound, IBound maxBound)
        {
            float[] borders;
            IBound bound = ControlsContext.Current.CreateLayoutBehaviour(stylesheet, this)
                .Vertical(ContainerBehaviour.Childrens, styleBound, maxBound, out borders, true);
            Behaviour.ScrollingArea = bound.Height;

            if (ContainerBehaviour.Childrens.Count > 0)
                Behaviour.SetBorders(borders);

            return bound;
        }

        protected override void OnScrollEnded(float startX, float startY)
        {
            float? offset = Behaviour.HandleSwipe(_view.ContentOffset.Y, startY);
            if (offset != null)
                Scroll(offset.Value);
        }

        protected override PointF GetContentOffset(float offset)
        {
            return new PointF(0, offset + _alignOffset);
        }

        protected override void SetupAlignOffset(IBound bound)
        {
            float top;
            float bottom;
            AlignOffset(out top, out bottom);
            _alignOffset = top;

            _view.ContentSize = new SizeF(bound.ContentWidth, bound.ContentHeight + top + bottom);

            foreach (Control control in ContainerBehaviour.Childrens)
            {
                IRectangle r = control.Frame;
                control.Frame = ControlsContext.Current.CreateRectangle(r.Left, r.Top + top, r.Width, r.Height);
            }
        }
    }
}