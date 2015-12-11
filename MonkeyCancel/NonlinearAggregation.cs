using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonkeyCancel
{
    struct LegErrorInfo
    {
        public float LegError { get; set; }
        public ISpread Spread { get; set; }
        public IInstrument MyInstrument { get; set; }
    }


    /// <summary>
    /// This class collects the flow-down errors and aggregates. The aggregate algorithm is described 
    /// in the "Calculate" method.
    /// </summary>
    public class NonlinearAggregation : IFlowDownErrorsAggregation
    {
        List<LegErrorInfo> m_errorInfoList = new List<LegErrorInfo>(32);

        public NonlinearAggregation()
        {
        }


        public void Reset()
        {
            m_errorInfoList.Clear();
        }


        /// <summary>
        /// The method collects the flow-down errors.
        /// The invalid (NaN) and uninformative (zero) flow-down errors are ignored.
        /// </summary>
        /// <param name="legError">The flow-down error for leg</param>
        /// <param name="spread">The Spread instrument which is the source of this flow-down error</param>
        /// <param name="myInstrument">The leg instrument which calculation is performed for</param>
        public void AddLegError(float legError, ISpread spread, IInstrument myInstrument)
        {
            if (float.IsNaN(legError))
                return;

            if (Math.Abs(legError) < 0.00001)
                return;

            m_errorInfoList.Add(new LegErrorInfo()
            {
                LegError = legError,
                Spread = spread,
                MyInstrument = myInstrument
            });
        }


        /// <summary>
        /// Aggregates the flow-down errors according to the following rules:
        /// a. if there is only one informative flow-down error then the Vwmpt is not adjusted 
        /// (i.e remains as in Basic price model)
        /// b.If there are only two or three informative flow-down errors and all are 
        /// positive (or all are negative) then the final flow-down error is MeanOf(FlowDownErrors)
        /// if number of informative flow-down errors > 3 and 
        ///    if NumberOf(PositiveFlowDownErrors) > 2 * NumberOf(NegativeFlowDownErrors) 
        ///       then the final flow-down error is MeanOf(PositiveFlowDownErrors)
        /// if number of informative flow-down errors > 3 and 
        ///    if NumberOf(NegativeFlowDownErrors) > 2 * NumberOf(PositiveFlowDownErrors) 
        ///       then the final flow-down error is MeanOf(NegativeFlowDownErrors)
        /// </summary>
        /// <returns></returns>
        public double Calculate()
        {
            int numberOfErrors = m_errorInfoList.Count;

            if (numberOfErrors <= 1)
                return 0;

            int numberOfPositive = m_errorInfoList.Count(e => e.LegError > 0);
            int numberOfNegative = numberOfErrors - numberOfPositive;

            if (numberOfErrors <= 3) 
            {
                //Check if all are positive or all are vegative
                if (numberOfPositive == numberOfErrors || numberOfPositive == 0)
                    return m_errorInfoList.Sum(e => e.LegError) / numberOfErrors;

                return 0;
            }


            if (numberOfPositive > 2 * numberOfNegative)
                return m_errorInfoList.Where(e => e.LegError > 0).Sum(e => e.LegError) / numberOfPositive;

            if (numberOfNegative > 2 * numberOfPositive)
                return m_errorInfoList.Where(e => e.LegError < 0).Sum(e => e.LegError) / numberOfNegative;

            return 0;
        }

        public override string ToString()
        {
            return "Aggregation: Nonlinear";
        }
    }
}
