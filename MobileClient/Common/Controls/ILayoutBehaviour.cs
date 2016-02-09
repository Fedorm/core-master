using System.Collections.Generic;
using BitMobile.Common.StyleSheet;

namespace BitMobile.Common.Controls
{
    public interface ILayoutBehaviour
    {
        IBound Horizontal(IEnumerable<ILayoutable> controls, IBound styleBound, IBound maxBound, out float[] borders, bool extensible = false);
        IBound Vertical(IEnumerable<ILayoutable> controls, IBound styleBound, IBound maxBound, out float[] borders, bool extensible = false);
        IBound Dock(IEnumerable<ILayoutable> controls, IBound styleBound, IBound maxBound);
        IBound Screen(ILayoutable control, IBound screenBound);
    }
}
