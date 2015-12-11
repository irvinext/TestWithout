using KT_TTAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonkeyCancel
{
    public interface ISpread : IInstrument
    {
        SpreadLeg[] Legs { get; }
        bool IsNetChangePx { get; }
    }


    /// <summary>
    /// This class represents the Leg of Spread.
    /// The "Instrument" is the instance of leg instrument (the single instance, all legs share it).
    /// The "Ratio" is the leg ratio; can be negative. 
    /// </summary>
    public class SpreadLeg
    {
        public SpreadLeg(IInstrument instrument, int ratio)
        {
            Instrument = instrument;
            Ratio = ratio;
            Weight = ratio; 
        }

        public IInstrument Instrument { get; private set; }
        public int Ratio { get; private set; }
        public float Weight { get; set; }
    }


    /// <summary>
    /// This is the class of Spread instrument to keep the Spread-specific data like legs.
    /// </summary>
    public class InstrumentSpread : Instrument, ISpread
    {
        #region Variables

        /// <summary>
        /// The list of legs. It is assigned when the spread is found by TT.
        /// </summary>
        volatile SpreadLeg[] m_Legs;
        public SpreadLeg[] Legs
        {
            get
            {
                return m_Legs;
            }

            set
            {
                m_Legs = value;

                //If this is "CME" ICS instrument then it uses the net change pricing.
                if (string.Compare(m_ttInstrumentState.Exchange, "cme", true) == 0)
                    if (m_ttInstrumentState.SpreadType == TTSpreadType.Ics)
                        IsNetChangePx = true;
            }
        }

        /// <summary>
        /// The list of KT_TTAPI legs. It is used during the initialization to find the legs Intruments.
        /// </summary>
        public IEnumerable<ITTLeg> TTLegs
        {
            get { return m_ttInstrumentState.Legs; }
        }

        public TTSpreadType SpreadType
        {
            get { return m_ttInstrumentState.SpreadType; }
        }

        /// <summary>
        /// This value is true if Spread uses the net change pricing. Otherwise is false.
        /// </summary>
        public bool IsNetChangePx { get; private set; }

        #endregion


        public InstrumentSpread(ITTInstrumentState instrumentState)
            : base(instrumentState)
        {
        }


        public override string ToString()
        {

            StringBuilder sb = new StringBuilder(256);
            sb.Append("[SpreadInstrument ")
                .Append("IsNetChangePx=").Append(IsNetChangePx).Append(" ")
                .Append(base.ToString());

            var legs = Legs;
            if (legs != null) 
                foreach(var leg in legs)
                {
                    var instrument = leg.Instrument;
                    sb.Append(", [Leg: ").Append(leg.Ratio);
                    sb.Append(" ").Append(leg.Weight);
                    sb.Append(" ").Append(instrument != null ? instrument.getAlias() : string.Empty);
                    sb.Append(" /Leg]");
                }

            sb.Append("/SpreadInstrument]");

            return sb.ToString();
        }
    }
}