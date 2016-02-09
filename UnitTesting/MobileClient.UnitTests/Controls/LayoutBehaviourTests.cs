using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BitMobile.Controls;
using BitMobile.Controls.StyleSheet;

namespace MoblieClient.Tests.Controls
{
    [TestClass]
    public class LayoutBehaviourTests
    {
        [TestMethod]
        public void Horizontal_StandartLayouts_ChangeControlsFrame()
        {
            #region Prepare

            StubControl parent = new StubControl
            {
                Width = 480,
                Heigth = 320,
                PaddingLeft = 39,
                PaddingTop = 19,
                PaddingRight = 19,
                PaddingBottom = 39,
                BorderWidth = 1
            };
            parent.Frame = new Rectangle(0, 0, 480, 320);

            List<StubControl> controls = new List<StubControl>();
            controls.Add(new StubControl // 1
            {
                Width = 40,
                Heigth = 80,
                MarginLeft = 20,
                MarginTop = 20,
                MarginRight = 10,
                MarginBottom = 42,
                VerticalAlign = VerticalAlign.Align.Top
            });
            controls.Add(new StubControl // 2
            {
                Width = 40,
                Heigth = 80,
                MarginLeft = 10,
                MarginTop = 20,
                MarginRight = 0,
                MarginBottom = 0,
                VerticalAlign = VerticalAlign.Align.Central
            });
            controls.Add(new StubControl // 3
            {
                Width = 40,
                Heigth = 80,
                MarginLeft = 20,
                MarginTop = 42,
                MarginRight = 20,
                MarginBottom = 20,
                VerticalAlign = VerticalAlign.Align.Bottom
            });
            controls.Add(new StubControl // 4
            {
                Width = 40,
                Heigth = 10000,
                MarginLeft = 0,
                MarginTop = 20,
                MarginRight = 0,
                MarginBottom = 20,
                VerticalAlign = VerticalAlign.Align.Top
            });
            controls.Add(new StubControl // 5
            {
                Width = 0,
                Heigth = 40,
                MarginLeft = 20,
                MarginTop = 20,
                MarginRight = 0,
                MarginBottom = 20,
                ImageWidth = 50,
                ImageHeight = 100,
                VerticalAlign = VerticalAlign.Align.Top
            });
            controls.Add(new StubControl // 6
            {
                Width = 380,
                Heigth = 240,
                MarginLeft = 20,
                MarginTop = 20,
                MarginRight = 20,
                MarginBottom = 0,
                VerticalAlign = VerticalAlign.Align.Top
            });
            #endregion

            // inextensible
            Bound r1 = LayoutBehaviour.Horizontal(new StubStyleSheet(), parent, controls, new Bound(480, 320), new Bound(480, 320));

            Assert.AreEqual(new Rectangle(60, 40, 40, 80), controls[0].Frame);
            Assert.AreEqual(new Rectangle(120, 120, 40, 80), controls[1].Frame);
            Assert.AreEqual(new Rectangle(180, 180, 40, 80), controls[2].Frame);
            Assert.AreEqual(new Rectangle(240, 40, 40, 220), controls[3].Frame);
            Assert.AreEqual(new Rectangle(300, 40, 20, 40), controls[4].Frame);
            Assert.AreEqual(new Rectangle(340, 40, 100, 240), controls[5].Frame);
            Assert.AreEqual(r1, new Bound(480, 320));

            // extensible            
            Bound r2 = LayoutBehaviour.Horizontal(new StubStyleSheet(), parent, controls, new Bound(480, 320), new Bound(480, 320), true);

            Assert.AreEqual(new Rectangle(60, 40, 40, 80), controls[0].Frame);
            Assert.AreEqual(new Rectangle(120, 120, 40, 80), controls[1].Frame);
            Assert.AreEqual(new Rectangle(180, 180, 40, 80), controls[2].Frame);
            Assert.AreEqual(new Rectangle(240, 40, 40, 220), controls[3].Frame);
            Assert.AreEqual(new Rectangle(300, 40, 20, 40), controls[4].Frame);
            Assert.AreEqual(new Rectangle(340, 40, 380, 240), controls[5].Frame);
            Assert.AreEqual(r2.ContentWidth, 760);
        }

