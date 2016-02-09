using BitMobile.Common.Controls;
using BitMobile.Common.StyleSheet;
using BitMobile.Controls;
using BitMobile.StyleSheet;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using StyleSheetContext = BitMobile.Application.StyleSheet.StyleSheetContext;

namespace BitMobile.MobileClient.UnitTests.Controls
{
    // ReSharper disable MemberCanBePrivate.Local
    // ReSharper disable UnusedMember.Local
    [TestClass]
    public class LayoutBehaviourTests
    {

        #region Additional test attributes

        //Use TestInitialize to run code before running each test 
        [TestInitialize]
        public void MyTestInitialize()
        {
            StyleSheetContext.Init(new StubStyleSheetContext());
        }

        //Use TestCleanup to run code after each test has run
        [TestCleanup]
        public void MyTestCleanup()
        {
            StyleSheetContext.Init(null);
        }

        #endregion

        [TestMethod]
        public void Horizontal_SmallChildrensOutOfBorders_ChangeControlsFrame()
        {
            #region Prepare

            var parent = new StubControl
            {
                Width = 480,
                Heigth = 320,
                PaddingLeft = 39,
                PaddingTop = 19,
                PaddingRight = 19,
                PaddingBottom = 39,
                BorderWidth = 1,
                SizeToContentWidth = true,
                SizeToContentHeight = true
            };

            var controls = new List<StubControl>
            {
                new StubControl // 1
                {
                    Width = 200,
                    Heigth = 340,
                    MarginLeft = 20,
                    MarginTop = 20,
                    MarginRight = 40,
                    MarginBottom = 40,
                    VerticalAlign = VerticalAlignValues.Top
                },
                new StubControl // 2
                {
                    Width = 300,
                    Heigth = 20,
                    MarginLeft = 40,
                    MarginTop = 20,
                    MarginRight = 60,
                    MarginBottom = 0,
                    VerticalAlign = VerticalAlignValues.Bottom
                }
            };

            #endregion

            var behavior = new LayoutBehaviour(new StubStyleSheet(), parent);

            // inextensible
            {
                float[] borders;
                IBound r = behavior.Horizontal(controls, new Bound(480, 320), new Bound(640, 480), out borders);

                Assert.AreEqual(new Rectangle(60, 40, 200, 340), controls[0].Frame);
                Assert.AreEqual(new Rectangle(340, 400, 220, 20), controls[1].Frame);
                Assert.AreEqual(new Bound(640, 460), r);
                Assert.AreEqual(40, borders[0]);
                Assert.AreEqual(300, borders[1]);
                Assert.AreEqual(620, borders[2]);
            }

            // extensible         
            {
                float[] borders;
                IBound r = behavior.Horizontal(controls, new Bound(480, 320), new Bound(640, 480), out borders, extensible: true);

                Assert.AreEqual(new Rectangle(60, 40, 200, 340), controls[0].Frame);
                Assert.AreEqual(new Rectangle(340, 400, 300, 20), controls[1].Frame);
                Assert.AreEqual(new Bound(640, 460, 720, 460), r);
                Assert.AreEqual(40, borders[0]);
                Assert.AreEqual(300, borders[1]);
                Assert.AreEqual(700, borders[2]);
            }
        }

