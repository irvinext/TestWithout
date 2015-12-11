using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonkeyCancel
{
    /// <summary>
    /// This class calculates the Arithmetic Mean of flow-down errors.
    /// </summary>
    public class ArithmeticMeanAggregation : IFlowDownErrorsAggregation
    {
        double m_errorsSum;
        uint m_counter;

        public ArithmeticMeanAggregation()
        {
        }

        public void Reset()
        {
            m_errorsSum = 0;
            m_counter = 0;
        }

        /// <summary>
        /// The method calculates the sum of flow-down errors.
        /// The invalid (NaN) and uninformative (zero) flow-down errors are ignored.
        /// </summary>
        /// <param name="legError">Flow down error for leg</param>
        /// <param name="spread">Spread which is used to calculate the flow-down error</param>
        /// <param name="myInstrument">The leg instrument the calculation is performed for</param>
        public void AddLegError(float legError, ISpread spread, IInstrument myInstrument)
        {
            if (float.IsNaN(legError))
                return;

            m_errorsSum += legError;
            m_counter++;
        }

        /// <summary>
        /// The method returns the mean of all values passed to AddLegError.
        /// </summary>
        public double Calculate()
        {
            if (m_counter <= 0)
            {
                return 0;
            }

            return m_errorsSum / m_counter;
        }

        public override string ToString()
        {
            return "Aggregation: Arithmetic Mean";
        }
    }
}