        [TestMethod]
        public void Horizontal_SizeToContent_ChangeControlsFrame()
        {
            #region Prepare

            StubControl parent = new StubControl
            {
                Width = 480,
                Heigth = 320,
                PaddingLeft = 39,
                PaddingTop = 19,
                PaddingRight = 19,
                PaddingBottom = 39,
                BorderWidth = 1
            };
            parent.Frame = new Rectangle(0, 0, 480, 320);

            List<StubControl> controls = new List<StubControl>();
            controls.Add(new StubControl // 1
            {
                Width = 5,
                Heigth = 5,
                MarginLeft = 20,
                MarginTop = 20,
                MarginRight = 10,
                MarginBottom = 42,
                VerticalAlign = VerticalAlign.Align.Top,
                SizeToContentWidth = true,
                ContentWidth = 40,
                SizeToContentHeight = true,
                ContentHeigth = 80
            });
            controls.Add(new StubControl // 2
            {
                Width = 5,
                Heigth = 5,
                MarginLeft = 10,
                MarginTop = 20,
                MarginRight = 0,
                MarginBottom = 0,
                VerticalAlign = VerticalAlign.Align.Central,
                SizeToContentWidth = true,
                ContentWidth = 40,
                SizeToContentHeight = true,
                ContentHeigth = 80
            });
            controls.Add(new StubControl // 3
            {
                Width = 5,
                Heigth = 5,
                MarginLeft = 20,
                MarginTop = 42,
                MarginRight = 20,
                MarginBottom = 20,
                VerticalAlign = VerticalAlign.Align.Bottom,
                SizeToContentWidth = true,
                ContentWidth = 40,
                SizeToContentHeight = true,
                ContentHeigth = 80
            });
            controls.Add(new StubControl // 4
            {
                Width = 5,
                Heigth = 5,
                MarginLeft = 0,
                MarginTop = 20,
                MarginRight = 0,
                MarginBottom = 20,
                VerticalAlign = VerticalAlign.Align.Top,
                SizeToContentWidth = true,
                ContentWidth = 40,
                SizeToContentHeight = true,
                ContentHeigth = 10000
            });
            controls.Add(new StubControl // 5
            {
                Width = 0,
                Heigth = 40,
                MarginLeft = 20,
                MarginTop = 20,
                MarginRight = 0,
                MarginBottom = 20,
                ImageWidth = 50,
                ImageHeight = 100,
                VerticalAlign = VerticalAlign.Align.Top
            });
            controls.Add(new StubControl // 6
            {
                Width = 5,
                Heigth = 5,
                MarginLeft = 20,
                MarginTop = 20,
                MarginRight = 20,
                MarginBottom = 0,
                VerticalAlign = VerticalAlign.Align.Top,
                SizeToContentWidth = true,
                ContentWidth = 380,
                SizeToContentHeight = true,
                ContentHeigth = 240
            });
            #endregion

            // inextensible
            Bound r1 = LayoutBehaviour.Horizontal(new StubStyleSheet(), parent, controls, new Bound(480, 320), new Bound(480, 320));

            Assert.AreEqual(new Rectangle(60, 40, 40, 80), controls[0].Frame);
            Assert.AreEqual(new Rectangle(120, 120, 40, 80), controls[1].Frame);
            Assert.AreEqual(new Rectangle(180, 180, 40, 80), controls[2].Frame);
            Assert.AreEqual(new Rectangle(240, 40, 40, 220), controls[3].Frame);
            Assert.AreEqual(new Rectangle(300, 40, 20, 40), controls[4].Frame);
            Assert.AreEqual(new Rectangle(340, 40, 100, 240), controls[5].Frame);
            Assert.AreEqual(r1, new Bound(480, 320));

            // extensible            
            Bound r2 = LayoutBehaviour.Horizontal(new StubStyleSheet(), parent, controls, new Bound(480, 320), new Bound(480, 320), true);

            Assert.AreEqual(new Rectangle(60, 40, 40, 80), controls[0].Frame);
            Assert.AreEqual(new Rectangle(120, 120, 40, 80), controls[1].Frame);
            Assert.AreEqual(new Rectangle(180, 180, 40, 80), controls[2].Frame);
            Assert.AreEqual(new Rectangle(240, 40, 40, 220), controls[3].Frame);
            Assert.AreEqual(new Rectangle(300, 40, 20, 40), controls[4].Frame);
            Assert.AreEqual(new Rectangle(340, 40, 380, 240), controls[5].Frame);
            Assert.AreEqual(r2.ContentWidth, 760);
        }