        [TestMethod]
        public void Horizontal_ManyChildrens_ChangeControlsFrame()
        {
            #region Prepare

            var parent = new StubControl
            {
                Width = 480,
                Heigth = 320,
                PaddingLeft = 39,
                PaddingTop = 19,
                PaddingRight = 19,
                PaddingBottom = 39,
                BorderWidth = 1
            };

            var controls = new List<StubControl>
            {
                new StubControl // 1
                {
                    Width = 40,
                    Heigth = 80,
                    MarginLeft = 20,
                    MarginTop = 20,
                    MarginRight = 10,
                    MarginBottom = 42,
                    VerticalAlign = VerticalAlignValues.Top
                },
                new StubControl // 2
                {
                    Width = 40,
                    Heigth = 80,
                    MarginLeft = 10,
                    MarginTop = 20,
                    MarginRight = 0,
                    MarginBottom = 0,
                    VerticalAlign = VerticalAlignValues.Center
                },
                new StubControl // 3
                {
                    Width = 40,
                    Heigth = 80,
                    MarginLeft = 20,
                    MarginTop = 42,
                    MarginRight = 20,
                    MarginBottom = 20,
                    VerticalAlign = VerticalAlignValues.Bottom
                },
                new StubControl // 4
                {
                    Width = 40,
                    Heigth = 10000,
                    MarginLeft = 0,
                    MarginTop = 20,
                    MarginRight = 0,
                    MarginBottom = 20,
                    VerticalAlign = VerticalAlignValues.Top
                },
                new StubControl // 5
                {
                    Width = 20,
                    Heigth = 40,
                    MarginLeft = 20,
                    MarginTop = 20,
                    MarginRight = 0,
                    MarginBottom = 20,
                    VerticalAlign = VerticalAlignValues.Top
                },
                new StubControl // 6
                {
                    Width = 380,
                    Heigth = 240,
                    MarginLeft = 20,
                    MarginTop = 20,
                    MarginRight = 20,
                    MarginBottom = 0,
                    VerticalAlign = VerticalAlignValues.Top
                }
            };

            #endregion

            var behavior = new LayoutBehaviour(new StubStyleSheet(), parent);

            // inextensible
            float[] borders1;
            IBound r1 = behavior.Horizontal(controls, new Bound(480, 320), new Bound(480, 320), out borders1);

            Assert.AreEqual(new Rectangle(60, 40, 40, 80), controls[0].Frame);
            Assert.AreEqual(new Rectangle(120, 120, 40, 80), controls[1].Frame);
            Assert.AreEqual(new Rectangle(180, 180, 40, 80), controls[2].Frame);
            Assert.AreEqual(new Rectangle(240, 40, 40, 220), controls[3].Frame);
            Assert.AreEqual(new Rectangle(300, 40, 20, 40), controls[4].Frame);
            Assert.AreEqual(new Rectangle(340, 40, 100, 240), controls[5].Frame);
            Assert.AreEqual(new Bound(480, 320), r1);
            Assert.AreEqual(40, borders1[0]);
            Assert.AreEqual(110, borders1[1]);
            Assert.AreEqual(160, borders1[2]);
            Assert.AreEqual(240, borders1[3]);
            Assert.AreEqual(280, borders1[4]);
            Assert.AreEqual(320, borders1[5]);
            Assert.AreEqual(460, borders1[6]);

            // extensible         
            float[] borders2;
            IBound r2 = behavior.Horizontal(controls, new Bound(480, 320), new Bound(480, 320), out borders2, extensible: true);

            Assert.AreEqual(new Rectangle(60, 40, 40, 80), controls[0].Frame);
            Assert.AreEqual(new Rectangle(120, 120, 40, 80), controls[1].Frame);
            Assert.AreEqual(new Rectangle(180, 180, 40, 80), controls[2].Frame);
            Assert.AreEqual(new Rectangle(240, 40, 40, 220), controls[3].Frame);
            Assert.AreEqual(new Rectangle(300, 40, 20, 40), controls[4].Frame);
            Assert.AreEqual(new Rectangle(340, 40, 380, 240), controls[5].Frame);
            Assert.AreEqual(760, r2.ContentWidth);
            Assert.AreEqual(40, borders2[0]);
            Assert.AreEqual(110, borders2[1]);
            Assert.AreEqual(160, borders2[2]);
            Assert.AreEqual(240, borders2[3]);
            Assert.AreEqual(280, borders2[4]);
            Assert.AreEqual(320, borders2[5]);
            Assert.AreEqual(740, borders2[6]);
        }

