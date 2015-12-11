using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using KT_TTAPI;

namespace MonkeyCancel
{
    public interface IInstrument
    {
        string getAlias();

        event EventHandler<EventArgs> OnPriceChanged;

        float SettlePx { get; }
        float PointValue { get; }
        float TickSize { get; }

        float BidPx { get; }
        float AskPx { get; }

        float BidEdgeVwmpt { get; }
        float Vwmpt { get; }
        float AdjustedVwmpt { get; }
        float AskEdgeVwmpt { get; }
    }

    public class Instrument : IInstrument
    {
        #region Variables
        private static readonly int PRICE_RND_DIGITS = 8;
        protected readonly ITTInstrumentState m_ttInstrumentState;

        bool m_isModified = false;

        bool m_enableBidCxl = true;
        public bool EnableBidCxl
        {
            get { return m_enableBidCxl; }
            set
            {
                if (m_enableBidCxl != value)
                {
                    m_isModified = true;
                }

                m_enableBidCxl = value;
            }
        }

        bool m_enableAskCxl = true;
        public bool EnableAskCxl
        {
            get { return m_enableAskCxl; }
            set
            {
                if (m_enableAskCxl != value)
                {
                    m_isModified = true;
                }

                m_enableAskCxl = value;
            }
        }


        float m_bidCxlEdge = -1;
        public float BidCxlEdge
        {
            get { return m_bidCxlEdge; }
            set
            {
                if (Math.Abs(m_bidCxlEdge - value) > 0.000001)
                {
                    m_isModified = true;
                }

                m_bidCxlEdge = value;
            }
        }


        float m_askCxlEdge = -1;
        public float AskCxlEdge
        {
            get { return m_askCxlEdge; }
            set
            {
                if (Math.Abs(m_askCxlEdge - value) > 0.000001)
                {
                    m_isModified = true;
                }

                m_askCxlEdge = value;
            }
        }

        bool m_enableImpliedCxl = false;
        public bool EnableImpliedCxl
        {
            get { return m_enableImpliedCxl; }
            set
            {
                if (m_enableImpliedCxl != value)
                {
                    m_isModified = true;
                }

                m_enableImpliedCxl = value;
            }
        }


        int m_minImpliedQty = 0;
        public int MinImpliedQty
        {
            get { return m_minImpliedQty; }
            set
            {
                if (m_minImpliedQty != value)
                {
                    m_isModified = true;
                }

                m_minImpliedQty = value;
            }
        }

        private float m_impliedBidPx, m_impliedAskPx, m_bidQty, m_askQty, m_impliedBidQty, m_impliedAskQty;
        private bool m_isWideMkt;

        private float m_topBidWrkPx, m_topAskWrkPx;


        private float m_bidPx;
        public float BidPx
        {
            get { return m_bidPx; }
        }

        private float m_askPx;
        public float AskPx
        {
            get { return m_askPx; }
        }

        volatile private float m_bidEdgeVwmpt;
        public float BidEdgeVwmpt
        {
            get { return m_bidEdgeVwmpt; }
        }

        volatile private float m_askEdgeVwmpt;
        public float AskEdgeVwmpt
        {
            get { return m_askEdgeVwmpt; }
        }

        /// <summary>
        /// The last Vwmpt price adjusted by "spread-implied pricing" algorithm.
        /// For basic model instruments, it has the same value as Vwmpt.
        /// </summary>
        protected float m_adjustedVwmpt;
        public float AdjustedVwmpt
        {
            get { return m_adjustedVwmpt; }
        }

        /// <summary>
        /// The last unmodified Vwmpt value.
        /// </summary>
        volatile private float m_vwmpt;
        public float Vwmpt
        {
            get { return m_vwmpt; }
        }

        /// <summary>
        /// The instrument settlement price or NaN if it is absent.
        /// </summary>
        public float SettlePx
        {
            get { return m_ttInstrumentState.SettlePx; }
        }

        public virtual event EventHandler<EventArgs> OnPriceChanged = delegate { }; //Initialized by default delegate to be sure it cannot be null

        public float PointValue
        {
            get { return m_ttInstrumentState.PointValue; }
        }

        public float TickSize
        {
            get { return m_ttInstrumentState.Orderbook.TickSize; }
        }

        #endregion

        public Instrument(ITTInstrumentState instrumentState)
        {
            m_ttInstrumentState = instrumentState;
            m_ttInstrumentState.OnInstrumentStateChange += onInstrumentStateChange;
            m_ttInstrumentState.OnTobQuoteChange += onTobQuoteChange;
        }

        public string getAlias()
        {
            return m_ttInstrumentState.Alias;
        }

        public void UpdateIfModified()
        {
            if (!m_isModified) return;
            m_isModified = false;

            m_ttInstrumentState.InvokeAction(new Action(() => {
                doLogic();
            }));
        }


