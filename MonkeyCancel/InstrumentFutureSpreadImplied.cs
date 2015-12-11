using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KT_TTAPI;
using System.Diagnostics;

namespace MonkeyCancel
{
    public interface IFlowDownErrorTransformation
    {
        float Transform(float sourceError, ISpread spread, IInstrument myInstrument);
    }

    public interface IFlowDownErrorsAggregation
    {
        void Reset();
        void AddLegError(float legError, ISpread spread, IInstrument myInstrument);
        double Calculate();
    }


    /// <summary>
    /// The SpreadImpliedInstrument is the class for the outright instruments which uses the "Spread-Implied" price model.
    /// It inherits from Instrument class and overrides the CalculateAdjustedVwmpt() method, which calculates the 
    /// Adjusted Vwmpt value according to the "Spread-Implied" price model.
    /// </summary>
    public class InstrumentFutureSpreadImplied : Instrument
    {
        //The list of Spreads which contain this instrument as leg.
        volatile ISpread[] m_spreads = new ISpread[0]; 
        object m_synch = new object();

        IFlowDownErrorsAggregation m_aggregation;
        IFlowDownErrorTransformation[] m_transformations;

        public InstrumentFutureSpreadImplied(ITTInstrumentState instrumentState, IFlowDownErrorTransformation[] transformations, IFlowDownErrorsAggregation aggregation)
            : base(instrumentState)
        {
            m_transformations = transformations;
            m_aggregation = aggregation;
        }


        public void AddSpread(ISpread spreadToAdd)
        {
            Debug.Assert(spreadToAdd != null);
            if (spreadToAdd == null)
                return;

            lock (m_synch)
            {
                //The m_spreads array is immutable to avoid the "lock" in doPricing(). The m_spreads is changed during the startup only.
                var list = new List<ISpread>(16);
                list.AddRange(m_spreads);
                list.Add(spreadToAdd);
                m_spreads = list.ToArray();
            }
        }


        /// <summary>
        /// This event handler recalculates the theo prices and edges from this instrument thread.
        /// 
        /// It is called when spreads price or other legs prices are changed (only spreads which contain this instrument as leg and the legs
        /// of these spreads). 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public virtual void OnLinkedInstrumentChanged(object sender, EventArgs e)
        {
            m_ttInstrumentState.InvokeAction(new Action(() => {
                doPricing();
            }));
        }


        /// <summary>
        /// This method calculates the AdjustedVwmpt value, i.e. the Vwmpt adjusted by spreads prices.
        /// The method returns this instrument Vwmpt minus aggregated "flow-down errors" value.
        /// The array of IFlowDownErrorTransformation objects is used to adjust the flow-down errors.
        /// The adjusted value is passed to IFlowDownErrorsAggregation object.
        /// After all flow-down errors are processed the aggregated flow-down error is calculated by IFlowDownErrorsAggregation.Calculate method.
        /// If "flow-down errors" cannot be calculated for some Spread then this spread is ignored.
        /// If "flow-down errors" for all spreads cannot be calculated then the usual Vwmpt is returned from the method.
        /// </summary>
        /// <returns>Returns the Adjusted Vwmpt value</returns>
        protected override float CalculateAdjustedVwmpt()
        {
            m_aggregation.Reset();

            var spreads = m_spreads;
            foreach (var spread in spreads)
            {
                float legError = CalculateFlowDownErrorForSpread(spread);
                if (m_transformations != null)
                {
                    foreach(var transformation in m_transformations)
                    {
                        if (float.IsNaN(legError) || Math.Abs(legError) < 0.000001)
                            break; //There is no sence to transform further If zeroed or invalid

                        legError = transformation.Transform(legError, spread, this);
                    }
                }

                m_aggregation.AddLegError(legError, spread, this);
            }


            return Convert.ToSingle(Vwmpt - m_aggregation.Calculate());
        }