        [TestMethod]
        public void Horizontal_ManyChildrensWithSizeToContent_ChangeControlsFrame()
        {
            #region Prepare

            var parent = new StubControl
            {
                Width = 480,
                Heigth = 320,
                PaddingLeft = 39,
                PaddingTop = 19,
                PaddingRight = 19,
                PaddingBottom = 39,
                BorderWidth = 1
            };

            var controls = new List<StubControl>
            {
                new StubControl // 1
                {
                    Width = 5,
                    Heigth = 5,
                    MarginLeft = 20,
                    MarginTop = 20,
                    MarginRight = 10,
                    MarginBottom = 42,
                    VerticalAlign = VerticalAlignValues.Top,
                    SizeToContentWidth = true,
                    ContentWidth = 40,
                    SizeToContentHeight = true,
                    ContentHeigth = 80
                },
                new StubControl // 2
                {
                    Width = 5,
                    Heigth = 5,
                    MarginLeft = 10,
                    MarginTop = 20,
                    MarginRight = 0,
                    MarginBottom = 0,
                    VerticalAlign = VerticalAlignValues.Central,
                    SizeToContentWidth = true,
                    ContentWidth = 40,
                    SizeToContentHeight = true,
                    ContentHeigth = 80
                },
                new StubControl // 3
                {
                    Width = 5,
                    Heigth = 5,
                    MarginLeft = 20,
                    MarginTop = 42,
                    MarginRight = 20,
                    MarginBottom = 20,
                    VerticalAlign = VerticalAlignValues.Bottom,
                    SizeToContentWidth = true,
                    ContentWidth = 40,
                    SizeToContentHeight = true,
                    ContentHeigth = 80
                },
                new StubControl // 4
                {
                    Width = 5,
                    Heigth = 5,
                    MarginLeft = 0,
                    MarginTop = 20,
                    MarginRight = 0,
                    MarginBottom = 20,
                    VerticalAlign = VerticalAlignValues.Top,
                    SizeToContentWidth = true,
                    ContentWidth = 40,
                    SizeToContentHeight = true,
                    ContentHeigth = 10000
                },
                new StubControl // 5
                {
                    Width = 20,
                    Heigth = 40,
                    MarginLeft = 20,
                    MarginTop = 20,
                    MarginRight = 0,
                    MarginBottom = 20,
                    VerticalAlign = VerticalAlignValues.Top
                },
                new StubControl // 6
                {
                    Width = 5,
                    Heigth = 5,
                    MarginLeft = 20,
                    MarginTop = 20,
                    MarginRight = 20,
                    MarginBottom = 0,
                    VerticalAlign = VerticalAlignValues.Top,
                    SizeToContentWidth = true,
                    ContentWidth = 380,
                    SizeToContentHeight = true,
                    ContentHeigth = 240
                }
            };

            #endregion

            var behavior = new LayoutBehaviour(new StubStyleSheet(), parent);

            // inextensible
            float[] borders1;
            IBound r1 = behavior.Horizontal(controls, new Bound(480, 320), new Bound(480, 320), out borders1);

            Assert.AreEqual(new Rectangle(60, 40, 40, 80), controls[0].Frame);
            Assert.AreEqual(new Rectangle(120, 120, 40, 80), controls[1].Frame);
            Assert.AreEqual(new Rectangle(180, 180, 40, 80), controls[2].Frame);
            Assert.AreEqual(new Rectangle(240, 40, 40, 220), controls[3].Frame);
            Assert.AreEqual(new Rectangle(300, 40, 20, 40), controls[4].Frame);
            Assert.AreEqual(new Rectangle(340, 40, 100, 240), controls[5].Frame);
            Assert.AreEqual(new Bound(480, 320), r1);
            Assert.AreEqual(40, borders1[0]);
            Assert.AreEqual(110, borders1[1]);
            Assert.AreEqual(160, borders1[2]);
            Assert.AreEqual(240, borders1[3]);
            Assert.AreEqual(280, borders1[4]);
            Assert.AreEqual(320, borders1[5]);
            Assert.AreEqual(460, borders1[6]);

            // extensible  
            float[] borders2;
            IBound r2 = behavior.Horizontal(controls, new Bound(480, 320), new Bound(480, 320), out borders2, extensible: true);

            Assert.AreEqual(new Rectangle(60, 40, 40, 80), controls[0].Frame);
            Assert.AreEqual(new Rectangle(120, 120, 40, 80), controls[1].Frame);
            Assert.AreEqual(new Rectangle(180, 180, 40, 80), controls[2].Frame);
            Assert.AreEqual(new Rectangle(240, 40, 40, 220), controls[3].Frame);
            Assert.AreEqual(new Rectangle(300, 40, 20, 40), controls[4].Frame);
            Assert.AreEqual(new Rectangle(340, 40, 380, 240), controls[5].Frame);
            Assert.AreEqual(760, r2.ContentWidth);
            Assert.AreEqual(40, borders2[0]);
            Assert.AreEqual(110, borders2[1]);
            Assert.AreEqual(160, borders2[2]);
            Assert.AreEqual(240, borders2[3]);
            Assert.AreEqual(280, borders2[4]);
            Assert.AreEqual(320, borders2[5]);
            Assert.AreEqual(740, borders2[6]);
        }

        [TestMethod]
        public void Vertical_SmallChildrensOutOfBorders_ChangeControlsFrame()
        {
            #region Prepare

            var parent = new StubControl
            {
                Width = 320,
                Heigth = 480,
                PaddingLeft = 19,
                PaddingTop = 39,
                PaddingRight = 39,
                PaddingBottom = 19,
                BorderWidth = 1,
                SizeToContentWidth = true,
                SizeToContentHeight = true
            };

            var controls = new List<StubControl>
            {
                new StubControl // 1
                {
                    Width = 340,
                    Heigth = 200,
                    MarginLeft = 20,
                    MarginTop = 20,
                    MarginRight = 40,
                    MarginBottom = 40,
                    HorizontalAlign = HorizontalAlignValues.Left
                },
                new StubControl // 2
                {
                    Width = 20,
                    Heigth = 200,
                    MarginLeft = 20,
                    MarginTop = 40,
                    MarginRight = 0,
                    MarginBottom = 60,
                    HorizontalAlign = HorizontalAlignValues.Right
                }
            };

            #endregion

            var behavior = new LayoutBehaviour(new StubStyleSheet(), parent);

            // inextensible
            {
                float[] borders;
                IBound r = behavior.Vertical(controls, new Bound(320, 480), new Bound(480, 640), out borders);

                Assert.AreEqual(new Rectangle(40, 60, 340, 200), controls[0].Frame);
                Assert.AreEqual(new Rectangle(400, 340, 20, 200), controls[1].Frame);
                Assert.AreEqual(new Bound(460, 620), r);
                Assert.AreEqual(40, borders[0]);
                Assert.AreEqual(300, borders[1]);
                Assert.AreEqual(600, borders[2]);
            }

            // extensible         
            {
                float[] borders;
                IBound r = behavior.Vertical(controls, new Bound(320, 480), new Bound(480, 640), out borders, extensible: true);

                Assert.AreEqual(new Rectangle(40, 60, 340, 200), controls[0].Frame);
                Assert.AreEqual(new Rectangle(400, 340, 20, 200), controls[1].Frame);
                Assert.AreEqual(new Bound(460, 620, 460, 620), r);
                Assert.AreEqual(40, borders[0]);
                Assert.AreEqual(300, borders[1]);
                Assert.AreEqual(600, borders[2]);
            }
        }