        private void onInstrumentStateChange(object sender, EventArgs e)
        {
            m_ttInstrumentState.getTobData(out m_bidPx, out m_askPx, out m_bidQty, out m_askQty,
                out m_impliedBidPx, out m_impliedAskPx, out m_impliedBidQty, out m_impliedAskQty);

            m_isWideMkt = Math.Round(m_askPx - m_bidPx, PRICE_RND_DIGITS) > m_ttInstrumentState.Orderbook.TickSize;

            //calc md vars
            float q = (float)((m_bidQty / (m_bidQty + m_askQty)) - 0.5);
            float theo = (m_bidPx + m_askPx) / 2;

            if ((m_askPx > m_bidPx) && !m_isWideMkt)
                theo += q * m_ttInstrumentState.Orderbook.TickSize;

            m_vwmpt = theo;

            doPricing();

            OnPriceChanged(this, EventArgs.Empty);
        }

        protected void doPricing()
        {
            m_adjustedVwmpt = CalculateAdjustedVwmpt();

            m_bidEdgeVwmpt = (m_adjustedVwmpt - m_bidPx) / m_ttInstrumentState.Orderbook.TickSize;
            m_askEdgeVwmpt = (m_askPx - m_adjustedVwmpt) / m_ttInstrumentState.Orderbook.TickSize;

            doLogic();
        }

        protected virtual float CalculateAdjustedVwmpt()
        {
            return m_vwmpt;
        }

        private void onTobQuoteChange(object sender, EventArgs e)
        {
            float topBidWrkQty, topAskWrkQty; //unused for now
            bool topBidWrkStacked, topAskWrkStacked; //unused for now

            m_ttInstrumentState.Orderbook.GetTobWrkData(out m_topBidWrkPx, out m_topAskWrkPx, out topBidWrkQty, out topAskWrkQty, 
                out topBidWrkStacked, out topAskWrkStacked);

            doLogic();
        }

        private void doLogic()
        {
            bool onBboBid = m_topBidWrkPx == m_bidPx;
            bool onBboAsk = m_topAskWrkPx == m_askPx;

            bool cxlBboBid = onBboBid && checkEdgeCxl(EnableBidCxl, BidCxlEdge, m_bidEdgeVwmpt);
            bool cxlBboOffer = onBboAsk && checkEdgeCxl(EnableAskCxl, AskCxlEdge, m_askEdgeVwmpt);

            //if (onBboBid)
            //    Debug.WriteLine(onBboBid + " " + m_bidPx + " " + EnableBidCxl + " " + BidCxlEdge + " " + m_bidEdgeVwmpt);

            if (EnableImpliedCxl)
            {
                if (!cxlBboBid && onBboBid)
                    cxlBboBid = checkImpliedCxl(m_topBidWrkPx, m_impliedBidPx, m_impliedBidQty);

                if (!cxlBboOffer && onBboAsk)
                    cxlBboOffer = checkImpliedCxl(m_topAskWrkPx, m_impliedAskPx, m_impliedAskQty);
            }

            if (cxlBboBid)
            {
                m_ttInstrumentState.Orderbook.cancelTobSide(Side.Buy, m_bidPx);
                Debug.WriteLine("cxl bid " + m_bidPx);
            }

            if (cxlBboOffer)
            {
                m_ttInstrumentState.Orderbook.cancelTobSide(Side.Sell, m_askPx);
                Debug.WriteLine("cxl ask " + m_askPx);
            }
        }

        private bool checkEdgeCxl(bool cxlEnabled, float cxlEdge, float bboEdge)
        {
            if (m_isWideMkt)
                return false;

            if (!cxlEnabled)
                return false;

            if (float.IsNaN(bboEdge))
                return false;

            if (cxlEdge < bboEdge)
                return false;

            return true;
        }

        private bool checkImpliedCxl(float topWrkPx, float bboImpliedPx, float bboImpliedQty)
        {
            if (topWrkPx != bboImpliedPx)
                return true;

            if (bboImpliedQty < MinImpliedQty)
                return true;

            return false;
        }


        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(256);
            sb.Append("[Instrument: Alias=").Append(m_ttInstrumentState.Alias)
                .Append(", TopWrkPx=").Append(m_topBidWrkPx).Append("--").Append(m_topAskWrkPx)
                .Append(", Edge=")
                .Append(EnableBidCxl).Append(",").Append(BidCxlEdge).Append("--")
                .Append(EnableAskCxl).Append(",").Append(AskCxlEdge)
                .Append(", MinImpliedQty=").Append(m_enableImpliedCxl).Append(",").Append(MinImpliedQty)
                .Append(", ToB=")
                .Append(m_bidPx).Append("/").Append(m_bidQty).Append("--")
                .Append(m_askPx).Append("/").Append(m_askQty)
                .Append(", iToB=")
                .Append(m_impliedBidPx).Append("/").Append(m_impliedBidQty).Append("--")
                .Append(m_impliedAskPx).Append("/").Append(m_impliedAskQty)
                .Append(", isWideMkt=").Append(m_isWideMkt)
                .Append(", Vwmpt=").Append(m_vwmpt)
                .Append(", AdjustedVwmpt=").Append(m_adjustedVwmpt)
                .Append(", EdgeVwmpt=").Append(m_bidEdgeVwmpt).Append("--").Append(m_askEdgeVwmpt)
                .Append("\n").Append(m_ttInstrumentState.ToString())
                .Append("/Instrument]");

            return sb.ToString();
        }

        internal void WriteDump(DumpWriter dumpWriter)
        {
            m_ttInstrumentState.InvokeAction(new Action(() => {
                dumpWriter.WriteString(ToString());
            }));
        }
    }
}