using KT_TTAPI;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace MonkeyCancel
{
    /// <summary>
    /// This class is used to create and update the Instrument objects.
    /// On the start of the application, the CreateInstrument method is used to create the instrument instances based on instruments list data.
    /// The UpdateInstrument should be called when Instrument is found by TT in order to finish the Instrument initialization.
    /// </summary>
    public class InstrumentFactory
    {
        readonly Dictionary<string, InstrumentFutureSpreadImplied> m_outrightsSpreadImpliedByKey = new Dictionary<string, InstrumentFutureSpreadImplied>();
        readonly Dictionary<string, Instrument> m_outrightsBasicByKey = new Dictionary<string, Instrument>();
        readonly Dictionary<ITTInstrumentState, InstrumentSpread> m_spreadsByRef = new Dictionary<ITTInstrumentState, InstrumentSpread>();
        readonly Dictionary<ITTInstrumentState, InstrumentSpreadFlowdown> m_flowdownSpreadsByRef = new Dictionary<ITTInstrumentState, InstrumentSpreadFlowdown>();

        readonly object m_buildSync = new object();

        public InstrumentFactory()
        {
        }

        /// <summary>
        /// This method is used to create the Instrument class. The def parameter defines the Instrument Type and Price Model.
        /// Possible values for def.ProdType : Spread, Future.
        /// Possible values for def.PriceModel : Basic, SpreadImplied, SpreadImpliedNonlinear, FlowdownSpread.
        /// The SpreadImplied and SpreadImpliedNonlinear models can be used for Futures only.
        /// The FlowdownSpread can be used for Spreads only.
        /// </summary>
        /// <param name="def">The instrument definition.</param>
        /// <param name="instState">The KT_TTAPI.TTInstrument instance</param>
        /// <returns></returns>
        public Instrument CreateInstrument(InstrDef def, ITTInstrumentState instState)
        {
            bool isSpread = string.Compare(strA: def.ProdType, strB: "Spread", ignoreCase: true) == 0;

            bool isBasicPriceModel = string.Compare(strA: def.PriceModel, strB: "Basic", ignoreCase: true) == 0;
            bool isSpreadImpliedModel = string.Compare(strA: def.PriceModel, strB: "SpreadImplied", ignoreCase: true) == 0;
            bool isSpreadImpliedNonlinearModel = string.Compare(strA: def.PriceModel, strB: "SpreadImpliedNonlinear", ignoreCase: true) == 0;
            bool isFlowdownSpreadPriceModel = string.Compare(strA: def.PriceModel, strB: "FlowdownSpread", ignoreCase: true) == 0;

            if (!isBasicPriceModel && !isSpreadImpliedModel && !isSpreadImpliedNonlinearModel && !isFlowdownSpreadPriceModel)
            {
                Debug.WriteLine("An unknown price model {0} is used for instrument {1}", def.PriceModel, def.Alias);
            }

            if (isSpread && !isBasicPriceModel && !isFlowdownSpreadPriceModel)
            {
                Debug.WriteLine("An unexpected price model {0} is used for spread instrument {1}", def.PriceModel, def.Alias);
            }

            if (!isSpread && !isBasicPriceModel && !isSpreadImpliedModel && !isSpreadImpliedNonlinearModel)
            {
                Debug.WriteLine("An unexpected price model {0} is used for outright instrument {1}", def.PriceModel, def.Alias);
            }

            lock (m_buildSync)
            {
                if (isSpread)
                {
                    if (isFlowdownSpreadPriceModel)
                        return m_flowdownSpreadsByRef[instState] = CreateFlowdownSpreadInstrument(instState);

                    return m_spreadsByRef[instState] = CreateSpreadInstrument(instState);
                }

                var key = MakeKey(exchange: def.Exchange, productName: def.Product, productType: def.ProdType, expiry: def.Contract);

                if (isSpreadImpliedModel)
                    return m_outrightsSpreadImpliedByKey[key] = CreateSpreadImpliedOutright(instState);

                if (isSpreadImpliedNonlinearModel)
                    return m_outrightsSpreadImpliedByKey[key] = CreateSpreadImpliedNonlinearOutright(instState);

                return m_outrightsBasicByKey[key] = CreateBasicOutright(instState);
            }
        }

        string MakeKey(string exchange, string productName, string productType, string expiry)
        {
            return exchange.ToLower() + "##" + productName.ToLower() + "##" + productType.ToLower() + "##" + expiry.ToLower();
        }

        public virtual Instrument CreateBasicOutright(ITTInstrumentState instState)
        {
            return new Instrument(instState);
        }

        public virtual InstrumentFutureSpreadImplied CreateSpreadImpliedOutright(ITTInstrumentState instState)
        {
            return new InstrumentFutureSpreadImplied(instrumentState: instState, transformations: null, aggregation: new ArithmeticMeanAggregation());
        }

        public virtual InstrumentFutureSpreadImplied CreateSpreadImpliedNonlinearOutright(ITTInstrumentState instState)
        {
            var transformations = new IFlowDownErrorTransformation[] { new WidthLiquidityTransformation() };
            var aggregation = new NonlinearAggregation();
            return new InstrumentFutureSpreadImplied(instrumentState: instState, transformations: transformations, aggregation: aggregation);
        }

        public virtual InstrumentSpread CreateSpreadInstrument(ITTInstrumentState instState)
        {
            return new InstrumentSpread(instState);
        }

        public virtual InstrumentSpreadFlowdown CreateFlowdownSpreadInstrument(ITTInstrumentState instState)
        {
            return new InstrumentSpreadFlowdown(instState);
        }


        /// <summary>
        /// The UpdateInstrument should be called after the instrument is found by TT (and as the result the instState is initialized).
        /// 
        /// For outright instruments, this method does nothing.
        /// 
        /// For spread instrument, this method builds the  lists of legs and performs the subscription to spread and other legs prices for 
        /// "spread-implied" price model instruments.
        /// In the case of error, the list of legs is not created.
        /// Only ICS, Calendar and ReducedTickSpread are supported. 
        /// </summary>
        /// <param name="instState">The ITTInstrumentState reference passed to CreateInstrument on creation.</param>
        public void UpdateInstrument(ITTInstrumentState instState)
        {
            InstrumentSpread spread = null;
            InstrumentSpreadFlowdown flowdownSpread = null;
            m_spreadsByRef.TryGetValue(instState, out spread);
            if (spread == null)
            {
                m_flowdownSpreadsByRef.TryGetValue(instState, out flowdownSpread);
                if (flowdownSpread == null)
                    return;

                spread = flowdownSpread;
            }

            switch (spread.SpreadType)
            {
                case TTSpreadType.Ics:
                case TTSpreadType.Calendar:
                case TTSpreadType.ReducedTickSpread:
                    break;
                default:
                    Debug.WriteLine("Cannot build the spread '{0}'. Reason : the Spread Type {1} is not supported.", spread.getAlias(), spread.SpreadType);
                    return;
            }

            var ttLegs = spread.TTLegs;
            if (ttLegs == null)
            {
                Debug.WriteLine("Cannot build the spread '{0}'. Reason : unexpected number of legs.", spread.getAlias());
                return;
            }

            List<SpreadLeg> spreadLegs = new List<SpreadLeg>(4);
            List<InstrumentFutureSpreadImplied> spreadImpliedInstruments = new List<InstrumentFutureSpreadImplied>(4);

            foreach (var ttLeg in ttLegs)
            {
                string key = MakeKey(exchange: ttLeg.Exchange, productName: ttLeg.ProductName, productType: ttLeg.ProductType, expiry: ttLeg.Expiry);

                int legRatio = ttLeg.Ratio;
                if (legRatio == 0)
                {
                    Debug.WriteLine("Cannot build the spread '{0}'. Reason : the legs {1} ratio is zero.", spread.getAlias(), key);
                    return;
                }

                InstrumentFutureSpreadImplied foundSpreadImpliedInstrument = null;
                Instrument foundBasicInstrument = null;
                m_outrightsSpreadImpliedByKey.TryGetValue(key, out foundSpreadImpliedInstrument);
                if (foundSpreadImpliedInstrument == null)
                {
                    m_outrightsBasicByKey.TryGetValue(key, out foundBasicInstrument);
                    if (foundBasicInstrument == null)
                    {
                        Debug.WriteLine("Cannot build the spread '{0}'. Reason : the leg {1} is unknown.", spread.getAlias(), key);
                        return;
                    }
                }

                if (foundSpreadImpliedInstrument != null)
                {
                    foundSpreadImpliedInstrument.AddSpread(spread);
                    spreadImpliedInstruments.Add(foundSpreadImpliedInstrument);
                }

                var legInstrument = foundBasicInstrument ?? foundSpreadImpliedInstrument;
                spreadLegs.Add(new SpreadLeg(legInstrument, legRatio));

                if (flowdownSpread != null)
                {
                    //The flowdown spread should recalculate on leg price change
                    legInstrument.OnPriceChanged -= flowdownSpread.OnLinkedInstrumentChanged;
                    legInstrument.OnPriceChanged += flowdownSpread.OnLinkedInstrumentChanged;
                }
            }

            if (spreadLegs.Count != 2)
            {
                Debug.WriteLine("The spread '{0}' is ignored. Reason : unsupported number of legs {1}.", spread.getAlias(), spreadLegs.Count);
                return;
            }

            float minPointValue = spreadLegs.Min(l => l.Instrument.PointValue);
            if (float.IsNaN(minPointValue) || minPointValue < 0.0000001)
            {
                Debug.WriteLine("The spread '{0}' is ignored. Reason : wrong min point value {1}.", spread.getAlias(), minPointValue);
                return;
            }

            foreach(var leg in spreadLegs)
            {
                leg.Weight = leg.Ratio * leg.Instrument.PointValue / minPointValue;

                if (float.IsNaN(leg.Weight))
                {
                    Debug.WriteLine("The spread '{0}' is ignored. Reason : wrong leg {1} point value.", spread.getAlias(), leg.Instrument.getAlias());
                    return;
                }
            }

            spread.Legs = spreadLegs.ToArray();

            foreach (var instrument in spreadImpliedInstruments)
            {
                //The Spread-Implied model instrument should re-calculate on spread price change
                spread.OnPriceChanged -= instrument.OnLinkedInstrumentChanged;
                spread.OnPriceChanged += instrument.OnLinkedInstrumentChanged;

                //The Spread-Implied model instrument should -calculate on other leg change
                foreach (var spreadLeg in spreadLegs)
                {
                    if (!ReferenceEquals(spreadLeg.Instrument, instrument))
                    {
                        spreadLeg.Instrument.OnPriceChanged -= instrument.OnLinkedInstrumentChanged;
                        spreadLeg.Instrument.OnPriceChanged += instrument.OnLinkedInstrumentChanged;
                    }
                }
            }
        }


    }
}