        [TestMethod]
        public void Vertical_ManyChildrens_ChangeControlsFrame()
        {
            #region Prepare

            var parent = new StubControl
            {
                Width = 320,
                Heigth = 480,
                PaddingLeft = 19,
                PaddingTop = 39,
                PaddingRight = 39,
                PaddingBottom = 19,
                BorderWidth = 1
            };

            var controls = new List<StubControl>
            {
                new StubControl // 1
                {
                    Width = 80,
                    Heigth = 40,
                    MarginLeft = 20,
                    MarginTop = 20,
                    MarginRight = 42,
                    MarginBottom = 10,
                    HorizontalAlign = HorizontalAlignValues.Left
                },
                new StubControl // 2
                {
                    Width = 80,
                    Heigth = 40,
                    MarginLeft = 20,
                    MarginTop = 10,
                    MarginRight = 0,
                    MarginBottom = 0,
                    HorizontalAlign = HorizontalAlignValues.Central
                },
                new StubControl // 3
                {
                    Width = 80,
                    Heigth = 40,
                    MarginLeft = 42,
                    MarginTop = 20,
                    MarginRight = 20,
                    MarginBottom = 20,
                    HorizontalAlign = HorizontalAlignValues.Right
                },
                new StubControl // 4
                {
                    Width = 10000,
                    Heigth = 40,
                    MarginLeft = 20,
                    MarginTop = 0,
                    MarginRight = 20,
                    MarginBottom = 0,
                    HorizontalAlign = HorizontalAlignValues.Left
                },
                new StubControl // 5
                {
                    Width = 40,
                    Heigth = 20,
                    MarginLeft = 20,
                    MarginTop = 20,
                    MarginRight = 20,
                    MarginBottom = 0,
                    HorizontalAlign = HorizontalAlignValues.Left
                },
                new StubControl // 6
                {
                    Width = 240,
                    Heigth = 380,
                    MarginLeft = 20,
                    MarginTop = 20,
                    MarginRight = 0,
                    MarginBottom = 20,
                    HorizontalAlign = HorizontalAlignValues.Left
                }
            };

            #endregion

            var behavior = new LayoutBehaviour(new StubStyleSheet(), parent);

            // inextensible
            float[] borders1;
            IBound r1 = behavior.Vertical(controls, new Bound(320, 480), new Bound(320, 480), out borders1);

            Assert.AreEqual(new Rectangle(40, 60, 80, 40), controls[0].Frame);
            Assert.AreEqual(new Rectangle(120, 120, 80, 40), controls[1].Frame);
            Assert.AreEqual(new Rectangle(180, 180, 80, 40), controls[2].Frame);
            Assert.AreEqual(new Rectangle(40, 240, 220, 40), controls[3].Frame);
            Assert.AreEqual(new Rectangle(40, 300, 40, 20), controls[4].Frame);
            Assert.AreEqual(new Rectangle(40, 340, 240, 100), controls[5].Frame);
            Assert.AreEqual(new Bound(320, 480), r1);
            Assert.AreEqual(40, borders1[0]);
            Assert.AreEqual(110, borders1[1]);
            Assert.AreEqual(160, borders1[2]);
            Assert.AreEqual(240, borders1[3]);
            Assert.AreEqual(280, borders1[4]);
            Assert.AreEqual(320, borders1[5]);
            Assert.AreEqual(460, borders1[6]);

            // extensible  
            float[] borders2;
            IBound r2 = behavior.Vertical(controls, new Bound(320, 480), new Bound(320, 480), out borders2, extensible: true);

            Assert.AreEqual(new Rectangle(40, 60, 80, 40), controls[0].Frame);
            Assert.AreEqual(new Rectangle(120, 120, 80, 40), controls[1].Frame);
            Assert.AreEqual(new Rectangle(180, 180, 80, 40), controls[2].Frame);
            Assert.AreEqual(new Rectangle(40, 240, 220, 40), controls[3].Frame);
            Assert.AreEqual(new Rectangle(40, 300, 40, 20), controls[4].Frame);
            Assert.AreEqual(new Rectangle(40, 340, 240, 380), controls[5].Frame);
            Assert.AreEqual(760, r2.ContentHeight);
            Assert.AreEqual(40, borders2[0]);
            Assert.AreEqual(110, borders2[1]);
            Assert.AreEqual(160, borders2[2]);
            Assert.AreEqual(240, borders2[3]);
            Assert.AreEqual(280, borders2[4]);
            Assert.AreEqual(320, borders2[5]);
            Assert.AreEqual(740, borders2[6]);
        }

