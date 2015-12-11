using MonkeyCancel;
using System;
using System.Linq;
using Xunit;
using System.Windows.Forms;
using KT_TTAPI;
using NSubstitute;

namespace MonkeyCancelTest.WinFormTests
{
    public class MonkeyCancelFrm_Update_Tests
    {
        internal class TestData
        {
            public ITTInstrumentState InstrumentStateZBU5 { get; set; }
            public Instrument InstrumentZBU5 { get; set; }
            public ITTInstrumentState InstrumentStateZFU5 { get; set; }
            public Instrument InstrumentZFU5 { get; set; }
            public ITTInstrumentState InstrumentStateZNU5 { get; set; }
            public Instrument InstrumentZNU5 { get; set; }
            public MonkeyCancelFrm FormInstance { get; set; }
        }

        internal static class Helpers
        {
            internal static Instrument CreateInstrumentByAlias(string alias, ITTInstrumentState instrumentState)
            {
                var orderbook = Substitute.For<ITTOrderBook>();
                instrumentState.Orderbook.Returns(orderbook);
                instrumentState.Alias.Returns(alias);
                return new Instrument(instrumentState);
            }

            internal static TestData CreateTestData()
            {
                var result = new TestData();
                result.InstrumentStateZBU5 = Substitute.For<ITTInstrumentState>();
                result.InstrumentZBU5 = CreateInstrumentByAlias("ZBU5", result.InstrumentStateZBU5);
                result.InstrumentStateZFU5 = Substitute.For<ITTInstrumentState>();
                result.InstrumentZFU5 = CreateInstrumentByAlias("ZFU5", result.InstrumentStateZFU5);
                result.InstrumentStateZNU5 = Substitute.For<ITTInstrumentState>();
                result.InstrumentZNU5 = CreateInstrumentByAlias("ZNU5", result.InstrumentStateZNU5);

                var form = new MonkeyCancelFrm();
                result.FormInstance = form;

                form.Show();

                form.InstrumentDefinitions = new InstrDef[]
                {
                    new InstrDef("CME", "ZB", "FUTURE", "Sep15", "ZBU5", "Basic"),
                    new InstrDef("CME", "ZF", "FUTURE", "Sep15", "ZFU5", "Basic"),
                    new InstrDef("CME", "ZN", "FUTURE", "Sep15", "ZNU5", "Basic"),
                };

                form.AddInstrument(result.InstrumentZBU5);
                form.AddInstrument(result.InstrumentZFU5);
                form.AddInstrument(result.InstrumentZNU5);

                return result;    
            }

        }

        [Fact]
        public void bidCxl_Update_Test()
        {
            var testData = Helpers.CreateTestData();

            var control = testData.FormInstance.Controls.Find("bidCxlCheckBoxZBU5", true).FirstOrDefault() as CheckBox;
            Assert.True(testData.InstrumentZBU5.EnableBidCxl);
            control.Checked = true;
            testData.InstrumentStateZBU5.ClearReceivedCalls();

            control.Checked = true;
            Assert.True(testData.InstrumentZBU5.EnableBidCxl);
            testData.InstrumentStateZBU5.DidNotReceive().InvokeAction(Arg.Any<Action>());
            testData.InstrumentStateZBU5.ClearReceivedCalls();

            control.Checked = false;
            Assert.False(testData.InstrumentZBU5.EnableBidCxl);
            testData.InstrumentStateZBU5.Received().InvokeAction(Arg.Any<Action>());
            testData.InstrumentStateZBU5.ClearReceivedCalls();

            control.Checked = false;
            Assert.False(testData.InstrumentZBU5.EnableBidCxl);
            testData.InstrumentStateZBU5.DidNotReceive().InvokeAction(Arg.Any<Action>());
            testData.InstrumentStateZBU5.ClearReceivedCalls();

            control.Checked = true;
            Assert.True(testData.InstrumentZBU5.EnableBidCxl);
            testData.InstrumentStateZBU5.Received().InvokeAction(Arg.Any<Action>());
            testData.InstrumentStateZBU5.ClearReceivedCalls();

            testData.FormInstance.Dispose();
        }



