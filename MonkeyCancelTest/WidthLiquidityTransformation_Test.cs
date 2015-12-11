using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using NSubstitute;
using MonkeyCancel;

namespace MonkeyCancelTest
{
    public class WidthLiquidityTransformation_Test
    {
        private float RunTransform(float legBidPx, float legAskPx, float spreadBidPx, float spreadAskPx, float sourceError)
        {
            float tickSize = 1f / 64f;

            var transformation = new WidthLiquidityTransformation();

            IInstrument myLeg = Substitute.For<IInstrument>();

            IInstrument opposizeLeg = Substitute.For<IInstrument>();
            opposizeLeg.TickSize.Returns(tickSize);
            opposizeLeg.BidPx.Returns(legBidPx);
            opposizeLeg.AskPx.Returns(legAskPx);

            ISpread spread = Substitute.For<ISpread>();
            spread.TickSize.Returns(tickSize);
            spread.BidPx.Returns(spreadBidPx);
            spread.AskPx.Returns(spreadAskPx);

            spread.Legs.Returns(new SpreadLeg[2] { new SpreadLeg(myLeg, 9), new SpreadLeg(opposizeLeg, -5) });

            return transformation.Transform(sourceError, spread, myLeg);
        }

        [Fact]
        public void Leg1TickSpread2Ticks_Test()
        {
            float error =  100f * 1f / 64f;
            float tickSize = 1f / 64f;

            float result = RunTransform(legBidPx: 100f, legAskPx: 100f + tickSize, spreadBidPx: 1f, spreadAskPx: 1f + 2 * tickSize,  sourceError: error);
            Assert.Equal(result, error * 0.5f, 5);
        }

        [Fact]
        public void Leg2TicksSpread1Tick_Test()
        {
            float error = -20 / 64f;
            float tickSize = 1f / 64f;

            float result = RunTransform(legBidPx: 100f, legAskPx: 100f + 2 * tickSize, spreadBidPx: 1f, spreadAskPx: 1f + 1 * tickSize,  sourceError: error);
            Assert.Equal(result, error * 0.5f, 5);
        }


        [Fact]
        public void Leg1TickSpread1Tick_Test()
        {
            float error = -2002.2123f;
            float tickSize = 1f / 64f;

            float result = RunTransform(legBidPx: 100f, legAskPx: 100f + 1 * tickSize, spreadBidPx: 1f, spreadAskPx: 1f + 1 * tickSize, sourceError: error);
            Assert.Equal(result, error, 5);
        }


        [Fact]
        public void Leg2TickSpread2Ticsk_Test()
        {
            float error = 2002.2123f;
            float tickSize = 1f / 64f;

            float result = RunTransform(legBidPx: 100f, legAskPx: 100f + 2 * tickSize, spreadBidPx: 1f, spreadAskPx: 1f + 2 * tickSize, sourceError: error);
            Assert.Equal(result, error * 0.5, 5);
        }

        [Fact]
        public void Leg1TickSpread3Ticks_Test()
        {
            float error = 2002.2123f;
            float tickSize = 1f / 64f;

            float result = RunTransform(legBidPx: 100f, legAskPx: 100f + 1 * tickSize, spreadBidPx: 1f, spreadAskPx: 1f + 3 * tickSize, sourceError: error);
            Assert.Equal(result, 0, 5);
        }

        [Fact]
        public void Leg3TicksSpread1Tick_Test()
        {
            float error = 1.11111f;
            float tickSize = 1f / 64f;

            float result = RunTransform(legBidPx: 100f, legAskPx: 100f + 3 * tickSize, spreadBidPx: 1f, spreadAskPx: 1f + 1 * tickSize, sourceError: error);
            Assert.Equal(result, 0, 5);
        }

        [Fact]
        public void LegAndSpread3Ticks_Test()
        {
            float error = 1.11111f;
            float tickSize = 1f / 64f;

            float result = RunTransform(legBidPx: 100f, legAskPx: 100f + 3 * tickSize, spreadBidPx: 1f, spreadAskPx: 1f + 3 * tickSize, sourceError: error);
            Assert.Equal(result, 0, 5);
        }


        [Fact]
        public void LegBidIsNan_Test()
        {
            float error = 1.11111f;
            float tickSize = 1f / 64f;

            float result = RunTransform(legBidPx: float.NaN, legAskPx: 100f + tickSize, spreadBidPx: 1f, spreadAskPx: 1f + tickSize, sourceError: error);
            Assert.Equal(result, 0, 5);
        }



        [Fact]
        public void LegAskIsNan_Test()
        {
            float error = 1.11111f;
            float tickSize = 1f / 64f;

            float result = RunTransform(legBidPx: 100f, legAskPx: float.NaN, spreadBidPx: 1f, spreadAskPx: 1f + tickSize, sourceError: error);
            Assert.Equal(result, 0, 5);
        }


        [Fact]
        public void SpreadBidIsNan_Test()
        {
            float error = 1.11111f;
            float tickSize = 1f / 64f;

            float result = RunTransform(legBidPx: 100, legAskPx: 100f + tickSize, spreadBidPx: float.NaN, spreadAskPx: 1f + tickSize, sourceError: error);
            Assert.Equal(result, 0, 5);
        }



        [Fact]
        public void SpreadAskIsNan_Test()
        {
            float error = 1.11111f;
            float tickSize = 1f / 64f;

            float result = RunTransform(legBidPx: 100f, legAskPx: 100f + tickSize, spreadBidPx: 1f, spreadAskPx: float.NaN, sourceError: error);
            Assert.Equal(result, 0, 5);
        }
    }
}
