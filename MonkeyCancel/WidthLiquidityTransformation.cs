using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonkeyCancel
{
    /// <summary>
    /// This class transforms the flow-down errors as the function of Liquidity.
    /// The Liquidity is calculated based using the width between the bid and ask.
    /// </summary>
    public class WidthLiquidityTransformation : IFlowDownErrorTransformation
    {
        public WidthLiquidityTransformation()
        {
        }


        /// <summary>
        /// The method calculates the width between best bid and ask in ticks (TickWidth) and returns the Liquidity.
        /// </summary>
        /// <param name="instrument">The instrument to calculate the liquidity</param>
        /// <returns>1 (for TickWidth = 1), 0.5 (for TickWidth = 2) and 0 (otherwise)</returns>
        private float CalculateLiquidityFactor(IInstrument instrument)
        {
            float bidPx = instrument.BidPx;
            float askPx = instrument.AskPx;
            float tickSize = instrument.TickSize;

            float width = (askPx - bidPx) / tickSize;
            if (width <= 1.0001)
                return 1f;

            if (width <= 2.0001)
                return 0.5f;

            return 0f;
        }


        /// <summary>
        /// This method transforms the flow-down error using the following formula:
        /// Leg.FlowDownError * min(OppositeLeg.Liquidity, Spread.Liquidity) .
        /// </summary>
        /// <param name="sourceError">The flow-down error.</param>
        /// <param name="spread">The Spread instrument which give this flow-down error.</param>
        /// <param name="myInstrument">The instrument which flow-down error is calculated for.</param>
        /// <returns></returns>
        public float Transform(float sourceError, ISpread spread, IInstrument myInstrument)
        {
            var legs = spread.Legs;
            if (legs == null)
                return 0;

            var spreadLiquidityFactor = CalculateLiquidityFactor(spread);
            if (float.IsNaN(spreadLiquidityFactor))
                return 0;

            float minLiquitityFactor = spreadLiquidityFactor;

            foreach (var leg in legs)
            {
                if (ReferenceEquals(leg.Instrument, myInstrument))
                    continue;

                var legLiquidityFactor = CalculateLiquidityFactor(leg.Instrument);
                if (float.IsNaN(legLiquidityFactor))
                    return 0;

                minLiquitityFactor = Math.Min(minLiquitityFactor, legLiquidityFactor);
            }


            return sourceError * minLiquitityFactor;
        }

        public override string ToString()
        {
            return "Transformation: Width Liquidity";
        }
    }
}