        [TestMethod]
        public void Vertical_ManyChildrensWithSizeToContent_ChangeControlsFrame()
        {
            #region Prepare

            var parent = new StubControl
            {
                Width = 320,
                Heigth = 480,
                PaddingLeft = 19,
                PaddingTop = 39,
                PaddingRight = 39,
                PaddingBottom = 19,
                BorderWidth = 1
            };

            var controls = new List<StubControl>
            {
                new StubControl // 1
                {
                    Width = 80,
                    Heigth = 40,
                    MarginLeft = 20,
                    MarginTop = 20,
                    MarginRight = 42,
                    MarginBottom = 10,
                    HorizontalAlign = HorizontalAlignValues.Left
                },
                new StubControl // 2
                {
                    Width = 80,
                    Heigth = 40,
                    MarginLeft = 20,
                    MarginTop = 10,
                    MarginRight = 0,
                    MarginBottom = 0,
                    HorizontalAlign = HorizontalAlignValues.Central
                },
                new StubControl // 3
                {
                    Width = 5,
                    Heigth = 5,
                    MarginLeft = 42,
                    MarginTop = 20,
                    MarginRight = 20,
                    MarginBottom = 20,
                    HorizontalAlign = HorizontalAlignValues.Right,
                    SizeToContentWidth = true,
                    ContentWidth = 80,
                    SizeToContentHeight = true,
                    ContentHeigth = 40
                },
                new StubControl // 4
                {
                    Width = 10000,
                    Heigth = 40,
                    MarginLeft = 20,
                    MarginTop = 0,
                    MarginRight = 20,
                    MarginBottom = 0,
                    HorizontalAlign = HorizontalAlignValues.Left
                },
                new StubControl // 5
                {
                    Width = 40,
                    Heigth = 20,
                    MarginLeft = 20,
                    MarginTop = 20,
                    MarginRight = 20,
                    MarginBottom = 0,
                    HorizontalAlign = HorizontalAlignValues.Left
                },
                new StubControl // 6
                {
                    Width = 240,
                    Heigth = 380,
                    MarginLeft = 20,
                    MarginTop = 20,
                    MarginRight = 0,
                    MarginBottom = 20,
                    HorizontalAlign = HorizontalAlignValues.Left
                }
            };

            #endregion

            var behavior = new LayoutBehaviour(new StubStyleSheet(), parent);

            // inextensible
            float[] borders1;
            IBound r1 = behavior.Vertical(controls, new Bound(320, 480), new Bound(320, 480), out borders1);

            Assert.AreEqual(new Rectangle(40, 60, 80, 40), controls[0].Frame);
            Assert.AreEqual(new Rectangle(120, 120, 80, 40), controls[1].Frame);
            Assert.AreEqual(new Rectangle(180, 180, 80, 40), controls[2].Frame);
            Assert.AreEqual(new Rectangle(40, 240, 220, 40), controls[3].Frame);
            Assert.AreEqual(new Rectangle(40, 300, 40, 20), controls[4].Frame);
            Assert.AreEqual(new Rectangle(40, 340, 240, 100), controls[5].Frame);
            Assert.AreEqual(new Bound(320, 480), r1);
            Assert.AreEqual(110, borders1[1]);
            Assert.AreEqual(160, borders1[2]);
            Assert.AreEqual(240, borders1[3]);
            Assert.AreEqual(280, borders1[4]);
            Assert.AreEqual(320, borders1[5]);
            Assert.AreEqual(460, borders1[6]);

            // extensible   
            float[] borders2;
            IBound r2 = behavior.Vertical(controls, new Bound(320, 480), new Bound(320, 480), out borders2, extensible: true);

            Assert.AreEqual(new Rectangle(40, 60, 80, 40), controls[0].Frame);
            Assert.AreEqual(new Rectangle(120, 120, 80, 40), controls[1].Frame);
            Assert.AreEqual(new Rectangle(180, 180, 80, 40), controls[2].Frame);
            Assert.AreEqual(new Rectangle(40, 240, 220, 40), controls[3].Frame);
            Assert.AreEqual(new Rectangle(40, 300, 40, 20), controls[4].Frame);
            Assert.AreEqual(new Rectangle(40, 340, 240, 380), controls[5].Frame);
            Assert.AreEqual(760, r2.ContentHeight);
            Assert.AreEqual(40, borders2[0]);
            Assert.AreEqual(110, borders2[1]);
            Assert.AreEqual(160, borders2[2]);
            Assert.AreEqual(240, borders2[3]);
            Assert.AreEqual(280, borders2[4]);
            Assert.AreEqual(320, borders2[5]);
            Assert.AreEqual(740, borders2[6]);
        }