        [TestMethod]
        public void Vertical_StandartLayouts_ChangeControlsFrame()
        {
            #region Prepare

            StubControl parent = new StubControl
            {
                Width = 320,
                Heigth = 480,
                PaddingLeft = 19,
                PaddingTop = 39,
                PaddingRight = 39,
                PaddingBottom = 19,
                BorderWidth = 1
            };
            parent.Frame = new Rectangle(0, 0, 320, 480);

            List<StubControl> controls = new List<StubControl>();
            controls.Add(new StubControl // 1
            {
                Width = 80,
                Heigth = 40,
                MarginLeft = 20,
                MarginTop = 20,
                MarginRight = 42,
                MarginBottom = 10,
                HorizontalAlign = HorizontalAlign.Align.Left
            });
            controls.Add(new StubControl // 2
            {
                Width = 80,
                Heigth = 40,
                MarginLeft = 20,
                MarginTop = 10,
                MarginRight = 0,
                MarginBottom = 0,
                HorizontalAlign = HorizontalAlign.Align.Central
            });
            controls.Add(new StubControl // 3
            {
                Width = 80,
                Heigth = 40,
                MarginLeft = 42,
                MarginTop = 20,
                MarginRight = 20,
                MarginBottom = 20,
                HorizontalAlign = HorizontalAlign.Align.Right
            });
            controls.Add(new StubControl // 4
            {
                Width = 10000,
                Heigth = 40,
                MarginLeft = 20,
                MarginTop = 0,
                MarginRight = 20,
                MarginBottom = 0,
                HorizontalAlign = HorizontalAlign.Align.Left
            });
            controls.Add(new StubControl // 5
            {
                Width = 40,
                Heigth = 0,
                MarginLeft = 20,
                MarginTop = 20,
                MarginRight = 20,
                MarginBottom = 0,
                ImageWidth = 100,
                ImageHeight = 50,
                HorizontalAlign = HorizontalAlign.Align.Left
            });
            controls.Add(new StubControl // 6
            {
                Width = 240,
                Heigth = 380,
                MarginLeft = 20,
                MarginTop = 20,
                MarginRight = 0,
                MarginBottom = 20,
                HorizontalAlign = HorizontalAlign.Align.Left
            });
            #endregion

            // inextensible
            Bound r1 = LayoutBehaviour.Vertical(new StubStyleSheet(), parent, controls, new Bound(320, 480), new Bound(320, 480));

            Assert.AreEqual(new Rectangle(40, 60, 80, 40), controls[0].Frame);
            Assert.AreEqual(new Rectangle(120, 120, 80, 40), controls[1].Frame);
            Assert.AreEqual(new Rectangle(180, 180, 80, 40), controls[2].Frame);
            Assert.AreEqual(new Rectangle(40, 240, 220, 40), controls[3].Frame);
            Assert.AreEqual(new Rectangle(40, 300, 40, 20), controls[4].Frame);
            Assert.AreEqual(new Rectangle(40, 340, 240, 100), controls[5].Frame);
            Assert.AreEqual(r1, new Bound(320, 480));

            // extensible            
            Bound r2 = LayoutBehaviour.Vertical(new StubStyleSheet(), parent, controls, new Bound(320, 480), new Bound(320, 480), true);

            Assert.AreEqual(new Rectangle(40, 60, 80, 40), controls[0].Frame);
            Assert.AreEqual(new Rectangle(120, 120, 80, 40), controls[1].Frame);
            Assert.AreEqual(new Rectangle(180, 180, 80, 40), controls[2].Frame);
            Assert.AreEqual(new Rectangle(40, 240, 220, 40), controls[3].Frame);
            Assert.AreEqual(new Rectangle(40, 300, 40, 20), controls[4].Frame);
            Assert.AreEqual(new Rectangle(40, 340, 240, 380), controls[5].Frame);
            Assert.AreEqual(r2.ContentHeight, 760);
        }

