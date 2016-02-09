using BitMobile.Common.StyleSheet;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BitMobile.StyleSheet.UnitTests
{
    [TestClass]
    public class StyleSheetContextTests
    {
        private readonly IStyleSheetContext _context;

        public StyleSheetContextTests()
        {
            _context = new StyleSheetContext();
        }

        [TestMethod]
        public void MergeBound_SaveProportion_ReturnsBound()
        {
            IBound bound = _context.CreateBound(100, 200);
            IBound maxBound = _context.CreateBound(200, 300);
            const float w = 150;
            const float h = 200;

            IBound actual = _context.MergeBound(bound, w, h, maxBound, true);

            Assert.AreEqual(_context.CreateBound(150, 300), actual);
        }

        [TestMethod]
        public void MergeBound_SaveProportionoutOfMaxBound_ReturnsBound()
        {
            IBound bound = _context.CreateBound(100, 200);
            IBound maxBound = _context.CreateBound(200, 300);
            const float w = 200;
            const float h = 200;

            IBound actual = _context.MergeBound(bound, w, h, maxBound, true);

            Assert.AreEqual(_context.CreateBound(200, 300), actual);
        }
    }
}