        [Fact]
        public void askCxl_Update_Test()
        {
            var testData = Helpers.CreateTestData();

            var control = testData.FormInstance.Controls.Find("askCxlCheckBoxZFU5", true).FirstOrDefault() as CheckBox;
            Assert.True(testData.InstrumentZFU5.EnableAskCxl);
            control.Checked = true;
            testData.InstrumentStateZFU5.ClearReceivedCalls();

            control.Checked = true;
            Assert.True(testData.InstrumentZFU5.EnableAskCxl);
            testData.InstrumentStateZFU5.DidNotReceive().InvokeAction(Arg.Any<Action>());
            testData.InstrumentStateZFU5.ClearReceivedCalls();

            control.Checked = false;
            Assert.False(testData.InstrumentZFU5.EnableAskCxl);
            testData.InstrumentStateZFU5.Received().InvokeAction(Arg.Any<Action>());
            testData.InstrumentStateZFU5.ClearReceivedCalls();

            control.Checked = false;
            Assert.False(testData.InstrumentZFU5.EnableAskCxl);
            testData.InstrumentStateZFU5.DidNotReceive().InvokeAction(Arg.Any<Action>());
            testData.InstrumentStateZFU5.ClearReceivedCalls();

            control.Checked = true;
            Assert.True(testData.InstrumentZFU5.EnableAskCxl);
            testData.InstrumentStateZFU5.Received().InvokeAction(Arg.Any<Action>());
            testData.InstrumentStateZFU5.ClearReceivedCalls();

            testData.FormInstance.Dispose();
        }


        [Fact]
        public void minImpliedCxl_Update_Test()
        {
            var testData = Helpers.CreateTestData();

            var control = testData.FormInstance.Controls.Find("minImpliedCxlCheckBoxZNU5", true).FirstOrDefault() as CheckBox;
            Assert.False(testData.InstrumentZNU5.EnableImpliedCxl);
            control.Checked = true;
            testData.InstrumentStateZNU5.ClearReceivedCalls();

            control.Checked = true;
            Assert.True(testData.InstrumentZNU5.EnableImpliedCxl);
            testData.InstrumentStateZNU5.DidNotReceive().InvokeAction(Arg.Any<Action>());
            testData.InstrumentStateZNU5.ClearReceivedCalls();

            control.Checked = false;
            Assert.False(testData.InstrumentZNU5.EnableImpliedCxl);
            testData.InstrumentStateZNU5.Received().InvokeAction(Arg.Any<Action>());
            testData.InstrumentStateZNU5.ClearReceivedCalls();

            control.Checked = false;
            Assert.False(testData.InstrumentZNU5.EnableImpliedCxl);
            testData.InstrumentStateZNU5.DidNotReceive().InvokeAction(Arg.Any<Action>());
            testData.InstrumentStateZNU5.ClearReceivedCalls();

            control.Checked = true;
            Assert.True(testData.InstrumentZNU5.EnableImpliedCxl);
            testData.InstrumentStateZNU5.Received().InvokeAction(Arg.Any<Action>());
            testData.InstrumentStateZNU5.ClearReceivedCalls();

            testData.FormInstance.Dispose();
        }


        void SetCheckTextBoxValue(Form form, ITTInstrumentState instrumentState, TextBox control, string textValue1, bool shouldCallAction)
        {
            control.Text = textValue1;

            var button = form.Controls.Find("buttonSaveEdges", true).FirstOrDefault() as Button;
            button.PerformClick();

            if (shouldCallAction)
            {
                instrumentState.Received().InvokeAction(Arg.Any<Action>());
            }
            else
            {
                instrumentState.DidNotReceive().InvokeAction(Arg.Any<Action>());
            }

            instrumentState.ClearReceivedCalls();
        }