        [TestMethod]
        public void Dock_SmallChildrensOutOfBorders_ChangeControlsFrame()
        {
            #region Prepare

            var parent = new StubControl
            {
                Width = 320,
                Heigth = 480,
                PaddingLeft = 19,
                PaddingTop = 39,
                PaddingRight = 39,
                PaddingBottom = 19,
                BorderWidth = 1,
                SizeToContentWidth = true,
                SizeToContentHeight = true
            };

            var controls = new List<StubControl>
            {
                new StubControl // 1
                {
                    Width = 20,
                    Heigth = 480,
                    MarginLeft = 0,
                    MarginTop = 0,
                    MarginRight = 0,
                    MarginBottom = 0,
                    DockAlign = DockAlignValues.Right
                },
                new StubControl // 2
                {
                    Width = 280,
                    Heigth = 40,
                    MarginLeft = 10,
                    MarginTop = 0,
                    MarginRight = 10,
                    MarginBottom = 10,
                    DockAlign = DockAlignValues.Bottom
                },
                new StubControl // 3
                {
                    Width = 1000,
                    Heigth = 40,
                    MarginLeft = 10,
                    MarginTop = 10,
                    MarginRight = 10,
                    MarginBottom = 0,
                    DockAlign = DockAlignValues.Top
                },
                new StubControl // 4
                {
                    Width = 320,
                    Heigth = 1000,
                    MarginLeft = 20,
                    MarginTop = 40,
                    MarginRight = 0,
                    MarginBottom = 60,
                    HorizontalAlign = HorizontalAlignValues.Left
                }
            };

            #endregion

            var behavior = new LayoutBehaviour(new StubStyleSheet(), parent);

            IBound r = behavior.Dock(controls, new Bound(320, 480), new Bound(480, 640));

            Assert.AreEqual(new Rectangle(420, 40, 20, 480), controls[0].Frame);
            Assert.AreEqual(new Rectangle(30, 570, 280, 40), controls[1].Frame);
            Assert.AreEqual(new Rectangle(30, 50, 380, 40), controls[2].Frame);
            Assert.AreEqual(new Rectangle(40, 130, 320, 380), controls[3].Frame);
            Assert.AreEqual(new Bound(480, 640), r);
        }

        [TestMethod]
        public void Dock_ManyChildrens_ChangeControlsFrame()
        {
            #region Prepare

            var parent = new StubControl
            {
                Width = 320,
                Heigth = 480,
                PaddingLeft = 19,
                PaddingTop = 39,
                PaddingRight = 39,
                PaddingBottom = 19,
                BorderWidth = 1
            };

            var controls = new List<StubControl>
            {
                new StubControl // 1
                {
                    Width = 60,
                    Heigth = 180,
                    MarginLeft = 20,
                    MarginTop = 20,
                    MarginRight = 20,
                    MarginBottom = 42,
                    DockAlign = DockAlignValues.Left
                },
                new StubControl // 2
                {
                    Width = 100,
                    Heigth = 40,
                    MarginLeft = 40,
                    MarginTop = 20,
                    MarginRight = 20,
                    MarginBottom = 20,
                    DockAlign = DockAlignValues.Top
                },
                new StubControl // 3
                {
                    Width = 60,
                    Heigth = 120,
                    MarginLeft = 0,
                    MarginTop = 80,
                    MarginRight = 0,
                    MarginBottom = 0,
                    DockAlign = DockAlignValues.Right
                },
                new StubControl // 4
                {
                    Width = 10000,
                    Heigth = 60,
                    MarginLeft = 0,
                    MarginTop = 20,
                    MarginRight = 0,
                    MarginBottom = 0,
                    DockAlign = DockAlignValues.Bottom
                },
                new StubControl // 5
                {
                    Width = 120,
                    Heigth = 60,
                    MarginLeft = 20,
                    MarginTop = 20,
                    MarginRight = 20,
                    MarginBottom = 20,
                    DockAlign = DockAlignValues.Top
                },
                new StubControl // 6
                {
                    Width = 60,
                    Heigth = 140,
                    MarginLeft = 20,
                    MarginTop = 20,
                    MarginRight = 20,
                    MarginBottom = 0,
                    DockAlign = DockAlignValues.Left
                }
            };

            #endregion

            var behavior = new LayoutBehaviour(new StubStyleSheet(), parent);
            IBound r = behavior.Dock(controls, new Bound(320, 480), new Bound(320, 480));

            Assert.AreEqual(new Rectangle(40, 60, 60, 180), controls[0].Frame);
            Assert.AreEqual(new Rectangle(160, 60, 100, 40), controls[1].Frame);
            Assert.AreEqual(new Rectangle(220, 200, 60, 120), controls[2].Frame);
            Assert.AreEqual(new Rectangle(120, 400, 100, 60), controls[3].Frame);
            Assert.AreEqual(new Rectangle(140, 140, 60, 60), controls[4].Frame);
            Assert.AreEqual(new Rectangle(140, 240, 60, 140), controls[5].Frame);
            Assert.AreEqual(new Bound(320, 480), r);
        }