        [TestMethod]
        public void Vertical_SizeToContent_ChangeControlsFrame()
        {
            #region Prepare

            StubControl parent = new StubControl
            {
                Width = 320,
                Heigth = 480,
                PaddingLeft = 19,
                PaddingTop = 39,
                PaddingRight = 39,
                PaddingBottom = 19,
                BorderWidth = 1
            };
            parent.Frame = new Rectangle(0, 0, 320, 480);

            List<StubControl> controls = new List<StubControl>();
            controls.Add(new StubControl // 1
            {
                Width = 80,
                Heigth = 40,
                MarginLeft = 20,
                MarginTop = 20,
                MarginRight = 42,
                MarginBottom = 10,
                HorizontalAlign = HorizontalAlign.Align.Left
            });
            controls.Add(new StubControl // 2
            {
                Width = 80,
                Heigth = 40,
                MarginLeft = 20,
                MarginTop = 10,
                MarginRight = 0,
                MarginBottom = 0,
                HorizontalAlign = HorizontalAlign.Align.Central
            });
            controls.Add(new StubControl // 3
            {
                Width = 5,
                Heigth = 5,
                MarginLeft = 42,
                MarginTop = 20,
                MarginRight = 20,
                MarginBottom = 20,
                HorizontalAlign = HorizontalAlign.Align.Right,
                SizeToContentWidth = true,
                ContentWidth = 80,
                SizeToContentHeight = true,
                ContentHeigth = 40
            });
            controls.Add(new StubControl // 4
            {
                Width = 10000,
                Heigth = 40,
                MarginLeft = 20,
                MarginTop = 0,
                MarginRight = 20,
                MarginBottom = 0,
                HorizontalAlign = HorizontalAlign.Align.Left
            });
            controls.Add(new StubControl // 5
            {
                Width = 40,
                Heigth = 0,
                MarginLeft = 20,
                MarginTop = 20,
                MarginRight = 20,
                MarginBottom = 0,
                ImageWidth = 100,
                ImageHeight = 50,
                HorizontalAlign = HorizontalAlign.Align.Left
            });
            controls.Add(new StubControl // 6
            {
                Width = 240,
                Heigth = 380,
                MarginLeft = 20,
                MarginTop = 20,
                MarginRight = 0,
                MarginBottom = 20,
                HorizontalAlign = HorizontalAlign.Align.Left
            });
            #endregion

            // inextensible
            Bound r1 = LayoutBehaviour.Vertical(new StubStyleSheet(), parent, controls, new Bound(320, 480), new Bound(320, 480));

            Assert.AreEqual(new Rectangle(40, 60, 80, 40), controls[0].Frame);
            Assert.AreEqual(new Rectangle(120, 120, 80, 40), controls[1].Frame);
            Assert.AreEqual(new Rectangle(180, 180, 80, 40), controls[2].Frame);
            Assert.AreEqual(new Rectangle(40, 240, 220, 40), controls[3].Frame);
            Assert.AreEqual(new Rectangle(40, 300, 40, 20), controls[4].Frame);
            Assert.AreEqual(new Rectangle(40, 340, 240, 100), controls[5].Frame);
            Assert.AreEqual(r1, new Bound(320, 480));

            // extensible            
            Bound r2 = LayoutBehaviour.Vertical(new StubStyleSheet(), parent, controls, new Bound(320, 480), new Bound(320, 480), true);

            Assert.AreEqual(new Rectangle(40, 60, 80, 40), controls[0].Frame);
            Assert.AreEqual(new Rectangle(120, 120, 80, 40), controls[1].Frame);
            Assert.AreEqual(new Rectangle(180, 180, 80, 40), controls[2].Frame);
            Assert.AreEqual(new Rectangle(40, 240, 220, 40), controls[3].Frame);
            Assert.AreEqual(new Rectangle(40, 300, 40, 20), controls[4].Frame);
            Assert.AreEqual(new Rectangle(40, 340, 240, 380), controls[5].Frame);
            Assert.AreEqual(r2.ContentHeight, 760);
        }