        [Fact]
        public void BidAskEdge_Update_Test()
        {
            var testData = Helpers.CreateTestData();

            var bidEdgeZBU5 = testData.FormInstance.Controls.Find("bidCxlEdgeTextboxZBU5", true).FirstOrDefault() as TextBox;
            var bidEdgeZFU5 = testData.FormInstance.Controls.Find("bidCxlEdgeTextboxZFU5", true).FirstOrDefault() as TextBox;
            var bidEdgeZNU5 = testData.FormInstance.Controls.Find("bidCxlEdgeTextboxZNU5", true).FirstOrDefault() as TextBox;

            var askEdgeZBU5 = testData.FormInstance.Controls.Find("askCxlEdgeTextboxZBU5", true).FirstOrDefault() as TextBox;
            var askEdgeZFU5 = testData.FormInstance.Controls.Find("askCxlEdgeTextboxZFU5", true).FirstOrDefault() as TextBox;
            var askEdgeZNU5 = testData.FormInstance.Controls.Find("askCxlEdgeTextboxZNU5", true).FirstOrDefault() as TextBox;

            var minImpliedZBU5 = testData.FormInstance.Controls.Find("minImpliedQtyTextboxZBU5", true).FirstOrDefault() as TextBox;
            var minImpliedZFU5 = testData.FormInstance.Controls.Find("minImpliedQtyTextboxZFU5", true).FirstOrDefault() as TextBox;
            var minImpliedZNU5 = testData.FormInstance.Controls.Find("minImpliedQtyTextboxZNU5", true).FirstOrDefault() as TextBox;

            SetCheckTextBoxValue(testData.FormInstance, testData.InstrumentStateZBU5, minImpliedZBU5, "101", true);
            Assert.Equal(testData.InstrumentZBU5.MinImpliedQty, 101);

            SetCheckTextBoxValue(testData.FormInstance, testData.InstrumentStateZBU5, minImpliedZBU5, "101", false);
            Assert.Equal(testData.InstrumentZBU5.MinImpliedQty, 101);

            SetCheckTextBoxValue(testData.FormInstance, testData.InstrumentStateZBU5, minImpliedZBU5, "0", true);
            Assert.Equal(testData.InstrumentZBU5.MinImpliedQty, 0);

            SetCheckTextBoxValue(testData.FormInstance, testData.InstrumentStateZBU5, minImpliedZBU5, "2", true);
            Assert.Equal(testData.InstrumentZBU5.MinImpliedQty, 2);


            SetCheckTextBoxValue(testData.FormInstance, testData.InstrumentStateZFU5, bidEdgeZFU5, "0.015", true);
            Assert.Equal(testData.InstrumentZFU5.BidCxlEdge, 0.015, 5);

            SetCheckTextBoxValue(testData.FormInstance, testData.InstrumentStateZFU5, bidEdgeZFU5, "0.015", false);
            Assert.Equal(testData.InstrumentZFU5.BidCxlEdge, 0.015, 5);

            SetCheckTextBoxValue(testData.FormInstance, testData.InstrumentStateZFU5, bidEdgeZFU5, "0", true);
            Assert.Equal(testData.InstrumentZFU5.BidCxlEdge, 0.0, 5);

            SetCheckTextBoxValue(testData.FormInstance, testData.InstrumentStateZFU5, bidEdgeZFU5, "0.334", true);
            Assert.Equal(testData.InstrumentZFU5.BidCxlEdge, 0.334, 5);


            SetCheckTextBoxValue(testData.FormInstance, testData.InstrumentStateZNU5, askEdgeZNU5, "0.031", true);
            Assert.Equal(testData.InstrumentZNU5.AskCxlEdge, 0.031, 5);

            SetCheckTextBoxValue(testData.FormInstance, testData.InstrumentStateZNU5, askEdgeZNU5, "0.031", false);
            Assert.Equal(testData.InstrumentZNU5.AskCxlEdge, 0.031, 5);

            SetCheckTextBoxValue(testData.FormInstance, testData.InstrumentStateZNU5, askEdgeZNU5, "0", true);
            Assert.Equal(testData.InstrumentZNU5.AskCxlEdge, 0.0, 5);

            SetCheckTextBoxValue(testData.FormInstance, testData.InstrumentStateZNU5, askEdgeZNU5, "122.01", true);
            Assert.Equal(testData.InstrumentZNU5.AskCxlEdge, 122.01, 5);


            testData.FormInstance.Dispose();
        }

    }
}
