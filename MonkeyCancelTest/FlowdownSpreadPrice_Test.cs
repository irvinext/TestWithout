using KT_TTAPI;
using MonkeyCancel;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MonkeyCancelTest
{
    public class FlowdownSpreadImpliedPrice_Test
    {
        InstrumentSpreadFlowdown BuildCalendar(float vwmptA, float vwmptB, float spreadBid, float spreadAsk)
        {
            var legA = Substitute.For<IInstrument>();
            legA.Vwmpt.Returns(vwmptA);
            legA.SettlePx.Returns(1000f);

            var legB = Substitute.For<IInstrument>();
            legB.Vwmpt.Returns(vwmptB);
            legB.SettlePx.Returns(1000f);

            var spreadInstrumentState = Substitute.For<ITTInstrumentState>();
            var spreadOrderbook = Substitute.For<ITTOrderBook>();
            spreadInstrumentState.Exchange.Returns("CME");
            spreadInstrumentState.SpreadType.Returns(TTSpreadType.Calendar);
            spreadOrderbook.TickSize.Returns(1);
            spreadInstrumentState.Orderbook.Returns(spreadOrderbook);
            spreadInstrumentState.Alias.Returns("+ZNH6-ZNM6");
            var spread = Substitute.ForPartsOf<InstrumentSpreadFlowdown>(spreadInstrumentState);
            spread.Legs = new SpreadLeg[2]
            {
                new SpreadLeg(legA, 1) { Weight = 1 },
                new SpreadLeg(legB, -1) { Weight = -1 }
            };

            legA.OnPriceChanged += spread.OnLinkedInstrumentChanged;
            legB.OnPriceChanged += spread.OnLinkedInstrumentChanged;

            Helpers.PassTobData(spreadInstrumentState, spreadBid, spreadAsk, 100f, 100f, float.NaN, float.NaN, 0f, 0f);
            spreadInstrumentState.OnInstrumentStateChange += Raise.Event();

            var dummy = legA.DidNotReceive().SettlePx;
            dummy = legB.DidNotReceive().SettlePx;

            return spread;
        }

        [Fact]
        // Spread : +ZNH6-ZNM6
        // LegA.vwmpt = 10
        // LegB.vwmpt = 12
        // Spread.vwmpt = 4
        // Spread.impliedVwmpt = LegA.theo  + LegB.theo * (LegB.weight / LegA.weight), where LegA is the first leg.
        // Spread.impliedVwmpt = 10 + 12 * (-1 / 1) = -2
        // Spread.adjustedVwmpt = (Spread.vwmpt + Spread.impliedVwmpt) / 2 = (4 + -2) / 2 = 1
        public void FlowdownSpreadForCalendar_NormalPrices_Test()
        {
            var spread = BuildCalendar(vwmptA: 10, vwmptB: 12, spreadBid: 3, spreadAsk: 5);

            Assert.Equal(spread.IsNetChangePx, false);
            Assert.Equal(4f, spread.Vwmpt, 7);
            Assert.Equal(1f, spread.AdjustedVwmpt, 7);
        }

        [Fact]
        public void FlowdownSpreadForCalendar_NoPricesForA_Test()
        {
            var spread = BuildCalendar(vwmptA: float.NaN, vwmptB: 12, spreadBid: 3, spreadAsk: 5);

            Assert.Equal(spread.IsNetChangePx, false);
            Assert.Equal(4f, spread.Vwmpt, 7);
            Assert.Equal(4f, spread.AdjustedVwmpt, 7);
        }

        [Fact]
        public void FlowdownSpreadForCalendar_NoPricesForB_Test()
        {
            var spread = BuildCalendar(vwmptA: 10, vwmptB: float.NaN, spreadBid: 3, spreadAsk: 5);

            Assert.Equal(spread.IsNetChangePx, false);
            Assert.Equal(4f, spread.Vwmpt, 7);
            Assert.Equal(4f, spread.AdjustedVwmpt, 7);
        }


        [Fact]
        public void FlowdownSpreadForCalendar_NoPricesForSpread_Test()
        {
            var spread = BuildCalendar(vwmptA: 10, vwmptB: 12, spreadBid: float.NaN, spreadAsk: float.NaN);

            Assert.Equal(spread.IsNetChangePx, false);
            Assert.Equal(float.NaN, spread.Vwmpt, 7);
            Assert.Equal(float.NaN, spread.AdjustedVwmpt, 7);
        }


        [Fact]
        // ICS Spread : 3xZF-2xZN, Spread.Theo = -0.01875
        // LegA.vwmpt = 120.5578125
        // LegA.settle = 120.65625
        // LegB.vwmpt = 128.815625
        // LegB.settle = 128.9375
        // Spread.impliedVwmpt = (LegA.theo - LegA.settle) + (LegB.theo - LegA.settle) * (LegB.weight / LegA.weight), where LegA is the first leg.
        // Spread.impliedVwmpt = (120.5578125 - 120.65625) + (128.815625  - 128.9375) * (-2 / 3) = -0.0171875
        //
        // Spread.adjustedVwmpt = (Spread.vwmpt + Spread.impliedVwmpt) / 2 = (-0.0171875 + -1.05) / 2 = -0.53359375
        public void FlowdownSpreadForIcsSpread_NormalPrices_Test()
        {
            var legA = Substitute.For<IInstrument>();
            legA.Vwmpt.Returns(120.5578125f);
            legA.SettlePx.Returns(120.65625f);

            var legB = Substitute.For<IInstrument>();
            legB.Vwmpt.Returns(128.815625f);
            legB.SettlePx.Returns(128.9375f);

            var spreadInstrumentState = Substitute.For<ITTInstrumentState>();
            spreadInstrumentState.Exchange.Returns("CME");
            spreadInstrumentState.SpreadType.Returns(TTSpreadType.Ics);
            var spreadOrderbook = Substitute.For<ITTOrderBook>();
            spreadOrderbook.TickSize.Returns(1);
            spreadInstrumentState.Orderbook.Returns(spreadOrderbook);
            spreadInstrumentState.Alias.Returns("3xZF-2xZN");
            var spread = Substitute.ForPartsOf<InstrumentSpreadFlowdown>(spreadInstrumentState);
            spread.Legs = new SpreadLeg[2]
            {
                new SpreadLeg(legA, 3) { Weight = 3 },
                new SpreadLeg(legB, -2) { Weight = -2 }
            };

            legA.OnPriceChanged += spread.OnLinkedInstrumentChanged;
            legB.OnPriceChanged += spread.OnLinkedInstrumentChanged;

            Helpers.PassTobData(spreadInstrumentState, -1f, -1.1f, 100f, 100f, float.NaN, float.NaN, 0f, 0f);
            spreadInstrumentState.OnInstrumentStateChange += Raise.Event();

            Assert.Equal(spread.IsNetChangePx, true);
            Assert.Equal(-1.05f, spread.Vwmpt, 7);
            Assert.Equal(-0.53359f, spread.AdjustedVwmpt, 5);
        }

    }
}