        [TestMethod]
        public void Dock_ManyChildrensWithSizeToContent_ChangeControlsFrame()
        {
            #region Prepare

            var parent = new StubControl
            {
                Width = 320,
                Heigth = 480,
                PaddingLeft = 19,
                PaddingTop = 39,
                PaddingRight = 39,
                PaddingBottom = 19,
                BorderWidth = 1
            };

            var controls = new List<StubControl>
            {
                new StubControl // 1
                {
                    Width = 5,
                    Heigth = 5,
                    MarginLeft = 20,
                    MarginTop = 20,
                    MarginRight = 20,
                    MarginBottom = 42,
                    DockAlign = DockAlignValues.Left,
                    SizeToContentWidth = true,
                    ContentWidth = 60,
                    SizeToContentHeight = true,
                    ContentHeigth = 180
                },
                new StubControl // 2
                {
                    Width = 5,
                    Heigth = 5,
                    MarginLeft = 40,
                    MarginTop = 20,
                    MarginRight = 20,
                    MarginBottom = 20,
                    DockAlign = DockAlignValues.Top,
                    SizeToContentWidth = true,
                    ContentWidth = 100,
                    SizeToContentHeight = true,
                    ContentHeigth = 40
                },
                new StubControl // 3
                {
                    Width = 60,
                    Heigth = 120,
                    MarginLeft = 0,
                    MarginTop = 80,
                    MarginRight = 0,
                    MarginBottom = 0,
                    DockAlign = DockAlignValues.Right
                },
                new StubControl // 4
                {
                    Width = 10000,
                    Heigth = 5,
                    MarginLeft = 0,
                    MarginTop = 20,
                    MarginRight = 0,
                    MarginBottom = 0,
                    DockAlign = DockAlignValues.Bottom,
                    SizeToContentWidth = true,
                    ContentWidth = 10000,
                    SizeToContentHeight = true,
                    ContentHeigth = 60
                },
                new StubControl // 5
                {
                    Width = 120,
                    Heigth = 60,
                    MarginLeft = 20,
                    MarginTop = 20,
                    MarginRight = 20,
                    MarginBottom = 20,
                    DockAlign = DockAlignValues.Top,
                    SizeToContentWidth = true,
                    ContentWidth = 10000,
                    SizeToContentHeight = true,
                    ContentHeigth = 60
                },
                new StubControl // 6
                {
                    Width = 60,
                    Heigth = 140,
                    MarginLeft = 20,
                    MarginTop = 20,
                    MarginRight = 20,
                    MarginBottom = 0,
                    DockAlign = DockAlignValues.Left
                }
            };

            #endregion

            var behavior = new LayoutBehaviour(new StubStyleSheet(), parent);
            IBound r = behavior.Dock(controls, new Bound(320, 480), new Bound(320, 480));

            Assert.AreEqual(new Rectangle(40, 60, 60, 180), controls[0].Frame);
            Assert.AreEqual(new Rectangle(160, 60, 100, 40), controls[1].Frame);
            Assert.AreEqual(new Rectangle(220, 200, 60, 120), controls[2].Frame);
            Assert.AreEqual(new Rectangle(120, 400, 100, 60), controls[3].Frame);
            Assert.AreEqual(new Rectangle(140, 140, 60, 60), controls[4].Frame);
            Assert.AreEqual(new Rectangle(140, 240, 60, 140), controls[5].Frame);
            Assert.AreEqual(new Bound(320, 480), r);
        }

        [TestMethod]
        public void Screen_OneChild_ChangeControlsFrame()
        {
            #region Prepare
            var parent = new StubControl
            {
                Width = 320,
                Heigth = 480,
                PaddingLeft = 39,
                PaddingTop = 19,
                PaddingRight = 19,
                PaddingBottom = 39,
                BorderWidth = 1
            };

            var control = new StubControl
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

            var behavior = new LayoutBehaviour(new StubStyleSheet(), parent);
            behavior.Screen(control, new Bound(320, 480));

            Assert.AreEqual(new Rectangle(60, 40, 230, 300), control.Frame);
        }

        class StubStyleSheetContext : IStyleSheetContext
        {
            public float Scale { get; set; }
            public IBound EmptyBound { get { throw new NotImplementedException(); } }
            public IStyleSheet CreateStyleSheet()
            {
                throw new NotImplementedException();
            }

            public IBound CreateBound(float width, float height)
            {
                return new Bound(width, height);
            }

            public IBound CreateBound(float width, float height, float contentWidth, float contentHeight)
            {
                return new Bound(width, height, contentWidth, contentHeight);
            }

            public IBound MergeBound(IBound bound, float width, float height, IBound maxBound, bool safeProportion)
            {
                throw new NotImplementedException();
            }

            public IBound StrechBoundInProportion(IBound styleBound, IBound maxBound, float widthProportion, float heightProportion)
            {
                throw new NotImplementedException();
            }

            public IStyleHelper CreateHelper(IDictionary<Type, IStyle> styles, IStyleSheet styleSheet, IStyledObject subject)
            {
                throw new NotImplementedException();
            }
            
            public IBound MergeBound(IBound bound, float width, float height)
            {
                throw new NotImplementedException();
            }

            public IBound BoundInProportion(IBound styleBound, float virtualWidth, float virtualHeight)
            {
                throw new NotImplementedException();
            }
        }

        class StubStyleSheetHelper : IStyleSheetHelper
        {
            public float Width(IStyledObject control, float parentSize)
            {
                return ToStub(control).Width;
            }

            public float Height(IStyledObject control, float parentSize)
            {
                return ToStub(control).Heigth;
            }

            public float MarginLeft(IStyledObject control, float parentSize)
            {
                return ToStub(control).MarginLeft;
            }

            public float MarginTop(IStyledObject control, float parentSize)
            {
                return ToStub(control).MarginTop;
            }

            public float MarginRight(IStyledObject control, float parentSize)
            {
                return ToStub(control).MarginRight;
            }

