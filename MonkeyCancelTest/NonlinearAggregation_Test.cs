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
    public class NonlinearAggregation_Test
    {
        [Fact]
        public void OneError_Return0_Test()
        {
            var aggregation = new NonlinearAggregation();
            aggregation.Reset();

            var myInstrument = Substitute.For<IInstrument>();

            float error1 = 100f;
            var spread1 = Substitute.For<ISpread>();

            aggregation.AddLegError(error1, spread1, myInstrument);

            var result = aggregation.Calculate();

            Assert.Equal(result, 0, 5);
        }


        [Fact]
        public void OnePositiveOneNaN_Return0_Test()
        {
            var aggregation = new NonlinearAggregation();
            aggregation.Reset();

            var myInstrument = Substitute.For<IInstrument>();

            float error1 = 100f;
            var spread1 = Substitute.For<ISpread>();
            aggregation.AddLegError(error1, spread1, myInstrument);

            float error2 = float.NaN;
            var spread2 = Substitute.For<ISpread>();
            aggregation.AddLegError(error2, spread2, myInstrument);

            var result = aggregation.Calculate();

            Assert.Equal(result, 0, 5);
        }


        [Fact]
        public void OnePositiveOneZero_Return0_Test()
        {
            var aggregation = new NonlinearAggregation();
            aggregation.Reset();

            var myInstrument = Substitute.For<IInstrument>();

            float error1 = 0;
            var spread1 = Substitute.For<ISpread>();
            aggregation.AddLegError(error1, spread1, myInstrument);

            float error2 = 101;
            var spread2 = Substitute.For<ISpread>();
            aggregation.AddLegError(error2, spread2, myInstrument);

            var result = aggregation.Calculate();

            Assert.Equal(result, 0, 5);
        }


        [Fact]
        public void OneNegativeOneNaN_Return0_Test()
        {
            var aggregation = new NonlinearAggregation();
            aggregation.Reset();

            var myInstrument = Substitute.For<IInstrument>();

            float error1 = -1f;
            var spread1 = Substitute.For<ISpread>();
            aggregation.AddLegError(error1, spread1, myInstrument);

            float error2 = float.NaN;
            var spread2 = Substitute.For<ISpread>();
            aggregation.AddLegError(error2, spread2, myInstrument);

            var result = aggregation.Calculate();

            Assert.Equal(result, 0, 5);
        }


        [Fact]
        public void TwoPositive_ReturnMean_Test()
        {
            var aggregation = new NonlinearAggregation();
            aggregation.Reset();

            var myInstrument = Substitute.For<IInstrument>();

            float error1 = 100f;
            var spread1 = Substitute.For<ISpread>();
            aggregation.AddLegError(error1, spread1, myInstrument);

            float error2 = 200f;
            var spread2 = Substitute.For<ISpread>();
            aggregation.AddLegError(error2, spread2, myInstrument);

            var result = aggregation.Calculate();

            Assert.Equal(result, 150.0, 5);
        }


        [Fact]
        public void TwoNegative_ReturnMean_Test()
        {
            var aggregation = new NonlinearAggregation();
            aggregation.Reset();

            var myInstrument = Substitute.For<IInstrument>();

            float error1 = -100f;
            var spread1 = Substitute.For<ISpread>();
            aggregation.AddLegError(error1, spread1, myInstrument);

            float error2 = -200f;
            var spread2 = Substitute.For<ISpread>();
            aggregation.AddLegError(error2, spread2, myInstrument);

            var result = aggregation.Calculate();

            Assert.Equal(result, -150.0, 5);
        }


        [Fact]
        public void TwoPositiveAndNegative_Return0_Test()
        {
            var aggregation = new NonlinearAggregation();
            aggregation.Reset();

            var myInstrument = Substitute.For<IInstrument>();

            float error1 = 100f;
            var spread1 = Substitute.For<ISpread>();
            aggregation.AddLegError(error1, spread1, myInstrument);

            float error2 = -200f;
            var spread2 = Substitute.For<ISpread>();
            aggregation.AddLegError(error2, spread2, myInstrument);

            var result = aggregation.Calculate();

            Assert.Equal(result, 0, 5);
        }


        [Fact]
        public void ThreePositive_ReturnMean_Test()
        {
            var aggregation = new NonlinearAggregation();
            aggregation.Reset();

            var myInstrument = Substitute.For<IInstrument>();

            float error1 = 100f;
            var spread1 = Substitute.For<ISpread>();
            aggregation.AddLegError(error1, spread1, myInstrument);

            float error2 = 123f;
            var spread2 = Substitute.For<ISpread>();
            aggregation.AddLegError(error2, spread2, myInstrument);

            float error3 = 140f;
            var spread3 = Substitute.For<ISpread>();
            aggregation.AddLegError(error3, spread3, myInstrument);

            var result = aggregation.Calculate();

            Assert.Equal(result, 121, 5);
        }



        [Fact]
        public void ThreeNegative_ReturnMean_Test()
        {
            var aggregation = new NonlinearAggregation();
            aggregation.Reset();

            var myInstrument = Substitute.For<IInstrument>();

            float error1 = -100f;
            var spread1 = Substitute.For<ISpread>();
            aggregation.AddLegError(error1, spread1, myInstrument);

            float error2 = -123f;
            var spread2 = Substitute.For<ISpread>();
            aggregation.AddLegError(error2, spread2, myInstrument);

            float error3 = -140f;
            var spread3 = Substitute.For<ISpread>();
            aggregation.AddLegError(error3, spread3, myInstrument);

            var result = aggregation.Calculate();

            Assert.Equal(result, -121, 5);
        }


        [Fact]
        public void TwoPositiveAndOneNegative_Return0_Test()
        {
            var aggregation = new NonlinearAggregation();
            aggregation.Reset();

            var myInstrument = Substitute.For<IInstrument>();

            float error1 = 100f;
            var spread1 = Substitute.For<ISpread>();
            aggregation.AddLegError(error1, spread1, myInstrument);

            float error2 = 200f;
            var spread2 = Substitute.For<ISpread>();
            aggregation.AddLegError(error2, spread2, myInstrument);

            float error3 = -1f;
            var spread3 = Substitute.For<ISpread>();
            aggregation.AddLegError(error3, spread3, myInstrument);

            var result = aggregation.Calculate();

            Assert.Equal(result, 0, 5);
        }


        [Fact]
        public void TwoNegativeAndOnePositive_Return0_Test()
        {
            var aggregation = new NonlinearAggregation();
            aggregation.Reset();

            var myInstrument = Substitute.For<IInstrument>();

            float error1 = -100f;
            var spread1 = Substitute.For<ISpread>();
            aggregation.AddLegError(error1, spread1, myInstrument);

            float error2 = -200f;
            var spread2 = Substitute.For<ISpread>();
            aggregation.AddLegError(error2, spread2, myInstrument);

            float error3 = 5f;
            var spread3 = Substitute.For<ISpread>();
            aggregation.AddLegError(error3, spread3, myInstrument);

            var result = aggregation.Calculate();

            Assert.Equal(result, 0, 5);
        }


        [Fact]
        public void ThreePositiveOneNegative_ReturnMeanOfPositive_Test()
        {
            var aggregation = new NonlinearAggregation();
            aggregation.Reset();

            var myInstrument = Substitute.For<IInstrument>();

            float error1 = 100f;
            var spread1 = Substitute.For<ISpread>();
            aggregation.AddLegError(error1, spread1, myInstrument);

            float error2 = 123f;
            var spread2 = Substitute.For<ISpread>();
            aggregation.AddLegError(error2, spread2, myInstrument);

            float error3 = -1f;
            var spread3 = Substitute.For<ISpread>();
            aggregation.AddLegError(error3, spread3, myInstrument);

            float error4 = 140f;
            var spread4 = Substitute.For<ISpread>();
            aggregation.AddLegError(error4, spread4, myInstrument);

            var result = aggregation.Calculate();

            Assert.Equal(result, 121, 5);
        }



        [Fact]
        public void ThreeNegativeOnePositive_ReturnMeanOfNegative_Test()
        {
            var aggregation = new NonlinearAggregation();
            aggregation.Reset();

            var myInstrument = Substitute.For<IInstrument>();

            float error1 = -100f;
            var spread1 = Substitute.For<ISpread>();
            aggregation.AddLegError(error1, spread1, myInstrument);

            float error2 = -123f;
            var spread2 = Substitute.For<ISpread>();
            aggregation.AddLegError(error2, spread2, myInstrument);

            float error3 = 2f;
            var spread3 = Substitute.For<ISpread>();
            aggregation.AddLegError(error3, spread3, myInstrument);

            float error4 = -140f;
            var spread4 = Substitute.For<ISpread>();
            aggregation.AddLegError(error4, spread4, myInstrument);

            var result = aggregation.Calculate();

            Assert.Equal(result, -121, 5);
        }


        [Fact]
        public void ThreePositiveTwoNegative_Return0_Test()
        {
            var aggregation = new NonlinearAggregation();
            aggregation.Reset();

            var myInstrument = Substitute.For<IInstrument>();

            float error1 = -5f;
            var spread1 = Substitute.For<ISpread>();
            aggregation.AddLegError(error1, spread1, myInstrument);

            float error2 = -100f;
            var spread2 = Substitute.For<ISpread>();
            aggregation.AddLegError(error2, spread2, myInstrument);

            float error3 = 140f;
            var spread3 = Substitute.For<ISpread>();
            aggregation.AddLegError(error3, spread3, myInstrument);

            float error4 = 100f;
            var spread4 = Substitute.For<ISpread>();
            aggregation.AddLegError(error4, spread4, myInstrument);

            float error5 = 123f;
            var spread5 = Substitute.For<ISpread>();
            aggregation.AddLegError(error5, spread5, myInstrument);

            var result = aggregation.Calculate();

            Assert.Equal(result, 0, 5);
        }


        [Fact]
        public void ThreeNegativeTwoPositive_Return0_Test()
        {
            var aggregation = new NonlinearAggregation();
            aggregation.Reset();

            var myInstrument = Substitute.For<IInstrument>();

            float error1 = 5f;
            var spread1 = Substitute.For<ISpread>();
            aggregation.AddLegError(error1, spread1, myInstrument);

            float error2 = 100f;
            var spread2 = Substitute.For<ISpread>();
            aggregation.AddLegError(error2, spread2, myInstrument);

            float error3 = -140f;
            var spread3 = Substitute.For<ISpread>();
            aggregation.AddLegError(error3, spread3, myInstrument);

            float error4 = 100f;
            var spread4 = Substitute.For<ISpread>();
            aggregation.AddLegError(error4, spread4, myInstrument);

            float error5 = -123f;
            var spread5 = Substitute.For<ISpread>();
            aggregation.AddLegError(error5, spread5, myInstrument);

            var result = aggregation.Calculate();

            Assert.Equal(result, 0, 5);
        }


        [Fact]
        public void ThreePositiveOneZeroOneNegative_ReturnsMeanOfPositive_Test()
        {
            var aggregation = new NonlinearAggregation();
            aggregation.Reset();

            var myInstrument = Substitute.For<IInstrument>();

            float error1 = -2f;
            var spread1 = Substitute.For<ISpread>();
            aggregation.AddLegError(error1, spread1, myInstrument);

            float error2 = 100f;
            var spread2 = Substitute.For<ISpread>();
            aggregation.AddLegError(error2, spread2, myInstrument);

            float error3 = 0;
            var spread3 = Substitute.For<ISpread>();
            aggregation.AddLegError(error3, spread3, myInstrument);

            float error4 = 123f;
            var spread4 = Substitute.For<ISpread>();
            aggregation.AddLegError(error4, spread4, myInstrument);

            float error5 = 140f;
            var spread5 = Substitute.For<ISpread>();
            aggregation.AddLegError(error5, spread5, myInstrument);

            var result = aggregation.Calculate();

            Assert.Equal(result, 121, 5);
        }

    }
}
