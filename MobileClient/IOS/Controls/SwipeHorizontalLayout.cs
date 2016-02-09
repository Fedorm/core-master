using System.Drawing;
using BitMobile.Application.Controls;
using BitMobile.Common.Controls;
using BitMobile.Common.StyleSheet;
using BitMobile.UI;

namespace BitMobile.Controls
{
    [Synonym("shl")]
    [MarkupElement(MarkupElementAttribute.ControlsNamespace, "SwipeHorizontalLayout")]
    public class SwipeHorizontalLayout : CustomSwipeLayout
    {
        private float _alignOffset;

        protected override IBound LayoutChildren(IStyleSheet stylesheet, IBound styleBound, IBound maxBound)
        {
            float[] borders;
            IBound bound = ControlsContext.Current.CreateLayoutBehaviour(stylesheet, this)
                .Horizontal(ContainerBehaviour.Childrens, styleBound, maxBound, out borders, true);
            Behaviour.ScrollingArea = bound.Width;

            if (ContainerBehaviour.Childrens.Count > 0)
                Behaviour.SetBorders(borders);

            return bound;
        }

        protected override void OnScrollEnded(float startX, float startY)
        {
            float? offset = Behaviour.HandleSwipe(_view.ContentOffset.X, startX);
            if (offset != null)
                Scroll(offset.Value);
        }

        protected override PointF GetContentOffset(float offset)
        {
            return new PointF(offset + _alignOffset, 0);
        }

        protected override void SetupAlignOffset(IBound bound)
        {
            float left;
            float right;
            AlignOffset(out left, out right);
            _alignOffset = left;

            _view.ContentSize = new SizeF(bound.ContentWidth + left + right, bound.ContentHeight);

            foreach (Control control in ContainerBehaviour.Childrens)
            {
                IRectangle r = control.Frame;
                control.Frame = ControlsContext.Current.CreateRectangle(r.Left + left, r.Top, r.Width, r.Height);
            }
        }
    }
}