        [TestMethod]
        public void Dock_StandartLayouts_ChangeControlsFrame()
        {
            #region Prepare

            StubControl parent = new StubControl
            {
                Width = 320,
                Heigth = 480,
                PaddingLeft = 19,
                PaddingTop = 39,
                PaddingRight = 39,
                PaddingBottom = 19,
                BorderWidth = 1
            };
            parent.Frame = new Rectangle(0, 0, 320, 480);

            List<StubControl> controls = new List<StubControl>();
            controls.Add(new StubControl // 1
            {
                Width = 60,
                Heigth = 180,
                MarginLeft = 20,
                MarginTop = 20,
                MarginRight = 20,
                MarginBottom = 42,
                DockAlign = DockAlign.Align.Left
            });
            controls.Add(new StubControl // 2
            {
                Width = 100,
                Heigth = 40,
                MarginLeft = 40,
                MarginTop = 20,
                MarginRight = 20,
                MarginBottom = 20,
                DockAlign = DockAlign.Align.Top
            });
            controls.Add(new StubControl // 3
            {
                Width = 60,
                Heigth = 0,
                MarginLeft = 0,
                MarginTop = 80,
                MarginRight = 0,
                MarginBottom = 0,
                ImageWidth = 50,
                ImageHeight = 100,
                DockAlign = DockAlign.Align.Right
            });
            controls.Add(new StubControl // 4
            {
                Width = 10000,
                Heigth = 60,
                MarginLeft = 0,
                MarginTop = 20,
                MarginRight = 0,
                MarginBottom = 0,
                DockAlign = DockAlign.Align.Bottom
            });
            controls.Add(new StubControl // 5
            {
                Width = 0,
                Heigth = 60,
                MarginLeft = 20,
                MarginTop = 20,
                MarginRight = 20,
                MarginBottom = 20,
                ImageWidth = 2,
                ImageHeight = 1,
                DockAlign = DockAlign.Align.Top
            });
            controls.Add(new StubControl // 6
            {
                Width = 60,
                Heigth = 140,
                MarginLeft = 20,
                MarginTop = 20,
                MarginRight = 20,
                MarginBottom = 0,
                DockAlign = DockAlign.Align.Left
            });
            #endregion

            Bound r = LayoutBehaviour.Dock(new StubStyleSheet(), parent, controls, new Bound(320, 480), new Bound(320, 480));

            Assert.AreEqual(new Rectangle(40, 60, 60, 180), controls[0].Frame);
            Assert.AreEqual(new Rectangle(160, 60, 100, 40), controls[1].Frame);
            Assert.AreEqual(new Rectangle(220, 200, 60, 120), controls[2].Frame);
            Assert.AreEqual(new Rectangle(120, 400, 100, 60), controls[3].Frame);
            Assert.AreEqual(new Rectangle(140, 140, 60, 60), controls[4].Frame);
            Assert.AreEqual(new Rectangle(140, 240, 60, 140), controls[5].Frame);
            Assert.AreEqual(new Bound(320, 480), r);
        }

