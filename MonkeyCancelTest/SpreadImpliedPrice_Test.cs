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
    public static class Helpers
    {
        public static void PassTobData(ITTInstrumentState instrument, float bidPx, float askPx, float bidQty, float askQty, float impliedBidPx, float impliedAskPx, float impliedBidQty, float impliedAskQty)
        {
            float bidPxD, askPxD, bidQtyD, askQtyD, impliedBidPxD, impliedAskPxD, impliedBidQtyD, impliedAskQtyD;

            instrument
                .When(x => x.getTobData(out bidPxD, out askPxD, out bidQtyD, out askQtyD,
                            out impliedBidPxD, out impliedAskPxD, out impliedBidQtyD, out impliedAskQtyD))
                .Do(x =>
                {
                    x[0] = bidPx;
                    x[1] = askPx;
                    x[2] = bidQty;        
                    x[3] = askQty;        
                    x[4] = impliedBidPx;  
                    x[5] = impliedAskPx;  
                    x[6] = impliedBidQty; 
                    x[7] = impliedAskQty; 
                });
        }
    };

    public class SpreadImpliedPrice_Test3
    {
        [Fact]
        // 1.25 = (X*1 + 99*-1)
        // X = 100.25, so A_error = Vwmpt(A) - 100.25 = 100 - 100.25 = -0.25
        // Check of X :
        //   1.25 = (100.25 * 1 + 99 * -1)
        //   1.25 = 1.25
        public void SpreadImpliedForCalendarPositiveLeg_Test()
        {
            var legAInstrumentState = Substitute.For<ITTInstrumentState>();
            var legAOrderbook = Substitute.For<ITTOrderBook>();
            legAOrderbook.TickSize.Returns(1);
            legAInstrumentState.Orderbook.Returns(legAOrderbook);
            legAInstrumentState.Alias.Returns("ZNZ5");
            var legA = Substitute.ForPartsOf<InstrumentFutureSpreadImplied>(legAInstrumentState, null, new ArithmeticMeanAggregation());

            var legB = Substitute.For<IInstrument>();
            legB.Vwmpt.Returns(99f);

            var spread = Substitute.For<ISpread>();
            spread.IsNetChangePx.Returns(false);
            spread.Vwmpt.Returns(1.25f);
            spread.Legs.Returns(new SpreadLeg[2]
            {
                new SpreadLeg(legA, 1) { Weight = 1 },
                new SpreadLeg(legB, -1) { Weight = -1 }
            });

            legA.AddSpread(spread);
            Helpers.PassTobData(legAInstrumentState, 90f, 110f, 100f, 100f, float.NaN, float.NaN, 0f, 0f);

            legAInstrumentState.OnInstrumentStateChange += Raise.Event();

            Assert.Equal(legA.Vwmpt, 100f, 7);
            Assert.Equal(legA.AdjustedVwmpt, 100.25f, 7);
        }


        [Fact]
        //1.25 = (100*1 + X*-1); X = 98.75; B_error = 99-98.75 = 0.25
        // Check of X :
        //   1.25 = (100 * 1 + 98.75 * -1)
        //   1.25 = 1.25
        public void SpreadImpliedForCalendarNegativeLeg_Test()
        {
            var legBInstrumentState = Substitute.For<ITTInstrumentState>();
            var legBOrderbook = Substitute.For<ITTOrderBook>();
            legBOrderbook.TickSize.Returns(1);
            legBInstrumentState.Orderbook.Returns(legBOrderbook);
            legBInstrumentState.Alias.Returns("ZNH6");
            var legB = Substitute.ForPartsOf<InstrumentFutureSpreadImplied>(legBInstrumentState, null, new ArithmeticMeanAggregation());

            var legA = Substitute.For<IInstrument>();
            legA.Vwmpt.Returns(100f);

            var spread = Substitute.For<ISpread>();
            spread.IsNetChangePx.Returns(false);
            spread.Vwmpt.Returns(1.25f);
            spread.Legs.Returns(new SpreadLeg[2]
            {
                new SpreadLeg(legA, 1) { Weight = 1 },
                new SpreadLeg(legB, -1) { Weight = -1 }
            });

            legB.AddSpread(spread);
            Helpers.PassTobData(legBInstrumentState, 98f, 100f, 100f, 100f, float.NaN, float.NaN, 0f, 0f);

            legBInstrumentState.OnInstrumentStateChange += Raise.Event();

            Assert.Equal(legB.Vwmpt, 99, 7);
            Assert.Equal(legB.AdjustedVwmpt, 98.75f, 7);
        }


        [Fact]
        // ICS Spread : 3xZF-2xZN, Spread.Theo = -0.01875
        // ZF.Theo = 120.5578125, ZF.Settl = 120.65625
        // ZN.Theo = 128.815625, ZN.Settl = 128.9375
        // ZF.error = (ZF.theo - ZF.settle) - (Spread.theo - (ZN.theo - ZN.settle) * (ZN.ratio / ZF.ratio))
        // ZF.error = (120.5578125 - 120.65625) - (-0.01875 - (128.815625 - 128.9375) * (-2 / 3))
        // ZF.error = (-0.0984375) - (-0.01875 - (-0.121875) * (-2 / 3))
        // ZF.error = (-0.0984375) - (-0.01875 - 0.08125)
        // ZF.error = (-0.0984375) - (-0.1)
        // ZF.error = -0.0984375 + 0.1 = 0.0015625
        // ZF.AdjustedTheo = ZF.Theo - ZF.error = 120.5578125 - 0.0015625 = 120.55625
        // Checking:
        // LegB.Theo - LegB.Settl = (Spread.Theo - (LegA.AdjustedTheo - LegA.Settl)) * (3/-2)
        // 128.815625 - 128.9375 = (-0.01875 - (120.55625 - 120.65625)) * (3/-2)
        // -0.1219 = -0.1219
        public void SpreadImpliedForICSPositiveLeg_Test()
        {
            var legAInstrumentState = Substitute.For<ITTInstrumentState>();
            var legAOrderbook = Substitute.For<ITTOrderBook>();
            legAOrderbook.TickSize.Returns(1);
            legAInstrumentState.Orderbook.Returns(legAOrderbook);
            legAInstrumentState.Alias.Returns("ZFZ5");
            var legA = Substitute.ForPartsOf<InstrumentFutureSpreadImplied>(legAInstrumentState, null, new ArithmeticMeanAggregation());
            legA.SettlePx.Returns(120.65625f);

            var legB = Substitute.For<IInstrument>();
            legB.Vwmpt.Returns(128.815625f);
            legB.SettlePx.Returns(128.9375f);

            var spread = Substitute.For<ISpread>();
            spread.IsNetChangePx.Returns(true);
            spread.Vwmpt.Returns(-0.01875f);
            spread.Legs.Returns(new SpreadLeg[2]
            {
                new SpreadLeg(legA, 3) { Weight = 3 },
                new SpreadLeg(legB, -2) { Weight = -2 }
            });

            legA.AddSpread(spread);
            Helpers.PassTobData(legAInstrumentState, 120.4578125f, 120.6578125f, 100f, 100f, float.NaN, float.NaN, 0f, 0f);

            legAInstrumentState.OnInstrumentStateChange += Raise.Event();

            Assert.Equal(legA.Vwmpt, 120.5578125f, 7);
            Assert.Equal(legA.AdjustedVwmpt, 120.55625f, 7);
        }



        [Fact]
        // ICS Spread : 3xZF-2xZN, Spread.Theo = -0.01875
        // ZF.Theo = 120.5578125, ZF.Settl = 120.65625
        // ZN.Theo = 128.815625, ZN.Settl = 128.9375
        // ZN.error = (ZN.theo - ZN.settle) - (Spread.theo - (ZF.theo - ZF.settle))_ * (ZF.weight / ZN.weight)
        // ZN.error = (128.815625 - 128.9375) - (-0.01875 - (120.5578125 - 120.65625)) * (3 / -2)
        // ZN.error = (128.815625 - 128.9375) - (-0.01875 - 120.5578125 + 120.65625)) * (3 / -2)
        // ZN.error = -0.121875 + 0.11953125 = -0.00234375
        // ZN.AdjustedTheo = ZN.Theo - ZN.error = 128.815625 + 0.00234375 = 128.81796875
        // Checking:
        // LegA.Theo - LegA.Settl = Spread.Theo - (LegB.AdjustedTheo - LegB.Settl) * legB.Ratio / legA.Ratio
        // 120.5578125 - 120.65625 = -0.01875 - (128.81796875 - 128.9375) * -2 / 3
        // 120.5578125 - 120.65625 = -0.01875 - (128.81796875 - 128.9375) * -2 / 3
        // -0.0984 = -0.0984
        public void SpreadImpliedForICSNegativeLeg_Test()
        {
            var legBInstrumentState = Substitute.For<ITTInstrumentState>();
            var legBOrderbook = Substitute.For<ITTOrderBook>();
            legBOrderbook.TickSize.Returns(1);
            legBInstrumentState.Orderbook.Returns(legBOrderbook);
            legBInstrumentState.Alias.Returns("ZNZ5");
            var legB = Substitute.ForPartsOf<InstrumentFutureSpreadImplied>(legBInstrumentState, null, new ArithmeticMeanAggregation());
            legB.SettlePx.Returns(128.9375f);

            var legA = Substitute.For<IInstrument>();
            legA.Vwmpt.Returns(120.5578125f);
            legA.SettlePx.Returns(120.65625f);

            var spread = Substitute.For<ISpread>();
            spread.IsNetChangePx.Returns(true);
            spread.Vwmpt.Returns(-0.01875f);
            spread.Legs.Returns(new SpreadLeg[2]
            {
                new SpreadLeg(legA, 3) { Weight = 3 },
                new SpreadLeg(legB, -2) { Weight = -2 }
            });

            legB.AddSpread(spread);
            Helpers.PassTobData(legBInstrumentState, 128.715625f, 128.915625f, 100f, 100f, float.NaN, float.NaN, 0f, 0f);

            legBInstrumentState.OnInstrumentStateChange += Raise.Event();

            Assert.Equal(legB.Vwmpt, 128.815625f, 4);
            Assert.Equal(legB.AdjustedVwmpt, 128.81796875f, 4);
        }


        [Fact]
        // Spread 1 : +A-B1, Theo = 1.25
        // Spread 2 : +A-B2, Theo = 2.25
        // TheoA = 100
        // TheoB1 = 99
        // TheoB2 = 99
        // A.err = A.Theo -  (Spread.Theo - SUM(leg(i).Theo * leg(i).ratio)) / A.ratio
        // A_err_spread1 = 100 - (1.25 - (99 * -1)) / 1 = -0.25
        // A_err_spread2 = 100 - (2.25 - (99 * -1)) / 1 = -1.25
        // A_err_mean = (-0.25 - 1.25) / 2 = -0.75
        // A.AdjustedTheo = TheoA - A_err_mean = 100 - (-0.75) = 100.75
        // Check of A.AdjustedTheo:
        // 1.25 = (A * 1 + 99 * -1)  and  2.25 = (A * 1 + 99 * -1)
        // 1.25 + 2.25 = (A * 1 + 99 * -1) + (A * 1 + 99 * -1)
        // A = (1.25 + 2.25 + 2 * 99) / 2 = 100.75
        public void SpreadImpliedForCalendarPositiveLeg_TwoSpreads_Test()
        {
            var legAInstrumentState = Substitute.For<ITTInstrumentState>();
            var legAOrderbook = Substitute.For<ITTOrderBook>();
            legAOrderbook.TickSize.Returns(1);
            legAInstrumentState.Orderbook.Returns(legAOrderbook);
            legAInstrumentState.Alias.Returns("ZNZ5");
            var legA = Substitute.ForPartsOf<InstrumentFutureSpreadImplied>(legAInstrumentState, null, new ArithmeticMeanAggregation());

            var legB = Substitute.For<IInstrument>();
            legB.Vwmpt.Returns(99f);

            var legB2 = Substitute.For<IInstrument>();
            legB2.Vwmpt.Returns(99f);

            var spread1 = Substitute.For<ISpread>();
            spread1.IsNetChangePx.Returns(false);
            spread1.Vwmpt.Returns(1.25f);
            spread1.Legs.Returns(new SpreadLeg[2]
            {
                new SpreadLeg(legA, 1) { Weight = 1 },
                new SpreadLeg(legB, -1) { Weight = -1 }
            });

            var spread2 = Substitute.For<ISpread>();
            spread2.IsNetChangePx.Returns(false);
            spread2.Vwmpt.Returns(2.25f);
            spread2.Legs.Returns(new SpreadLeg[2]
            {
                new SpreadLeg(legA, 1) { Weight = 1 },
                new SpreadLeg(legB2, -1) { Weight = -1 }
            });

            legA.AddSpread(spread1);
            legA.AddSpread(spread2);

            Helpers.PassTobData(legAInstrumentState, 90f, 110f, 100f, 100f, float.NaN, float.NaN, 0f, 0f);

            legAInstrumentState.OnInstrumentStateChange += Raise.Event();

            Assert.Equal(legA.Vwmpt, 100f, 7);
            Assert.Equal(legA.AdjustedVwmpt, 100.75f, 7);
        }


        [Fact]
        // ICS Spread : 3xZF-10xZT, Spread.Theo = -0.01875, The ZT Point Value = ZF Point Value * 2
        // ZF.Theo = 120.5578125, ZF.Settl = 120.65625
        // ZT.Theo = 128.815625, ZT.Settl = 128.9375
        // ZT.error = (ZT.theo - ZT.settle) - (Spread.theo - (ZF.theo - ZF.settle))_ * (ZF.weight / ZT.weight)
        // ZT.error = (128.815625 - 128.9375) - (-0.01875 - (120.5578125 - 120.65625)) * (3 / (-10 * 2))
        // ZT.error = -0.1099
        // ZT.AdjustedTheo = ZT.Theo - ZT.error = 128.815625 + 0.1099 = 128.9255
        public void SpreadImpliedForICSWithDifferentPointValuesNegativeLeg_Test()
        {
            var legBInstrumentState = Substitute.For<ITTInstrumentState>();
            var legBOrderbook = Substitute.For<ITTOrderBook>();
            legBOrderbook.TickSize.Returns(1);
            legBInstrumentState.Orderbook.Returns(legBOrderbook);
            legBInstrumentState.Alias.Returns("ZTZ5");
            var legB = Substitute.ForPartsOf<InstrumentFutureSpreadImplied>(legBInstrumentState, null, new ArithmeticMeanAggregation());
            legB.SettlePx.Returns(128.9375f);

            var legA = Substitute.For<IInstrument>();
            legA.Vwmpt.Returns(120.5578125f);
            legA.SettlePx.Returns(120.65625f);

            var spread = Substitute.For<ISpread>();
            spread.IsNetChangePx.Returns(true);
            spread.Vwmpt.Returns(-0.01875f);
            spread.Legs.Returns(new SpreadLeg[2]
            {
                new SpreadLeg(legA, 3) { Weight = 3 * 1000 / 1000 },
                new SpreadLeg(legB, -10) { Weight = -10 * 2000 / 1000 }
            });

            legB.AddSpread(spread);
            Helpers.PassTobData(legBInstrumentState, 128.715625f, 128.915625f, 100f, 100f, float.NaN, float.NaN, 0f, 0f);

            legBInstrumentState.OnInstrumentStateChange += Raise.Event();

            Assert.Equal(legB.Vwmpt, 128.815625f, 4);
            Assert.Equal(legB.AdjustedVwmpt, 128.9256f, 4);
        }


        [Fact]
        // ICS Spread : 3xZF-10xZT, Spread.Theo = -0.01875, The ZT Point Value = ZF Point Value * 2
        // ZF.Theo = 120.5578125, ZF.Settl = 120.65625
        // ZN.Theo = 128.815625, ZN.Settl = 128.9375
        // ZF.error = (ZF.theo - ZF.settle) - (Spread.theo - (ZT.theo - ZT.settle) * (ZT.weight / ZF.weight))
        // ZF.error = (120.5578125 - 120.65625) - (-0.01875 - (128.815625 - 128.9375) * (-10 * 2 / 3))
        // ZF.error = 0.7328
        // ZF.AdjustedTheo = ZF.Theo - ZF.error = 120.5578125 - 0.7328 = 119.8250
        public void SpreadImpliedForICSWithDifferentPointValuesPositiveLeg_Test()
        {
            var legAInstrumentState = Substitute.For<ITTInstrumentState>();
            var legAOrderbook = Substitute.For<ITTOrderBook>();
            legAOrderbook.TickSize.Returns(1);
            legAInstrumentState.Orderbook.Returns(legAOrderbook);
            legAInstrumentState.Alias.Returns("ZFZ5");
            var legA = Substitute.ForPartsOf<InstrumentFutureSpreadImplied>(legAInstrumentState, null, new ArithmeticMeanAggregation());
            legA.SettlePx.Returns(120.65625f);

            var legB = Substitute.For<IInstrument>();
            legB.Vwmpt.Returns(128.815625f);
            legB.SettlePx.Returns(128.9375f);

            var spread = Substitute.For<ISpread>();
            spread.IsNetChangePx.Returns(true);
            spread.Vwmpt.Returns(-0.01875f);
            spread.Legs.Returns(new SpreadLeg[2]
            {
                new SpreadLeg(legA, 3) { Weight = 3 * 1000 / 1000 },
                new SpreadLeg(legB, -10) { Weight = -10 * 2000 / 1000 }
            });

            legA.AddSpread(spread);

            Helpers.PassTobData(legAInstrumentState, 120.4578125f, 120.6578125f, 100f, 100f, float.NaN, float.NaN, 0f, 0f);
            legAInstrumentState.OnInstrumentStateChange += Raise.Event();

            Assert.Equal(legA.Vwmpt, 120.5578125f, 4);
            Assert.Equal(legA.AdjustedVwmpt, 119.8250f, 4);
        }



        [Fact]
        // Spread 1 : +A-B, Theo = -1.25
        // Spread 2 : +B-C, Theo = 1.5
        // TheoA = 100
        // TheoB = 110
        // TheoC = 120
        // B.err = B.Theo -  (Spread.Theo - SUM(leg(i).Theo * leg(i).ratio)) / B.ratio
        // B_err_spread1 = 110 - (-1.25 - (100 * 1)) / -1 = 8.75
        // B_err_spread2 = 110 - (1.5 - (120 * -1)) / 1 = -11.5
        // B_err_mean = (8.75 -11.5) / 2 = -1.375
        // B.AdjustedTheo = TheoB + B_err_mean = 110 - (-1.375) = 111.375
        // Check of B.AdjustedTheo:
        // -1,25 = (100 * 1 + B * (-1))  and  1.5 = (B * 1 + 120 * (-1))
        // B = (1.5 + 1.25 + 100 + 120) / 2 = 111.375
        public void SpreadImpliedForCalendarPositiveAndNegativeLeg_TwoSpreads_Test()
        {
            var legBInstrumentState = Substitute.For<ITTInstrumentState>();
            var legBOrderbook = Substitute.For<ITTOrderBook>();
            legBOrderbook.TickSize.Returns(1);
            legBInstrumentState.Orderbook.Returns(legBOrderbook);
            legBInstrumentState.Alias.Returns("ZNZ5");
            var legB = Substitute.ForPartsOf<InstrumentFutureSpreadImplied>(legBInstrumentState, null, new ArithmeticMeanAggregation());

            var legA = Substitute.For<IInstrument>();
            legA.Vwmpt.Returns(100f);

            var legC = Substitute.For<IInstrument>();
            legC.Vwmpt.Returns(120f);

            var spread1 = Substitute.For<ISpread>();
            spread1.IsNetChangePx.Returns(false);
            spread1.Vwmpt.Returns(-1.25f);
            spread1.Legs.Returns(new SpreadLeg[2]
            {
                new SpreadLeg(legA, 1) { Weight = 1 },
                new SpreadLeg(legB, -1) { Weight = -1 }
            });

            var spread2 = Substitute.For<ISpread>();
            spread2.IsNetChangePx.Returns(false);
            spread2.Vwmpt.Returns(1.5f);
            spread2.Legs.Returns(new SpreadLeg[2]
            {
                new SpreadLeg(legB, 1) { Weight = 1 },
                new SpreadLeg(legC, -1) { Weight = -1 }
            });

            legB.AddSpread(spread1);
            legB.AddSpread(spread2);

            Helpers.PassTobData(legBInstrumentState, 100f, 120f, 100f, 100f, float.NaN, float.NaN, 0f, 0f);

            legBInstrumentState.OnInstrumentStateChange += Raise.Event();

            Assert.Equal(legB.Vwmpt, 110f, 7);
            Assert.Equal(legB.AdjustedVwmpt, 111.375f, 7);
        }

    }
}