            public float MarginBottom(IStyledObject control, float parentSize)
            {
                return ToStub(control).MarginBottom;
            }

            public float PaddingLeft(IStyledObject control, float parentSize)
            {
                return ToStub(control).PaddingLeft;
            }

            public float PaddingTop(IStyledObject control, float parentSize)
            {
                return ToStub(control).PaddingTop;
            }

            public float PaddingRight(IStyledObject control, float parentSize)
            {
                return ToStub(control).PaddingRight;
            }

            public float PaddingBottom(IStyledObject control, float parentSize)
            {
                return ToStub(control).PaddingBottom;
            }

            private StubControl ToStub(IStyledObject obj)
            {
                return (StubControl)obj;
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

            public DockAlignValues DockAlign(IStyledObject control)
            {
                return ((StubControl)control).DockAlign;
            }

            public HorizontalAlignValues HorizontalAlign(IStyledObject control)
            {
                return ((StubControl)control).HorizontalAlign;
            }

            public VerticalAlignValues VerticalAlign(IStyledObject control)
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

            public IColorInfo Color(IStyledObject control)
            {
                throw new NotImplementedException();
            }

            public IColorInfo BackgroundColor(IStyledObject control)
            {
                throw new NotImplementedException();
            }

            public IColorInfo SelectedColor(IStyledObject control)
            {
                throw new NotImplementedException();
            }

            public IColorInfo SelectedBackground(IStyledObject control)
            {
                throw new NotImplementedException();
            }

            public IColorInfo BorderColor(IStyledObject control)
            {
                throw new NotImplementedException();
            }

            public IColorInfo PlaceholderColor(IStyledObject control)
            {
                throw new NotImplementedException();
            }

            public string BackgroundImage(IStyledObject control)
            {
                throw new NotImplementedException();
            }

            public float FontSize(IStyledObject control, float parentHeight)
            {
                throw new NotImplementedException();
            }

            public string FontName(IStyledObject control)
            {
                throw new NotImplementedException();
            }

            public TextAlignValues TextAlign(IStyledObject control, TextAlignValues defaultValue = TextAlignValues.Left)
            {
                throw new NotImplementedException();
            }

            public TextFormatValues TextFormat(IStyledObject control, TextFormatValues defaulValue = TextFormatValues.Text)
            {
                throw new NotImplementedException();
            }

            public WhiteSpaceKind WhiteSpace(IStyledObject control, WhiteSpaceKind defaultValue = WhiteSpaceKind.Nowrap)
            {
                throw new NotImplementedException();
            }
        }

        class StubStyleSheet : IStyleSheet
        {
            public StubStyleSheet()
            {
                Helper = new StubStyleSheetHelper();
            }

            public void Dispose()
            {

            }

            public IStyleSheetHelper Helper { get; private set; }
            public IDictionary<Type, IStyle> GetStyles(object obj)
            {
                throw new NotImplementedException();
            }

            public void Assign(ILayoutable root)
            {
                throw new NotImplementedException();
            }

            public void Reassign(ILayoutable root)
            {
                throw new NotImplementedException();
            }

            public void Load(Stream stream)
            {
                throw new NotImplementedException();
            }

            public bool StylesEquality(IDictionary<Type, IStyle> oldStyles, IDictionary<Type, IStyle> newStyles)
            {
                throw new NotImplementedException();
            }

            public IDictionary<Type, IStyle> StylesIntersection(IDictionary<Type, IStyle> oldStyles, IDictionary<Type, IStyle> newStyles)
            {
                throw new NotImplementedException();
            }

            public T GetCache<T>() where T : class, IDisposable
            {
                throw new NotImplementedException();
            }

            public void SetCache<T>(T cache) where T : class, IDisposable
            {
                throw new NotImplementedException();
            }

            public void RemoveCache()
            {
                throw new NotImplementedException();
            }
        }

        class StubControl : IControl<string>
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

            public DockAlignValues DockAlign { get; set; }
            public HorizontalAlignValues HorizontalAlign { get; set; }
            public VerticalAlignValues VerticalAlign { get; set; }

            public bool SizeToContentWidth { get; set; }
            public bool SizeToContentHeight { get; set; }

            public float ContentWidth { get; set; }
            public float ContentHeigth { get; set; }

            public void CreateView()
            {
                throw new NotImplementedException();
            }

            public void Refresh()
            {
                throw new NotImplementedException();
            }

            public string View
            {
                get { throw new NotImplementedException(); }
            }

            public ILayoutableContainer Parent
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

            public IStyleKey StyleKey { get; set; }
            public IDictionary<Type, IStyle> Styles { get; set; }
            public void Relayout()
            {
                throw new NotImplementedException();
            }

            public IDictionary<Type, IStyle> StylesOld { get; set; }

            public string Id { get; set; }
            public IRectangle Frame { get; set; }

            public bool Visible { get; set; }

            public string Name
            {
                get { throw new NotImplementedException(); }
            }

            public string CssClass
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            public IBound ApplyStyles(IStyleSheet stylesheet, IBound styleBound, IBound maxBound)
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
        }
    }
}