        [TestMethod]
        public void Dock_SizeToContent_ChangeControlsFrame()
        {
            #region Prepare

            StubControl parent = new StubControl
            {
                Width = 320,
                Heigth = 480,
                PaddingLeft = 19,
                PaddingTop = 39,
                PaddingRight = 39,
                PaddingBottom = 19,
                BorderWidth = 1
            };
            parent.Frame = new Rectangle(0, 0, 320, 480);

            List<StubControl> controls = new List<StubControl>();
            controls.Add(new StubControl // 1
            {
                Width = 5,
                Heigth = 5,
                MarginLeft = 20,
                MarginTop = 20,
                MarginRight = 20,
                MarginBottom = 42,
                DockAlign = DockAlign.Align.Left,
                SizeToContentWidth = true,
                ContentWidth = 60,
                SizeToContentHeight = true,
                ContentHeigth = 180
            });
            controls.Add(new StubControl // 2
            {
                Width = 5,
                Heigth = 5,
                MarginLeft = 40,
                MarginTop = 20,
                MarginRight = 20,
                MarginBottom = 20,
                DockAlign = DockAlign.Align.Top,
                SizeToContentWidth = true,
                ContentWidth = 100,
                SizeToContentHeight = true,
                ContentHeigth = 40
            });
            controls.Add(new StubControl // 3
            {
                Width = 60,
                Heigth = 0,
                MarginLeft = 0,
                MarginTop = 80,
                MarginRight = 0,
                MarginBottom = 0,
                ImageWidth = 50,
                ImageHeight = 100,
                DockAlign = DockAlign.Align.Right
            });
            controls.Add(new StubControl // 4
            {
                Width = 10000,
                Heigth = 5,
                MarginLeft = 0,
                MarginTop = 20,
                MarginRight = 0,
                MarginBottom = 0,
                DockAlign = DockAlign.Align.Bottom,
                SizeToContentWidth = true,
                ContentWidth = 10000,
                SizeToContentHeight = true,
                ContentHeigth = 60
            });
            controls.Add(new StubControl // 5
            {
                Width = 0,
                Heigth = 60,
                MarginLeft = 20,
                MarginTop = 20,
                MarginRight = 20,
                MarginBottom = 20,
                ImageWidth = 2,
                ImageHeight = 1,
                DockAlign = DockAlign.Align.Top,
                SizeToContentWidth = true,
                ContentWidth = 10000,
                SizeToContentHeight = true,
                ContentHeigth = 60
            });
            controls.Add(new StubControl // 6
            {
                Width = 60,
                Heigth = 140,
                MarginLeft = 20,
                MarginTop = 20,
                MarginRight = 20,
                MarginBottom = 0,
                DockAlign = DockAlign.Align.Left
            });
            #endregion

            Bound r = LayoutBehaviour.Dock(new StubStyleSheet(), parent, controls, new Bound(320, 480), new Bound(320, 480));

            Assert.AreEqual(new Rectangle(40, 60, 60, 180), controls[0].Frame);
            Assert.AreEqual(new Rectangle(160, 60, 100, 40), controls[1].Frame);
            Assert.AreEqual(new Rectangle(220, 200, 60, 120), controls[2].Frame);
            Assert.AreEqual(new Rectangle(120, 400, 100, 60), controls[3].Frame);
            Assert.AreEqual(new Rectangle(140, 140, 60, 60), controls[4].Frame);
            Assert.AreEqual(new Rectangle(140, 240, 60, 140), controls[5].Frame);
            Assert.AreEqual(new Bound(320, 480), r);
        }

        [TestMethod]
        public void Screen_StandartLayouts_ChangeControlsFrame()
        {
            #region Prepare
            StubControl parent = new StubControl
            {
                Width = 320,
                Heigth = 480,
                PaddingLeft = 39,
                PaddingTop = 19,
                PaddingRight = 19,
                PaddingBottom = 39,
                BorderWidth = 1
            };
            parent.Frame = new Rectangle(0, 0, 320, 480);

            StubControl control = new StubControl
            {
                Width = 1000,
                Heigth = 200,
                MarginLeft = 20,
                MarginTop = 20,
                MarginRight = 10,
                MarginBottom = 42,
                SizeToContentHeight = true,
                ContentHeigth = 300,
                SizeToContentWidth = true,
                ContentWidth = 30
            };
            #endregion

            LayoutBehaviour.Screen(new StubStyleSheet(), parent, control, new Bound(320, 480));

            Assert.AreEqual(new Rectangle(60, 40, 230, 300), control.Frame);
        }

        class StubStyleSheetHelper : IStyleSheetHelper
        {
            public float Width(IStyledObject control, float parentSize)
            {
                return ((StubControl)control).Width;
            }

            public float Height(IStyledObject control, float parentSize)
            {
                return ((StubControl)control).Heigth;
            }

            public float Margin<T>(IStyledObject control, float parentSize) where T : Margin
            {
                StubControl c = (StubControl)control;
                if (typeof(T) == typeof(MarginLeft))
                    return c.MarginLeft;
                if (typeof(T) == typeof(MarginTop))
                    return c.MarginTop;
                if (typeof(T) == typeof(MarginRight))
                    return c.MarginRight;
                if (typeof(T) == typeof(MarginBottom))
                    return c.MarginBottom;
                throw new Exception();
            }

