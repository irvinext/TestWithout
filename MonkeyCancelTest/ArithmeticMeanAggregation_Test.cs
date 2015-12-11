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
    public class ArithmeticMeanAggregation_Test
    {
        [Fact]
        public void Calculation_Test()
        {
            var aggregation = new ArithmeticMeanAggregation();
            aggregation.Reset();

            var myInstrument = Substitute.For<IInstrument>();

            float error1 = -2f;
            var spread1 = Substitute.For<ISpread>();
            aggregation.AddLegError(error1, spread1, myInstrument);

            float error2 = 100f;
            var spread2 = Substitute.For<ISpread>();
            aggregation.AddLegError(error2, spread2, myInstrument);

            float error3 = -3;
            var spread3 = Substitute.For<ISpread>();
            aggregation.AddLegError(error3, spread3, myInstrument);

            float error4 = float.NaN;
            var spread4 = Substitute.For<ISpread>();
            aggregation.AddLegError(error4, spread4, myInstrument);

            float error5 = 140f;
            var spread5 = Substitute.For<ISpread>();
            aggregation.AddLegError(error5, spread5, myInstrument);

            var result = aggregation.Calculate();

            Assert.Equal(result, 58.75, 5);
        }

    }
}
