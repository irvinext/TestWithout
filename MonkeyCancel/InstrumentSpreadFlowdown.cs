using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KT_TTAPI;

namespace MonkeyCancel
{
    public class InstrumentSpreadFlowdown : InstrumentSpread
    {
        public InstrumentSpreadFlowdown(ITTInstrumentState instrumentState)
            : base(instrumentState)
        {
        }

        /// <summary>
        /// This event handler recalculates the theo prices and edges using the spread thread.
        /// 
        /// It is called when spreads legs prices are changed.
        /// </summary>
        public virtual void OnLinkedInstrumentChanged(object sender, EventArgs e)
        {
            m_ttInstrumentState.InvokeAction(new Action(() => {
                doPricing();
            }));
        }

        /// <summary>
        /// The method adjusts the spread Vwmpt using the following formula:
        /// Spread.adjustedVwmpt = (Spread.vwmpt + Spread.impliedVwmpt) / 2
        /// , where Spread.vwmpt is the Vwmpt calculated using the Basic price model
        /// and Spread.impliedVwmpt is Vwmpt calculated using the Vwmpts of legs.
        /// </summary>
        /// <returns>Returns adjusted Vwmpt or original (Basic price model) Vwmpt if cannot be calculated.</returns>
        protected override float CalculateAdjustedVwmpt()
        {
            float myVwmpt = Vwmpt;
            if (float.IsNaN(myVwmpt))
                return Vwmpt;

            double impliedVwmpt = CalculateImpliedVwmpt();
            if (double.IsNaN(impliedVwmpt))
                return Vwmpt;

            return (float)((myVwmpt + impliedVwmpt) / 2);
        }

        /// <summary>
        /// The method calculated the implied Vwmpt for spread using the following formula:
        /// For ICS Spreads : Spread.impliedVwmpt = (LegA.theo - LegA.settle) + (LegB.theo - LegA.settle) * (LegB.weight / LegA.weight)
        /// For usual Spreads: Spread.impliedVwmpt = LegA.theo  + LegB.theo * (LegB.weight / LegA.weight)
        /// , where LegA is the first leg
        /// </summary>
        /// <returns>Returns the calculated value or NaN if cannot be calculated</returns>
        double CalculateImpliedVwmpt()
        {
            var legs = Legs;
            if (legs == null)
                return double.NaN;

            if (legs.Length != 2) 
                return double.NaN; //The formula is for two-legs spreads only 

            SpreadLeg legA = legs[0];
            SpreadLeg legB = legs[1];

            float legAVwmpt = legA.Instrument.Vwmpt;
            if (float.IsNaN(legAVwmpt))
                return double.NaN;

            float legBVwmpt = legB.Instrument.Vwmpt;
            if (float.IsNaN(legBVwmpt))
                return double.NaN;


            if (IsNetChangePx)
            {
                float legASettlePx = legA.Instrument.SettlePx;
                if (float.IsNaN(legASettlePx))
                    return double.NaN;

                legAVwmpt -= legASettlePx;


                float legBSettlePx = legB.Instrument.SettlePx;
                if (float.IsNaN(legBSettlePx))
                    return double.NaN;

                legBVwmpt -= legBSettlePx;
            }

            return (double)legAVwmpt + legBVwmpt * (legB.Weight / legA.Weight);
        }



        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(256);
            sb.Append("[FlowdownSpreadInstrument ");
            sb.Append(base.ToString());
            sb.Append("/FlowdownSpreadInstrument]");

            return sb.ToString();

        }
    }
}
