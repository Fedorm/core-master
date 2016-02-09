using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BitMobile.Common.Controls;
using BitMobile.Common.StyleSheet;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BitMobile.StyleSheet.UnitTests
{
    [TestClass]
    public class StyleSheetTests
    {
        private readonly StyleSheetContext _context;
        private IStyleSheet _styleSheet;

        public StyleSheetTests()
        {
            _context = new StyleSheetContext();
        }

        [TestInitialize]
        public void OnTestInitialize()
        {
            _styleSheet = _context.CreateStyleSheet();
        }

        [TestCleanup]
        public void OnTestCleanup()
        {
            _styleSheet.Dispose();
            _styleSheet = null;
        }

        [TestMethod]
        public void Assign_body()
        {
            LoadStyleSheet("Styles.body.css");

            var mockControl = new LayoutableFake("body");

            _styleSheet.Assign(mockControl);

            var styles = _styleSheet.GetStyles(mockControl);
            Assert.AreEqual("#ff0000", FindStyle<BackgroundColor>(styles).Value.ToString());
        }

        [TestMethod]
        public void Assign_layouts()
        {
            LoadStyleSheet("Styles.layouts.css");

            var body = new ContainerFake("body").Add(
                new ContainerFake("vl").Add(
                    new ContainerFake("hl", "red").Add(
                        new ContainerFake("dl", "green"),
                        new ContainerFake("dl", "borders")
                    ),
                    new ContainerFake("hl").Add(
                        new ContainerFake("dl", "clickable")
                    )
                )
            );

            _styleSheet.Assign(body);

            #region Assert

            // body
            var styles = _styleSheet.GetStyles(body);
            Assert.AreEqual("#fff", FindStyle<BackgroundColor>(styles).Value.ToString());

            // body vl
            styles = _styleSheet.GetStyles(body[0]);
            Assert.AreEqual(640, FindStyle<Width>(styles).Amount);
            Assert.AreEqual(Measure.ScreenPercent, FindStyle<Width>(styles).Measure);
            Assert.AreEqual(100, FindStyle<Height>(styles).Amount);
            Assert.AreEqual(Measure.ScreenPercent, FindStyle<Height>(styles).Measure);
            Assert.AreEqual("#000", FindStyle<BackgroundColor>(styles).Value.ToString());
            Assert.IsNull(FindStyle<BackgroundImage>(styles).Path);
            Assert.AreEqual(BorderStyleValues.None, FindStyle<Border>(styles).Style);
            Assert.AreEqual(0, FindStyle<BorderWidth>(styles).Amount);
            Assert.AreEqual(Measure.ScreenPercent, FindStyle<BorderWidth>(styles).Measure);
            Assert.AreEqual(null, FindStyle<BorderColor>(styles).Value);
            Assert.AreEqual(0, FindStyle<BorderRadius>(styles).Amount);
            Assert.AreEqual(Measure.ScreenPercent, FindStyle<BorderRadius>(styles).Measure);
            Assert.AreEqual(null, FindStyle<SelectedColor>(styles).Value);

            // body vl (hl|red)
            styles = _styleSheet.GetStyles(body[0][0]);
            Assert.AreEqual(100, FindStyle<Width>(styles).Amount);
            Assert.AreEqual(Measure.Percent, FindStyle<Width>(styles).Measure);
            Assert.AreEqual(20, FindStyle<Height>(styles).Amount);
            Assert.AreEqual(Measure.Millimetre, FindStyle<Height>(styles).Measure);
            Assert.AreEqual("#f00", FindStyle<BackgroundColor>(styles).Value.ToString());
            Assert.IsNull(FindStyle<BackgroundImage>(styles).Path);
            Assert.AreEqual(BorderStyleValues.None, FindStyle<Border>(styles).Style);
            Assert.AreEqual(0, FindStyle<BorderWidth>(styles).Amount);
            Assert.AreEqual(Measure.ScreenPercent, FindStyle<BorderWidth>(styles).Measure);
            Assert.AreEqual(null, FindStyle<BorderColor>(styles).Value);
            Assert.AreEqual(0, FindStyle<BorderRadius>(styles).Amount);
            Assert.AreEqual(Measure.ScreenPercent, FindStyle<BorderRadius>(styles).Measure);
            Assert.AreEqual(null, FindStyle<SelectedColor>(styles).Value);

            // body vl (hl|red) (dl|green)
            styles = _styleSheet.GetStyles(body[0][0][0]);
            Assert.AreEqual(10, FindStyle<Width>(styles).Amount);
            Assert.AreEqual(Measure.Percent, FindStyle<Width>(styles).Measure);
            Assert.AreEqual(100, FindStyle<Height>(styles).Amount);
            Assert.AreEqual(Measure.Percent, FindStyle<Height>(styles).Measure);
            Assert.AreEqual("#0f0", FindStyle<BackgroundColor>(styles).Value.ToString());
            Assert.IsNull(FindStyle<BackgroundImage>(styles).Path);
            Assert.AreEqual(BorderStyleValues.None, FindStyle<Border>(styles).Style);
            Assert.AreEqual(0, FindStyle<BorderWidth>(styles).Amount);
            Assert.AreEqual(Measure.ScreenPercent, FindStyle<BorderWidth>(styles).Measure);
            Assert.AreEqual(null, FindStyle<BorderColor>(styles).Value);
            Assert.AreEqual(0, FindStyle<BorderRadius>(styles).Amount);
            Assert.AreEqual(Measure.ScreenPercent, FindStyle<BorderRadius>(styles).Measure);
            Assert.AreEqual(null, FindStyle<SelectedColor>(styles).Value);

            // body vl (hl|red) (dl|borders)
            styles = _styleSheet.GetStyles(body[0][0][1]);
            Assert.AreEqual(10, FindStyle<Width>(styles).Amount);
            Assert.AreEqual(Measure.Percent, FindStyle<Width>(styles).Measure);
            Assert.AreEqual(100, FindStyle<Height>(styles).Amount);
            Assert.AreEqual(Measure.Percent, FindStyle<Height>(styles).Measure);
            Assert.AreEqual("#000", FindStyle<BackgroundColor>(styles).Value.ToString());
            Assert.IsNull(FindStyle<BackgroundImage>(styles).Path);
            Assert.AreEqual(BorderStyleValues.Solid, FindStyle<Border>(styles).Style);
            Assert.AreEqual(1, FindStyle<BorderWidth>(styles).Amount);
            Assert.AreEqual(Measure.ScreenPercent, FindStyle<BorderWidth>(styles).Measure);
            Assert.AreEqual("#fff", FindStyle<BorderColor>(styles).Value.ToString());
            Assert.AreEqual(1, FindStyle<BorderRadius>(styles).Amount);
            Assert.AreEqual(Measure.ScreenPercent, FindStyle<BorderRadius>(styles).Measure);
            Assert.AreEqual(null, FindStyle<SelectedColor>(styles).Value);

            // body vl hl
            styles = _styleSheet.GetStyles(body[0][1]);
            Assert.AreEqual(100, FindStyle<Width>(styles).Amount);
            Assert.AreEqual(Measure.Percent, FindStyle<Width>(styles).Measure);
            Assert.AreEqual(20, FindStyle<Height>(styles).Amount);
            Assert.AreEqual(Measure.Millimetre, FindStyle<Height>(styles).Measure);
            Assert.AreEqual("#000", FindStyle<BackgroundColor>(styles).Value.ToString());
            Assert.IsNull(FindStyle<BackgroundImage>(styles).Path);
            Assert.AreEqual(BorderStyleValues.None, FindStyle<Border>(styles).Style);
            Assert.AreEqual(0, FindStyle<BorderWidth>(styles).Amount);
            Assert.AreEqual(Measure.ScreenPercent, FindStyle<BorderWidth>(styles).Measure);
            Assert.AreEqual(null, FindStyle<BorderColor>(styles).Value);
            Assert.AreEqual(0, FindStyle<BorderRadius>(styles).Amount);
            Assert.AreEqual(Measure.ScreenPercent, FindStyle<BorderRadius>(styles).Measure);
            Assert.AreEqual(null, FindStyle<SelectedColor>(styles).Value);

            // body vl hl (dl|clickable)
            styles = _styleSheet.GetStyles(body[0][1][0]);
            Assert.AreEqual(10, FindStyle<Width>(styles).Amount);
            Assert.AreEqual(Measure.Percent, FindStyle<Width>(styles).Measure);
            Assert.AreEqual(100, FindStyle<Height>(styles).Amount);
            Assert.AreEqual(Measure.Percent, FindStyle<Height>(styles).Measure);
            Assert.AreEqual("#000", FindStyle<BackgroundColor>(styles).Value.ToString());
            Assert.IsNull(FindStyle<BackgroundImage>(styles).Path);
            Assert.AreEqual(BorderStyleValues.None, FindStyle<Border>(styles).Style);
            Assert.AreEqual(0, FindStyle<BorderWidth>(styles).Amount);
            Assert.AreEqual(Measure.ScreenPercent, FindStyle<BorderWidth>(styles).Measure);
            Assert.AreEqual(null, FindStyle<BorderColor>(styles).Value);
            Assert.AreEqual(0, FindStyle<BorderRadius>(styles).Amount);
            Assert.AreEqual(Measure.ScreenPercent, FindStyle<BorderRadius>(styles).Measure);
            Assert.AreEqual("#fff", FindStyle<SelectedColor>(styles).Value.ToString());
            #endregion
        }

        [TestMethod]
        public void Assign_button()
        {
            LoadStyleSheet("Styles.font.css");

            var body = new ContainerFake("body").Add(
                new ContainerFake("vl").Add(
                    new ContainerFake("hl").Add(
                        new LayoutableFake("Button"),
                        new LayoutableFake("Button", "small")
                    ),
                    new LayoutableFake("Button", "small"),
                    new ContainerFake("hl", "small").Add(
                        new LayoutableFake("Button")
                    ),
                    new LayoutableFake("Button")
                )
            );

            _styleSheet.Assign(body);

            // body
            var styles = _styleSheet.GetStyles(body[0][0][0]);
            Assert.AreEqual(50, FindStyle<Font>(styles).Value);

            styles = _styleSheet.GetStyles(body[0][0][1]);
            Assert.AreEqual(30, FindStyle<Font>(styles).Value);

            styles = _styleSheet.GetStyles(body[0][1]);
            Assert.AreEqual(10, FindStyle<Font>(styles).Value);

            styles = _styleSheet.GetStyles(body[0][2][0]);
            Assert.AreEqual(70, FindStyle<Font>(styles).Value);

            styles = _styleSheet.GetStyles(body[0][3]);
            Assert.AreEqual(90, FindStyle<Font>(styles).Value);
        }

        [TestMethod]
        public void Assign_ReassignControl_ChangeState()
        {
            LoadStyleSheet("Styles.for_reassign.css");
            var button = new LayoutableFake("Button", "old-style");
            var body = new ContainerFake("body").Add(button);

            // prepare styles
            _styleSheet.Assign(body);
            // check
            var styles = _styleSheet.GetStyles(button);
            Assert.AreEqual("#f00", FindStyle<TextColor>(styles).Value.ToString());
            // reassign
            button.CssClass = "new-style";
            _styleSheet.Assign(button);
            // check
            styles = _styleSheet.GetStyles(button);
            Assert.AreEqual("#0f0", FindStyle<TextColor>(styles).Value.ToString());
        }

        [TestMethod]
        public void StylesIntersection_FewStyles_ReturnsDelta()
        {
            var oldStyles = new Dictionary<Type, IStyle>();
            var color = new TextColor(0);
            color.FromString("#f00");
            oldStyles.Add(color.GetType(), color);
            var font = new Font(2);
            font.FromString("80% arial");
            oldStyles.Add(font.GetType(), font);
            var dockAlign = new DockAlign(4);
            dockAlign.FromString("top");
            oldStyles.Add(dockAlign.GetType(), dockAlign);

            var newStyles = new Dictionary<Type, IStyle>();
            var whiteSpace = new WhiteSpace(1);
            whiteSpace.FromString("nowrap");
            newStyles.Add(whiteSpace.GetType(), whiteSpace);
            var newFont = new Font(3);
            newFont.FromString("5px tahoma");
            newStyles.Add(newFont.GetType(), newFont);
            var newColor = new TextColor(5);
            newColor.FromString("#F00");
            newStyles.Add(newColor.GetType(), newColor);

            IDictionary<Type, IStyle> actual = _styleSheet.StylesIntersection(oldStyles, newStyles);

            Assert.AreEqual(2, actual.Count);
            Assert.AreEqual(whiteSpace, actual[typeof(WhiteSpace)]);
            Assert.AreEqual(newFont, actual[typeof(Font)]);
        }

        private void LoadStyleSheet(string fileName)
        {
            string path = string.Format("BitMobile.StyleSheet.UnitTests.{0}", fileName);
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path))
                _styleSheet.Load(stream);
        }

        private T FindStyle<T>(IDictionary<Type, IStyle> styles)
        {
            if (styles.ContainsKey(typeof(T)))
                return (T)styles[typeof(T)];
            throw new KeyNotFoundException();
        }

        class LayoutableFake : ILayoutable
        {
            private IDictionary<Type, IStyle> _styles;

            public LayoutableFake(string name, string cssClass = null)
            {
                Name = name;
                CssClass = cssClass;
            }

            public virtual LayoutableFake this[int index]
            {
                get { throw new NotImplementedException(); }
            }

            public string Name { get; private set; }

            public string CssClass { get; set; }

            public IBound ApplyStyles(IStyleSheet stylesheet, IBound styleBound, IBound maxBound)
            {
                throw new NotImplementedException();
            }

            public void Dispose()
            {
                throw new NotImplementedException();
            }

            public string Id { get; set; }

            public IRectangle Frame
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            public ILayoutableContainer Parent { get; set; }

            public IStyleKey StyleKey { get; set; }

            public IDictionary<Type, IStyle> Styles { get; set; }
            public void CreateView()
            {
                throw new NotImplementedException();
            }

            public void Refresh()
            {
                throw new NotImplementedException();
            }

            public IDictionary<Type, IStyle> StylesOld { get; set; }
        }

        class ContainerFake : LayoutableFake, ILayoutableContainer
        {
            private readonly List<ILayoutable> _childs = new List<ILayoutable>();

            public ContainerFake(string name, string cssClass = null)
                : base(name, cssClass)
            {
            }

            public override LayoutableFake this[int index]
            {
                get { return (LayoutableFake)_childs[index]; }
            }

            public ContainerFake Add(params ILayoutable[] objs)
            {
                foreach (var obj in objs)
                    AddChild(obj);
                return this;
            }

            public void AddChild(object obj)
            {
                var layoutable = obj as ILayoutable;
                if (layoutable == null)
                    throw new ArgumentException("obj");

                layoutable.Parent = this;
                _childs.Add(layoutable);
            }

            public void Insert(int index, object obj)
            {
                throw new NotImplementedException();
            }

            public void Withdraw(int index)
            {
                throw new NotImplementedException();
            }

            public void Inject(int index, string xml)
            {
                throw new NotImplementedException();
            }

            public void CreateChildrens()
            {
                throw new NotImplementedException();
            }

            public object[] Controls
            {
                get { return _childs.OfType<object>().ToArray(); }
            }

            public object GetControl(int index)
            {
                return _childs[index];
            }

            public void RelayoutByChild(ILayoutable child)
            {
                throw new NotImplementedException();
            }
        }
    }
}