        /// <summary>
        /// This method calculates the flow-down error for the spread.
        /// It supports the two-legs spreads only.
        /// The calculation forrmula for ICS:
        /// A.error = (A.theo - A.settle) - (Spread.theo - (B.theo - B.settle) * (B.weight / A.weight))
        /// B.error = (B.theo - B.settle) - (Spread.theo - (A.theo - A.settle)) * (A.weight / B.weight) 
        /// , where A is positive leg and B is negative leg
        ///   and Leg.Weight = Leg.Ratio * Leg.PointValue / (min Point Value among the legs Point Values)
        /// For Calendar and other two-legs spreads the method does not subtract the Settlement prices, i.e. 
        /// the formula is the following:
        /// A.error = A.theo - (Spread.theo - B.theo * (B.weight / A.weight))
        /// B.error = B.theo - (Spread.theo - A.theo) * (A.weight / B.weight)
        /// </summary>
        /// <param name="spread">The spread reference which have this instrument as leg</param>
        /// <returns></returns>
        protected virtual float CalculateFlowDownErrorForSpread(ISpread spread)
        {
            var legs = spread.Legs;
            if (legs == null)
                return float.NaN;

            if (legs.Length != 2)
                return float.NaN;

            SpreadLeg mySpreadLeg = null;
            SpreadLeg oppositeSpreadLeg = null;
            if (ReferenceEquals(legs[0].Instrument, this))
            {
                mySpreadLeg = legs[0];
                oppositeSpreadLeg = legs[1];
            }
            else if (ReferenceEquals(legs[1].Instrument, this))
            {
                mySpreadLeg = legs[1];
                oppositeSpreadLeg = legs[0];
            }
            else
            {
                Debug.Assert(false, "The leg does not belong to strategy");
                return float.NaN; //The leg is not found
            }


            float spreadVwmpt = spread.Vwmpt;
            if (float.IsNaN(spreadVwmpt))
                return float.NaN;

            float myLegVwmpt = mySpreadLeg.Instrument.Vwmpt;
            if (float.IsNaN(myLegVwmpt))
                return float.NaN;

            float oppositeLegVwmpt = oppositeSpreadLeg.Instrument.Vwmpt;
            if (float.IsNaN(oppositeLegVwmpt))
                return float.NaN;

            if (spread.IsNetChangePx)
            {
                float mySettlePx = mySpreadLeg.Instrument.SettlePx;
                if (float.IsNaN(mySettlePx))
                    return float.NaN;

                myLegVwmpt -= mySettlePx;

                float oppositeSettlePx = oppositeSpreadLeg.Instrument.SettlePx;
                if (float.IsNaN(oppositeSettlePx))
                    return float.NaN;

                oppositeLegVwmpt -= oppositeSettlePx;
            }


            if (mySpreadLeg.Ratio > 0)
            {
                // A.error = (A.theo - A.settle) - (Spread.theo - (B.theo - B.settle) * (B.Weight / A.Weight))
                return myLegVwmpt - (spreadVwmpt - oppositeLegVwmpt * oppositeSpreadLeg.Weight / mySpreadLeg.Weight);
            }

            // B.error = (B.theo - B.settle) - (Spread.theo - (A.theo - A.settle)) * (A.Weight / B.Weight) 
            return myLegVwmpt - (spreadVwmpt - oppositeLegVwmpt) * oppositeSpreadLeg.Weight / mySpreadLeg.Weight;
        }


        public override string ToString()
        {
            var spreads = m_spreads;

            StringBuilder sb = new StringBuilder(256);
            sb.Append("[SpreadImpliedInstrument ")
                .Append("Spreads=").Append(spreads != null ? spreads.Length.ToString() : string.Empty).Append(" ")
                .Append(base.ToString())
                .Append(", ").Append(m_aggregation);

            if (m_transformations != null)
                foreach (var transformation in m_transformations)
                    sb.Append(", ").Append(transformation);

            sb.Append("/SpreadImpliedInstrument]");

            return sb.ToString();
        }

    }
}