            public float Padding<T>(IStyledObject control, float parentSize) where T : Padding
            {
                StubControl c = (StubControl)control;
                if (typeof(T) == typeof(PaddingLeft))
                    return c.PaddingLeft;
                if (typeof(T) == typeof(PaddingTop))
                    return c.PaddingTop;
                if (typeof(T) == typeof(PaddingRight))
                    return c.PaddingRight;
                if (typeof(T) == typeof(PaddingBottom))
                    return c.PaddingBottom;
                throw new Exception();
            }

            public bool HasBorder(IStyledObject control)
            {
                throw new NotImplementedException();
            }

            public float BorderWidth(IStyledObject control)
            {
                return ((StubControl)control).BorderWidth;
            }

            public float BorderRadius(IStyledObject control)
            {
                throw new NotImplementedException();
            }

            public DockAlign.Align DockAlign(IStyledObject control)
            {
                return ((StubControl)control).DockAlign;
            }

            public HorizontalAlign.Align HorizontalAlign(IStyledObject control)
            {
                return ((StubControl)control).HorizontalAlign;
            }

            public VerticalAlign.Align VerticalAlign(IStyledObject control)
            {
                return ((StubControl)control).VerticalAlign;
            }

            public bool SizeToContentWidth(IStyledObject control)
            {
                return ((StubControl)control).SizeToContentWidth;
            }

            public bool SizeToContentHeight(IStyledObject control)
            {
                return ((StubControl)control).SizeToContentHeight;
            }
        }

        class StubStyleSheet : StyleSheet
        {
            public StubStyleSheet()
            {
                Helper = new StubStyleSheetHelper();
            }

            public override Dictionary<Type, Style> GetStyles(object obj)
            {
                throw new NotImplementedException();
            }

            //            public override void ProcessChildren(object obj)
            //            {
            //                throw new NotImplementedException();
            //            }

            public override void Assign(object root)
            {
                throw new NotImplementedException();
            }
        }

        class StubControl : IControl<string>, IImageContainer
        {
            public float Width { get; set; }
            public float Heigth { get; set; }

            public float MarginLeft { get; set; }
            public float MarginTop { get; set; }
            public float MarginRight { get; set; }
            public float MarginBottom { get; set; }

            public float PaddingLeft { get; set; }
            public float PaddingTop { get; set; }
            public float PaddingRight { get; set; }
            public float PaddingBottom { get; set; }

            public float BorderWidth { get; set; }

            public DockAlign.Align DockAlign { get; set; }
            public HorizontalAlign.Align HorizontalAlign { get; set; }
            public VerticalAlign.Align VerticalAlign { get; set; }

            public bool SizeToContentWidth { get; set; }
            public bool SizeToContentHeight { get; set; }

            public float ContentWidth { get; set; }
            public float ContentHeigth { get; set; }

            public string CreateView()
            {
                throw new NotImplementedException();
            }

            public string View
            {
                get { throw new NotImplementedException(); }
            }

            public object Parent
            {
                get
                {
                    throw new NotImplementedException();
                }
                set
                {
                    throw new NotImplementedException();
                }
            }

            public Rectangle Frame { get; set; }

            public bool Visible { get; set; }

            public string CssClass
            {
                get
                {
                    throw new NotImplementedException();
                }
                set
                {
                    throw new NotImplementedException();
                }
            }

            public Bound Apply(StyleSheet stylesheet, Bound styleBound, Bound maxBound)
            {
                float w = styleBound.Width;
                float h = styleBound.Height;

                if (SizeToContentWidth && ContentWidth > w)
                    w = ContentWidth > maxBound.Width ? maxBound.Width : ContentWidth;

                if (SizeToContentHeight && ContentHeigth > h)
                    h = ContentHeigth > maxBound.Height ? maxBound.Height : ContentHeigth;

                return new Bound(w, h);
            }

            public void Dispose()
            {
                throw new NotImplementedException();
            }

            public bool InitializeImage(StyleSheet stylesheet)
            {
                return ImageWidth > 0 && ImageHeight > 0;
            }

            public int ImageWidth { get; set; }

            public int ImageHeight { get; set; }
        }
    }
}
